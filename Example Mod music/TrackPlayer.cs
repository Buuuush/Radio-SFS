// Fournit IEnumerator pour les coroutines qui chargent un fichier audio.
using System.Collections;
// Fournit Path et File.Exists pour manipuler les fichiers audio.
using System.IO;
// Fournit Select, Where, OrderBy et ToList pour filtrer les pistes.
using System.Linq;
// Fournit MethodInfo et BindingFlags pour appeler du code privé de SFS.
using System.Reflection;
// Fournit MusicPlaylist, MusicTrack et MusicPlaylistPlayer.
using SFS.Audio;
// Fournit AudioClip, AudioSource, Debug et les types Unity courants.
using UnityEngine;
// Fournit UnityWebRequestMultimedia pour charger des fichiers audio locaux.
using UnityEngine.Networking;
// Utilise le générateur aléatoire de .NET sous le nom court Random.
using Random = System.Random;

namespace CustomMusic
{
    // Sélectionne et lit les pistes musicales, natives ou personnalisées.
    public static class TrackPlayer
    {
        // Indique qu'un changement de piste est déjà en cours.
        private static bool isSwitchingTracks;

        // Générateur utilisé pour choisir une piste au hasard.
        private static readonly Random Rng = new();

        // Sélectionne puis démarre une piste.
        public static bool TryPlayTrack(MusicPlaylistPlayer player, int? requestedIndex, float fadeTime)
        {
            // Refuse un second changement pendant le premier.
            if (isSwitchingTracks) return false;

            // Verrouille temporairement le changement de piste.
            isSwitchingTracks = true;

            // Récupère la playlist du lecteur.
            MusicPlaylist playlist = player.playlist;
            // Arrête la méthode si la playlist est absente ou vide.
            if (!playlist || playlist.tracks.Count == 0)
            {
                // Libère le verrou avant chaque sortie anticipée.
                isSwitchingTracks = false;
                return false;
            }

            // Lit l'index de l'ancienne piste pour éviter de la reprendre immédiatement.
            var lastTrack = GetCurrentTrack(player);
            // Utilise l'index demandé, ou choisit automatiquement une piste si null.
            var index = requestedIndex ?? GetNextValidTrack(player, lastTrack);

            // Vérifie que l'index obtenu existe dans la liste.
            if (index < 0 || index >= playlist.tracks.Count)
            {
                isSwitchingTracks = false;
                return false;
            }

            // Récupère l'objet MusicTrack correspondant à l'index.
            MusicTrack track = playlist.tracks[index];
            // Un chemin qui correspond à un fichier présent indique une piste personnalisée.
            var isCustom = File.Exists(track.clipName);

            // Informe SFS de l'index qui va être lu.
            SetCurrentTrack(player, index);
            // Arrête l'ancienne piste avec la durée de fondu demandée.
            player.StopPlaying(fadeTime);

            // Les pistes personnalisées passent par une coroutine de chargement.
            if (isCustom)
                CoroutineRunner.Instance.StartCoroutine(PlayCustomTrack(player, track, fadeTime));
            // Les pistes natives sont déjà des ressources Unity connues de SFS.
            else
                PlayVanilla(player, track, fadeTime);

            // Libère le verrou après avoir lancé le chargement ou la lecture.
            isSwitchingTracks = false;
            return true;
        }

        // Construit la liste des pistes autorisées et choisit la prochaine.
        private static int GetNextValidTrack(MusicPlaylistPlayer player, int lastTrack)
        {
            // Récupère la playlist, le nom de scène et le réglage de musique native.
            MusicPlaylist playlist = player.playlist;
            var scene = player.gameObject.scene.name;
            var allowVanilla = MusicInjector.ShouldIncludeVanilla(scene);

            // Conserve les indices des pistes autorisées sans copier les objets MusicTrack.
            var validIndices = playlist.tracks
                // Associe chaque piste à son index dans la liste.
                .Select((track, idx) => new { track, idx })
                // Garde les pistes locales, ou toutes les pistes si le vanilla est autorisé.
                .Where(x => File.Exists(x.track.clipName) || allowVanilla)
                // Ne conserve ensuite que l'index.
                .Select(x => x.idx)
                // Transforme le résultat en liste utilisable plusieurs fois.
                .ToList();

            // Traite les cas où il y a zéro ou une seule piste possible.
            switch (validIndices.Count)
            {
                // Aucune piste n'est disponible.
                case 0:
                    return -1;
                // Une seule piste est disponible, elle est forcément le choix.
                case 1:
                    return validIndices[0];
            }

            // Demande un choix aléatoire au premier démarrage ou après PlayRandom.
            var doShuffle =
                lastTrack == -1 ||
                (lastTrack >= 0 &&
                 lastTrack < playlist.tracks.Count &&
                 playlist.tracks[lastTrack].onTrackEnd == MusicTrack.OnTrackEnd.PlayRandom);

            // Si le mélange est demandé, retire l'ancienne piste et choisit au hasard.
            if (doShuffle)
                return validIndices
                    .Where(i => i != lastTrack)
                    // Trie avec une valeur aléatoire pour mélanger les indices.
                    .OrderBy(_ => Rng.Next())
                    // Prend le premier résultat mélangé.
                    .First();

            // Sinon, prend la première piste différente de la précédente.
            var fallback = validIndices.FirstOrDefault(i => i != lastTrack);
            // Si aucune piste différente n'existe, utilise la première piste disponible.
            return fallback != -1 ? fallback : validIndices[0];
        }

