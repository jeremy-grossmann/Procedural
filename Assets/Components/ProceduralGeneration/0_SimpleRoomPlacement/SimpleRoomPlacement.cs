using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VTools.Grid;
using VTools.ScriptableObjectDatabase;

namespace Components.ProceduralGeneration.SimpleRoomPlacement
{
    [CreateAssetMenu(menuName = "Procedural Generation Method/Simple Room Placement")]
    public class SimpleRoomPlacement : ProceduralGenerationMethod
    {
        [Header("Room Parameters")]
        [SerializeField] private int _maxRooms = 10;
        [SerializeField] int minSizeRoom = 3;
        [SerializeField] int maxSizeRoom = 10;
        [SerializeField] int spacingBetweenRoom = 5;

        protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
        {
            // Declare variables here
            int roomsCreated = 0;
            List<RectInt> listRooms = new List<RectInt>(_maxRooms);
            int currentRoom = 0;
            int connectedRooms = 0;



            for (int i = 0; i < _maxSteps; i++)
            {
                // Check for cancellation
                cancellationToken.ThrowIfCancellationRequested();

                // Your algorithm here

                // ajouter une room (Rectangle) aléatoire si le nombre de room n'est pas atteint
                if (roomsCreated < _maxRooms)
                {
                    int widthRoom = RandomService.Range(minSizeRoom, maxSizeRoom);
                    int lengthRoom = RandomService.Range(minSizeRoom, maxSizeRoom);
                    int xRoom = RandomService.Range(0, Grid.Width);
                    int yRoom = RandomService.Range(0, Grid.Lenght);

                    RectInt room = new RectInt(xRoom, yRoom, widthRoom, lengthRoom);

                    if(CanPlaceRoom(room, spacingBetweenRoom))
                    {
                        PlaceRoom(room, ROOM_TILE_NAME);
                        listRooms.Add(room);
                        roomsCreated++;
                    }

                }
                else // ajouter une route
                {
                    if(listRooms.Count == 1)
                        break; // Toutes les rooms sont reliées

                    // On cherche la premiere room la plus proche de l'origine si c'est la première connexion
                    if (connectedRooms == 0)
                    {
                        Vector2 origin = new Vector2(0, 0);
                        float minDistance = float.MaxValue;

                        for (int r = 0; r < listRooms.Count; r++)
                        {
                            float dist = Vector2.Distance(origin, listRooms[r].center);
                            if (dist < minDistance)
                            {
                                currentRoom = r;
                                minDistance = dist;
                            }
                        }
                    }

                    // On cherche la room la plus proche de la room courante dans la liste
                    RectInt roomA = listRooms[currentRoom];
                    float minDistanceB = float.MaxValue;
                    int closestRoomIndex = -1;
                    for (int r = 0; r < listRooms.Count; r++)
                    {
                        if (r == currentRoom)
                            continue;
                        float dist = Vector2.Distance(roomA.center, listRooms[r].center);
                        if (dist < minDistanceB)
                        {
                            closestRoomIndex = r;
                            minDistanceB = dist;
                        }
                    }

                    RectInt roomB = listRooms[closestRoomIndex];

                    // On crée le couloir entre les deux rooms
                    CreateCorridor(roomA.center, roomB.center);

                    // On supprime la room A de la liste pour ne pas la reconnecter
                    listRooms.RemoveAt(currentRoom);
                    currentRoom = closestRoomIndex > currentRoom ? closestRoomIndex - 1 : closestRoomIndex;
                    connectedRooms++;

                }



                // Waiting between steps to see the result.
                await UniTask.Delay(GridGenerator.StepDelay, cancellationToken : cancellationToken);
            }
            
            // Final ground building.
            BuildGround();
        }

        // ------------------------------------------------------------
        // COULOIR : création d’un chemin entre deux points
        // ------------------------------------------------------------
        private void CreateCorridor(Vector2 a, Vector2 b)
        {
            int x1 = Mathf.RoundToInt(a.x);
            int y1 = Mathf.RoundToInt(a.y);
            int x2 = Mathf.RoundToInt(b.x);
            int y2 = Mathf.RoundToInt(b.y);

            // 1. Mouvement horizontal
            int stepX = x1 < x2 ? 1 : -1;
            for (int x = x1; x != x2; x += stepX)
            {
                TryPlaceTile(x, y1);
            }

            // 2. Mouvement vertical
            int stepY = y1 < y2 ? 1 : -1;
            for (int y = y1; y != y2; y += stepY)
            {
                TryPlaceTile(x2, y);
            }
        }

        private void TryPlaceTile(int x, int y)
        {
            if (Grid.TryGetCellByCoordinates(x, y, out var cell))
            {
                AddTileToCell(cell, CORRIDOR_TILE_NAME, true);
            }
        }

        private void BuildGround()
        {
            var groundTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Grass");
            
            // Instantiate ground blocks
            for (int x = 0; x < Grid.Width; x++)
            {
                for (int z = 0; z < Grid.Lenght; z++)
                {
                    if (!Grid.TryGetCellByCoordinates(x, z, out var chosenCell))
                    {
                        Debug.LogError($"Unable to get cell on coordinates : ({x}, {z})");
                        continue;
                    }
                    
                    //AddTileToCell(chosenCell, GRASS_TILE_NAME, false);
                    GridGenerator.AddGridObjectToCell(chosenCell, groundTemplate, false);
                }
            }
        }

    }
}