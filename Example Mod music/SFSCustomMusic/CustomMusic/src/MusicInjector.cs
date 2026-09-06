using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using SFS.Audio;
using UnityEngine;

namespace CustomMusic
{
    // Remplace le contenu des playlists SFS par une combinaison de pistes
    // natives et de pistes personnalisées.
    //
    // L'injection est séparée du démarrage de la lecture : cette classe décide
    // quelles pistes doivent exister dans la playlist, tandis que TrackPlayer
    // décide comment les lire.
    public static class MusicInjector
    {
        // Reconstruit la playlist d'un lecteur donné.
        //
        // Les pistes personnalisées déjà présentes sont d'abord retirées afin
        // d'éviter les doublons. Si les réglages autorisent la musique native,
        // une copie propre du cache vanilla est concaténée aux nouvelles
        // pistes. Sinon, seules les pistes du dossier Music sont conservées.
        private static void Inject(MusicPlaylistPlayer player, string sceneName)
        {
            if (!player || !player.playlist)
                return;

            MusicPlaylist playlist = player.playlist;
            var customTracks = MusicLoader.LoadForScene(sceneName);

            // Retire les anciennes pistes personnalisées. File.Exists permet
            // de distinguer une piste locale d'un identifiant de ressource
            // utilisé par la musique native de SFS.
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

            // Force le prochain choix de piste à repartir d'un état neutre.
            // currentTrack est privé dans SFS, d'où l'utilisation de la
            // réflexion.
            typeof(MusicPlaylistPlayer).GetField("currentTrack", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(player, -1);
        }

        // Attend l'apparition du MusicPlaylistPlayer, puis injecte la playlist.
        // Une attente est nécessaire car l'événement de scène peut survenir
        // avant que les objets audio de SFS ne soient créés.
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

            // Empêche une seconde injection lorsque la scène a déjà été
            // traitée et contient encore au moins une piste personnalisée.
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

        // Retourne le réglage correspondant à une scène donnée.
        // Les scènes inconnues conservent la musique native par défaut afin de
        // ne pas modifier accidentellement un autre système audio de SFS.
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

        // Reconstruit immédiatement les lecteurs concernés lorsqu'un toggle
        // du menu est modifié.
        public static void OnSceneToggleChanged(string sceneName)
        {
            var players = Object.FindObjectsOfType<MusicPlaylistPlayer>();
            foreach (MusicPlaylistPlayer p in players)
                if (p != null && p.playlist != null && p.gameObject.scene.name == sceneName)
                    Inject(p, sceneName);
        }
    }
}