// Fournit File.Exists pour distinguer les pistes locales.
using System.IO;
// Fournit les attributs HarmonyPatch et le fonctionnement des patches.
using HarmonyLib;
// Fournit UsedImplicitly, car Harmony appelle certaines méthodes indirectement.
using JetBrains.Annotations;
// Fournit MusicPlaylistPlayer et MusicPlaylist.
using SFS.Audio;
// Fournit AudioSource et les méthodes de recherche d'objets Unity.
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace CustomMusic
{
    // Indique à Harmony de modifier la méthode StartPlaying de SFS.
    [HarmonyPatch(typeof(MusicPlaylistPlayer), "StartPlaying")]
    public static class Patch_StartPlaying
    {
        [UsedImplicitly]
        private static bool Prefix(MusicPlaylistPlayer __instance, float fadeTime)
        {
            // Essaie de démarrer une piste personnalisée ou native.
            // Le ! inverse le résultat : false demande à Harmony de ne pas exécuter l'original.
            return !TrackPlayer.TryPlayTrack(__instance, null, fadeTime);
        }
    }

    // Indique à Harmony de modifier la méthode Update de SFS.
    [HarmonyPatch(typeof(MusicPlaylistPlayer), "Update")]
    public static class Patch_MusicPlaylistPlayer_Update
    {
        [UsedImplicitly]
        private static void Postfix(MusicPlaylistPlayer __instance)
        {
            // Récupère la playlist et la source audio du lecteur actuel.
            MusicPlaylist playlist = __instance.playlist;
            AudioSource source = __instance.source;

            // Quitte si l'un des objets nécessaires est absent ou arrêté.
            if (!playlist || !source || !source.isPlaying)
                return;

            // Lit dans le lecteur l'index privé de la piste en cours.
            var currentTrack = ReflectionUtils.GetPrivateField<int>(__instance, "currentTrack");

            // Si l'index est impossible, demande immédiatement une nouvelle piste.
            if (currentTrack < 0 || currentTrack >= playlist.tracks.Count)
            {
                TrackPlayer.TryPlayTrack(__instance, null, 1f);
                return;
            }

            // Récupère la piste correspondant à l'index valide.
            MusicTrack track = playlist.tracks[currentTrack];
            // Un chemin existant sur le disque indique une piste du mod.
            var isCustom = File.Exists(track.clipName);
            // Vérifie si les pistes originales sont autorisées dans cette scène.
            var allowVanilla = MusicInjector.ShouldIncludeVanilla(__instance.gameObject.scene.name);

            // Si une piste native est interdite, la remplace par une piste autorisée.
            if (!isCustom && !allowVanilla) TrackPlayer.TryPlayTrack(__instance, null, 1f);
        }
    }
}