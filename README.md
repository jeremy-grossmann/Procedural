

**GROSSMANN Jérémy**
> Gaming Campus <br>
> GTech3 2025
> Groupe GameBoy <br>
> Semaine `JEU PROCEDURAL UNITY` <br>


### Sommaires
<br>

- [Initialisation](#Initialisation)
- [SimpleRoomPlacement](#SimpleRoomPlacement)
- [BSP](#BSP)
- [CellularAutomata](#CellularAutomata)
- [Noise](#Noise)


- - - - -
### Initialisation
<br>

 Installer `UniTask`: <br>
Guide d'installation d'UniTask ([**Lien UniTask OpenUPM**](https://openupm.com/packages/com.cysharp.unitask/#modal-manualinstallation)) <br>
<br>
Apres avoir installé UniTask, récup le package donné en cours : [**LienDriveCampus**](https://drive.google.com/drive/folders/1QxmWzBSGsTq-miRODwUX_zA8UEcFaUDW) <br>
Nom du package: `ArchitectureProceduralGeneration.unitypackage` <br>

- - - - -
## SimpleRoomPlacement
<br>

Sur Unity, utilise la scène `GridGenerator`. Vérifie que GenerationMethod utilise le scriptableObject `SimpleRoomPlacement` sur le GameObject `ProceduralGridGenerator`

Explication du script `SimpleRoomPlacement.cs` <br>

Le script `SimplRoomPlacement.cs` place aléatoirement des rooms en fonction des paramètres dans le scriptableObject. <br>
Essaie de placer le nombre max de salles selon la valeur de `maxRooms` (changeable dans l'inspector) <br>
Chaque room a une taille random comprise entre le `minSize` et le `maxSize` que tu choisis (changeable dans l'inspector) <br>
Le `minSize` définit la longueur et la hauteur minimum, le `maxSize` définit la longueur et la hauteur maximum. <br>
Les rooms sont donc des rectangles aléatoires. <br>
Le paramètre `spacingBetweenRoom` définit l'espacement maximum entre les rooms. <br>
Si la room ne peux pas etre placée car elle est dans une autre room ou que l'espacement n'est pas respecté, l'algo ressaye avec une autre room. <br>
Ensuite, les rooms sont reliées entre elles par un corridor. L'algo prend la pièce la plus proche de l'origine (0,0) pour la première room, puis check la room la plus proche et trace le corridor en L par rapport à leur centre. <br>
Par la suite, l'algo garde en mémoire la dernière room qui a été relié et continue de la relier à sa plus proche.<br>
- - - - -
## BSP
<br>

Sur Unity, utilise la scène `GridGenerator`. Vérifie que GenerationMethod utilise le scriptableObject `BSP` sur le GameObject `ProceduralGridGenerator`

Explication du script `BSP.cs` <br>

Le `BSP`(BinarySpacePartitioning) effectue des divisions recursives pour diviser une zone en deux plus petites zones et ainsi de suite. <br>
Le BSP est donc un arbre binaire qui contient des noeuds possédant deux noeuds enfant. <br>
Chaque noeud possède une zone dans la grid où les rooms pourront être créées et possède aussi des méthodes récursives. <br>
Il y a une méthode pour se split en deux noeuds sur l'axe horizontal ou vertical selon le paramètre `Horizontal Split Chance` qui influe sur la chance que le split se fasse horizontalement ou verticalement. <br>
Il y a une méthode pour créer aléatoirement les rooms dans les zones de chaque noeud, une autre pour connecter les noeuds soeurs (les noeuds qui sont enfants du même noeud) avec un corridor et une dernière pour dessiner les rooms. <br>
L'algo fait dans l'ordre : Split, Création, Connection et Draw. <br>
Le paramètre `Max Steps` permet d'indiquer le nombre d'étapes dans l'arbre, c'est à dire le nombre de fois que les noeuds vont se split donc le nombre de rooms. <br>
Les paramètres `Min Size` et `Max Size` définissent la taille minimale et maximale des rooms. <br>

- - - - -
## CellularAutomata
<br>

Sur Unity, utilise la scène `GridGenerator`. Vérifie que GenerationMethod utilise le scriptableObject `CellularAutomata` sur le GameObject `ProceduralGridGenerator`

Le `CellularAutomata` est une simulation de vie organiuqe , comme le "jeu de la vie" (cf video ego) <br>
La première étape de cet algo est de créer des tuiles de manière random selon le paramètre `noise density`. Ce paramètre indique en pourcentage la chance de générer une tuile d'eau ou de terre. <br>
Ensuite, il donne à chaque tuile des voisins au nombre de 8 maximum selon si elles sont sur les bords ou le coté de la grid. <br>
Une fois cela fait, l'algo va faire une itération de plusieurs actions pour pouvoir générer quelque chose de concret. <br>
La première action est un scan des voisins de chaque tuile. Pour chaque tuile de la grid, l'algo va enregistrer un booléen qui correspond au prochain état de la tuile selon ces voisins. <br>
C'est à dire que si la tuile actuelle est de terre et qu'au moins 4 de ses voisins sont de terre alors elle reste inchangée sinon elle devient de l'eau. <br>
Mais si la tuille actuelle est de l'eau, alors elle devient de la terre uniquement si au moins 5 de ses voisins sont de terre. <br>
Une fois le scan effectué, l'algo remplace les tuiles qu'il faut changer selon l'enregistrement du scan fait juste avant, c'est à dire si le booléen correspondant à chaque tuile est True ou False. <br>
Il y a donc deux paramètres important, le `Noise Density`pour gérer le nombre de base des tuiles de terre et le paramètre `Iteration Count` pour le nomre d'itération de l'algo. <br>
Si le premier paramètre est trop petit alors il n'y aura que de l'eau, s'il est trop grand alors il n'y aura que de la terre. Si le nombre d'itération est trop petit, le résultat sera chaotique.
- - - - -
## Noise
<br>

Sur Unity, utilise la scène `GridGenerator`. Vérifie que GenerationMethod utilise le scriptableObject `Noise` sur le GameObject `ProceduralGridGenerator`
L'algo a pour but de générer des tuiles d'eau, de sable, d'herbe et de roche selon des paramètres modifiables.
Utilisation de bruit (FastNoiseLite) pour gerer les variations d'altitude <br>
1. La frequency : est globalement un "Zoom" sur le bruit, plus la valeur est basse, plus les motifs seront larges.
2. Les octaves : c'est des couches de bruit superposées, plus on met d'octaves (donc de couches) plus c'est detailé, alors qu'un seul octave sera un terrain vague avcec des grandes collines douces<br>
3. Lacunarity/Persistance : la lacunarité c'est un peu la différence de "taille" entre les details, en gros c'est la différence de taille entre les couches (si on prend des pinceaux, par exemple 3 octaves = 3 pinceaux de plus en plus gros. La lacunarité sera la difference de taille entre ces pinceaux) <br>
La persistance est l'impact qu'aura chaque couche (Plus ou moins visible) <br>
