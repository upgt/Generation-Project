using System;
using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts;
using Application;

public class Generator : MonoBehaviour
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

    public int depth = 256;
    public float[,] heightMap;  
    public int width = 256;
    public int height = 256;
    public float grain = 8; // Коэффициент зернистости
    public float r;

    //горы
    public int[] mountainsX;
    public int[] mountainsY;
    public int[] mountainsW;
    private List<Mountain> mountains = new List<Mountain>();
    //массив нулей и единиц
    public int[,] mountainsNulliki;                         

    private float[,] heights;   

    private float xTerrain = 0;
    private float zTerrain = 0;

    public static implicit operator float[,] (TerrainGenerator t)
    {
        return t.heightMap;
    }

    private void Start()
    {
        Terrain = GetComponent<Terrain>();
        Terrain.terrainData = CreateTerrain(Terrain.terrainData);

        // Работает только тогда когда в массиве деревьев есть хотя бы одно деревоVector3 position = new Vector3(xTerrain, 0, zTerrain);                     
        if (Terrain.terrainData.treePrototypes.Length > 0)
        {
            Vector3 positionEnd = new Vector3(width + xTerrain, 0, width + zTerrain);
        }
    }

    TerrainData CreateTerrain(TerrainData terrainData)
    {
        int resolution = width;

        //создаем равнину
        DiamondSquare diamondSquare = new DiamondSquare(width, height, grain, r, false);
        heights = diamondSquare.DrawPlasma(width, height);
        mountainsNulliki = new int[width, height];
        SetOnes(mountainsNulliki);

        //располагаем горы на карте
        for (int i = 0; i<mountainsW.Length; i++)
        {
            int x = mountainsX[i] > 0 ? mountainsX[i] : 0;
            int y = mountainsY[i] > 0 ? mountainsY[i] : 0;
            int w = mountainsW[i] > 0 ? mountainsW[i] : 0;
            mountains.Add(new Mountain(x, y, w, InitMountainBase(x, y, w)));
        }

        foreach (Mountain e in mountains)
        {
            e.SetOnField(heights);
            e.SetNull(mountainsNulliki);
        }

        // Применяем изменения
        terrainData.size = new Vector3(width, depth, height);
        terrainData.heightmapResolution = resolution;
        terrainData.SetHeights(0, 0, heights);

        return terrainData;
    }
    private void SetOnes(int[,] array)
    {
        for (int i = 0; i < array.GetLength(0); i++)
            for (int j = 0; i < array.GetLength(1); j++)
                array[i, j] = 1;
    }

    private float[] InitMountainBase(int x, int y, int w)
    {
        float[] mBase = new float[4];

        if (x < -w / 2 || x > heights.GetLength(0) || y < -w / 2 || y > heights.GetLength(1))
            throw new Exception();

        if (PointInField(x, y))
            mBase[3] = heights[x, y];
        else
            mBase[3] = heights[0, 0];

        if (PointInField(x + w, y))
            mBase[2] = heights[x + w, y];
        else
            mBase[2] = heights[width - 1, 0];

        if (PointInField(x, y + w))
            mBase[1] = heights[x, y + w];
        else
            mBase[1] = heights[0, height - 1];

        if (PointInField(x + w, y + w))
            mBase[0] = heights[x + w, y + w];
        else
            mBase[0] = heights[width - 1, height - 1];

        return mBase;
    }

    private bool PointInField(int x, int y)
    { 
        return false || (x >= 0 && x < heights.GetLength(0) && y >= 0 && y < heights.GetLength(1));
    }
}