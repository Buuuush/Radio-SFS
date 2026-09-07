// Fournit List<T>, une liste dont le type des éléments est connu.
using System.Collections.Generic;
// Fournit les méthodes pour parcourir les dossiers et reconnaître les fichiers.
using System.IO;
// Fournit les méthodes LINQ utilisées pour filtrer et transformer les fichiers.
using System.Linq;
// Fournit MusicTrack, le type de piste audio de SFS.
using SFS.Audio;
// Fournit MonoBehaviour, GameObject et les méthodes Unity utilisées plus bas.
using UnityEngine;

namespace CustomMusic
{
    // Recherche les fichiers audio personnalisés d'une scène et crée les objets MusicTrack.
    // Cette classe prépare les pistes mais ne les lit pas.
    public static class MusicLoader
    {
        // Chemin vers le dossier qui contient un sous-dossier par scène.
        private static readonly string MusicBasePath = Path.Combine(Main.modFolder, "Music");

        // Extensions de fichiers que Unity sait charger avec ce mod.
        private static readonly string[] SupportedExt = { ".mp3", ".wav", ".ogg", ".aiff" };

        // Retourne les fichiers audio compatibles trouvés dans le dossier d'une scène.
        public static List<MusicTrack> LoadForScene(string sceneName)
        {
            // Construit le chemin du dossier correspondant au nom reçu.
            var path = Path.Combine(MusicBasePath, sceneName);
            // Si le dossier est absent, retourne une liste vide plutôt qu'une erreur.
            if (!Directory.Exists(path)) return new List<MusicTrack>();

            // Récupère les fichiers, garde les extensions connues, puis crée une MusicTrack pour chacun.
            return Directory.GetFiles(path)
                // GetExtension extrait l'extension et ToLowerInvariant ignore les majuscules/minuscules.
                .Where(f => SupportedExt.Contains(Path.GetExtension(f).ToLowerInvariant()))
                // Transforme chaque chemin f en nouvel objet MusicTrack.
                .Select(f => new MusicTrack
                {
                    // Le chemin complet sert à reconnaître une piste personnalisée.
                    clipName = f,
                    // Volume normal : 1 représente 100 % du volume.
                    volume = 1f,
                    // Vitesse normale de lecture.
                    pitch = 1f,
                    // Après cette piste, le lecteur choisira la suivante.
                    onTrackEnd = MusicTrack.OnTrackEnd.PlayNext
                })
                // Convertit le résultat LINQ en List<MusicTrack> concrète.
                .ToList();
        }
    }

    // Objet Unity persistant dont le seul rôle est d'exécuter des coroutines.
    public class CoroutineRunner : MonoBehaviour
    {
        // Référence vers l'unique CoroutineRunner créé par le mod.
        private static CoroutineRunner instance;

        // Retourne l'objet existant ou le crée à la première demande.
        public static CoroutineRunner Instance
        {
            get
            {
                // Un objet Unity nul est traité comme false par Unity.
                if (!instance)
                    Create();
                // Retourne l'objet maintenant disponible.
                return instance;
            }
        }

        // Crée le GameObject qui héberge les coroutines, une seule fois.
        public static void Create()
        {
            // Quitte immédiatement si l'objet existe déjà.
            if (instance) return;

            // Crée un nouvel objet dans la scène Unity actuelle.
            var go = new GameObject("CustomMusicMod_CoroutineRunner");
            // Conserve cet objet lors des changements de scène.
            DontDestroyOnLoad(go);
            // Ajoute ce composant et mémorise l'instance créée.
            instance = go.AddComponent<CoroutineRunner>();
        }
    }
}