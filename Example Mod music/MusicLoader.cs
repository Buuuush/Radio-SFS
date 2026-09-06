using System.Collections.Generic;
using System.IO;
using System.Linq;
using SFS.Audio;
using UnityEngine;

namespace CustomMusic
{
    // Recherche les fichiers audio personnalisés dans le dossier de la scène.
    // Cette classe ne démarre pas la lecture : elle transforme simplement les
    // fichiers trouvés en objets MusicTrack compris par SFS.Audio.
    public static class MusicLoader
    {
        // Dossier racine contenant les sous-dossiers Home_PC, Build_PC et
        // World_PC. Main.modFolder est initialisé avant le premier appel dans
        // le cycle normal de chargement du mod.
        private static readonly string MusicBasePath = Path.Combine(Main.modFolder, "Music");

        // Formats que UnityWebRequestMultimedia et GetAudioType savent traiter
        // dans TrackPlayer.
        private static readonly string[] SupportedExt = { ".mp3", ".wav", ".ogg", ".aiff" };

        // Charge toutes les pistes compatibles d'un dossier de scène.
        //
        // Le chemin complet est placé dans clipName volontairement. Pour une
        // piste native, clipName est un identifiant de ressource SFS ; pour
        // une piste personnalisée, un chemin de fichier permet à TrackPlayer
        // de reconnaître le fichier et de déclencher son chargement local.
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

    // MonoBehaviour minimal utilisé uniquement pour héberger des coroutines.
    // L'objet est conservé entre les scènes afin que les opérations réseau ou
    // de chargement ne soient pas interrompues par un changement de scène.
    public class CoroutineRunner : MonoBehaviour
    {
        // Instance Unity unique du runner.
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