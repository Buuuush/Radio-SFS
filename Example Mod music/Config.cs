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
    // Gestionnaire des réglages du mod.
    //
    // ModSettings prend en charge la sérialisation dans Config.txt. Les trois
    // valeurs de SettingsData indiquent si les pistes natives de SFS doivent
    // rester disponibles dans chacune des scènes concernées.
    public class Config : ModSettings<Config.SettingsData>
    {
        // Instance globale utilisée par le menu et MusicInjector.
        private static Config main;

        // Emplacement du fichier de configuration persistant.
        protected override FilePath SettingsFile => Main.modFolder.ExtendToFile("Config.txt");

        // Initialise les réglages et ajoute la page Custom Music au menu SFS.
        public static void Load()
        {
            main = new Config();
            main.Initialize();
            ConfigurationMenu.Add("Custom Music", new (string, Func<Transform, GameObject>)[]
            {
                ("Config", transform1 => MenuItems(transform1, ConfigurationMenu.ContentSize))
            });
        }

        // Construit les trois interrupteurs du menu.
        // Chaque changement est appliqué immédiatement à la playlist de la
        // scène correspondante, sans nécessiter de redémarrage.
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

        // Demande à ModSettings de sauvegarder les valeurs lors de la fermeture
        // du jeu.
        protected override void RegisterOnVariableChange(Action onChange)
        {
            Application.quitting += onChange;
        }

        // Valeurs réellement enregistrées par ModSettings.
        // Les valeurs par défaut conservent le comportement musical vanilla.
        public class SettingsData
        {
            public Bool_Local buildScene = new() { Value = true };
            public Bool_Local homeMenu = new() { Value = true };
            public Bool_Local worldScene = new() { Value = true };
        }
    }
}