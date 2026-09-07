# Ce qui est utile dans `Example Mod music` pour Radio-SFS

Ce document explique les parties du dossier `Example Mod music` qui peuvent t'aider pour ton mod de radio. Il est ecrit pour quelqu'un qui commence tout juste en C#.

## L'idee generale

Le mod d'exemple fait ceci :

1. Il est charge par SFS.
2. Il attend que la scene et le lecteur audio du jeu existent.
3. Il cherche des fichiers audio dans des dossiers.
4. Il construit une playlist.
5. Il remplace ou complete le fonctionnement audio de SFS avec Harmony.
6. Il charge les fichiers audio avec une coroutine Unity.

Ton mod a une difference importante :

- l'exemple lit des fichiers locaux (`.mp3`, `.wav`, `.ogg`, `.aiff`) ;
- Radio-SFS doit lire un flux venant d'une URL, le garder en memoire tampon, le decoder et le jouer progressivement.

Tu peux donc reutiliser les idees d'organisation et de cycle de vie, mais pas copier tout le lecteur de l'exemple tel quel.

## Priorites pour toi

### A apprendre en premier

1. **Les classes et les methodes** : une classe regroupe des donnees et des fonctions.
2. **Les types** : `string` pour du texte, `int` pour un entier, `float` pour un nombre decimal, `bool` pour vrai/faux.
3. **Les listes** : `List<MusicTrack>` est une liste dont chaque element est un `MusicTrack`.
4. **Les conditions** : `if`, `else`, `switch`.
5. **Les boucles** : `foreach` permet de traiter chaque element d'une liste.
6. **Les exceptions** : `try`, `catch` permettent de gerer une erreur sans faire planter le jeu.
7. **Les evenements et callbacks** : une methode peut etre appelee automatiquement quand une scene est chargee ou quand le jeu se ferme.
8. **Les coroutines** : une methode peut attendre une operation Unity sans bloquer le jeu.
9. **L'asynchronisme** : utile pour le reseau, mais different d'une coroutine Unity.

### A etudier ensuite

- Harmony et les patches `Prefix` / `Postfix`.
- La reflexion C# pour acceder a du code prive du jeu.
- `UnityWebRequest` pour charger un fichier audio local ou une ressource distante.
- La synchronisation entre le thread reseau et le thread audio.

## Fichier par fichier

## `Main.cs` : le cycle de vie du mod

### Ce qui est interessant

La classe `Main` herite de `Mod`. Elle fournit les informations du mod et contient `Early_Load()`, appelee au debut du chargement.

Les idees importantes sont :

- preparer les dossiers avec `Directory.Exists` et `Directory.CreateDirectory` ;
- charger la configuration une seule fois ;
- creer Harmony avec un identifiant propre au mod ;
- creer un objet Unity persistant avec `DontDestroyOnLoad` ;
- ecouter un evenement de chargement de scene ;
- arreter une coroutine precedente avant d'en lancer une nouvelle.

### Ce que tu peux reprendre dans Radio-SFS

Ton `src/Main.cs` est deja le bon endroit pour :

- charger `RadioSettings` ;
- creer `RadioPlayer` ;
- initialiser le client reseau ;
- fermer proprement la connexion quand le jeu quitte.

Le principe `Early_Load()` puis `Load()` est a comprendre, mais ne mets pas tout ton code dans `Main`. Garde les responsabilites dans des classes separees.

## `Config.cs` : configuration persistante et interface

### Ce qui est interessant

`Config` herite de `ModSettings<Config.SettingsData>`. Cela permet de sauvegarder les valeurs dans un fichier et de les afficher dans le menu des mods.

A retenir :

- `SettingsData` contient les valeurs sauvegardees ;
- `Bool_Local` represente un booleen gere par le systeme de configuration de SFS ;
- `SettingsFile` indique ou sauvegarder les reglages ;
- `ConfigurationMenu.Add` ajoute une page au menu ;
- un callback est appele quand l'utilisateur change un reglage.

### Ce que tu peux reprendre dans Radio-SFS

Pour Radio-SFS, le meme principe peut servir a sauvegarder :

