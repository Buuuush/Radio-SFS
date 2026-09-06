using System.IO;
using HarmonyLib;
using JetBrains.Annotations;
using SFS.Audio;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace CustomMusic
{
    // Remplace le démarrage de lecture de SFS.
    // Le Prefix retourne false lorsque TrackPlayer a pris en charge la lecture,
    // ce qui empêche Harmony d'exécuter ensuite la méthode vanilla.
    [HarmonyPatch(typeof(MusicPlaylistPlayer), "StartPlaying")]
    public static class Patch_StartPlaying
    {
        [UsedImplicitly]
        private static bool Prefix(MusicPlaylistPlayer __instance, float fadeTime)
        {
            // Demande à notre lecteur de choisir une piste. Le résultat false
            // indique que la lecture personnalisée a réussi et que la logique
            // originale doit être ignorée.
            return !TrackPlayer.TryPlayTrack(__instance, null, fadeTime);
        }
    }

    // Surveille chaque mise à jour du lecteur musical après l'exécution de la
    // logique SFS. Ce patch sert surtout à récupérer les transitions de fin de
    // piste et à empêcher une piste native de continuer lorsque l'utilisateur
    // les a désactivées pour la scène actuelle.
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

            // Si l'index interne est invalide, tente de sélectionner une piste
            // valide avant que le lecteur ne reste bloqué sans musique.
            if (currentTrack < 0 || currentTrack >= playlist.tracks.Count)
            {
                TrackPlayer.TryPlayTrack(__instance, null, 1f);
                return;
            }

            MusicTrack track = playlist.tracks[currentTrack];
            var isCustom = File.Exists(track.clipName);
            var allowVanilla = MusicInjector.ShouldIncludeVanilla(__instance.gameObject.scene.name);

            // Si la piste actuelle est native mais que la configuration les
            // interdit, passe immédiatement à une piste personnalisée.
            if (!isCustom && !allowVanilla) TrackPlayer.TryPlayTrack(__instance, null, 1f);
        }
    }
}