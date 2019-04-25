<<<<<<< HEAD
﻿using Application;
using UnityEngine;
using System;
using System.Collections.Generic;

public class TreeInfo
{
    public static float maxScale;
    public static float minScale;
    public static int countCast;
    TreeInstance tree;
    public TreeInstance Tree
    {
        get
        {
            return tree;
        }
        private set
        {
            tree = value;
        }
    }
    public float aloneRadius;
    public float parentRadius;
    int castType;

    int CastType(float scale)
    {
        float castZone = maxScale - minScale;
        return 1 - (int)(countCast * (tree.heightScale - minScale) / castZone);
    }

    //конструктор в котором радиус одиночества и родительский радиус зависят от размеров дерева scale
    public TreeInfo(Vector3 pos, int prototypeIndex, float scale)
    {
        tree.position = pos;
        tree.prototypeIndex = prototypeIndex;
        tree.heightScale = scale;
        tree.widthScale = scale;
        tree.lightmapColor = Color.white;
        tree.color = Color.white;
        aloneRadius = scale * 0.02f;
        parentRadius = scale * 0.5f;
        castType = CastType(scale);

    }

    public TreeInfo(Vector3 pos, int prototypeIndex, float scale, float aloneR = 1, float parentR = 10)
        : this(pos, prototypeIndex, scale)
    {
        aloneRadius = aloneR;
        parentRadius = parentR;
    }

    public static implicit operator Vector3(TreeInfo a)
    {
        return a.tree.position;
    }

    public static implicit operator TreeInfo(Vector3 a)
    {
        return new TreeInfo(a, 0, 0);
    }

    public static implicit operator Vector2(TreeInfo a)
    {
        Vector2 result = new Vector2(a.tree.position.x, a.tree.position.z);
        return result;
    }

    public static implicit operator TreeInstance(TreeInfo a)
    {
        return a.tree;
    }
}


public class TerrainGenerator : MonoBehaviour
{
    public int depth = 200;
    public int plainDepth = 20;
=======
using System;
using UnityEngine;
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
>>>>>>> e9a2c11c089404f99e82ab814c75eb8d62eba416
    public int height = 256;
    public int width = 256;

    public float scale = 20f;
    public float offsetX = 100f;
    public float offsetY = 100f;

<<<<<<< HEAD
    public int mountainHeight = 200;
    public int mountainX = 100;
    public int mountainY = 100;
    public double angle = 30f;

    public float flatCoefficient = 0.2f;    //сглаживание шума
    public double exponent = 1f;
    public float[] noiseCoefficients;
    private float[,] heights;

    Terrain terrain;

    //Владение гошана
    public List<List<TreeInfo>> trees = new List<List<TreeInfo>>();
    public List<Vector3> questZones; // x = x; y = radius; z = z
    private float xTerrain = 0;
    private float zTerrain = 0;
    public int casts;
    private int castCount;

    public int CastCount
    {
        get { return castCount; }
        set
        {
            castCount = value;
            if (value > 8)
            {
                castCount = 8;
            }
            else if (value < 2)
            {
                castCount = 2;
            }
        }
    }

    public int maxDist = 4;
    public int minDist = 3;
    public float maxTreeScale = 2;
    public float minTreeScale = 1;
    public int ind = 0;

    bool checkQzone(Vector3 zone, Vector2 coord)
    {
        float r = zone.y;
        if (zone.x + r > coord.x && zone.x - r < coord.x
        && zone.z + r > coord.y && zone.z - r < coord.y)
        {
            return true;
        }
        return false;
    }

    bool isQZones(List<Vector3> zone, Vector2 coord)
    {
        bool result = false;
        for (int i = 0; i < zone.Count; i++)
        {
            if (checkQzone(zone[i], coord))
            {
                result = true;
            }
        }
        return result;
    }

    bool isQZones(Vector2 coord)
    {
        return isQZones(questZones, coord);
    }

    void gCasts()
    {
        castCount = casts;
        casts = castCount;
        TreeInfo.countCast = casts;

        for (int i = 0; i < casts; i++)
        {
            trees.Add(new List<TreeInfo>());
        }
    }

    void DrawCastTree()
    {
        for (int i = 0; i < castCount; i++)
        {
            for (int j = 0; j < trees[i].Count; j++)
            {
                terrain.AddTreeInstance(trees[i][j]);
            }
        }
        terrain.Flush();
    }

    void gTreesQuestZones(int cast)
    {
        for (int i = 0; i < trees[cast].Count; i++)
        {
            Vector3 AloneRadius = trees[cast][i];
            AloneRadius.y = Math.Max(trees[cast][i].aloneRadius, minDist);
            questZones.Add(AloneRadius);
        }
    }

    int checkParent(int castIndex, Vector2 currentChildIndx, System.Random rn)
    {
        List<int> prIndexCollection = new List<int>();
        if (castIndex > 1)
        {
            for (int i = 0; i < trees[castIndex - 1].Count; i++)
            {
                Vector3 param = trees[castIndex - 1][i];
                param.y = trees[castIndex - 1][i].parentRadius;
                if (checkQzone(param, currentChildIndx))
                {
                    prIndexCollection.Add(trees[castIndex - 1][i].Tree.prototypeIndex);
                }
            }
        }

        if (prIndexCollection.Count == 0)
        {
            return rn.Next(0, terrain.terrainData.treePrototypes.Length);
        }

        return prIndexCollection[rn.Next(prIndexCollection.Count)];
    }

