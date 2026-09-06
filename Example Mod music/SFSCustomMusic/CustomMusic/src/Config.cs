using System;
using SFS.IO;
using SFS.UI.ModGUI;
using SFS.Variables;
using UITools;
using UnityEngine;
using static SFS.UI.ModGUI.Builder;
using Type = SFS.UI.ModGUI.Type;

namespace CustomMusic
{
    // Gestionnaire de configuration du mod.
    //
    // ModSettings fournit à SFS le mécanisme de sérialisation des réglages.
    // Les trois valeurs de SettingsData déterminent si les musiques natives
    // doivent être conservées dans chaque scène. Les fichiers personnalisés
    // restent gérés par MusicInjector.
    public class Config : ModSettings<Config.SettingsData>
    {
        // Instance globale utilisée par MusicInjector et par le menu.
        private static Config main;

        // Fichier local dans lequel ModSettings enregistre les réglages.
        protected override FilePath SettingsFile => Main.modFolder.ExtendToFile("Config.txt");

        // Initialise la configuration puis ajoute une page au menu de
        // configuration de SFS.
        public static void Load()
        {
            main = new Config();
            main.Initialize();
            ConfigurationMenu.Add("Custom Music", new (string, Func<Transform, GameObject>)[]
            {
                ("Config", transform1 => MenuItems(transform1, ConfigurationMenu.ContentSize))
            });
        }

        // Construit visuellement la page de configuration.
        //
        // Chaque bouton inverse une valeur Bool_Local puis demande à
        // MusicInjector de reconstruire immédiatement la playlist de la scène
        // concernée. L'utilisateur n'a donc pas besoin de redémarrer la scène
        // pour voir le changement.
        private static GameObject MenuItems(Transform parent, Vector2Int size)
        {
            Box box = CreateBox(parent, size.x, size.y);
            box.CreateLayoutGroup(Type.Vertical, TextAnchor.UpperCenter, 35, new RectOffset(15, 15, 15, 15));
            var width = size.x - 60;
            CreateLabel(box, size.x, 50, text: "Custom Music");

            Container scale = CreateContainer(box);
            scale.CreateLayoutGroup(Type.Horizontal, spacing: 0);
            CreateLabel(box, width, 32, text: "Allow vanilla music in:");
            CreateToggleWithLabel(box, width, 32, () => settings.homeMenu, () =>
                {
                    settings.homeMenu.Value = !settings.homeMenu.Value;
                    MusicInjector.OnSceneToggleChanged("Home_PC");
                },
                labelText: "Home Menu");
            CreateToggleWithLabel(box, width, 32, () => settings.buildScene, () =>
                {
                    settings.buildScene.Value = !settings.buildScene.Value;
                    MusicInjector.OnSceneToggleChanged("Build_PC");
                },
                labelText: "Build Scene");
            CreateToggleWithLabel(box, width, 32, () => settings.worldScene, () =>
                {
                    settings.worldScene.Value = !settings.worldScene.Value;
                    MusicInjector.OnSceneToggleChanged("World_PC");
                },
                labelText: "World Scene");
            return box.gameObject;
        }

        // Demande la sauvegarde lorsque l'application est en train de quitter.
        // onChange est fourni par ModSettings et déclenche sa sérialisation.
        protected override void RegisterOnVariableChange(Action onChange)
        {
            Application.quitting += onChange;
        }

        // Données réellement persistées par ModSettings.
        // Les valeurs par défaut conservent les musiques natives dans les
        // trois scènes, ce qui évite de rendre le jeu silencieux avant toute
        // personnalisation de l'utilisateur.
        public class SettingsData
        {
            public Bool_Local buildScene = new() { Value = true };
            public Bool_Local homeMenu = new() { Value = true };
            public Bool_Local worldScene = new() { Value = true };
        }
    }
}