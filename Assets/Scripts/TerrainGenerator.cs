<<<<<<< HEAD
﻿using System;
using UnityEngine;
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
    public int depth = 20;

    protected float[,] heightMap;
    public int height = 256;
    public int width = 256;

    public float scale = 20f;
    public float offsetX = 100f;
    public float offsetY = 100f;
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

    List<List<float>> CreateNoise(int w, int h)
    {
        System.Random rn = new System.Random();
        List<List<float>> heights = new List<List<float>>();

        for (int x = 0; x < w; x++)
        {
            List<float> wei = new List<float>();
            for (int y = 0; y < h; y++)
            {
                wei.Add(0.001f * rn.Next(0, 1000));
            }
            heights.Add(wei);
        }
        return heights;
    }

    void addCastTree(int castIndx, float[,] noise, List<List<float>> noise1)
    {
        System.Random rn = new System.Random();

        for (int x = 0; x < width / minDist; x++)
        {
            for (int z = 0; z < height / minDist; z++)
            {

                if (noise1[x][z] >= minTreeScale)
                {
                    var xCoord = x * minDist;
                    Vector2 coord = new Vector2(xCoord, z);
                    var position = new Vector3((float)xCoord / width, heightMap[xCoord, z], (float)z * minDist / height);
                    int prototypeIndex = checkParent(castIndx, coord, rn);
                    var tree = new TreeInfo(position, prototypeIndex, noise1[x][z]);
                    trees[castIndx].Add(tree);
                }
            }
        }
    }

    void addCastTree(int castIndx, System.Random rn)
    {
        var alphaMaps = terrain.terrainData.GetAlphamaps(0, 0, terrain.terrainData.alphamapWidth, terrain.terrainData.alphamapHeight);

        int cTextureOnTerH = terrain.terrainData.alphamapHeight / height;
        int cTextureOnTerW = terrain.terrainData.alphamapWidth / width;
        int iTextureGraund = 1; /// индекс текстуры земли КОСТЫЛЬ
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
                    var position = new Vector3((float)xNoise / width, heightMap[xNoise, z], ((float)z) / height);
                    int prototypeIndex = checkParent(castIndx, coord, rn);

                    var tree = new TreeInfo(position, prototypeIndex, rn.Next(minTS, maxTS) * 0.001f);
                    trees[castIndx].Add(tree);
                }
            }
        }
    }

    /*0, 0.1 ,1*/

    void gTree(float xTer, float zTer)
    {
        gCasts();
        float[,] noise = CreateHeights(width / minDist, height / minDist);
        List<List<float>> whiteNoise = CreateNoise(width / minDist, height / minDist);
        for (int i = 0; i < castCount; i++)
        {
            System.Random rn = new System.Random();
            addCastTree(i, noise, whiteNoise);
            //addCastTree(i, rn);
            gTreesQuestZones(i);
        }
        DrawCastTree();
    }


    // Конец власти Гошана


    private void Start()
    {
        TreeInfo.maxScale = maxTreeScale;
        TreeInfo.minScale = minTreeScale;
        terrain = GetComponent<Terrain>(); // ЭТО ОБЯЗ В НАЧАЛЕ ПЛАНЕТЫ СУКА!!!!!!!!!!!!

        offsetX = UnityEngine.Random.Range(0, 1000f);
        offsetY = UnityEngine.Random.Range(0, 1000f);


        terrain.terrainData = CreateTerrain(terrain.terrainData);
        // Зона Гошана



        // Работает только тогда когда в массиве деревьев есть хотя бы одно деревоVector3 position = new Vector3(xTerrain, 0, zTerrain);                     // Крайняя начальная точка ---- это нужно гашану
        if (terrain.terrainData.treePrototypes.Length > 0)
        {
            Vector3 positionEnd = new Vector3(width + xTerrain, 0, height + zTerrain);    // Крайняя конечная точка ---- это нужно гашану
            gTree(xTerrain, zTerrain);
        }


        // Конец зоны Goshana
    }
    private void Update()
    {
        casts = castCount;

        //offsetX += Time.deltaTime * 2f;
    }

    TerrainData CreateTerrain(TerrainData terrainData)
    {
        terrainData.heightmapResolution = width + 1;
        terrainData.size = new Vector3(width, depth, height);
        heightMap = CreateHeights();
        terrainData.SetHeights((int)xTerrain, (int)zTerrain, heightMap);
        return terrainData;
    }

    float[,] CreateHeights()
    {
        return CreateHeights(width, height);
    }

    float[,] CreateHeights(int w, int h)
    {
        float[,] heights = new float[w, h];
        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                heights[x, y] = CalculateHeight(x, y);
            }
        }
        return heights;
    }



    float CalculateHeight(int x, int y)
    {
        float xCoord = (float)x / width * scale + offsetX;
        float yCoord = (float)y / height * scale + offsetY;

        return Mathf.PerlinNoise(xCoord, yCoord);
    }
}
=======
﻿using System;
using UnityEngine;
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
    public int depth = 20;

    protected float[,] heightMap;
    public int height = 256;
    public int width = 256;

    public float scale = 20f;
    public float offsetX = 100f;
    public float offsetY = 100f;

    public float flatCoefficient = 1f;
    public float[] noiseCoefficients;
    public float exponent = 1f;

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
        
        int cTextureOnTerH = terrain.terrainData.alphamapHeight/height;
        int cTextureOnTerW = terrain.terrainData.alphamapWidth/width;
        int iTextureGraund = 1; /// индекс текстуры земли КОСТЫЛЬ
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
                if (!isQZones(coord) && (alphaMaps[z*cTextureOnTerW, xNoise * cTextureOnTerH,iTextureGraund] < 1))
                {
                var position = new Vector3((float)xNoise / width, heightMap[xNoise, z], ((float)z) / height);
                int prototypeIndex = checkParent(castIndx, coord,rn);

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
            addCastTree(i,rn);
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

        terrain = GetComponent<Terrain>(); // ЭТО ОБЯЗ В НАЧАЛЕ ПЛАНЕТЫ СУКА!!!!!!!!!!!!
        terrain.terrainData = CreateTerrain(terrain.terrainData);
        // Зона Гошана



        // Работает только тогда когда в массиве деревьев есть хотя бы одно деревоVector3 position = new Vector3(xTerrain, 0, zTerrain);                     // Крайняя начальная точка ---- это нужно гашану
        if (terrain.terrainData.treePrototypes.Length > 0)
        {
            Vector3 positionEnd = new Vector3(width + xTerrain, 0, height + zTerrain);    // Крайняя конечная точка ---- это нужно гашану
            gTree(xTerrain, zTerrain);
        }


        // Конец зоны Goshana
    }
    private void Update()
    {
        casts = castCount;

        terrain = GetComponent<Terrain>(); // ЭТО ОБЯЗ В НАЧАЛЕ ПЛАНЕТЫ СУКА!!!!!!!!!!!!
        terrain.terrainData = CreateTerrain(terrain.terrainData);

        //offsetX += Time.deltaTime * 2f;
    }

    TerrainData CreateTerrain(TerrainData terrainData)
    {
        terrainData.heightmapResolution = width + 1;
        terrainData.size = new Vector3(width, depth, height);
        heightMap = CreateHeights();
        terrainData.SetHeights((int)xTerrain, (int)zTerrain, heightMap);
        return terrainData;
    }

    float[,] CreateHeights()
    {
        float[,] heights = new float[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                heights[x, y] = CalculateHeight(x, y);
            }
        }
        return heights;
    }

    float CalculateHeight(int x, int y)
    {
        float xCoord = (float)x / width * scale + offsetX;
        float yCoord = (float)y / height * scale + offsetY;

        var _height = (Mathf.PerlinNoise(noiseCoefficients[0] * xCoord, noiseCoefficients[0] * yCoord)
             + 0.5f * Mathf.PerlinNoise(noiseCoefficients[1] * xCoord, noiseCoefficients[1] * yCoord)
             + 0.25f * Mathf.PerlinNoise(noiseCoefficients[2] * xCoord, noiseCoefficients[2] * yCoord)) * flatCoefficient;

        _height = (float)Math.Pow(_height, exponent);

        return _height;
    }
}
>>>>>>> e69bd9249906d42ebbacda9aa9eb5c299aa3cd06