    void addCastTree(int castIndx, System.Random rn)
    {
        var alphaMaps = terrain.terrainData.GetAlphamaps(0, 0, terrain.terrainData.alphamapWidth, terrain.terrainData.alphamapHeight);

        int cTextureOnTerH = terrain.terrainData.alphamapHeight / height;
        int cTextureOnTerW = terrain.terrainData.alphamapWidth / width;
        int iTextureGraund = 1; /// индекс текстуры земли КОСТЫЛЬ   //P.s Ground 
        float cTS = (maxTreeScale - minTreeScale) / castCount;
        float scale = (maxTreeScale - (castIndx * cTS)) * 1000;
        int maxTS = (int)scale;
        scale = (maxTreeScale - ((castIndx + 1) * cTS)) * 1000;
        int minTS = (int)scale;

        for (int x = 0; x < width - minDist; x += rn.Next(minDist, maxDist) * castCount)
        {
            for (int z = 0; z < height; z += rn.Next(minDist, maxDist) * castCount)
            {
                int noiseX = rn.Next(-(int)(minDist * 0.5f), (int)(minDist * 0.5f));
                int xNoise = Math.Abs(x + noiseX);

                Vector2 coord = new Vector2(xNoise, z);
                if (!isQZones(coord) && (alphaMaps[z * cTextureOnTerW, xNoise * cTextureOnTerH, iTextureGraund] < 1))
                {
                    var position = new Vector3((float)xNoise / width, heights[xNoise, z], ((float)z) / height);
                    int prototypeIndex = checkParent(castIndx, coord, rn);

                    var tree = new TreeInfo(position, prototypeIndex, rn.Next(minTS, maxTS) * 0.001f);
                    trees[castIndx].Add(tree);
                }
            }
        }
    }

    void gTree(float xTer, float zTer)
    {
        gCasts();
        for (int i = 0; i < castCount; i++)
        {
            System.Random rn = new System.Random();
            addCastTree(i, rn);
            gTreesQuestZones(i);
        }
        DrawCastTree();
    }


    // Конец власти Гошана
    private void Start()
    {
        TreeInfo.maxScale = maxTreeScale;
        TreeInfo.minScale = minTreeScale;

        offsetX = UnityEngine.Random.Range(0, 1000f);
        offsetY = UnityEngine.Random.Range(0, 1000f);
        //terrain = GetComponent<Terrain>();
        //terrain.terrainData = CreateTerrain(terrain.terrainData);

    }
    private void Update()
    {
        casts = castCount;
        terrain = GetComponent<Terrain>();
        terrain.terrainData = CreateTerrain(terrain.terrainData);
        //offsetX += Time.deltaTime * 2f;
    }

    //Начало власти Ивана
    private TerrainData CreateTerrain(TerrainData terrainData)
    {
        terrainData.heightmapResolution = width + 1;
        terrainData.size = new Vector3(width, plainDepth, height);

        terrainData.SetHeights((int)xTerrain, (int)zTerrain, CreateHeightsPN());              //генерация шумом перлина
        //terrainData.SetHeights(0, 0, CreateHeights());         //для экспериментов с шумами
        return terrainData;
    }

    private float[,] CreateHeightsPN()
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

    private float[,] CreateHeights()
    {
        heights = new float[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                heights[x, y] = Noise(x, y);
            }
        }

        return heights;
    }

    private float Noise(float x, float y)
    { 
        return UnityEngine.Random.Range(-1f * flatCoefficient, 1 * flatCoefficient);
    }

    //private float[,] RedNoise(float[,] noise)
    //{
    //    var _heights = new float[width, height];
    //    for (int i = 0; i < width; i++)
    //    {
    //        for (int j = 0; j < height; j++)
    //        {
    //            _heights[i, j] = flatCoefficient * (noise[i, j] + noise[i, j + 1] + noise[i + 1, j]) / 3;
    //        }
    //    }
    //    return _heights;
    //}

    private float CalculateHeight(int x, int y)
=======
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
>>>>>>> e9a2c11c089404f99e82ab814c75eb8d62eba416
    {
        float xCoord = (float)x / width * scale + offsetX;
        float yCoord = (float)y / height * scale + offsetY;

        var _height = (Mathf.PerlinNoise(noiseCoefficients[0] * xCoord, noiseCoefficients[0] * yCoord)
             + 0.5f * Mathf.PerlinNoise(noiseCoefficients[1] * xCoord, noiseCoefficients[1] * yCoord)
             + 0.25f * Mathf.PerlinNoise(noiseCoefficients[2] * xCoord, noiseCoefficients[2] * yCoord)) * flatCoefficient;

        _height = (float)Math.Pow(_height, exponent);

        return _height;
    }

<<<<<<< HEAD
    //private void AddMountain()
    //{
    //    Mountain mountain = new Mountain(mountainHeight, angle);
    //    int halfWidth = mountain.BaseWidth / 2;
    //    for (int i = 0; i < mountain.BaseWidth; i++)
    //        for (int j = 0; j < mountain.BaseWidth; j++)
    //        {
    //            if (mountain.Heights[i,j] > 0)
    //                heights[mountainX - halfWidth + i, mountainY - halfWidth + j] = mountain.Heights[i, j];
    //        }
    //}
}


=======
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
>>>>>>> e9a2c11c089404f99e82ab814c75eb8d62eba416
