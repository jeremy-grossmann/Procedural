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
        [SerializeField] int maxSizeRoom = 6;

        protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
        {
            // Declare variables here
            int roomsCreated = 0;
            int spacingMin = 1;
            int spacingMax = 3;
            RectInt[] listRooms = new RectInt[_maxRooms];
            int currentRoom = 0;
            int currentRoomCount = 0;



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
                    int spacing = RandomService.Range(spacingMin, spacingMin);

                    RectInt room = new RectInt(xRoom, yRoom, widthRoom, lengthRoom);

                    if(CanPlaceRoom(room, spacing))
                    {
                        PlaceRoom(room, ROOM_TILE_NAME);
                        roomsCreated++;
                    }

                }
                else // ajouter une route
                {
                    // On check si toute les rooms sont reliées
                    if (currentRoomCount >= _maxRooms) break;

                    // Chercher la room la plus proche de la current room

                    int distance = 0;

                    //for(int k = 0; k < listRooms.Length; k++)
                    //{
                    //    int tempDistance = Mathf.Sqrt(Mathf.Pow((float)(listRooms[k].x - listRooms[currentRoom].x), 2) + Mathf.Pow(listRooms[k].x - listRooms[currentRoom].x, 2));
                    //    if (distance < Mathf.Abs(listRooms[k].center - listRooms[currentRoom].center))
                    //}

                }





                // Si le nombre de room est atteint on fait juste des couloirs
                // COULOIR
                // 

                // ajouter un couloir



                // Waiting between steps to see the result.
                await UniTask.Delay(GridGenerator.StepDelay, cancellationToken : cancellationToken);
            }
            
            // Final ground building.
            BuildGround();
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