using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using Components.ProceduralGeneration;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using VTools.Grid;

[CreateAssetMenu(menuName = "Procedural Generation Method/Cellular")]
public class Test : ProceduralGenerationMethod
{

    [Header("Test Parameters")]
    [SerializeField] [Range(1, 100)] private int noiseDensity = 50;
    [SerializeField][Range(1, 20)] private int iterationCount = 5;


    protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
    {
        var time = System.DateTime.Now;
        int width = Grid.Width;
        int length = Grid.Lenght;
        CellA[,] gridA = new CellA[width, length];

        // Générer l'eau et la terre avec le bruit
        for (int y = 0; y < Grid.Lenght; y++)
        {
            for (int x = 0; x < Grid.Width; x++)
            {
                int randomValue = RandomService.Range(1, 101); // entre 1 et 100
                bool isGround = randomValue <= noiseDensity;

                Grid.TryGetCellByCoordinates(x, y, out var cell);
                AddTileToCell(cell, isGround ? GRASS_TILE_NAME : WATER_TILE_NAME, false);

                gridA[x, y] = new CellA(x, y, isGround);
            }
        }

        Debug.Log("Initial noise generated in " + (System.DateTime.Now - time).TotalSeconds + " seconds.");

        await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);

        // Initialiser les voisins
        for (int y = 0; y < Grid.Lenght; y++)
        {
            for (int x = 0; x < Grid.Width; x++)
            {
                // On check les 8 voisins
                for (int offsetY = -1; offsetY <= 1; offsetY++)
                {
                    for (int offsetX = -1; offsetX <= 1; offsetX++)
                    {
                        if (offsetX == 0 && offsetY == 0) continue; // Skip self

                        int neighborX = x + offsetX;
                        int neighborY = y + offsetY;

                        // Vérifier les limites de la grille
                        if (neighborX >= 0 && neighborX < Grid.Width && neighborY >= 0 && neighborY < Grid.Lenght)
                        {
                            gridA[x, y].AddNeightboor(gridA[neighborX, neighborY]);
                        }
                    }

                }
            }
        }

        Debug.Log("Neighbors initialized in " + (System.DateTime.Now - time).TotalSeconds + " seconds.");

        await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);

        for (int i = 0; i < _maxSteps; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            bool[,] nextState = new bool[width, length];
            bool anyChange = false;

            // Faire un scan sur la grid
            for (int y = 0; y < Grid.Lenght; y++)
            {
                for (int x = 0; x < Grid.Width; x++)
                {
                    nextState[x, y] = gridA[x, y].Scan();
                }
            }

            // Ajouter les tiles qui ont changé
            for (int y = 0; y < Grid.Lenght; y++)
            {
                for (int x = 0; x < Grid.Width; x++)
                {

                    bool newGround = nextState[x, y];
                    if (gridA[x, y].isGround != newGround)
                    {
                        anyChange = true;
                    }

                    gridA[x, y].isGround = newGround;

                    Grid.TryGetCellByCoordinates(x, y, out var cell);
                    AddTileToCell(cell, newGround ? GRASS_TILE_NAME : WATER_TILE_NAME, true);

                }
            }

            if (!anyChange || i >= iterationCount)
            {
                break;
            }

            await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);

           
        }
    }



    class CellA
    {
        public int x, y;
        public bool isGround;

        List<CellA> neightboors;

        public CellA(int x, int y, bool isGround)
        {
            this.x = x;
            this.y = y;
            this.isGround = isGround;
            neightboors = new List<CellA>(8);
        }

        public bool Scan()
        {

            int countGround = 0;

            for (int k = 0; k < neightboors.Count; k++)
            {
                if(neightboors[k].isGround) countGround++;
                
            }

            if (isGround)
                return countGround >= 4; // reste terre si 4+ voisins terre
            else
                return countGround >= 5; // devient terre si 5+ voisins terre
        }

        public void AddNeightboor(CellA cell)
        {
            neightboors.Add(cell);
        }

    }


    
}