- l'URL de la radio ;
- le volume ;
- l'activation automatique de la radio ;
- un mode de reconnexion ;
- le comportement lors du chargement d'une scene.

Ne melange pas la construction du menu, le stockage des reglages et la lecture audio. Le menu doit demander au lecteur de changer un reglage, pas manipuler directement le flux reseau.

## `MusicLoader.cs` : rechercher et transformer des fichiers

### Ce qui est interessant

`MusicLoader.LoadForScene` :

- construit un chemin avec `Path.Combine` ;
- verifie que le dossier existe ;
- recupere les fichiers ;
- filtre les extensions ;
- transforme chaque chemin en `MusicTrack` avec LINQ.

Quelques constructions C# a reconnaitre :

```csharp
.Where(f => condition)
.Select(f => transformation)
.ToList()
```

Cela signifie : filtrer, transformer, puis obtenir une liste.

### Ce qui est utile pour Radio-SFS

Le meme raisonnement pourra servir a :

- valider une URL ;
- filtrer des stations ;
- transformer une reponse en objets `RadioStation` ;
- choisir les formats acceptes.

En revanche, `Directory.GetFiles` ne convient pas a une radio distante : une URL n'est pas un fichier a enumerer.

## `MusicInjector.cs` : preparer la playlist

### Ce qui est interessant

Cette classe montre une bonne separation des responsabilites : elle prepare la playlist, mais ne lit pas directement l'audio.

A retenir :

- attendre que `MusicPlaylistPlayer` soit disponible ;
- verifier les references avant de les utiliser ;
- conserver la playlist vanilla avant de la modifier ;
- eviter d'injecter deux fois les memes pistes ;
- utiliser `Concat` pour combiner deux listes ;
- reagir a un changement de configuration.

La coroutine `InjectAfterSceneLoad` montre aussi le principe de `yield return new WaitUntil(...)` : Unity verifie une condition sans bloquer le jeu.

### Ce que tu peux reprendre dans Radio-SFS

Tu peux appliquer cette separation :

- `RadioStreamClient` s'occupe du reseau ;
- `Mp3Decoder` s'occupe du decodage ;
- `AudioBuffer` stocke temporairement les donnees ;
- `RadioPlayer` coordonne la lecture ;
- `RadioUI` affiche les controles.

C'est probablement le fichier le plus utile pour comprendre comment organiser le moment ou ton lecteur devient disponible.

## `TrackPlayer.cs` : choisir et demarrer une piste

### Ce qui est interessant

Cette classe montre plusieurs notions essentielles :

- empecher deux changements de piste simultanes avec un verrou booleen ;
- verifier qu'une liste n'est pas vide ;
- choisir un element aleatoire ;
- distinguer une piste locale d'une ressource du jeu ;
- arreter l'ancienne lecture avant de demarrer la nouvelle ;
- traiter une erreur et passer a la piste suivante ;
- utiliser une coroutine pour le chargement audio.

La methode `TryPlayTrack` retourne un `bool`. C'est une convention utile : `true` signifie que le demarrage a ete accepte, `false` qu'il n'a pas pu avoir lieu.

### Ce qui est directement utile pour Radio-SFS

Ton `RadioPlayer` aura besoin d'etats similaires :

- arrete ;
- connexion en cours ;
- mise en tampon ;
- lecture ;
- reconnexion ;
- erreur ;
- fermeture.

Une radio n'a pas forcement une "prochaine piste" connue. L'equivalent de `SkipToNextTrack` sera plutot :

1. arreter le flux defectueux ;
2. vider ou invalider le buffer ;
3. fermer la connexion ;
4. attendre avant de reconnecter ;
5. reprendre la lecture si assez de donnees sont disponibles.

## `Patches.cs` : modifier le comportement du jeu avec Harmony

### Ce qui est interessant

L'exemple utilise :

```csharp
[HarmonyPatch(typeof(MusicPlaylistPlayer), "StartPlaying")]
```

Puis un `Prefix` ou un `Postfix` pour executer du code autour d'une methode du jeu.

