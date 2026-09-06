using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using JetBrains.Annotations;
using ModLoader;
using ModLoader.Helpers;
using SFS.IO;
using UITools;
using UnityEngine;

namespace CustomMusic
{
    // Point d'entrée principal du mod Custom Music.
    //
    // Cette classe ne lit pas directement les fichiers audio. Elle prépare
    // l'environnement du mod :
    //   1. création des dossiers de musique ;
    //   2. chargement de la configuration ;
    //   3. installation des patches Harmony ;
    //   4. lancement d'un gestionnaire de coroutines persistant ;
    //   5. réinjection des playlists après chaque chargement de scène.
    //
    // Le mod implémente IUpdatable parce que ModLoader attend cette capacité
    // pour certains mods, même si aucune logique de mise à jour n'est
    // nécessaire ici.
    [UsedImplicitly]
    public class Main : Mod, IUpdatable
    {
        // Instance Harmony conservée pendant toute la durée du jeu.
        // Elle n'est pas utilisée directement après PatchAll(), mais garder
        // une référence permet de conserver clairement la durée de vie du
        // système de patches.
        private static Harmony patcher;

        // Chemin du dossier du mod, converti en FolderPath pour utiliser les
        // helpers de fichiers fournis par SFS.
        public static FolderPath modFolder;

        // Noms des dossiers correspondant aux scènes dans lesquelles SFS
        // possède ses playlists musicales.
        private static readonly string[] SceneFolders =
        {
            "Home_PC",
            "Build_PC",
            "World_PC"
        };

        // Coroutine d'injection actuellement active. Elle est arrêtée avant
        // d'en démarrer une nouvelle afin d'éviter que deux recherches de
        // MusicPlaylistPlayer se déroulent en parallèle.
        private Coroutine activeInjectionCoroutine;

        // Métadonnées affichées par ModLoader.
        public override string ModNameID => "CustomMusic";
        public override string DisplayName => "Custom Music";
        public override string Author => "NeptuneSky";
        public override string MinimumGameVersionNecessary => "1.5.10.2";
        public override string ModVersion => "v2.0.2";
        public override string Description => "Simple mod that lets you import custom music.";

        // Fichier utilisé par le système de mise à jour automatique du mod.
        // La clé est l'URL distante et la valeur est le chemin local où la
        // nouvelle DLL doit être enregistrée.
        public Dictionary<string, FilePath> UpdatableFiles => new()
        {
            {
                "https://github.com/Neptune-Sky/SFSCustomMusic/releases/latest/download/CustomMusic.dll",
                new FolderPath(ModFolder).ExtendToFile("CustomMusic.dll")
            }
        };

        // Crée l'arborescence attendue par le mod :
        //
        // Music/
        //   Home_PC/
        //   Build_PC/
        //   World_PC/
        //
        // Directory.CreateDirectory() est utilisé seulement si le dossier
        // n'existe pas déjà. L'opération est protégée par try/catch afin qu'un
        // problème de droits ou de disque ne fasse pas quitter le jeu sans
        // laisser de message explicite dans le log Unity.
        private void EnsureDirectoriesExist()
        {
            var musicDir = Path.Combine(ModFolder, "Music");
            try
            {
                if (!Directory.Exists(musicDir)) Directory.CreateDirectory(musicDir);

                foreach (var scene in SceneFolders)
                {
                    var subfolder = Path.Combine(musicDir, scene);
                    if (!Directory.Exists(subfolder)) Directory.CreateDirectory(subfolder);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CustomMusicMod] Failed to create music directories: {ex}");
            }
        }

        // Early_Load est exécutée assez tôt pour installer les patches avant
        // que les playlists musicales du jeu ne commencent à être utilisées.
        public override void Early_Load()
        {
            // ModFolder est fourni par la classe de base Mod. On le convertit
            // en FolderPath une seule fois afin que les autres classes puissent
            // construire leurs chemins relatifs.
            modFolder = new FolderPath(ModFolder);
            EnsureDirectoriesExist();

            // Charge les valeurs persistantes et construit le menu de
            // configuration du mod.
            Config.Load();

            // Installe les méthodes Harmony déclarées dans Patches.cs.
            patcher = new Harmony("mods.NeptuneSky.CustomMusic");
            patcher.PatchAll();

            // Unity ne permet de démarrer une coroutine que depuis un
            // MonoBehaviour. CoroutineRunner fournit donc un objet Unity
            // persistant qui pourra lancer le chargement des pistes audio.
            CoroutineRunner.Create();

            // Une playlist peut être recréée ou réinitialisée lors d'un
            // changement de scène. On attend donc chaque événement, on arrête
            // l'injection précédente, puis on recherche la nouvelle playlist
            // dans une coroutine dédiée.
            SceneHelper.OnSceneLoaded += scene =>
            {
                if (activeInjectionCoroutine != null)
                    CoroutineRunner.Instance.StopCoroutine(activeInjectionCoroutine);
                activeInjectionCoroutine =
                    CoroutineRunner.Instance.StartCoroutine(MusicInjector.InjectAfterSceneLoad(scene.name));
            };
        }

        public override void Load()
        {
        }
    }
}