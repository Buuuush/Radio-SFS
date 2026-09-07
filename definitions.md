# Sommaire
- [Base du C#](#base-de-c)
- [Types de données](#types-de-données)
- [Conditions](#Conditions)

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

