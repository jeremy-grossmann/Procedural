using System.Threading;
using Components.ProceduralGeneration;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VTools.Grid;
using VTools.RandomService;
using VTools.ScriptableObjectDatabase;

[CreateAssetMenu(menuName = "Procedural Generation Method/BSP")]
public class BSP : ProceduralGenerationMethod
{
    [Header("Room Parameters")]
    [SerializeField] int minSizeRoom = 3;
    [SerializeField] int maxSizeRoom = 6;

    [Header("BSP Parameters")]
    [SerializeField][Range(0f, 1f)] private float horizontalSplitChance = 0.5f;

    protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
    {
        Node tree = new Node(
            RandomService,
            GridGenerator,
            new RectInt(0, 0, Grid.Width, Grid.Lenght),
            new Vector2(minSizeRoom, minSizeRoom),
            new Vector2(maxSizeRoom, maxSizeRoom)
        );

        // 1) SPLIT TREE
        tree.SplitRecursive(_maxSteps, horizontalSplitChance);

        // 2) CREATE ROOMS
        tree.CreateRooms();

        // 3) CONNECT CHILDREN
        tree.ConnectRooms();

        // 4) DRAW ROOMS & CORRIDORS
        tree.Draw();

        BuildGround();

        await UniTask.Delay(1);
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

    // -------------------------------------------------------------------
    // NODE CLASS
    // -------------------------------------------------------------------

    public class Node
    {
        private BaseGridGenerator _grid;
        private readonly RandomService _randomService;

        public Node? firstChild, secondChild;

        private RectInt _room;
        private bool _hasRoom = false;

        private Vector2 _minSize;
        private Vector2 _maxSize;

        public RectInt Bounds { get; private set; }

        public Node(RandomService randomService, BaseGridGenerator grid, RectInt bounds, Vector2 minSize, Vector2 maxSize)
        {
            _randomService = randomService;
            _grid = grid;
            _minSize = minSize;
            _maxSize = maxSize;
            Bounds = bounds;
        }

        // ------------------------------------------------------------
        // 1) SPLIT
        // ------------------------------------------------------------
        public void SplitRecursive(int iterations, float horizontalChance)
        {
            if (iterations <= 0) return;

            if (!Split(horizontalChance)) return;

            firstChild?.SplitRecursive(iterations - 1, horizontalChance);
            secondChild?.SplitRecursive(iterations - 1, horizontalChance);
        }

        public bool Split(float horizontalChance)
        {
            // Orientation
            bool horizontal = _randomService.Chance(horizontalChance);

            bool canH = Bounds.height >= _minSize.y * 2;
            bool canV = Bounds.width >= _minSize.x * 2;

            if (!canH && !canV) return false;

            if (horizontal && canH)
            {
                int splitY = _randomService.Range(Bounds.yMin + (int)_minSize.y, Bounds.yMax - (int)_minSize.y);
                RectInt bottom = new RectInt(Bounds.xMin, Bounds.yMin, Bounds.width, splitY - Bounds.yMin);
                RectInt top = new RectInt(Bounds.xMin, splitY, Bounds.width, Bounds.yMax - splitY);

                firstChild = new Node(_randomService, _grid, bottom, _minSize, _maxSize);
                secondChild = new Node(_randomService, _grid, top, _minSize, _maxSize);
            }
            else
            {
                if (!canV) return false;

                int splitX = _randomService.Range(Bounds.xMin + (int)_minSize.x, Bounds.xMax - (int)_minSize.x);
                RectInt left = new RectInt(Bounds.xMin, Bounds.yMin, splitX - Bounds.xMin, Bounds.height);
                RectInt right = new RectInt(splitX, Bounds.yMin, Bounds.xMax - splitX, Bounds.height);

                firstChild = new Node(_randomService, _grid, left, _minSize, _maxSize);
                secondChild = new Node(_randomService, _grid, right, _minSize, _maxSize);
            }

            return true;
        }

        // ------------------------------------------------------------
        // 2) CREATE ROOMS
        // ------------------------------------------------------------
        public void CreateRooms()
        {
            if (firstChild != null)
            {
                firstChild.CreateRooms();
                secondChild.CreateRooms();
                return;
            }

            // LEAF NODE → create room
            int roomWidth = _randomService.Range((int)_minSize.x, (int)_maxSize.x);
            int roomHeight = _randomService.Range((int)_minSize.y, (int)_maxSize.y);

            int x = _randomService.Range(Bounds.xMin, Bounds.xMax - roomWidth);
            int y = _randomService.Range(Bounds.yMin, Bounds.yMax - roomHeight);

            _room = new RectInt(x, y, roomWidth, roomHeight);
            _hasRoom = true;
        }

        // ------------------------------------------------------------
        // 3) CONNECT ROOMS (corridors)
        // ------------------------------------------------------------
        public RectInt GetRoom()
        {
            if (_hasRoom) return _room;
            if (firstChild != null)
            {
                RectInt left = firstChild.GetRoom();
                if (left.width != 0) return left;
            }
            if (secondChild != null)
            {
                RectInt right = secondChild.GetRoom();
                if (right.width != 0) return right;
            }
            return new RectInt(); // empty
        }

        public void ConnectRooms()
        {
            if (firstChild == null || secondChild == null)
                return;

            firstChild.ConnectRooms();
            secondChild.ConnectRooms();

            RectInt a = firstChild.GetRoom();
            RectInt b = secondChild.GetRoom();

            Connect(a.center, b.center);
        }

        private void Connect(Vector2 a, Vector2 b)
        {
            int x1 = Mathf.RoundToInt(a.x);
            int y1 = Mathf.RoundToInt(a.y);
            int x2 = Mathf.RoundToInt(b.x);
            int y2 = Mathf.RoundToInt(b.y);

            int stepX = x1 < x2 ? 1 : -1;
            int stepY = y1 < y2 ? 1 : -1;

            // Horizontal
            for (int x = x1; x != x2; x += stepX)
                PlaceCorridor(x, y1);

            // Vertical
            for (int y = y1; y != y2; y += stepY)
                PlaceCorridor(x2, y);
        }

        private void PlaceCorridor(int x, int y)
        {
            var corridorTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Corridor");

            if (_grid.Grid.TryGetCellByCoordinates(x, y, out var cell))
            {
                _grid.AddGridObjectToCell(cell, corridorTemplate, true);
            }
        }

        // ------------------------------------------------------------
        // 4) DRAW
        // ------------------------------------------------------------
        public void Draw()
        {
            if (firstChild != null)
            {
                firstChild.Draw();
                secondChild.Draw();
                return;
            }

            if (_hasRoom)
            {
                var roomTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Room");
                for (int x = _room.xMin; x < _room.xMax; x++)
                {
                    for (int y = _room.yMin; y < _room.yMax; y++)
                    {
                        if (_grid.Grid.TryGetCellByCoordinates(x, y, out var cell))
                        {
                            _grid.AddGridObjectToCell(cell, roomTemplate, true);
                        }
                    }
                }
            }
        }

    }
}
