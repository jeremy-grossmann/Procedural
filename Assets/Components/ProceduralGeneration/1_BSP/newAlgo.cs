using System.Threading;
using Components.ProceduralGeneration;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using VTools.Grid;
using VTools.RandomService;
using VTools.ScriptableObjectDatabase;

[CreateAssetMenu(menuName = "Procedural Generation Method/New Algo")]
public class newAlgo : ProceduralGenerationMethod
{

    [Header("Room Parameters")]
    [SerializeField] private int _maxIterations = 10;
    [SerializeField] int minSizeRoom = 3;
    [SerializeField] int maxSizeRoom = 6;


    protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
    {
        Node tree = new Node(
            RandomService,
            GridGenerator, 
            new RectInt(0, 0, Grid.Width, Grid.Lenght),
            new Vector2(minSizeRoom, minSizeRoom),
            new Vector2(maxSizeRoom, maxSizeRoom)
            );

        for (int i = 0; i < _maxSteps; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();




            await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);
        }


    }

    public class Node
    {
        private BaseGridGenerator _grid;
        private readonly RandomService _randomService;
        RectInt _room;
        public Node? firstChild, secondChild;
        Vector2 _minSize;
        Vector2 _maxSize;

        public RectInt Bounds { get; private set; }

        public Node(RandomService randomService, BaseGridGenerator grid, RectInt bounds, Vector2 minSize, Vector2 maxSize)
        {
            _randomService = randomService;
            _grid = grid;
            _minSize = minSize;
            _maxSize = maxSize;
            Bounds = bounds;
            Split();
        }

        public void Split()
        {

            if (!CanSplit())
            {
                return;
            }

            bool horizontal = _randomService.Chance(0.5f);

            if (horizontal && Bounds.width >= _room.width * 2)
            {
                int widthSplit = Bounds.width / 2;

                RectInt splitBoundsLeft = new RectInt(Bounds.xMin, Bounds.yMin, widthSplit, Bounds.height);
                RectInt splitBoundsRight = new RectInt(Bounds.xMin + widthSplit, Bounds.yMin, Bounds.width - widthSplit, Bounds.height);

                firstChild = new Node(_randomService, _grid, splitBoundsLeft, _minSize, _maxSize);
                secondChild = new Node(_randomService, _grid, splitBoundsRight, _minSize, _maxSize);

            }
            else
            {
                if (Bounds.height < _room.height * 2)
                {
                    return;
                }

                int heightSplit = Bounds.height / 2;

                RectInt splitBoundsBottom = new RectInt(Bounds.xMin, Bounds.yMin, Bounds.width, heightSplit);
                RectInt splitBoundsTop = new RectInt(Bounds.xMin, Bounds.yMin + heightSplit, Bounds.width, Bounds.height - heightSplit);

                firstChild = new Node(_randomService, _grid, splitBoundsBottom, _minSize, _maxSize);
                secondChild = new Node(_randomService, _grid, splitBoundsTop, _minSize, _maxSize);
            }
        }

        public bool CanSplit()
        {
            bool canSplitHorizontal = Bounds.width >= _room.width * 2;
            bool canSplitVertical = Bounds.height >= _room.height * 2;
            return canSplitHorizontal || canSplitVertical;
        }

        public void CreateRoom()
        {
            int roomWidth = _randomService.Range((int)_minSize.x, (int)_maxSize.x);
            int roomHeight = _randomService.Range((int)_minSize.y, (int)_maxSize.y);
            int roomX = _randomService.Range(Bounds.xMin, Bounds.xMax - roomWidth);
            int roomY = _randomService.Range(Bounds.yMin, Bounds.yMax - roomHeight);
            _room = new RectInt(roomX, roomY, roomWidth, roomHeight);

           
        }

    }
}

//public class BSPNode
//{
//    private readonly Grid _grid;
//    private readonly RandomService _randomService;
//    private readonly Vector2Int _minSize;

//    private BSPNode _parent;
//    private BSPNode _firstCild;
//    private BSPNode _secondChild;
//    private RectInt? _room;

//    public RectInt Bounds { get; private set; }

//    public BSPNode(RectInt bounds, Grid grid, RandomService randomService, Vector2Int minSize)
//    {
//        Bounds = bounds;
//        _grid = grid;
//        _randomService = randomService;
//        _minSize = minSize;

//    }

//    public bool CanSplit()
//    {
//        bool canSplitHorizontal = Bounds.width >= _minSize.x * 2;
//        bool canSplitVertical = Bounds.height >= _minSize.y * 2;
//    }
//}