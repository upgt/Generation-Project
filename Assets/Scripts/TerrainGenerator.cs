using System;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public int depth = 200;
    public int plainDepth = 20;
    public int height = 256;
    public int width = 256;

    public float scale = 20f;
    public float offsetX = 100f;
    public float offsetY = 100f;

    //public int mountainHeight = 200;
    //public int mountainX = 100;
    //public int mountainY = 100;
    //public double angle = 30f;

    public float flatCoefficient = 0.2f;    //сглаживание шума
    public float[] noiseCoefficients;
    public float exponent = 1f;
    private float[,] heights;

    private void Start()
    {
        offsetX = UnityEngine.Random.Range(0, 1000f);
        offsetY = UnityEngine.Random.Range(0, 1000f);
        Terrain terrain = GetComponent<Terrain>();
        terrain.terrainData = CreateTerrain(terrain.terrainData);

    }
    private void Update()
    {
        //Terrain terrain = GetComponent<Terrain>();
        //terrain.terrainData = CreateTerrain(terrain.terrainData);

        // offsetX += Time.deltaTime * 2f;
    }

    private TerrainData CreateTerrain(TerrainData terrainData)
    {
        terrainData.heightmapResolution = width + 1;
        terrainData.size = new Vector3(width, plainDepth, height);
        terrainData.SetHeights(0, 0, CreateHeights());              //генерация шумом перлина
        //terrainData.SetHeights(0, 0, CreateHeightsRed());         //для экспериментов с шумами
        return terrainData;
    }

    private float[,] CreateHeights()
    {
        heights = new float[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                heights[x, y] = CalculateHeight(x, y);
            }
        }
        return heights;
    }

    //private float[,] CreateHeightsRed()
    //{
    //    var _heights = new float[width+1, height+1];
    //    for (int x = 0; x < width; x++)
    //    {
    //        for (int y = 0; y < height; y++)
    //        {
    //            _heights[x, y] = Noise(x,y);
    //        }
    //    }
       
    //    return RedNoise(_heights);
    //}

    //private float Noise(int x, int y)
    //{
    //    return Mathf.Sin(x+y) + Mathf.Cos(x+y);
    //}

    //private float[,] RedNoise(float[,] noise)
    //{
    //    var _heights = new float[width, height];
    //    for (int i = 0; i < width; i++)
    //    {
    //        for (int j = 0; j < height; j++)
    //        {
    //            _heights[i, j] = flatCoefficient * (noise[i, j] + noise[i, j + 1] + noise[i+1, j])/3;
    //        }
    //    }
    //    return _heights;
    //}

    private float CalculateHeight(int x, int y)
    {
        float xCoord = (float)x / width * scale + offsetX;
        float yCoord = (float)y / height * scale +  offsetY;

        var _height = (Mathf.PerlinNoise(noiseCoefficients[0] * xCoord, noiseCoefficients[0] * yCoord)
             + 0.5f * Mathf.PerlinNoise(noiseCoefficients[1] * xCoord, noiseCoefficients[1] * yCoord)
             + 0.25f * Mathf.PerlinNoise(noiseCoefficients[2] * xCoord, noiseCoefficients[2] * yCoord)) * flatCoefficient;

        _height = (float)Math.Pow(_height, exponent);

        return _height;
    }

}


