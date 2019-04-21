using System;
using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts;

public  class Generator : MonoBehaviour
{
    public virtual Terrain Terrain { get; set; }

    public virtual float[,] CreateHeights(int w, int h, Calculated calculate)
    {
        throw new Exception("empty method");
    }

    public virtual float CalculateHeight(int x, int y)
    {
        throw new Exception("empty method");
    }
}
public class TerrainGenerator : Generator
{
    public int depth = 20;
    //public int plainDepth = 20;
    public float[,] heightMap;
    public int height = 256;
    public int width = 256;

    public float scale = 20f;
    public float offsetX = 100f;
    public float offsetY = 100f;

    public float flatCoefficient = 0.2f;    //сглаживание шума
    public float[] noiseCoefficients;
    public float exponent = 1f;
    private float[,] heights;

    private float xTerrain = 0;
    private float zTerrain = 0;
   
    public static implicit operator float[,](TerrainGenerator t)
    {
        return t.heightMap;
    }

    private void Start()
    {

        Terrain = GetComponent<Terrain>();

        offsetX = UnityEngine.Random.Range(0, 1000f);
        offsetY = UnityEngine.Random.Range(0, 1000f);

        Terrain.terrainData = CreateTerrain(Terrain.terrainData);

        // Работает только тогда когда в массиве деревьев есть хотя бы одно деревоVector3 position = new Vector3(xTerrain, 0, zTerrain);                     
        if (Terrain.terrainData.treePrototypes.Length > 0)
        {
            Vector3 positionEnd = new Vector3(width + xTerrain, 0, height + zTerrain);
        }
    }
    private void Update()
    {
        Terrain.terrainData = CreateTerrain(Terrain.terrainData);
    }

    TerrainData CreateTerrain(TerrainData terrainData)
    {
        terrainData.heightmapResolution = width + 1;
        terrainData.size = new Vector3(width, depth, height);
        heightMap = CreateHeights();
        terrainData.SetHeights((int)xTerrain, (int)zTerrain, heightMap);
        return terrainData;
    }

    

    /*По координатам карты (для карты)*/
    float[,] CreateHeights()
    {
        Calculated calculated = new Calculated(CalculateHeight);
        return CreateHeights(width, height, calculated);
    }

  


    public override float CalculateHeight(int x, int y)
    {
        float xCoord = (float)x / width * scale + offsetX;
        float yCoord = (float)y / height * scale + offsetY;

        var _height = (Mathf.PerlinNoise(noiseCoefficients[0] * xCoord, noiseCoefficients[0] * yCoord)
             + 0.5f * Mathf.PerlinNoise(noiseCoefficients[1] * xCoord, noiseCoefficients[1] * yCoord)
             + 0.25f * Mathf.PerlinNoise(noiseCoefficients[2] * xCoord, noiseCoefficients[2] * yCoord)) * flatCoefficient;

        _height = (float)Math.Pow(_height, exponent);

        return _height;
    }

    public override float[,] CreateHeights(int w, int h, Calculated calculate)
    {
        float[,] heights = new float[w, h];
        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                heights[x, y] = calculate.Invoke(x, y);
            }
        }
        return heights;
    }
}