- Un `Prefix` s'execute avant la methode originale.
- Un `Postfix` s'execute apres la methode originale.
- Un `Prefix` qui retourne `false` empeche la methode originale de s'executer.

### Attention pour Radio-SFS

Ne patche pas une methode audio au hasard. Avant de le faire, determine :

- quelle methode du jeu demarre effectivement l'audio ;
- si ton lecteur doit remplacer le lecteur vanilla ou jouer en parallele ;
- qui possede le `AudioSource` ;
- comment eviter que le patch soit execute a chaque frame inutilement.

Un patch `Update` est appele tres souvent. Il doit donc rester tres court et ne jamais lancer une connexion reseau ou une operation couteuse a chaque frame.

## `ReflectionUtils.cs` : acceder a un champ prive

### Ce qui est interessant

La reflexion permet a l'exemple de lire ou modifier `currentTrack`, qui est prive dans le jeu.

Cette ligne est le principe :

```csharp
type.GetField("nom", BindingFlags.NonPublic | BindingFlags.Instance)
```

### Pourquoi rester prudent

La reflexion :

- contourne l'encapsulation normale ;
- depend du nom interne exact du champ ;
- peut casser apres une mise a jour du jeu ;
- produit facilement une valeur `null` si le champ n'est pas trouve.

Pour ton mod, utilise-la seulement si une API publique ne suffit pas. Encapsule chaque acces reflechi dans une petite methode et journalise l'echec.

## `VanillaPlaylistCache.cs` : sauvegarder l'etat original

### Ce qui est interessant

La classe utilise un `Dictionary<MusicPlaylist, List<MusicTrack>>` pour associer chaque playlist a une copie de ses pistes originales.

A retenir :

- `Dictionary<TKey, TValue>` associe une cle a une valeur ;
- `ContainsKey` verifie si une cle existe ;
- copier les objets evite de modifier accidentellement les originaux ;
- mettre en cache permet de reconstruire un etat propre.

### Utilite pour Radio-SFS

Le meme principe peut servir a memoriser :

- le `AudioSource` original ;
- les reglages audio avant activation de la radio ;
- l'etat necessaire pour restaurer le son vanilla quand la radio s'arrete.

## Les notions Unity a retenir

### `GameObject` et `MonoBehaviour`

Un `GameObject` est un objet dans la scene Unity. Un `MonoBehaviour` est un composant que Unity peut gerer. `CoroutineRunner` cree un objet persistant uniquement pour executer des coroutines.

### `AudioSource`

`AudioSource` est le composant qui joue un `AudioClip`. Il possede notamment :

- `clip` : le son a jouer ;
- `Play()` : demarrer ;
- `Stop()` : arreter ;
- `volume` : volume ;
- `pitch` : vitesse et hauteur.

Pour un flux radio, le point difficile est que les donnees arrivent progressivement alors qu'un `AudioSource` classique attend generalement un `AudioClip` exploitable.

### Coroutine et `async/await`

Ce sont deux outils differents :

- une coroutine Unity utilise `yield return` et est pilotee par Unity ;
- `async/await` attend une operation C# ou reseau sans bloquer le thread appelant.

Dans Radio-SFS, le reseau peut utiliser `async/await`, tandis que la communication avec les objets Unity et la lecture audio doivent respecter le thread principal Unity. Il faut donc eviter de modifier un `AudioSource` depuis un thread reseau.

## Correspondance avec tes fichiers actuels

| Exemple | Dans Radio-SFS | A quoi cela correspond |
|---|---|---|
| `Main.cs` | `src/Main.cs` | Initialisation et fermeture du mod |
| `Config.cs` | `src/RadioSettings.cs` | Reglages persistants |
| `MusicLoader.cs` | `src/RadioStreamClient.cs` | Preparation de la source audio, mais a distance |
| `TrackPlayer.cs` | `src/RadioPlayer.cs` | Etat et orchestration de la lecture |
| chargement Unity | `src/Mp3Decoder.cs` | Conversion des octets en audio jouable |
| liste de pistes | `src/AudioBuffer.cs` | Donnees audio temporaires |
| menu de configuration | `src/RadioUI.cs` | Controles utilisateur |
| `Patches.cs` | `src/Main.cs` ou futur fichier de patch | Integration avec SFS |

