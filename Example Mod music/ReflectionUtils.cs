using System.Reflection;
using SFS.Audio;
using UnityEngine;

namespace CustomMusic
{
    // Helpers centralisant l'accès aux champs privés de SFS.
    // La réflexion est nécessaire car MusicPlaylistPlayer ne rend pas tous
    // ses états internes accessibles publiquement.
    public static class ReflectionUtils
    {
        // Définit un champ privé d'instance lorsqu'il existe. En cas d'échec,
        // écrit un avertissement dans le log plutôt que de faire planter le jeu.
        public static void SetPrivateField<T>(object obj, string fieldName, T value)
        {
            FieldInfo field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
                field.SetValue(obj, value);
            else
                Debug.LogWarning($"[CustomMusicMod] Failed to set private field '{fieldName}' on {obj}");
        }

        // Lit un champ privé de MusicPlaylistPlayer et le convertit vers le
        // type demandé par l'appelant.
        public static T GetPrivateField<T>(object obj, string fieldName)
        {
            return (T)typeof(MusicPlaylistPlayer)
                .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(obj);
        }
    }
}