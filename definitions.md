# Sommaire
- [Base du C#](#base-de-c)
- [Types de données](#types-de-données)
- [Conditions](#Conditions)
- [Boucles](#Boucles)
- [Absence de valeurs](#absence-de-valeurs)
- [Collections dynamiques](#collections-dynamiques)
- [Mots-clés](#mots-clés)
- [Classe instanciable](#classe-instanciable)
- [Propriété auto-implémentée](#propriété-auto-implémentée)
- [Gestion des exceptions](#gestion-des-exceptions)
- [retour de méthode](#retour-de-méthode)
- [Délégués et callbacks](#délégués-et-callbacks)

# Base de C#

## La classe

### Définition

Blueprint/plan

### Idée

Plan d'une voiture, permet de créer la voiture mais ca n'est pas une voiture

### Exemple

```csharp
public class Voiture
{
    // Une classe nommée Voiture
}
```

## Le champ

### Définition

Variable liée à une classe qui contient une donnée brute (pas safe)

### Idée

niveau de carburant => innaccessible depuis le volant sans passer par le tableau de bord

### Exemple

```csharp
public class Voiture
{
    private int _niveauCarburant; // Champ privé
}
```

## La propriété

### Définition

Accès contrôlé à un champ. Permet de lire ou ecrire une valeur avec get ou set

### Idée

Aiguille du carburant => permet dce lire le niveau de carburant mais on ne peut pas la changer manuellement

### Exemple

```csharp
public class Voiture
{
    private int _niveauCarburant;
    public int NiveauCarburant // Propriété
    {
        get { return _niveauCarburant; }
        set 
        { 
            if (value >= 0) _niveauCarburant = value; // Validation
        }
    }
}
```

## La méthode

### Définition

bloc d'actions, permet de définir le comportement de la classe et réaliser des taches précises

### Idée

Appuyer sur la pédale pour accélerer

### Exemple

```csharp
public class Voiture
{
    public void Accelerer() // Méthode
    {
        // Code qui fait avancer la voiture
    }
}
```

------------------------

# Types de données


## INT (Entier)

### Définition

Stocke éléments positifs négatifs sans virgule (type **valeur**)

### Taille

32 bits (4 octets) : **-2 147 483 648** à **2 147 483 647**

### But

Compter éléments, PV, etc

### Exemple

```csharp
int score = 1500;
int temperature = -5;
```

## Byte (Octet)

### Définition

Stocke un petit entier **positif** (non signé) (type **valeur**)

### Taille

8 bits (1 octet) : **0** à **255**

### But

Valeur RGB, octet de donnée réseau ou fichier

### Exemple

```csharp
byte niveauLuminosite = 255;
byte composanteRouge = 128;
```

## Float (Nombre à virgule)

### Définition

Stocker un nombre décimal (il faut ajouter f ou F à la fin) (type **valeur**)

### Taille

32 bits (4 octets) => 6 à 9 chiffres significatifs

### But

Coordonnées spatiales, vitesses, pourcentages

### Exemple

```csharp
float vitesse = 12.5f;
float positionX = -3.14159f;
```

## Bool (Booléen)

### Définition

Représente **true** ou **false** (type **valeur**)

### Taille

8 bits (1 octet) : `true` ou `false`

### But

Vérifier conditions, interrupteurs, permissions

### Exemple

```csharp
bool estEnVie = true;
bool aClefSecurit = false;
```

## String (Chaîne de caractère)

### Définition

Stocke une suite de texte (type **référence**)

### Taille

Suite de caractères entourés par `""`

### But

Noms, messages, identifiants

### Exemple

```csharp
string pseudo = "Alex";
string message = "Bonjour le monde !";
```

------------------------

# Conditions

## La condition simple (if)

### Définition

S'exécute si la condition est validée (`true`).

### Exemple

```csharp
int vie = 10;

if (vie <= 0)
{
    Console.WriteLine("Joueur éliminé.");
}
```

## La condition alternative (if/else)

### Définition

Le `else` s'exécute si `if` est `false`.

### Exemple

```csharp
bool aCarteAccès = false;

if (aCarteAccès)
{
    Console.WriteLine("Porte déverrouillée.");
}
else
{
    Console.WriteLine("Accès refusé.");
}
```

## Les conditions multiples (if/else if/else)

### Exemple

```csharp
int score = 85;

if (score >= 90)
{
    Console.WriteLine("Grade : A");
}
else if (score >= 75)
{
    Console.WriteLine("Grade : B");
}
else
{
    Console.WriteLine("Grade : C");
}
```

## Structure switch (classique)

### Définition

Compare la variable `x` et s'exécute sur le `case` correspondant.

### Exemple

```csharp
string etat = "EnAttente";

switch (etat)
{
    case "Connecte":
        Console.WriteLine("L'utilisateur est en ligne.");
        break; // Obligatoire en C# pour terminer le bloc

    case "EnAttente":
        Console.WriteLine("Connexion en cours...");
        break;

    default:
        // S'exécute si aucun 'case' ne correspond (l'équivalent du 'else')
        Console.WriteLine("État inconnu.");
        break;
}
```

## Switch avec des `case` semblables

### Utilisation

Moins de code si plusieurs `case` font la même action.

### Exemple

```csharp
int jour = 6;

switch (jour)
{
    case 1:
    case 2:
    case 3:
    case 4:
    case 5:
        Console.WriteLine("Jour de semaine.");
        break;
    case 6:
    case 7:
        Console.WriteLine("Week-end !");
        break;
}
```

## L'expression switch

### Définition

Version qui renvoie directement une valeur.
Elle remplace les `case` et `break` par des `default` et `_`.

### Exemple

```csharp
string etat = "EnAttente";

// Le résultat du switch est assigné directement à la variable
string message = etat switch
{
    "Connecte"   => "L'utilisateur est en ligne.",
    "EnAttente"  => "Connexion en cours...",
    "Deconnecte" => "L'utilisateur est hors ligne.",
    _            => "État inconnu." // Le '_' remplace 'default'
};

Console.WriteLine(message); // Envoie dans la console la variable 'message'
```

# Boucles

## La boucle `while`

### Définition

Si la condition de départ est fausse, elle ne s'exécute jamais.

### Exemple

```csharp
int compteARebours = 5;

while (compteARebours > 0)
{
    Console.WriteLine($"Décompte : {compteARebours}");
    compteARebours--; // Décrémentation pour éviter une boucle infinie (ou ++)
}

Console.WriteLine("Décollage !");
```

## La boucle `do...while`

### Définition

Comme `while` mais s'exécute au moins une fois car la condition est vérifiée après le premier passage.

### Exemple

```csharp
string saisie;

do
{
    Console.Write("Tapez 'oui' pour continuer : ");
    saisie = Console.ReadLine();
} 
while (saisie != "oui");
```

## La boucle `foreach`

### Définition

Elle permet de parcourir séquentiellement des éléments d'un tableau, liste, etc de facon sécurisée (pas de dépassement).

### Exemple

```csharp
string[] langages = { "C#", "Python", "Java", "C++" };

foreach (string langage in langages)
{
    Console.WriteLine($"Langage disponible : {langage}");
}
```

# Absence de valeur

## `null`

### Définition

Variable de type référence qui contient une **adresse mémoire** qui pointe vers un objet.
Quand elle vaut `null`, elle ne pointe rien.

### Exemple

```csharp
string nom = "Alex"; // Pointe vers un emplacement mémoire contenant "Alex"
string texte = null;  // Ne pointe vers aucun emplacement (adresse vide)
```

## Vérifier `null`

### Méthode 1 - L'opérateur `==`

```csharp
Voiture maVoiture = null;

if (maVoiture == null)
{
    Console.WriteLine("Aucune voiture assignée.");
}
```

### Méthode 2 - Le mot-clé `is null`
Fonctionnera toujours.

```csharp
if (maVoiture is null)
{
    Console.WriteLine("Aucune voiture assignée.");
}
// Pour vérifier l'inverse :
if (maVoiture is not null)
{
    maVoiture.Accelerer();
}
```

## Opérateurs pour `null`

### Opérateur de propagation `.?`
Exécute le code seulement si l'objet n'est pas `null`.
Si l'objet est `null`, elle renvoie `null` sans s'arrêter.

```csharp
string nom = joueur?.Nom; // Si joueur est null, nom vaudra null au lieu de faire planter le code
```

### L'opérateur de coalescing `??`
Permet de donner une valeur par défaut si l'élément de gauche est `null`

```csharp
string pseudo = nomSaisi ?? "JoueurAnonyme"; // Si nomSaisi est null, pseudo prend "JoueurAnonyme"
```

### L'opérateur d'assignation `??=`
Donne une valeur uniquement si la variable est `null`

```csharp
listeJoueurs ??= new List<Joueur>(); // Instancie la liste seulement si elle est null
```

# Collections dynamiques

## Liste dynamique ordonnée `List<T>`

### Principe
Une liste stocke une suite d'éléments ordonnés (comme en Python)

### Paramètre `<T>`
Remplacer T par le type de données de la liste (`string`, `int`, etc)

### Exemple
```csharp
using System.Collections.Generic;

// Déclaration d'une liste de chaînes de caractères
List<string> inventaire = new List<string>();

// Ajouter des éléments
inventaire.Add("Épée");
inventaire.Add("Bouclier");

// Accéder par index (0 = premier élément)
string premierObjet = inventaire[0]; // "Épée"

// Supprimer un élément
inventaire.Remove("Bouclier");

// Connaître le nombre d'éléments
int nombreObjets = inventaire.Count;

// Parcourir la liste
foreach (string objet in inventaire)
{
    Console.WriteLine(objet);
}
```

## `Dictionnary<TKey, TValue>`

### Principe

Un dictionnaire associe une clé unique `TKey` à une valeur `TValue`.
Au lieu de cehrcher avec l'index, on utilise la clé.

### Pourquoi l'utiliser
Recherche ultra rapide avec un identifiant (ID, code, etc).

### Règle
Chaque clé doit être unique. Erreur si 2 fois la même clé.

### Exemple
```csharp
using System.Collections.Generic;
// Clé : int (ID du joueur), Valeur : string (Nom du joueur)
Dictionary<int, string> joueurs = new Dictionary<int, string>();

joueurs.Add(101, "Alex"); // Ajouter des paires Clé / Valeur
joueurs.Add(102, "Sophie");

joueurs[103] = "Marc";// Syntaxe alternative d'ajout / modification

// Accéder à une valeur via sa clé
string nomJoueur = joueurs[101]; // "Alex"
// Vérifier la présence d'une clé avant d'y accéder (Sécurité)
if (joueurs.TryGetValue(102, out string nomTrouve))
{
    Console.WriteLine($"Joueur trouvé : {nomTrouve}");
}

foreach (KeyValuePair<int, string> entree in joueurs) // Parcourir un dictionnaire
{
    Console.WriteLine($"ID: {entree.Key} | Nom: {entree.Value}");
}
```

# Mots-clés

## `public` et `private`

### `private`

Variable uniquement accessible dans la classe où elle est située.
Permet de cacher des données sensibles et détails.

### `public`

La variable est accessible de partout.

### Exemple

```csharp
public class Joueur
{
    private int _sante = 100; // Accessible UNIQUEMENT dans Joueur

    public string Pseudo;    // Accessible depuis n'importe quelle autre classe

    public void SubirDegats(int degats)
    {
        // On a accès à _sante car on est À L'INTÉRIEUR de la classe
        _sante -= degats; 
    }
}
```

## `static`

### Définition
Par défaut, les membres (variable, propriété, méthode) d'une classe appartiennent à une instance spécifique (un objet créé avec `new`).
Le mot-clé `static` permet de créer des membres appartenant à la classe elle-même.

### Sans `static`
Il faut créer la classe (new Joueur()) pour utiliser le membre.

### Avec `static`
Le membre existe en un seul exemplaire partagé pour toute l'application. On y accède directement via le nom de la classe, sans faire de new.

### Exemple

```csharp
public class Compteur
{
    public static int TotalJoueurs = 0; // Une seule variable partagée par tout le jeu

    public static void AfficherMessage() // Méthode utilitaire
    {
        Console.WriteLine("Bienvenue !");
    }
}

// Utilisation sans faire de 'new' :
Compteur.TotalJoueurs++;
Compteur.AfficherMessage();
```

## `readonly`

### Définition
S'applique aux champs. Il garantit que la variable ne pourra plus être modifiée après sa déclaration.

### Exemple
```csharp
public class Configuration
{
    public readonly string ClefAPI; // Ne pourra plus changer après l'initialisation

    public Configuration(string clef)
    {
        ClefAPI = clef; // Autorisé UNIQUEMENT dans le constructeur
    }

    public void ModifierClef()
    {
        // ClefAPI = "123"; // ERREUR DE COMPILATION : impossible de modifier un champ readonly
    }
}
```

# Classe instanciable

## La classe instanciable `new`

### Définition
Le mot-clé `new` permet de créer un objet indépendant (une instance)
qui possède sa propre zone mémoire et ses propres valeurs.

### Usage
Représente des entités qui existent en plusieurs exemplaires ou qui possèdent des états variables (ex: un joueur, une voiture, une commande).

### Exemple

```csharp
public class Joueur
{
    public string Pseudo;
    public int Score;
}

// Utilisation :
Joueur j1 = new Joueur();
j1.Pseudo = "Alex";
j1.Score = 100;

Joueur j2 = new Joueur();
j2.Pseudo = "Sophie";
j2.Score = 250; 
// j1 et j2 sont deux objets totalement distincts en mémoire
```

## La classe `static`

### Définition
Ne peut pas être mis avec `new` et existe une seule fois dans tout le programe.

### Règles
- Tous les membres d'une classe `static` doivent **obliga.toirement** être en `static`.
- Elle ne peut pas avoir de paramètres.

### Usage
Créer des fonctions utilitaires, calculateurs ou un conf globale qui n'a pas besoin de créer d'objet.

### Exemple

```csharp
public static class Calculateur
{
    public static float Pi = 3.14159f;

    public static int Additionner(int a, int b)
    {
        return a + b;
    }
}

// Utilisation (SANS 'new') :
int somme = Calculateur.Additionner(5, 10);
float p = Calculateur.Pi;
```

# Propriété auto-implémentée

## Explication
### Décomposition

```csharp
public string Url { get; set; }
```
- `public`  : Propriété accessible depuis n'importe quelle classe du programme
- `string`  : Type de donnée stockée (ici `string`)
- `Url`     : Le nom de la propriété (commence par une **majuscule**)
- `get;`    : Permet de lire une valeur
- `set;`    : Permet de modifier une valeur

### Exemple

```csharp
public class StationRadio
{
    public string Url { get; set; }
}

// Utilisation :
StationRadio radio = new StationRadio();

radio.Url = "https://flux.example.com/stream.mp3"; // Déclenche le 'set'
string lienActuel = radio.Url;                     // Déclenche le 'get'
```

## Lecture seule (`private set`)

### Définition
Tout le monde peut lire la propriété mais seule la classe peut la modifier.

### Exemple

```csharp
public string Url { get; private set; }
```

## Initialisable seulement à la création (`init`)

### Définition
La valeur peut être définie à la création de l'objet et devient immuable.

### Exemple
```csharp
public string Url { get; init; }
```

## Valeur par défaut à la création

### Définition

On peut attribuer la valeur directement lors de la définition.

### Exemple
```csharp
public string Url { get; set; } = "https://localhost";
```

# Gestion des exceptions

## Rôle de chaque bloc

- `try`     : Contient le code qui peut mener à une erreur (lire fichier, division /0, requête réseau)
- `catch`   : S'exécute seulement s'il y a une erreur dans le bloc `try`. Permet de capturer l'erreur, de la traiter, d'afficher un message d'erreur.
- `finally` : S'exécute obligatoirement avec ou sans erreur. Utilisé pour nettoyer les ressources (fermer fichier, coupure réseau)

## Structure et exemple
``` csharp
try
{
    Console.WriteLine("Tentative d'ouverture du fichier...");
    string contenu = File.ReadAllText("config.txt"); // Risque de planter si le fichier n'existe pas
    Console.WriteLine(contenu);
}
catch (FileNotFoundException ex) // S'exécute uniquement si le fichier est introuvable
{
    Console.WriteLine($"Erreur : Le fichier est manquant ({ex.Message}).");
}
catch (Exception ex) // Capture toutes les autres erreurs non gérées au-dessus
{
    Console.WriteLine($"Une erreur inattendue est survenue : {ex.Message}");
}
finally // S'exécute TOUJOURS à la fin
{
    Console.WriteLine("Opération terminée (nettoyage des ressources).");
}
```

## Cibler des exceptions

### Explication

On peut superposer plusieurs blocs `catch` du plus spécifique au plus général.
Dès qu'une exception correspond à un type, son bloc s'exécute et les `catch` suivants sont ignorés.

### Principales exceptions
- `FileNotFoundException`       : Tentative d'accès à un fichier qui n'existe pas.
- `FormatException`             : Echec de conversion de texte en nombre (ex: `int.Parse("abc")`).
- `NullReferenceException`      : Utilisation d'une variable `null`.
- `IndexOutOfRangeException`    : Accès à un tableau hros limites.
- `Exception`                   : Sert à filtrer toutes les erreurs (dernier recours).

## Bloc `finally` vs Instruction `using`

### Explication
- `finally` sert à libérer de la mémoire ou des connnexions.
Pour tout objet utilisant `IDisposable` (flux de fichiers ou requête HTTP), `using` est mieux.
- `using` gère automatiquement `finally`.

### Exemple
```csharp
// Équivalent propre d'un try/finally pour fermer une ressource :
using (StreamReader reader = new StreamReader("fichier.txt"))
{
    string ligne = reader.ReadLine();
} // Le fichier est automatiquement fermé ici, même en cas d'erreur.

/* ------------- */

StreamReader reader = new StreamReader("fichier.txt");
try
{
    string ligne = reader.ReadLine();
    // Si une erreur plante le programme ici...
}
finally
{
    // ...le code d'ici s'exécute QUAND MÊME pour fermer le fichier.
    reader.Close();
}
```

# Retour de méthode

## `void` (ne renvoie rien)

### Principe
La fonction se lance, fait ses actions et ne renvoie rien.

### Exemple

```csharp
public void AfficherMessage(string message)
{
    Console.WriteLine($"[LOG] : {message}");
    // Pas de mot-clé 'return' obligatoire avec une valeur
}

// Utilisation :
AfficherMessage("Fichier chargé."); // On ne stocke rien dans une variable
```

## `bool` (Vrai/Faux)

### Principe
Effectue une vérification, calcul ou test logique et retourne `true` ou `false`.

### Exemple
```csharp
public bool EstPair(int nombre)
{
    return nombre % 2 == 0;
}

// Utilisation :
bool resultat = EstPair(4); // 'resultat' contient true
if (EstPair(4)) 
{
    // S'exécute car la méthode a renvoyé true
}
```

## Une liste (`List<T>`)

### Principe
Renvoie une collection de données (zéro, un, ou éléments du même type).

### Exemple
```csharp
public List<string> ObtenirFichiersMusiqes()
{
    List<string> morceaux = new List<string> { "track01.mp3", "track02.mp3" };
    return morceaux;
}

// Utilisation :
List<string> liste = ObtenirFichiersMusiqes();
Console.WriteLine($"Nombre de pistes : {liste.Count}");
```

## Objet personnalisé (Joueur, <nom au hasard>)

### Principe

Renvoie un ensemble complexe de données regroupées dans la classe. Si la recherche échoue, elle peut retourner `null`.

### Exemple

```csharp
public class StationRadio
{
    public string Nom { get; set; }
    public string Url { get; set; }
}

public StationRadio ObtenirStationFavorite()
{
    StationRadio station = new StationRadio
    {
        Nom = "Radio Metal",
        Url = "https://stream.example.com/metal"
    };

    return station; // Renvoie l'instance complète
}

// Utilisation :
StationRadio maRadio = ObtenirStationFavorite();
if (maRadio is not null)
{
    Console.WriteLine($"Lecture de : {maRadio.Nom}");
}
```

# Délégués et callbacks

## Callback

### Définition

Transmet une fonction à une méthode asynchrone ou longue, pour qu'elle l'exécute au bon moment.

### Exemple

```csharp
// 1. Une fonction qui fait une opération, et qui prend une fonction de callback en dernier argument
void CalculerEtPrevenir(int a, int b, Action<int> callbackFin)
{
    int resultat = a + b;
    
    // J'appelle le callback en lui passant le résultat
    callbackFin(resultat); 
}

// --- UTILISATION ---

// 2. J'appelle la fonction et je lui dis quoi faire avec le résultat une fois qu'elle a fini
CalculerEtPrevenir(5, 10, (res) => 
{
    Console.WriteLine($"Le calcul est fini, le résultat est : {res}");
});
```

## Délégué

### Définition

Définit la signature d'une d'une méthode.
Fait aussi office de pointeur vers une méthode.

### Exemple

```csharp
// Déclaration du délégué : accepte une méthode qui prend un string et ne renvoie rien (void)
public delegate void MessageCallback(string message);

public class Telechargeur
{
    public void TelechargerFichier(MessageCallback surFin)
    {
        Console.WriteLine("Téléchargement en cours...");
        // Simulation d'un traitement...
        
        // Exécution du callback passé en paramètre
        surFin?.Invoke("Téléchargement terminé avec succès !");
    }
}
```

## Délégué générique moderne `Action` 

### Définition
Représente une action qui ne renvoie rien (`void`)

- `Action`              : Méthode sans paramètre : `void MaMethode()`
- `Action<string>`      : Méthode avec un paramètre string : `void MaMethode(strings)`
- `Action<int, bool>`   : Méthode avec deux paramètres : `void MaMethode(int, bool b)`

Si la méthode doit renvoyer une valeur (un résultat), on utilise `Func<T>` au lieu d'`Action` (ex: `Func<int, string>` prend un `int` et renvoie un `string`).

## Callback avec `Action`

### Exemple

```csharp
public class Telechargeur
{
    // On utilise Action<string> au lieu d'un delegate personnalisé
    public void TelechargerFichier(string url, Action<string> surTermine)
    {
        Console.WriteLine($"Début du téléchargement : {url}");
        
        string contenu = "Données du fichier"; // Simulation du traitement

        surTermine?.Invoke(contenu); // Appel du callback en transmettant le résultat
    }
}

// Utilisation :
class Program
{
    static void Main()
    {
        Telechargeur telechargeur = new Telechargeur();

        // On passe le callback directement sous forme de fonction lambda
        telechargeur.TelechargerFichier("https://example.com/audio.mp3", (resultat) => 
        {
            Console.WriteLine($"Callback exécuté ! Fichier reçu : {resultat}");
        });
    }
}
```