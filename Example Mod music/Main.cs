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
    // l'environnement du mod : création des dossiers, chargement de la
    // configuration, installation des patches Harmony et réinjection des
    // playlists après chaque chargement de scène.
    //
    // IUpdatable est conservé pour respecter le contrat attendu par
    // ModLoader, même si ce mod n'a pas besoin d'une boucle Update dédiée.
    [UsedImplicitly]
    public class Main : Mod, IUpdatable
    {
        // Référence vers le gestionnaire Harmony utilisé pour installer les
        // patches du mod pendant toute la durée de la partie.
        private static Harmony patcher;

        // Chemin du dossier du mod, converti en FolderPath pour utiliser les
        // helpers de fichiers fournis par SFS.
        public static FolderPath modFolder;

        // Sous-dossiers correspondant aux scènes dont la musique peut être
        // personnalisée.
        private static readonly string[] SceneFolders =
        {
            "Home_PC",
            "Build_PC",
            "World_PC"
        };

        // Coroutine d'injection active. Elle est arrêtée lorsqu'une nouvelle
        // scène est chargée afin d'éviter deux injections concurrentes.
        private Coroutine activeInjectionCoroutine;

        public override string ModNameID => "CustomMusic";
        public override string DisplayName => "Custom Music";
        public override string Author => "NeptuneSky";
        public override string MinimumGameVersionNecessary => "1.5.10.2";
        public override string ModVersion => "v2.0.2";
        public override string Description => "Simple mod that lets you import custom music.";

        // URL et destination utilisées par le système de mise à jour du mod.
        public Dictionary<string, FilePath> UpdatableFiles => new()
        {
            {
                "https://github.com/Neptune-Sky/SFSCustomMusic/releases/latest/download/CustomMusic.dll",
                new FolderPath(ModFolder).ExtendToFile("CustomMusic.dll")
            }
        };

        // Crée l'arborescence Music/Home_PC, Music/Build_PC et
        // Music/World_PC si elle n'existe pas encore.
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

        // Méthode appelée tôt par ModLoader afin que les patches soient
        // installés avant l'utilisation des playlists de SFS.
        public override void Early_Load()
        {
            // Initialise le chemin partagé par les autres classes du mod.
            modFolder = new FolderPath(ModFolder);
            EnsureDirectoriesExist();

            // Charge les réglages et crée la page de configuration de SFS.
            Config.Load();

            // Installe les patches déclarés dans Patches.cs.
            patcher = new Harmony("mods.NeptuneSky.CustomMusic");
            patcher.PatchAll();

            // Crée le MonoBehaviour qui pourra héberger les coroutines Unity.
            CoroutineRunner.Create();

            // Après chaque changement de scène, attend la création du lecteur
            // musical puis reconstruit sa playlist.
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