Les fichiers `AudioBuffer.cs`, `Mp3Decoder.cs`, `RadioPlayer.cs` et `RadioStreamClient.cs` sont actuellement des squelettes. Le dossier d'exemple t'aide surtout pour l'architecture, mais il ne fournit pas la solution complete au streaming MP3.

## Ce que tu devrais coder en premier

1. Faire compiler un mod minimal qui se charge et ecrit un message dans le journal.
2. Charger et afficher l'URL depuis `RadioSettings`.
3. Creer un `RadioPlayer` avec un etat simple : arrete ou lecture.
4. Tester une requete reseau sans encore jouer le son.
5. Ajouter l'annulation avec `CancellationToken` quand le jeu se ferme.
6. Ajouter le buffer et mesurer s'il se remplit correctement.
7. Decoder un petit morceau audio.
8. Connecter enfin le resultat a un `AudioSource`.
9. Ajouter la reconnexion et les erreurs.
10. Ajouter les controles dans `RadioUI`.

A chaque etape, teste une seule responsabilite. Cela rend les erreurs beaucoup plus faciles a comprendre.

## Mots C# a retenir

- `public` : accessible depuis d'autres classes.
- `private` : accessible seulement dans la classe actuelle.
- `static` : appartient a la classe, pas a une instance precise.
- `readonly` : reference qui ne doit plus etre remplacee apres son initialisation.
- `override` : remplace une methode ou propriete heritee.
- `new` : cree un nouvel objet.
- `null` : aucune reference d'objet.
- `?.` : execute seulement si l'objet n'est pas `null`.
- `??` : utilise une valeur de remplacement si la precedente vaut `null`.
- `=>` : forme courte d'une expression ou d'une methode.
- `yield return` : rend temporairement la main a Unity dans une coroutine.
- `using` : importe un espace de noms, ou garantit la liberation d'une ressource selon le contexte.

## Resume

Les fichiers les plus importants a etudier dans cet ordre sont :

1. `Main.cs` pour le cycle de vie du mod et les coroutines.
2. `MusicInjector.cs` pour attendre Unity et separer la preparation de la lecture.
3. `TrackPlayer.cs` pour les etats, les validations et les erreurs.
4. `Patches.cs` pour l'integration Harmony.
5. `Config.cs` pour les reglages et l'interface.
6. `ReflectionUtils.cs` seulement si tu dois vraiment acceder a du code prive.
7. `VanillaPlaylistCache.cs` pour le cache et la restauration d'etat.
8. `MusicLoader.cs` pour apprendre LINQ et la manipulation de fichiers.

La regle principale est la suivante : reutilise l'architecture de l'exemple, mais traite le flux radio comme une source reseau continue, pas comme une simple liste de fichiers a charger un par un.

## TODO : feuille de route complete pour Radio-SFS

Lis cette liste dans l'ordre. Ne passe a l'etape suivante que lorsque le test indique est reussi. Les cases `- [ ]` peuvent etre cochees en `- [x]` au fur et a mesure.

### Phase 0 - Comprendre le projet

- [x] Ouvrir `README.md` et noter le but exact du mod.
- [x] Ouvrir `RadioSFS.csproj` et reperer le framework cible `net48`.
- [x] Comprendre que `src/Main.cs` est le point d'entree du mod.
- [x] Comprendre le role de chacun des fichiers dans `src`.
- [x] Comprendre la difference entre une classe, une methode, un champ et une propriete.
- [x] Apprendre a lire une erreur de compilation sans essayer de tout corriger en meme temps.
- [x] Compiler le projet avant toute modification pour connaitre son etat de depart.

**Termine quand :** tu peux expliquer en une phrase le role de `Main`, `RadioPlayer`, `RadioStreamClient`, `AudioBuffer`, `Mp3Decoder`, `RadioSettings` et `RadioUI`.

### Phase 1 - Bases C# indispensables

