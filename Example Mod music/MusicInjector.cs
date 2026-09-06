using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using SFS.Audio;
using UnityEngine;

namespace CustomMusic
{
    // Construit la playlist finale utilisée par SFS.
    //
    // Cette classe décide quelles pistes doivent figurer dans la playlist.
    // TrackPlayer s'occupe ensuite de choisir et de lire la piste courante.
    public static class MusicInjector
    {
        // Retire les anciennes pistes personnalisées, puis combine le cache
        // des pistes natives avec les fichiers personnalisés si la configuration
        // autorise la musique vanilla.
        private static void Inject(MusicPlaylistPlayer player, string sceneName)
        {
            if (!player || !player.playlist)
                return;

            MusicPlaylist playlist = player.playlist;
            var customTracks = MusicLoader.LoadForScene(sceneName);

            // Un chemin de fichier existant identifie une piste personnalisée.
            // Les autres entrées sont conservées comme pistes natives.
            playlist.tracks = playlist.tracks
                .Where(t => !File.Exists(t.clipName)) // keep vanilla only
                .ToList();

            if (ShouldIncludeVanilla(sceneName))
            {
                var cachedVanilla = VanillaPlaylistCache.GetCachedVanilla(playlist);
                playlist.tracks = cachedVanilla.Concat(customTracks).ToList();
            }
            else
            {
                playlist.tracks = customTracks;
            }

            // currentTrack est privé dans SFS. Le remettre à -1 force le
            // lecteur à choisir une nouvelle piste après la reconstruction.
            typeof(MusicPlaylistPlayer).GetField("currentTrack", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(player, -1);
        }

        // Attend que SFS ait créé son MusicPlaylistPlayer avant d'injecter la
        // playlist. L'attente est nécessaire car l'événement de scène peut
        // survenir avant les objets audio.
        public static IEnumerator InjectAfterSceneLoad(string sceneName)
        {
            Debug.Log($"[CustomMusicMod] Waiting for MusicPlaylistPlayer in scene: {sceneName}");

            MusicPlaylistPlayer player = null;
            yield return new WaitUntil(() =>
            {
                player = Object.FindObjectOfType<MusicPlaylistPlayer>();
                return player;
            });

            if (!player || !player.playlist)
            {
                Debug.LogWarning("[CustomMusicMod] Aborted: player or playlist became null after wait");
                yield break;
            }

            // Évite de traiter deux fois une playlist qui contient déjà une
            // piste personnalisée.
            if (player.playlist.tracks.Count > 0 &&
                player.playlist.tracks.Any(t => File.Exists(t.clipName)))
            {
                Debug.Log("[CustomMusicMod] Skipping inject: playlist already contains custom tracks");
                yield break;
            }

            VanillaPlaylistCache.CacheIfNeeded(player.playlist);
            Inject(player, sceneName);

            Debug.Log($"[CustomMusicMod] Final injected playlist: {player.playlist.tracks.Count} tracks");
        }

        // Retourne le réglage de conservation des pistes natives pour une scène.
        // Une scène inconnue conserve la musique native par sécurité.
        public static bool ShouldIncludeVanilla(string sceneName)
        {
            return sceneName switch
            {
                "Home_PC" => Config.settings.homeMenu.Value,
                "Build_PC" => Config.settings.buildScene.Value,
                "World_PC" => Config.settings.worldScene.Value,
                _ => true
            };
        }

        // Reconstruit immédiatement les lecteurs concernés après la modification
        // d'un interrupteur dans le menu de configuration.
        public static void OnSceneToggleChanged(string sceneName)
        {
            var players = Object.FindObjectsOfType<MusicPlaylistPlayer>();
            foreach (MusicPlaylistPlayer p in players)
                if (p != null && p.playlist != null && p.gameObject.scene.name == sceneName)
                    Inject(p, sceneName);
        }
    }
}