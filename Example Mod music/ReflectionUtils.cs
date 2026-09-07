// Fournit FieldInfo et BindingFlags pour accéder aux champs non publics.
using System.Reflection;
// Fournit le type MusicPlaylistPlayer utilisé pour les champs privés du lecteur.
using SFS.Audio;
// Fournit Debug pour écrire les avertissements dans la console Unity.
using UnityEngine;

namespace CustomMusic
{
    // Regroupe les méthodes qui accèdent aux champs privés de SFS.
    // La réflexion permet d'inspecter un objet pendant l'exécution, même si son champ est privé.
    public static class ReflectionUtils
    {
        // Modifie un champ privé d'un objet lorsqu'il existe.
        public static void SetPrivateField<T>(object obj, string fieldName, T value)
        {
            // Cherche un champ d'instance non public portant ce nom.
            FieldInfo field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            // Si le champ existe, écrit la nouvelle valeur dans l'objet.
            if (field != null)
                field.SetValue(obj, value);
            else
                // Sinon, signale le problème sans interrompre le jeu.
                Debug.LogWarning($"[CustomMusicMod] Failed to set private field '{fieldName}' on {obj}");
        }

        // Lit un champ privé et convertit sa valeur vers le type demandé T.
        public static T GetPrivateField<T>(object obj, string fieldName)
        {
            // Récupère le champ, lit sa valeur, puis la convertit en T.
            return (T)typeof(MusicPlaylistPlayer)
                .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(obj);
        }
    }
}