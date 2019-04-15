using System;
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
        aloneRadius = scale * 5f;
        parentRadius = scale * 9f;
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
    System.Random rn = new System.Random();

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
    float maxTreeScale = 1;
    public float minTreeScale = 0.4f; // не более 0,9
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

    int checkParent(int castIndex, Vector2 currentChildIndx)
    {
        List<int> prIndexCollection = new List<int>();
        if (castIndex > 1)
        {
            for (int i = 0; i < trees[0].Count; i++)
            {
                Vector3 param = trees[0][i];
                param.y = trees[0][i].parentRadius;
                if (checkQzone(param, currentChildIndx))
                {
                    prIndexCollection.Add(trees[0][i].Tree.prototypeIndex);
                }
            }
        }

        if (prIndexCollection.Count == 0)
        {
            return rn.Next(0, terrain.terrainData.treePrototypes.Length);
        }

        return prIndexCollection[rn.Next(prIndexCollection.Count)];
    }



    void addCastTree(int castIndx, float[,] noise, float[,] noise1)
    {
        float cTS = (maxTreeScale - minTreeScale) / castCount;
        int reversCI = castCount - castIndx - 1;
        float minCastParam = (cTS * reversCI) + minTreeScale;
        float maxCastParam = (cTS * (reversCI + 1)) + minTreeScale;

        for (int x = 0; x < width / minDist; x++)
        {
            for (int z = 0; z < height / minDist; z++)
            {
                
                int noiseX = rn.Next(-(int)(minDist * 0.1f), (int)(minDist * 0.1f));
                float xNoise = Math.Abs(x + noiseX);
                int noiseZ = rn.Next(-(int)(minDist * 0.1f), (int)(minDist * 0.1f));
                float zNoise = Math.Abs(z + noiseX);
                if(zNoise < height / minDist && xNoise < width / minDist)
                {
                    var alphaMaps = terrain.terrainData.GetAlphamaps(0, 0, terrain.terrainData.alphamapWidth, terrain.terrainData.alphamapHeight);
                    int cTextureOnTerH = terrain.terrainData.alphamapHeight / height;
                    int cTextureOnTerW = terrain.terrainData.alphamapWidth / width;
                    int iTextureGraund = 1; /// индекс текстуры земли КОСТЫЛЬ

                    int xCoord = minDist * (int)xNoise;
                    if (xCoord > 255)
                    {
                        xCoord = 255;
                    }
                    Vector2 coord = new Vector2(xCoord, zNoise * minDist);
                    float xD = xNoise * (float)minDist / (float)width;
                    float zD = zNoise * minDist / height;

                    if (noise1[(int)xNoise, (int)zNoise] > minCastParam && noise1[(int)xNoise, (int)zNoise] <= maxCastParam)
                    {
                        if (!isQZones(coord) && (alphaMaps[(int)zNoise * minDist * cTextureOnTerW, (int)xNoise * minDist * cTextureOnTerH, iTextureGraund] < 1))
                        {
                            var position = new Vector3(xD, heightMap[xCoord, z], zD);
                            int prototypeIndex = checkParent(castIndx, coord);
                            var tree = new TreeInfo(position, prototypeIndex, noise1[(int)xNoise, (int)zNoise]);
                            trees[castIndx].Add(tree);
                        }
                    }

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
                    int prototypeIndex = checkParent(castIndx, coord);

                    var tree = new TreeInfo(position, prototypeIndex, rn.Next(minTS, maxTS) * 0.001f);
                    trees[castIndx].Add(tree);
                }
            }
        }
    }


    void gTree(float xTer, float zTer)
    {
        gCasts();
        Calculated calculated = new Calculated(CalculateHeight);
        float[,] noise = CreateHeights(width / minDist, height / minDist, calculated);
        calculated = new Calculated(CalculateNoise);
        float[,] whiteNoise = CreateHeights(width / minDist, height / minDist, calculated);
        for (int i = 0; i < castCount; i++)
        {
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

    public delegate float Calculated(int weight, int height);

    /*По координатам карты (для карты)*/
    float[,] CreateHeights()
    {
        Calculated calculated = new Calculated(CalculateHeight);
        return CreateHeights(width, height, calculated);
    }

    /*По заданным координатам (для деревьев)*/
    float[,] CreateHeights(int w, int h, Calculated calculate)
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

    float CalculateNoise(int min, int max)
    {
        return 0.001f * rn.Next(0, 1000);
    }


    float CalculateHeight(int x, int y)
    {
        float xCoord = (float)x / width * scale + offsetX;
        float yCoord = (float)y / height * scale + offsetY;

        return Mathf.PerlinNoise(xCoord, yCoord);
    }
}
