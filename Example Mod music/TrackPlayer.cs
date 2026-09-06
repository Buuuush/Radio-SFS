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
    // Sélectionne et lit les pistes musicales.
    //
    // Une piste native est chargée comme ressource Unity de SFS. Une piste
    // personnalisée est un fichier local chargé avec UnityWebRequestMultimedia.
    // La présence d'un chemin de fichier complet permet de distinguer les deux.
    public static class TrackPlayer
    {
        // Empêche deux appels imbriqués de modifier la piste simultanément.
        private static bool isSwitchingTracks;

        // Générateur utilisé pour le choix aléatoire des pistes.
        private static readonly Random Rng = new();

        // Sélectionne et démarre une piste. Si requestedIndex vaut null, la
        // méthode choisit automatiquement une piste valide.
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

            // Conserve l'ancienne piste afin d'éviter de la reprendre
            // immédiatement lorsque plusieurs choix sont possibles.
            var lastTrack = GetCurrentTrack(player);
            var index = requestedIndex ?? GetNextValidTrack(player, lastTrack);

            if (index < 0 || index >= playlist.tracks.Count)
            {
                isSwitchingTracks = false;
                return false;
            }

            MusicTrack track = playlist.tracks[index];
            // Un fichier présent sur le disque est une piste personnalisée.
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

        // Construit la liste des pistes autorisées, puis choisit l'index de la
        // prochaine piste en respectant les réglages vanilla et le mode aléatoire.
        private static int GetNextValidTrack(MusicPlaylistPlayer player, int lastTrack)
        {
            MusicPlaylist playlist = player.playlist;
            var scene = player.gameObject.scene.name;
            var allowVanilla = MusicInjector.ShouldIncludeVanilla(scene);

            // Les indices sont conservés afin de réutiliser les MusicTrack
            // déjà présents dans la playlist sans les copier.
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

            // Le premier démarrage et PlayRandom demandent un choix aléatoire.
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

            // Sinon, prend la première piste différente de la précédente.
            var fallback = validIndices.FirstOrDefault(i => i != lastTrack);
            return fallback != -1 ? fallback : validIndices[0];
        }

        // Modifie le champ privé currentTrack utilisé par le lecteur SFS.
        private static void SetCurrentTrack(MusicPlaylistPlayer player, int index)
        {
            typeof(MusicPlaylistPlayer).GetField("currentTrack", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(player, index);
        }

        // Lit currentTrack. -1 signifie qu'aucune piste n'est sélectionnée.
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

            // Initialise les champs internes du fondu comme le code vanilla.
            ReflectionUtils.SetPrivateField(player, "fadeTime", fadeTime);
            ReflectionUtils.SetPrivateField(player, "targetFadeVolume", 1f);
            ReflectionUtils.SetPrivateField(player, "currentFadeVolume", 0f); // start silent and let vanilla fade it in

            // Met à jour l'état du volume avant de lancer la source audio.
            MethodInfo updateVolume = typeof(MusicPlaylistPlayer)
                .GetMethod("UpdateVolume", BindingFlags.NonPublic | BindingFlags.Instance);
            updateVolume?.Invoke(player, null);

            source.Play();
        }

        // Essaie une autre piste lorsqu'un fichier personnalisé est illisible.
        private static void SkipToNextTrack(MusicPlaylistPlayer player)
        {
            var current = GetCurrentTrack(player);
            var next = GetNextValidTrack(player, current);

            if (next != -1 && next != current) TryPlayTrack(player, next, 1f);
        }

        // Charge une piste locale dans une coroutine afin de ne pas bloquer le
        // thread principal pendant l'accès au fichier.
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

            // Un fichier existant peut être corrompu ou utiliser un format non
            // décodable : vérifie donc le résultat fourni par Unity.
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

        // Convertit une extension de fichier en AudioType Unity.
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