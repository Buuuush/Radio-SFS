using System.Collections.Generic;
using System.IO;
using System.Linq;
using SFS.Audio;

namespace CustomMusic
{
    // Conserve une copie des playlists natives avant toute modification.
    // Sans ce cache, une réinjection après changement de réglage ne pourrait
    // pas restaurer proprement les pistes vanilla supprimées auparavant.
    public static class VanillaPlaylistCache
    {
        // Une playlist Unity sert de clé : chaque lecteur possède ainsi son
        // propre ensemble de pistes natives originales.
        private static readonly Dictionary<MusicPlaylist, List<MusicTrack>> Cache = new();

        // Enregistre la playlist une seule fois et ignore déjà les pistes
        // personnalisées éventuelles.
        public static void CacheIfNeeded(MusicPlaylist playlist)
        {
            if (Cache.ContainsKey(playlist)) return;
            // Seules les pistes qui ne correspondent pas à un fichier local
            // sont considérées comme vanilla.
            var vanillaTracks = playlist.tracks
                .Where(t => !File.Exists(t.clipName))
                .Select(CloneTrack)
                .ToList();

            Cache[playlist] = vanillaTracks;
        }

        // Retourne le cache associé à la playlist ou une liste vide si aucun
        // cache n'a encore été créé.
        public static List<MusicTrack> GetCachedVanilla(MusicPlaylist playlist)
        {
            return Cache.TryGetValue(playlist, out var tracks) ? tracks : new List<MusicTrack>();
        }

        // Crée une nouvelle instance pour éviter de partager une référence
        // modifiable avec la playlist originale.
        private static MusicTrack CloneTrack(MusicTrack track)
        {
            return new MusicTrack
            {
                clipName = track.clipName,
                pitch = track.pitch,
                volume = track.volume,
                onTrackEnd = track.onTrackEnd
            };
        }
    }
}