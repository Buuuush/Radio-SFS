// Fournit Exception, utilisée pour décrire les erreurs rencontrées.
using System;
// Fournit Dictionary, utilisé par la liste des fichiers téléchargeables.
using System.Collections.Generic;
// Fournit les méthodes de manipulation des dossiers et chemins Windows.
using System.IO;
// Fournit Harmony, qui permet de remplacer ou compléter du code du jeu.
using HarmonyLib;
// Fournit l'attribut UsedImplicitly, qui indique qu'un élément est utilisé par un autre système.
using JetBrains.Annotations;
// Fournit la classe de base Mod et l'interface IUpdatable.
using ModLoader;
// Fournit SceneHelper et les autres outils du chargeur de mods.
using ModLoader.Helpers;
// Fournit FolderPath, le type de chemin utilisé par SFS.
using SFS.IO;
// Fournit CoroutineRunner et les outils d'interface nécessaires au mod.
using UITools;
// Fournit les objets Unity comme Coroutine, GameObject et Debug.
using UnityEngine;

namespace CustomMusic
{
    // Classe principale créée et appelée par le chargeur de mods.
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
        // Référence vers l'objet qui installe les patches Harmony.
        private static Harmony patcher;

        // Chemin du dossier du mod, partagé avec les autres classes.
        public static FolderPath modFolder;

        // Noms des sous-dossiers dans lesquels l'utilisateur place ses musiques.
        private static readonly string[] SceneFolders =
        {
            // Musiques du menu d'accueil.
            "Home_PC",
            // Musiques de la scène de construction.
            "Build_PC",
            // Musiques de la scène du monde.
            "World_PC"
        };

        // Coroutine actuellement en attente de la création du lecteur audio.
        private Coroutine activeInjectionCoroutine;

        // Identifiant interne utilisé par ModLoader.
        public override string ModNameID => "CustomMusic";
        // Nom affiché aux joueurs.
        public override string DisplayName => "Custom Music";
        // Nom de l'auteur affiché par le chargeur de mods.
        public override string Author => "NeptuneSky";
        // Version minimale du jeu compatible avec ce mod.
        public override string MinimumGameVersionNecessary => "1.5.10.2";
        // Version actuelle du mod.
        public override string ModVersion => "v2.0.2";
        // Description affichée dans la liste des mods.
        public override string Description => "Simple mod that lets you import custom music.";

        // Associe l'URL de téléchargement à l'endroit où le fichier doit être installé.
        public Dictionary<string, FilePath> UpdatableFiles => new()
        {
            {
                // Adresse du fichier publié sur GitHub.
                "https://github.com/Neptune-Sky/SFSCustomMusic/releases/latest/download/CustomMusic.dll",
                // Destination locale du fichier téléchargé.
                new FolderPath(ModFolder).ExtendToFile("CustomMusic.dll")
            }
        };

        // Crée les dossiers de musique nécessaires s'ils n'existent pas encore.
        private void EnsureDirectoriesExist()
        {
            // Combine le dossier du mod avec le nom du dossier Music.
            var musicDir = Path.Combine(ModFolder, "Music");
            try
            {
                // Crée le dossier principal uniquement s'il est absent.
                if (!Directory.Exists(musicDir)) Directory.CreateDirectory(musicDir);

                // Parcourt chacun des noms de scène définis plus haut.
                foreach (var scene in SceneFolders)
                {
                    // Construit le chemin du sous-dossier de cette scène.
                    var subfolder = Path.Combine(musicDir, scene);
                    // Crée le sous-dossier s'il n'existe pas.
                    if (!Directory.Exists(subfolder)) Directory.CreateDirectory(subfolder);
                }
            }
            catch (Exception ex)
            {
                // Écrit l'erreur dans le journal sans faire tomber le jeu.
                Debug.LogError($"[CustomMusicMod] Failed to create music directories: {ex}");
            }
        }

        // Méthode appelée tôt par ModLoader, avant le chargement normal du jeu.
        public override void Early_Load()
        {
            // Transforme le chemin du mod en FolderPath et le rend accessible partout.
            modFolder = new FolderPath(ModFolder);
            // S'assure que les dossiers de musique sont disponibles.
            EnsureDirectoriesExist();

            // Charge les réglages et crée la page de configuration.
            Config.Load();

            // Crée un identifiant Harmony propre à ce mod.
            patcher = new Harmony("mods.NeptuneSky.CustomMusic");
            // Recherche et installe les méthodes marquées avec HarmonyPatch.
            patcher.PatchAll();

            // Crée l'objet Unity qui pourra exécuter des coroutines.
            CoroutineRunner.Create();

            // Abonne une fonction à l'événement déclenché après chaque chargement de scène.
            SceneHelper.OnSceneLoaded += scene =>
            {
                // Arrête l'attente précédente si une nouvelle scène arrive avant sa fin.
                if (activeInjectionCoroutine != null)
                    CoroutineRunner.Instance.StopCoroutine(activeInjectionCoroutine);
                // Lance une nouvelle attente, avec le nom de la scène comme argument.
                activeInjectionCoroutine =
                    CoroutineRunner.Instance.StartCoroutine(MusicInjector.InjectAfterSceneLoad(scene.name));
            };
        }

        // ModLoader appelle aussi cette méthode pendant le chargement normal.
        // Elle reste vide car le travail est effectué dans Early_Load.
        public override void Load()
        {
        }
    }
}