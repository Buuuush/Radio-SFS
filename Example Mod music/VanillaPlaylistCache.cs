// Fournit Dictionary et List pour stocker les pistes en mémoire.
using System.Collections.Generic;
// Fournit File.Exists pour reconnaître les pistes personnalisées.
using System.IO;
// Fournit Where, Select et ToList pour filtrer et copier les pistes.
using System.Linq;
// Fournit les types MusicPlaylist et MusicTrack de SFS.
using SFS.Audio;

namespace CustomMusic
{
    // Conserve une copie des playlists natives avant que le mod les modifie.
    // Cette copie permet de revenir aux pistes d'origine si un réglage change.
    public static class VanillaPlaylistCache
    {
        // Associe chaque playlist à la copie de ses pistes natives.
        private static readonly Dictionary<MusicPlaylist, List<MusicTrack>> Cache = new();

        // Enregistre la playlist uniquement si elle n'a pas encore été enregistrée.
        public static void CacheIfNeeded(MusicPlaylist playlist)
        {
            // ContainsKey vérifie si le dictionnaire possède déjà cette playlist.
            if (Cache.ContainsKey(playlist)) return;

            // Garde seulement les pistes dont le nom n'est pas un fichier local.
            var vanillaTracks = playlist.tracks
                .Where(t => !File.Exists(t.clipName))
                // Crée une nouvelle MusicTrack pour chaque piste gardée.
                .Select(CloneTrack)
                // Transforme le résultat en liste.
                .ToList();

            // Enregistre la liste sous la clé correspondant à la playlist.
            Cache[playlist] = vanillaTracks;
        }

        // Retourne la copie enregistrée, ou une liste vide si elle n'existe pas.
        public static List<MusicTrack> GetCachedVanilla(MusicPlaylist playlist)
        {
            // TryGetValue cherche la clé et place la valeur trouvée dans tracks.
            return Cache.TryGetValue(playlist, out var tracks) ? tracks : new List<MusicTrack>();
        }

        // Crée une copie indépendante d'une piste.
        private static MusicTrack CloneTrack(MusicTrack track)
        {
            // Initialise une nouvelle piste avec les mêmes valeurs que l'originale.
            return new MusicTrack
            {
                // Copie le nom de la ressource audio.
                clipName = track.clipName,
                // Copie la vitesse de lecture.
                pitch = track.pitch,
                // Copie le volume.
                volume = track.volume,
                // Copie l'action à effectuer à la fin de la piste.
                onTrackEnd = track.onTrackEnd
            };
        }
    }
}