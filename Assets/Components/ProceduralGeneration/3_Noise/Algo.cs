using System.Threading;
using Components.ProceduralGeneration;
using Cysharp.Threading.Tasks;
using UnityEngine;


[CreateAssetMenu(menuName = "Procedural Generation Method/Noise")]
public class Algo : ProceduralGenerationMethod
{
    [Header("Noise Parameters")]
    [SerializeField] private FastNoiseLite.NoiseType noiseType = FastNoiseLite.NoiseType.Perlin;
    [SerializeField][Range(0f, 0.1f)] private float frequency = 0.05f;
    [SerializeField][Range(0.5f, 1.5f)] private float amplitude = 1f;

    [Header("Fractal Parameters")]
    [SerializeField] private FastNoiseLite.FractalType fractalType = FastNoiseLite.FractalType.FBm;
    [SerializeField][Range(1, 10)] private int octaves = 3;
    [SerializeField][Range(1, 10)] private float lacunarity = 2f;
    [SerializeField][Range(0f, 1f)] private float persistence = 0.5f;

    [Header("Heights")]
    [SerializeField][Range(-1, 1)] private float waterHeight = -0.2f;
    [SerializeField][Range(-1, 1)] private float sandHeight = 0f;
    [SerializeField][Range(-1, 1)] private float grassHeight = 0.4f;
    [SerializeField][Range(-1, 1)] private float rockHeight = 0.7f;

    [Header("Debug")]
    [SerializeField] private bool drawDebug = false;

    protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
    {
        // Variables 
        FastNoiseLite noise = new FastNoiseLite(RandomService.Seed);
        noise.SetNoiseType(noiseType);
        noise.SetFrequency(frequency);

        noise.SetFractalType(fractalType);
        noise.SetFractalOctaves(octaves);
        noise.SetFractalLacunarity(lacunarity);
        noise.SetFractalGain(persistence);

        float[,] noiseMap = new float[Grid.Lenght, Grid.Width];


        for (int i = 0; i < _maxSteps; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            for (int y = 0; y < Grid.Lenght; y++)
            {
                for (int x = 0; x < Grid.Width; x++)
                {
                    noiseMap[x, y] = noise.GetNoise(x, y) * amplitude;
                    Grid.TryGetCellByCoordinates(x, y, out var cell);
                    if (drawDebug)
                    {
                        //Color debugColor = Color.Lerp(Color.black, Color.white, (noiseValue + 1) / 2f);
                        //GridGenerator.DrawDebugCell(cell, debugColor);
                    }
                    if (noiseMap[x, y] < waterHeight)
                    {
                        AddTileToCell(cell, WATER_TILE_NAME, false);
                    }
                    else if (noiseMap[x, y] < sandHeight)
                    {
                        AddTileToCell(cell, SAND_TILE_NAME, false);
                    }
                    else if (noiseMap[x, y] < grassHeight)
                    {
                        AddTileToCell(cell, GRASS_TILE_NAME, false);
                    }
                    else if (noiseMap[x, y] < rockHeight)
                    {
                        AddTileToCell(cell, ROCK_TILE_NAME, false);
                    }
                }
            }



            await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);
        }
    }

}
