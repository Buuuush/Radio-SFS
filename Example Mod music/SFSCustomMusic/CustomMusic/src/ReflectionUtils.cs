using System.Reflection;
using SFS.Audio;
using UnityEngine;

namespace CustomMusic
{
    // Petits helpers centralisant l'accès aux champs privés de SFS.
    //
    // La réflexion est nécessaire parce que MusicPlaylistPlayer ne rend pas
    // certains états internes publics. Ces accès dépendent donc des noms et
    // de la structure interne de la version de SFS utilisée par le mod.
    public static class ReflectionUtils
    {
        // Définit un champ privé d'instance si celui-ci existe.
        // Un avertissement est écrit dans le log plutôt que de lancer une
        // exception afin que le jeu puisse continuer avec un comportement
        // audio dégradé mais identifiable.
        public static void SetPrivateField<T>(object obj, string fieldName, T value)
        {
            FieldInfo field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
                field.SetValue(obj, value);
            else
                Debug.LogWarning($"[CustomMusicMod] Failed to set private field '{fieldName}' on {obj}");
        }

        // Lit un champ privé de MusicPlaylistPlayer.
        // L'appelant doit demander le type attendu avec T ; un mauvais type
        // provoquerait une exception de conversion, ce qui rend les appels
        // dépendants de la définition exacte de SFS.Audio.
        public static T GetPrivateField<T>(object obj, string fieldName)
        {
            return (T)typeof(MusicPlaylistPlayer)
                .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(obj);
        }
    }
}