- [x] Revoir les types `string`, `int`, `float`, `bool` et `byte`.
- [x] Revoir les conditions `if` et `else`.
- [x] Revoir `switch` pour traiter plusieurs etats.
- [x] Revoir les boucles `foreach` et `while`.
- [ ] Comprendre `null` et les verifications `objet == null`.
- [ ] Comprendre `?.` et `??`.
- [ ] Comprendre `List<T>` et `Dictionary<TKey, TValue>`.
- [ ] Comprendre `public`, `private`, `static` et `readonly`.
- [ ] Comprendre la difference entre une classe `static` et une classe instanciee avec `new`.
- [ ] Comprendre une propriete comme `public string Url { get; set; }`.
- [ ] Comprendre `try`, `catch` et `finally`.
- [ ] Comprendre ce que retourne une methode : `void`, `bool`, une liste ou un objet.
- [ ] Comprendre les delegues, `Action` et les callbacks.

**Termine quand :** tu peux lire une methode simple du dossier d'exemple et expliquer chaque ligne avec tes propres mots.

### Phase 2 - Faire fonctionner le mod minimal

- [ ] Verifier que `src/Main.cs` herite bien de `Mod`.
- [ ] Verifier que `ModNameID`, `DisplayName`, `Author`, `ModVersion` et `Description` sont corrects.
- [ ] Ajouter un message de journal dans `Early_Load()`.
- [ ] Compiler le projet.
- [ ] Installer la DLL dans le dossier des mods de SFS.
- [ ] Lancer le jeu et verifier que le mod est charge.
- [ ] Retrouver le message de journal du mod.
- [ ] Ajouter une gestion d'erreur autour de l'initialisation si necessaire.

**Termine quand :** le jeu demarre avec la DLL et le journal confirme que Radio-SFS est initialise.

### Phase 3 - Comprendre et finir les reglages

- [ ] Ouvrir `src/RadioSettings.cs`.
- [ ] Identifier l'endroit ou l'URL de la radio est sauvegardee.
- [ ] Identifier la valeur utilisee si aucune URL n'est configuree.
- [ ] Ajouter ou verifier un reglage de volume.
- [ ] Ajouter ou verifier un reglage d'activation automatique.
- [ ] Ajouter un reglage de reconnexion si le projet en a besoin.
- [ ] Valider qu'une URL vide est refusee proprement.
- [ ] Valider qu'une URL mal formee est refusee proprement.
- [ ] Sauvegarder les reglages dans le fichier prevu par le mod.
- [ ] Recharger le jeu et verifier que les reglages persistent.

**Termine quand :** tu peux modifier l'URL dans le menu, redemarrer le jeu et retrouver la meme URL.

### Phase 4 - Apprendre le fonctionnement Unity de l'exemple

- [ ] Lire `Example Mod music/Main.cs`.
- [ ] Comprendre pourquoi `Early_Load()` est utilise.
- [ ] Comprendre pourquoi `CoroutineRunner` herite de `MonoBehaviour`.
- [ ] Comprendre le role de `GameObject`.
- [ ] Comprendre `DontDestroyOnLoad`.
- [ ] Comprendre `StartCoroutine`.
- [ ] Comprendre `yield return`.
- [ ] Comprendre `WaitUntil`.
- [ ] Comprendre pourquoi il faut verifier qu'un objet Unity existe avant de l'utiliser.
- [ ] Lire `Example Mod music/MusicInjector.cs`.
- [ ] Noter la difference entre preparer une lecture et lire effectivement un son.

**Termine quand :** tu sais expliquer pourquoi une coroutine est necessaire pour attendre un objet Unity sans bloquer le jeu.

### Phase 5 - Definir les etats du lecteur

- [ ] Ouvrir `src/RadioPlayer.cs`.
- [ ] Creer ou definir un etat `Stopped`.
- [ ] Creer ou definir un etat `Connecting`.
- [ ] Creer ou definir un etat `Buffering`.
- [ ] Creer ou definir un etat `Playing`.
- [ ] Creer ou definir un etat `Reconnecting`.
- [ ] Creer ou definir un etat `Error`.
- [ ] Creer ou definir un etat `Closing` si necessaire.
- [ ] Definir les transitions autorisees entre ces etats.
- [ ] Ajouter un message de journal a chaque changement d'etat.
- [ ] Empecher deux operations `Play` ou `Stop` de s'executer en meme temps.
- [ ] Ajouter une methode publique claire pour demarrer la radio.
- [ ] Ajouter une methode publique claire pour arreter la radio.