        // Modifie le champ privé currentTrack utilisé par SFS.
        private static void SetCurrentTrack(MusicPlaylistPlayer player, int index)
        {
            // Cherche le champ puis lui affecte l'index reçu.
            typeof(MusicPlaylistPlayer).GetField("currentTrack", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(player, index);
        }

        // Lit l'index privé currentTrack, ou retourne -1 s'il est absent.
        private static int GetCurrentTrack(MusicPlaylistPlayer player)
        {
            // ?? -1 remplace une valeur null par -1.
            return (int)(typeof(MusicPlaylistPlayer)
                .GetField("currentTrack", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(player) ?? -1);
        }

        // Prépare puis démarre une piste native avec le fondu de SFS.
        private static void PlayVanilla(MusicPlaylistPlayer player, MusicTrack track, float fadeTime)
        {
            // Récupère la source audio du lecteur.
            AudioSource source = player.source;

            // Charge le clip Unity dont le nom est stocké dans la piste.
            source.clip = Resources.Load<AudioClip>(track.clipName);
            // Applique la vitesse de lecture de la piste.
            source.pitch = track.pitch;

            // Configure les champs privés utilisés par le fondu audio.
            ReflectionUtils.SetPrivateField(player, "fadeTime", fadeTime);
            ReflectionUtils.SetPrivateField(player, "targetFadeVolume", 1f);
            ReflectionUtils.SetPrivateField(player, "currentFadeVolume", 0f);

            // Récupère la méthode privée qui recalcule le volume.
            MethodInfo updateVolume = typeof(MusicPlaylistPlayer)
                .GetMethod("UpdateVolume", BindingFlags.NonPublic | BindingFlags.Instance);
            // Appelle cette méthode si elle a été trouvée.
            updateVolume?.Invoke(player, null);

            // Démarre la lecture du clip.
            source.Play();
        }

        // Essaie de démarrer une autre piste si le fichier actuel est illisible.
        private static void SkipToNextTrack(MusicPlaylistPlayer player)
        {
            // Récupère la piste actuelle, puis cherche la suivante.
            var current = GetCurrentTrack(player);
            var next = GetNextValidTrack(player, current);

            // Lance la piste suivante seulement si elle existe et change réellement.
            if (next != -1 && next != current) TryPlayTrack(player, next, 1f);
        }

        // Charge une piste locale dans une coroutine pour ne pas bloquer Unity.
        private static IEnumerator PlayCustomTrack(MusicPlaylistPlayer player, MusicTrack track, float fadeTime)
        {
            // Transforme le chemin Windows en URL file:// comprise par Unity.
            var url = "file://" + track.clipName.Replace("\\", "/");
            // Convertit l'extension en type audio Unity.
            AudioType type = GetAudioType(Path.GetExtension(track.clipName));

            // Prépare une requête réseau, même si la source est un fichier local.
            using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, type);
            // Demande à Unity de lire progressivement le fichier.
            ((DownloadHandlerAudioClip)www.downloadHandler).streamAudio = true;
            // Attend la fin de la requête sans bloquer le thread principal.
            yield return www.SendWebRequest();

            // Vérifie si Unity a signalé une erreur de chargement.
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[CustomMusicMod] Failed to load track: {track.clipName} | Error: {www.error}");
                // Tente de continuer avec une autre piste.
                SkipToNextTrack(player);
                yield break;
            }

            // Récupère le AudioClip produit par la requête.
            AudioClip clip = DownloadHandlerAudioClip.GetContent(www);

            // Vérifie que le clip existe, est chargé et possède une durée positive.
            if (!clip || clip.loadState != AudioDataLoadState.Loaded || clip.length <= 0)
            {
                Debug.LogError($"[CustomMusicMod] Clip load failed or unsupported format: {track.clipName}");
                SkipToNextTrack(player);
                yield break;
            }

            // Configure la source audio avec le fichier chargé.
            AudioSource source = player.source;
            source.clip = clip;
            source.pitch = track.pitch;

            // Configure le fondu comme pour une piste native.
            ReflectionUtils.SetPrivateField(player, "fadeTime", fadeTime);
            ReflectionUtils.SetPrivateField(player, "targetFadeVolume", 1f);
            ReflectionUtils.SetPrivateField(player, "currentFadeVolume", 0f);

            // Demande à SFS de recalculer le volume du lecteur.
            typeof(MusicPlaylistPlayer)
                .GetMethod("UpdateVolume", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.Invoke(player, null);

            // Démarre la lecture du fichier personnalisé.
            source.Play();
        }

        // Convertit une extension de fichier en type audio Unity.
        private static AudioType GetAudioType(string ext)
        {
            // Compare l'extension et retourne le type Unity correspondant.
            return ext.ToLowerInvariant() switch
            {
                // MP3 correspond au format MPEG de Unity.
                ".mp3" => AudioType.MPEG,
                // WAV correspond directement au type WAV.
                ".wav" => AudioType.WAV,
                // OGG correspond au type OGGVORBIS.
                ".ogg" => AudioType.OGGVORBIS,
                // AIFF et AIF utilisent le même type Unity.
                ".aiff" or ".aif" => AudioType.AIFF,
                // Toute extension inconnue est refusée.
                _ => AudioType.UNKNOWN
            };
        }
    }
}