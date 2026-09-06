using System.Collections.Generic;
using System.IO;
using System.Linq;
using SFS.Audio;

namespace CustomMusic
{
    // Conserve une copie des playlists natives avant modification.
    // Ce cache permet de restaurer les pistes vanilla lorsqu'un réglage change.
    public static class VanillaPlaylistCache
    {
        // Chaque playlist possède son propre cache de pistes natives.
        private static readonly Dictionary<MusicPlaylist, List<MusicTrack>> Cache = new();

        // Enregistre une playlist uniquement lors de sa première rencontre.
        public static void CacheIfNeeded(MusicPlaylist playlist)
        {
            if (Cache.ContainsKey(playlist)) return;
            // Les pistes correspondant à un fichier local sont exclues du
            // cache, car elles appartiennent au mod et non au jeu de base.
            var vanillaTracks = playlist.tracks
                .Where(t => !File.Exists(t.clipName))
                .Select(CloneTrack)
                .ToList();

            Cache[playlist] = vanillaTracks;
        }

        // Retourne le cache de la playlist, ou une liste vide si elle n'a pas
        // encore été enregistrée.
        public static List<MusicTrack> GetCachedVanilla(MusicPlaylist playlist)
        {
            return Cache.TryGetValue(playlist, out var tracks) ? tracks : new List<MusicTrack>();
        }

        // Crée une copie indépendante d'une piste pour éviter de modifier
        // l'objet original détenu par SFS.
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