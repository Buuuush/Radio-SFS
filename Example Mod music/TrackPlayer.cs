using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using SFS.Audio;
using UnityEngine;
using UnityEngine.Networking;
using Random = System.Random;

namespace CustomMusic
{
    // Contrôle la sélection et la lecture des pistes musicales.
    //
    // Les pistes natives sont chargées comme ressources Unity de SFS. Les
    // pistes personnalisées sont chargées depuis le disque avec
    // UnityWebRequestMultimedia. Cette différence est importante : un chemin
    // de fichier complet dans MusicTrack.clipName sert aussi de marqueur pour
    // reconnaître une piste personnalisée.
    public static class TrackPlayer
    {
        // Empêche deux appels imbriqués de changer la piste simultanément.
        // Cette protection évite notamment que Update et un changement de scène
        // modifient currentTrack au même moment.
        private static bool isSwitchingTracks;

        // Générateur utilisé pour choisir une piste aléatoire sans créer un
        // nouvel objet à chaque appel.
        private static readonly Random Rng = new();

        // Sélectionne puis démarre une piste.
        // requestedIndex permet de demander une piste précise ; lorsqu'il est
        // null, GetNextValidTrack choisit la prochaine piste autorisée.
        // La méthode renvoie false lorsqu'aucune lecture ne peut commencer.
        public static bool TryPlayTrack(MusicPlaylistPlayer player, int? requestedIndex, float fadeTime)
        {
            if (isSwitchingTracks) return false;

            isSwitchingTracks = true;

            MusicPlaylist playlist = player.playlist;
            if (!playlist || playlist.tracks.Count == 0)
            {
                isSwitchingTracks = false;
                return false;
            }

            // Lit l'index privé de la piste précédente avant de le remplacer.
            var lastTrack = GetCurrentTrack(player);
            var index = requestedIndex ?? GetNextValidTrack(player, lastTrack);

            if (index < 0 || index >= playlist.tracks.Count)
            {
                isSwitchingTracks = false;
                return false;
            }

            MusicTrack track = playlist.tracks[index];
            // Un fichier existant est une piste locale ; sinon SFS doit charger
            // clipName comme une ressource audio native.
            var isCustom = File.Exists(track.clipName);


            SetCurrentTrack(player, index);
            player.StopPlaying(fadeTime);

            if (isCustom)
                CoroutineRunner.Instance.StartCoroutine(PlayCustomTrack(player, track, fadeTime));
            else
                PlayVanilla(player, track, fadeTime);

            isSwitchingTracks = false;
            return true;
        }

        // Construit la liste des pistes autorisées puis choisit un index.
        // Les pistes natives sont filtrées lorsque l'utilisateur les a
        // désactivées dans la configuration.
        private static int GetNextValidTrack(MusicPlaylistPlayer player, int lastTrack)
        {
            MusicPlaylist playlist = player.playlist;
            var scene = player.gameObject.scene.name;
            var allowVanilla = MusicInjector.ShouldIncludeVanilla(scene);

            // Construit une liste d'indices plutôt qu'une liste de copies de
            // MusicTrack afin de conserver les objets déjà présents dans la
            // playlist.
            var validIndices = playlist.tracks
                .Select((track, idx) => new { track, idx })
                .Where(x => File.Exists(x.track.clipName) || allowVanilla)
                .Select(x => x.idx)
                .ToList();

            switch (validIndices.Count)
            {
                case 0:
                    return -1;
                case 1:
                    return validIndices[0];
            }

            // Au premier démarrage ou lorsqu'une piste demande PlayRandom,
            // mélange les indices tout en évitant autant que possible de
            // sélectionner immédiatement la même piste.
            var doShuffle =
                lastTrack == -1 || // first time playing
                (lastTrack >= 0 &&
                 lastTrack < playlist.tracks.Count &&
                 playlist.tracks[lastTrack].onTrackEnd == MusicTrack.OnTrackEnd.PlayRandom);

            if (doShuffle)
                return validIndices
                    .Where(i => i != lastTrack)
                    .OrderBy(_ => Rng.Next())
                    .First();

            // Comportement normal : prend la première piste différente de la
            // précédente. Le dernier retour garantit qu'une playlist valide
            // d'une seule piste peut tout de même continuer à jouer.
            var fallback = validIndices.FirstOrDefault(i => i != lastTrack);
            return fallback != -1 ? fallback : validIndices[0];
        }

