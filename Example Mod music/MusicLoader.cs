using System.Collections.Generic;
using System.IO;
using System.Linq;
using SFS.Audio;
using UnityEngine;

namespace CustomMusic
{
    // Recherche les fichiers audio personnalisés d'une scène et les convertit
    // en MusicTrack. Cette classe ne démarre pas la lecture.
    public static class MusicLoader
    {
        // Racine contenant les sous-dossiers des différentes scènes.
        private static readonly string MusicBasePath = Path.Combine(Main.modFolder, "Music");

        // Extensions reconnues par le lecteur personnalisé.
        private static readonly string[] SupportedExt = { ".mp3", ".wav", ".ogg", ".aiff" };

        // Charge les fichiers audio compatibles du dossier de la scène.
        // Le chemin complet est conservé dans clipName : TrackPlayer l'utilise
        // comme marqueur pour déclencher le chargement local.
        public static List<MusicTrack> LoadForScene(string sceneName)
        {
            var path = Path.Combine(MusicBasePath, sceneName);
            if (!Directory.Exists(path)) return new List<MusicTrack>();

            return Directory.GetFiles(path)
                .Where(f => SupportedExt.Contains(Path.GetExtension(f).ToLowerInvariant()))
                .Select(f => new MusicTrack
                {
                    clipName = f, // full path = trigger custom load
                    volume = 1f,
                    pitch = 1f,
                    onTrackEnd = MusicTrack.OnTrackEnd.PlayNext
                })
                .ToList();
        }
    }

    // MonoBehaviour persistant utilisé uniquement pour héberger les coroutines.
    public class CoroutineRunner : MonoBehaviour
    {
        // Instance Unity unique du gestionnaire.
        private static CoroutineRunner instance;

        // Retourne l'instance existante ou la crée si nécessaire.
        public static CoroutineRunner Instance
        {
            get
            {
                if (!instance)
                    Create();
                return instance;
            }
        }

        // Crée le GameObject persistant une seule fois.
        public static void Create()
        {
            if (instance) return;

            var go = new GameObject("CustomMusicMod_CoroutineRunner");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<CoroutineRunner>();
        }
    }
}