**Termine quand :** le lecteur peut passer de `Stopped` a `Connecting`, puis revenir a `Stopped` en cas d'erreur sans laisser une operation active.

### Phase 6 - Tester le reseau sans audio

- [ ] Ouvrir `src/RadioStreamClient.cs`.
- [ ] Choisir l'API reseau utilisee par le projet, probablement `HttpClient` ou `Stream`.
- [ ] Lire l'URL depuis `RadioSettings`.
- [ ] Verifier que l'URL utilise un protocole autorise, par exemple `http` ou `https`.
- [ ] Creer une requete vers une station de test.
- [ ] Verifier le code de reponse HTTP.
- [ ] Verifier que la reponse contient bien des donnees audio.
- [ ] Lire les donnees par petits blocs au lieu de tout charger en memoire.
- [ ] Ajouter un delai d'expiration de connexion.
- [ ] Ajouter un delai d'expiration de lecture.
- [ ] Gerer une URL invalide.
- [ ] Gerer un serveur inaccessible.
- [ ] Gerer une connexion interrompue.
- [ ] Ecrire dans le journal la taille de chaque bloc uniquement pour les tests.
- [ ] Ne jamais ecrire les donnees audio binaires completes dans le journal.

**Termine quand :** le mod se connecte a une station de test et confirme qu'il recoit des octets, sans essayer de jouer le son.

### Phase 7 - Arreter proprement le reseau

- [ ] Ajouter un `CancellationTokenSource` pour la connexion actuelle.
- [ ] Annuler la connexion avant d'en demarrer une nouvelle.
- [ ] Annuler la connexion quand l'utilisateur appuie sur Stop.
- [ ] Annuler la connexion quand le jeu se ferme.
- [ ] Liberer la reponse HTTP et le flux reseau avec `using` ou `Dispose`.
- [ ] Ne pas afficher une erreur quand l'annulation est volontaire.
- [ ] Verifier qu'une reconnexion ne demarre pas apres la fermeture du jeu.

**Termine quand :** fermer la radio ou le jeu ne laisse pas de tache reseau active et ne produit pas d'erreur inutile.

### Phase 8 - Construire `AudioBuffer`

- [ ] Ouvrir `src/AudioBuffer.cs`.
- [ ] Choisir la structure de stockage des octets audio.
- [ ] Definir une taille maximale du buffer.
- [ ] Definir une taille minimale avant de commencer la lecture.
- [ ] Ajouter une methode pour ecrire des donnees dans le buffer.
- [ ] Ajouter une methode pour lire des donnees depuis le buffer.
- [ ] Ajouter une methode pour connaitre le nombre d'octets disponibles.
- [ ] Gerer le cas ou le buffer est vide.
- [ ] Gerer le cas ou le buffer est plein.
- [ ] Eviter de perdre des donnees quand le reseau ecrit pendant que Unity lit.
- [ ] Utiliser `lock` ou une collection adaptee si plusieurs threads accedent au buffer.
- [ ] Ne jamais bloquer indefiniment le thread principal Unity.
- [ ] Ajouter des logs temporaires indiquant le niveau du buffer.

**Termine quand :** un test peut ecrire et lire des blocs dans le buffer sans corruption et sans exception.

### Phase 9 - Comprendre le decodage audio