        // Modifie le champ privé currentTrack de SFS.
        private static void SetCurrentTrack(MusicPlaylistPlayer player, int index)
        {
            typeof(MusicPlaylistPlayer).GetField("currentTrack", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(player, index);
        }

        // Lit le champ privé currentTrack de SFS. -1 est utilisé si le champ
        // n'est pas trouvé ou si aucune piste n'est actuellement sélectionnée.
        private static int GetCurrentTrack(MusicPlaylistPlayer player)
        {
            return (int)(typeof(MusicPlaylistPlayer)
                .GetField("currentTrack", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(player) ?? -1);
        }

        // Prépare une piste native et réutilise le système de fondu de SFS.
        private static void PlayVanilla(MusicPlaylistPlayer player, MusicTrack track, float fadeTime)
        {
            AudioSource source = player.source;

            source.clip = Resources.Load<AudioClip>(track.clipName);
            source.pitch = track.pitch;

            // Initialise les champs internes du fondu comme le ferait le code
            // vanilla : démarrage silencieux, puis augmentation progressive.
            ReflectionUtils.SetPrivateField(player, "fadeTime", fadeTime);
            ReflectionUtils.SetPrivateField(player, "targetFadeVolume", 1f);
            ReflectionUtils.SetPrivateField(player, "currentFadeVolume", 0f); // start silent and let vanilla fade it in

            // Met immédiatement à jour le volume avant de lancer la source.
            MethodInfo updateVolume = typeof(MusicPlaylistPlayer)
                .GetMethod("UpdateVolume", BindingFlags.NonPublic | BindingFlags.Instance);
            updateVolume?.Invoke(player, null);

            source.Play();
        }

        // Passe à une autre piste lorsqu'un fichier personnalisé ne peut pas
        // être chargé ou n'est pas lisible.
        private static void SkipToNextTrack(MusicPlaylistPlayer player)
        {
            var current = GetCurrentTrack(player);
            var next = GetNextValidTrack(player, current);

            if (next != -1 && next != current) TryPlayTrack(player, next, 1f);
        }

        // Charge et joue une piste locale de manière asynchrone.
        // Le chargement dans une coroutine évite de bloquer le thread principal
        // pendant que Unity lit le fichier depuis le disque.
        private static IEnumerator PlayCustomTrack(MusicPlaylistPlayer player, MusicTrack track, float fadeTime)
        {
            var url = "file://" + track.clipName.Replace("\\", "/");
            AudioType type = GetAudioType(Path.GetExtension(track.clipName));

            using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, type);
            ((DownloadHandlerAudioClip)www.downloadHandler).streamAudio = true;
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[CustomMusicMod] Failed to load track: {track.clipName} | Error: {www.error}");
                SkipToNextTrack(player);
                yield break;
            }

            AudioClip clip = DownloadHandlerAudioClip.GetContent(www);

            // Vérifie que Unity a réellement décodé le fichier avant de le
            // donner à AudioSource. Un fichier trouvé sur le disque peut tout
            // de même être corrompu ou utiliser un format non supporté.
            if (!clip || clip.loadState != AudioDataLoadState.Loaded || clip.length <= 0)
            {
                Debug.LogError($"[CustomMusicMod] Clip load failed or unsupported format: {track.clipName}");
                SkipToNextTrack(player);
                yield break;
            }

            AudioSource source = player.source;
            source.clip = clip;
            source.pitch = track.pitch;

            ReflectionUtils.SetPrivateField(player, "fadeTime", fadeTime);
            ReflectionUtils.SetPrivateField(player, "targetFadeVolume", 1f);
            ReflectionUtils.SetPrivateField(player, "currentFadeVolume", 0f);

            typeof(MusicPlaylistPlayer)
                .GetMethod("UpdateVolume", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.Invoke(player, null);

            source.Play();
        }

        // Traduit l'extension du fichier en valeur AudioType attendue par
        // UnityWebRequestMultimedia.GetAudioClip.
        private static AudioType GetAudioType(string ext)
        {
            return ext.ToLowerInvariant() switch
            {
                ".mp3" => AudioType.MPEG,
                ".wav" => AudioType.WAV,
                ".ogg" => AudioType.OGGVORBIS,
                ".aiff" or ".aif" => AudioType.AIFF,
                _ => AudioType.UNKNOWN
            };
        }
    }
}