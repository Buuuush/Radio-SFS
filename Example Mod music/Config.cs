// Fournit le type Action utilisé pour enregistrer une méthode à appeler plus tard.
using System;
// Fournit les types de chemins utilisés par le jeu.
using SFS.IO;
// Fournit les éléments de l'interface graphique du menu des mods.
using SFS.UI.ModGUI;
// Fournit Bool_Local, un booléen que ModSettings peut sauvegarder.
using SFS.Variables;
// Fournit les outils pratiques utilisés pour construire le menu.
using UITools;
// Fournit les types Unity comme Transform, GameObject et Vector2Int.
using UnityEngine;
// Autorise l'utilisation directe des méthodes statiques de Builder.
using static SFS.UI.ModGUI.Builder;
// Donne un nom plus court au Type de l'interface SFS.
using Type = SFS.UI.ModGUI.Type;

namespace CustomMusic
{
    // Regroupe les réglages affichés dans la page de configuration du mod.
    //
    // ModSettings prend en charge la sérialisation dans Config.txt. Les trois
    // valeurs de SettingsData indiquent si les pistes natives de SFS doivent
    // rester disponibles dans chacune des scènes concernées.
    public class Config : ModSettings<Config.SettingsData>
    {
        // Référence vers l'unique objet Config créé par Load.
        // static signifie que cette référence appartient à la classe et non à un objet précis.
        private static Config main;

        // Indique à ModSettings où enregistrer les valeurs entre deux lancements.
        // override signifie que cette propriété remplace celle de la classe parente.
        protected override FilePath SettingsFile => Main.modFolder.ExtendToFile("Config.txt");

        // Prépare l'objet de configuration et ajoute sa page au menu des mods.
        public static void Load()
        {
            // Crée l'objet qui contiendra les réglages sauvegardés.
            main = new Config();
            // Demande à ModSettings de lire Config.txt ou de créer les valeurs par défaut.
            main.Initialize();
            // Ajoute une page nommée Config dans la section Custom Music.
            // Le tableau contient ici un seul couple : le nom de la page et une fonction qui la construit.
            ConfigurationMenu.Add("Custom Music", new (string, Func<Transform, GameObject>)[]
            {
                // transform1 est le parent visuel dans lequel la page sera créée.
                ("Config", transform1 => MenuItems(transform1, ConfigurationMenu.ContentSize))
            });
        }

        // Construit les contrôles visuels de la page de configuration.
        private static GameObject MenuItems(Transform parent, Vector2Int size)
        {
            // Crée un cadre qui sera le conteneur principal de la page.
            Box box = CreateBox(parent, size.x, size.y);
            // Organise les contrôles verticalement avec une marge intérieure de 15 pixels.
            box.CreateLayoutGroup(Type.Vertical, TextAnchor.UpperCenter, 35, new RectOffset(15, 15, 15, 15));
            // Calcule une largeur légèrement plus petite pour les contrôles internes.
            var width = size.x - 60;
            // Ajoute le titre affiché en haut de la page.
            CreateLabel(box, size.x, 50, text: "Custom Music");

            // Crée un conteneur horizontal pour le contenu suivant.
            Container scale = CreateContainer(box);
            // Demande à ce conteneur de placer ses éléments côte à côte.
            scale.CreateLayoutGroup(Type.Horizontal, spacing: 0);
            // Ajoute le texte qui décrit les trois options ci-dessous.
            CreateLabel(box, width, 32, text: "Allow vanilla music in:");
            // Ajoute un interrupteur lié au réglage du menu d'accueil.
            CreateToggleWithLabel(box, width, 32, () => settings.homeMenu, () =>
                {
                    // Inverse la valeur actuelle : true devient false et inversement.
                    settings.homeMenu.Value = !settings.homeMenu.Value;
                    // Réapplique immédiatement la playlist de la scène concernée.
                    MusicInjector.OnSceneToggleChanged("Home_PC");
                },
                labelText: "Home Menu");
            // Ajoute un interrupteur lié au réglage de la scène de construction.
            CreateToggleWithLabel(box, width, 32, () => settings.buildScene, () =>
                {
                    // Inverse la valeur enregistrée pour cette scène.
                    settings.buildScene.Value = !settings.buildScene.Value;
                    // Réapplique immédiatement la playlist de la scène de construction.
                    MusicInjector.OnSceneToggleChanged("Build_PC");
                },
                labelText: "Build Scene");
            // Ajoute un interrupteur lié au réglage de la scène de monde.
            CreateToggleWithLabel(box, width, 32, () => settings.worldScene, () =>
                {
                    // Inverse la valeur enregistrée pour cette scène.
                    settings.worldScene.Value = !settings.worldScene.Value;
                    // Réapplique immédiatement la playlist de la scène du monde.
                    MusicInjector.OnSceneToggleChanged("World_PC");
                },
                labelText: "World Scene");
            // Retourne le GameObject afin que le menu puisse l'afficher.
            return box.gameObject;
        }

        // Enregistre la méthode qui sauvegardera les valeurs quand l'application se ferme.
        protected override void RegisterOnVariableChange(Action onChange)
        {
            // += ajoute onChange à la liste des méthodes appelées à la fermeture.
            Application.quitting += onChange;
        }

        // Contient les valeurs réellement sauvegardées par ModSettings.
        public class SettingsData
        {
            // true signifie que la musique originale reste autorisée dans la scène de construction.
            public Bool_Local buildScene = new() { Value = true };
            // true signifie que la musique originale reste autorisée dans le menu d'accueil.
            public Bool_Local homeMenu = new() { Value = true };
            // true signifie que la musique originale reste autorisée dans la scène du monde.
            public Bool_Local worldScene = new() { Value = true };
        }
    }
}