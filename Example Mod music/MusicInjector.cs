// Fournit IEnumerator, nécessaire pour écrire une coroutine Unity.
using System.Collections;
// Fournit File.Exists pour reconnaître les fichiers du mod.
using System.IO;
// Fournit Where, Any et Concat pour manipuler les pistes.
using System.Linq;
// Fournit BindingFlags pour accéder à un champ privé du jeu.
using System.Reflection;
// Fournit MusicPlaylist et MusicPlaylistPlayer.
using SFS.Audio;
// Fournit les méthodes Unity comme WaitUntil, Debug et Object.FindObjectOfType.
using UnityEngine;

namespace CustomMusic
{
    // Construit la playlist finale utilisée par SFS.
    // TrackPlayer choisit et lit ensuite la piste courante.
    public static class MusicInjector
    {
        // Retire les anciennes pistes du mod, puis ajoute les pistes autorisées.
        private static void Inject(MusicPlaylistPlayer player, string sceneName)
        {
            // Ne fait rien si le lecteur ou sa playlist n'existe pas.
            if (!player || !player.playlist)
                return;

            // Mémorise la playlist manipulée.
            MusicPlaylist playlist = player.playlist;
            // Charge les fichiers audio présents dans le dossier de la scène.
            var customTracks = MusicLoader.LoadForScene(sceneName);

            // Garde uniquement les pistes dont le nom n'est pas un fichier local.
            playlist.tracks = playlist.tracks
                .Where(t => !File.Exists(t.clipName))
                .ToList();

            // Si la musique originale est autorisée, la combine avec les pistes du mod.
            if (ShouldIncludeVanilla(sceneName))
            {
                // Récupère la copie intacte des pistes originales.
                var cachedVanilla = VanillaPlaylistCache.GetCachedVanilla(playlist);
                // Concat ajoute les pistes personnalisées à la suite des pistes originales.
                playlist.tracks = cachedVanilla.Concat(customTracks).ToList();
            }
            else
            {
                // Sinon, la playlist ne contient que les pistes personnalisées.
                playlist.tracks = customTracks;
            }

            // currentTrack est privé dans SFS, donc la réflexion est nécessaire pour le modifier.
            // -1 signifie qu'aucune piste n'est actuellement sélectionnée.
            typeof(MusicPlaylistPlayer).GetField("currentTrack", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(player, -1);
        }

        // Attend que le lecteur audio existe, puis injecte la playlist de la scène.
        public static IEnumerator InjectAfterSceneLoad(string sceneName)
        {
            // Écrit dans le journal que la coroutine commence son attente.
            Debug.Log($"[CustomMusicMod] Waiting for MusicPlaylistPlayer in scene: {sceneName}");

            // Cette variable recevra le lecteur trouvé par Unity.
            MusicPlaylistPlayer player = null;
            // Attend jusqu'à ce que la fonction retourne true.
            yield return new WaitUntil(() =>
            {
                // Recherche un objet MusicPlaylistPlayer dans la scène.
                player = Object.FindObjectOfType<MusicPlaylistPlayer>();
                // Un objet trouvé est considéré comme true par Unity.
                return player;
            });

            // Vérifie encore les références après l'attente, car la scène peut avoir changé.
            if (!player || !player.playlist)
            {
                // yield break termine la coroutine immédiatement.
                Debug.LogWarning("[CustomMusicMod] Aborted: player or playlist became null after wait");
                yield break;
            }

            // Évite une seconde injection si une piste locale est déjà présente.
            if (player.playlist.tracks.Count > 0 &&
                player.playlist.tracks.Any(t => File.Exists(t.clipName)))
            {
                // La playlist a déjà été préparée : la coroutine peut se terminer.
                Debug.Log("[CustomMusicMod] Skipping inject: playlist already contains custom tracks");
                yield break;
            }

            // Sauvegarde d'abord les pistes originales avant de modifier la playlist.
            VanillaPlaylistCache.CacheIfNeeded(player.playlist);
            // Construit la playlist finale.
            Inject(player, sceneName);

            // Affiche le nombre de pistes après l'injection.
            Debug.Log($"[CustomMusicMod] Final injected playlist: {player.playlist.tracks.Count} tracks");
        }

        // Retourne le réglage qui indique si les pistes natives sont autorisées.
        public static bool ShouldIncludeVanilla(string sceneName)
        {
            // switch choisit le réglage correspondant au nom de scène.
            return sceneName switch
            {
                // Utilise le réglage du menu d'accueil pour Home_PC.
                "Home_PC" => Config.settings.homeMenu.Value,
                // Utilise le réglage de la scène de construction pour Build_PC.
                "Build_PC" => Config.settings.buildScene.Value,
                // Utilise le réglage de la scène du monde pour World_PC.
                "World_PC" => Config.settings.worldScene.Value,
                // Une scène inconnue conserve la musique originale par sécurité.
                _ => true
            };
        }

        // Reconstruit immédiatement les playlists après un clic dans le menu.
        public static void OnSceneToggleChanged(string sceneName)
        {
            // Récupère tous les lecteurs présents dans la scène actuelle.
            var players = Object.FindObjectsOfType<MusicPlaylistPlayer>();
            // Examine chaque lecteur trouvé.
            foreach (MusicPlaylistPlayer p in players)
                // Vérifie que le lecteur, sa playlist et sa scène correspondent.
                if (p != null && p.playlist != null && p.gameObject.scene.name == sceneName)
                    // Réinjecte immédiatement la playlist de ce lecteur.
                    Inject(p, sceneName);
        }
    }
}