- [ ] Ouvrir `src/Mp3Decoder.cs`.
- [ ] Identifier le format exact fourni par les stations de test.
- [ ] Distinguer MP3, OGG, WAV et AAC.
- [ ] Verifier si le flux est un MP3 continu ou des morceaux separes.
- [ ] Choisir une bibliotheque de decodage compatible avec `net48` et Unity/SFS.
- [ ] Verifier que la licence de la bibliotheque est compatible avec le mod.
- [ ] Decoder un bloc audio hors du jeu si possible.
- [ ] Convertir les donnees decodees en echantillons audio utilisables.
- [ ] Connaitre le nombre de canaux audio.
- [ ] Connaitre la frequence d'echantillonnage.
- [ ] Gerer les frames incompletes entre deux blocs reseau.
- [ ] Gerer les erreurs de decodage.

**Termine quand :** un fichier ou un flux de test est decode en echantillons audio valides.

### Phase 10 - Relier le decodeur a Unity

- [ ] Identifier comment creer ou recuperer un `AudioSource`.
- [ ] Creer un objet Unity persistant si le lecteur doit survivre aux changements de scene.
- [ ] Ne manipuler les objets Unity que depuis le thread principal.
- [ ] Creer la strategie de lecture : `AudioClip` dynamique, buffers successifs ou autre solution compatible.
- [ ] Alimenter Unity avec des echantillons deja decodes.
- [ ] Definir le nombre de secondes d'avance necessaires avant `Play()`.
- [ ] Demarrer la lecture uniquement quand le buffer est assez rempli.
- [ ] Eviter les coupures quand le buffer descend temporairement.
- [ ] Arreter l'`AudioSource` quand la radio est stoppee.
- [ ] Appliquer le volume configure par l'utilisateur.
- [ ] Tester que le son vanilla ne double pas le son de la radio.

**Termine quand :** une station de test est audible dans le jeu pendant au moins quelques minutes.

### Phase 11 - Gerer les coupures et la reconnexion

- [ ] Detecter la fin du flux.
- [ ] Detecter une erreur HTTP.
- [ ] Detecter une erreur de lecture reseau.
- [ ] Detecter un buffer vide pendant la lecture.
- [ ] Arreter ou mettre en pause proprement l'audio pendant une coupure.
- [ ] Afficher un etat `Reconnecting`.
- [ ] Definir un delai avant la premiere reconnexion.
- [ ] Ajouter un delai croissant entre plusieurs echecs.
- [ ] Limiter le nombre de tentatives si necessaire.
- [ ] Reinitialiser le compteur apres une reconnexion reussie.
- [ ] Ne pas lancer plusieurs reconnexions simultanees.
- [ ] Reprendre la lecture uniquement apres avoir rempli assez le buffer.
- [ ] Afficher une erreur facile a comprendre apres plusieurs echecs.

**Termine quand :** couper puis retablir la connexion ne force pas le redemarrage du jeu et reprend la radio proprement.

### Phase 12 - Integrer avec le son de SFS

- [ ] Lire `Example Mod music/Patches.cs`.
- [ ] Identifier la methode SFS qui demarre la musique vanilla.
- [ ] Determiner si un patch Harmony est vraiment necessaire.
- [ ] Si necessaire, creer un fichier de patch separe.
- [ ] Installer les patches une seule fois dans `Early_Load()`.
- [ ] Eviter les traitements lourds dans `Update`.
- [ ] Sauvegarder l'etat de l'audio vanilla avant de le modifier.
- [ ] Restaurer l'audio vanilla quand la radio est arretee.
- [ ] Tester le changement de scene avec la radio arretee.
- [ ] Tester le changement de scene avec la radio active.
- [ ] Verifier qu'un patch ne s'applique pas deux fois.
- [ ] N'utiliser la reflexion que si aucune API publique ne convient.

**Termine quand :** la radio et la musique de SFS ne se superposent pas de facon inattendue, quelle que soit la scene.

### Phase 13 - Creer l'interface utilisateur

- [ ] Ouvrir `src/RadioUI.cs`.
- [ ] Ajouter un champ pour l'URL.
- [ ] Ajouter un bouton `Play`.
- [ ] Ajouter un bouton `Stop`.
- [ ] Ajouter un controle de volume.
- [ ] Afficher l'etat actuel du lecteur.
- [ ] Afficher `Connexion`, `Mise en tampon`, `Lecture`, `Erreur` et `Arrete`.
- [ ] Desactiver `Play` pendant une connexion deja en cours.
- [ ] Desactiver `Stop` quand rien ne joue.
- [ ] Afficher une erreur sans afficher une exception technique complete a l'utilisateur.
- [ ] Ajouter un bouton ou une action pour reessayer.
- [ ] Verifier que les controles ne lancent pas plusieurs operations concurrentes.

