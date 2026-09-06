using System.IO;
using HarmonyLib;
using JetBrains.Annotations;
using SFS.Audio;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace CustomMusic
{
    // Remplace le démarrage standard d'une playlist SFS.
    // Lorsque TrackPlayer réussit à lancer une piste, le retour false empêche
    // Harmony d'exécuter ensuite la méthode originale.
    [HarmonyPatch(typeof(MusicPlaylistPlayer), "StartPlaying")]
    public static class Patch_StartPlaying
    {
        [UsedImplicitly]
        private static bool Prefix(MusicPlaylistPlayer __instance, float fadeTime)
        {
            // Choisit une piste native ou personnalisée selon la playlist.
            return !TrackPlayer.TryPlayTrack(__instance, null, fadeTime);
        }
    }

    // Surveille le lecteur après chaque mise à jour Unity. Ce patch permet de
    // récupérer les transitions et d'empêcher une piste vanilla lorsque celle-ci
    // est désactivée pour la scène courante.
    [HarmonyPatch(typeof(MusicPlaylistPlayer), "Update")]
    public static class Patch_MusicPlaylistPlayer_Update
    {
        [UsedImplicitly]
        private static void Postfix(MusicPlaylistPlayer __instance)
        {
            MusicPlaylist playlist = __instance.playlist;
            AudioSource source = __instance.source;

            if (!playlist || !source || !source.isPlaying)
                return;

            var currentTrack = ReflectionUtils.GetPrivateField<int>(__instance, "currentTrack");

            // Répare un index invalide en sélectionnant une piste valide.
            if (currentTrack < 0 || currentTrack >= playlist.tracks.Count)
            {
                TrackPlayer.TryPlayTrack(__instance, null, 1f);
                return;
            }

            MusicTrack track = playlist.tracks[currentTrack];
            var isCustom = File.Exists(track.clipName);
            var allowVanilla = MusicInjector.ShouldIncludeVanilla(__instance.gameObject.scene.name);

            // Si la piste actuelle est native mais interdite par la config,
            // passe immédiatement à une piste personnalisée.
            if (!isCustom && !allowVanilla) TrackPlayer.TryPlayTrack(__instance, null, 1f);
        }
    }
}