**Termine quand :** un utilisateur peut configurer une station, la lancer, l'arreter et comprendre l'etat du lecteur sans regarder le journal.

### Phase 14 - Nettoyage et qualite du code

- [ ] Donner un nom clair a chaque variable et methode.
- [ ] Remplacer les nombres importants par des constantes nommees.
- [ ] Eviter les classes qui font a la fois reseau, audio et interface.
- [ ] Garder une seule classe responsable de l'etat de lecture.
- [ ] Supprimer les logs temporaires trop verbeux.
- [ ] Garder les erreurs importantes dans le journal.
- [ ] Verifier tous les chemins de sortie avec `return`.
- [ ] Verifier tous les objets qui peuvent etre `null`.
- [ ] Fermer les flux et annuler les taches au bon moment.
- [ ] Ajouter des commentaires seulement pour les parties difficiles.
- [ ] Compiler apres chaque petit groupe de changements.
- [ ] Ne pas corriger des erreurs sans rapport avec la radio.

**Termine quand :** chaque classe a une responsabilite claire et le projet compile sans avertissement important lie a ton code.

### Phase 15 - Tests finaux

- [ ] Tester une URL vide.
- [ ] Tester une URL mal formee.
- [ ] Tester une station indisponible.
- [ ] Tester une station qui renvoie une erreur HTTP.
- [ ] Tester une station MP3 valide.
- [ ] Tester une station avec un format non supporte.
- [ ] Tester une connexion lente.
- [ ] Tester une coupure pendant la lecture.
- [ ] Tester plusieurs reconnexions.
- [ ] Tester Play puis Stop rapidement.
- [ ] Tester Stop puis Play rapidement.
- [ ] Tester deux clics rapides sur Play.
- [ ] Tester un changement de scene pendant la connexion.
- [ ] Tester un changement de scene pendant la lecture.
- [ ] Tester la fermeture du jeu pendant la connexion.
- [ ] Tester la fermeture du jeu pendant la lecture.
- [ ] Tester la sauvegarde et le rechargement des reglages.
- [ ] Tester le volume minimum et maximum.
- [ ] Tester l'absence de fuite de taches ou de flux reseau.
- [ ] Tester que la musique vanilla revient correctement.

**Termine quand :** aucun scenario courant ne bloque le jeu, ne laisse une connexion active ou ne produit une lecture impossible a arreter.

### Phase 16 - Version publiable

- [ ] Mettre a jour `README.md` avec l'installation.
- [ ] Ajouter les dependances necessaires.
- [ ] Indiquer les formats et types de stations supportes.
- [ ] Expliquer ou trouver les logs en cas de probleme.
- [ ] Ajouter les limitations connues.
- [ ] Verifier le numero de version dans `Main.cs`.
- [ ] Compiler en configuration finale.
- [ ] Tester la DLL compilee dans une installation propre du jeu.
- [ ] Verifier qu'aucun fichier de debug inutile n'est inclus.
- [ ] Verifier la licence des bibliotheques ajoutees.
- [ ] Creer une archive d'installation claire.
- [ ] Faire une derniere installation complete depuis cette archive.

**Termine quand :** une autre personne peut installer le mod en suivant uniquement le `README.md` et utiliser une station sans intervention de developpeur.

## Regle de travail pendant le developpement

Pour chaque case :

1. Modifier une petite partie.
2. Compiler.
3. Lire l'erreur exacte s'il y en a une.
4. Tester uniquement la responsabilite modifiee.
5. Cocher la case quand le test est reussi.
6. Passer a la case suivante.

Ne commence pas par `Mp3Decoder.cs` si le mod ne se charge pas encore. L'ordre est important : initialisation, reglages, reseau, buffer, decodage, audio Unity, interface, puis reconnexion et finition.
