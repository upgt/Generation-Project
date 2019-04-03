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
        aloneRadius = scale * 0.1f;
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

    void DrawCastTree(int cast)
    {
        for (int i = 0; i < trees[cast].Count; i++)
        {
            terrain.AddTreeInstance(trees[cast][i]);
            terrain.Flush();
        }
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
        System.Random rn = new System.Random();
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
            return rn.Next(0, terrain.terrainData.treePrototypes.Length - 1);
        }

        return prIndexCollection[rn.Next(prIndexCollection.Count)];
    }

    void addCastTree(int castIndx)
    {
        float cTS = castIndx * (maxTreeScale - minTreeScale) / castCount;
        int maxTS = (int)(maxTreeScale - cTS) * 100;
        int minTS = (int)(minTreeScale + cTS) * 100;

        System.Random rn = new System.Random();
        for (int x = 0; x < width - minDist; x += rn.Next(minDist, maxDist) * castCount)
        {
            for (int z = 0; z < height; z += rn.Next(minDist, maxDist) * castCount)
            {
                int noiseX = rn.Next(-(int)(minDist * 0.7f), (int)(minDist * 0.7f));
                int xNoise = Math.Abs(x + noiseX);

                Vector2 coord = new Vector2(xNoise, z);
                //if (!isQZones(coord))
                //{
                var position = new Vector3((float)xNoise / width, heightMap[xNoise, z], ((float)z) / height);
                int prototypeIndex = checkParent(castIndx, coord);

                var tree = new TreeInfo(position, prototypeIndex, rn.Next(minTS, maxTS) * 0.01f);
                trees[castIndx].Add(tree);
                //}
            }
        }
    }

    void gTree(float xTer, float zTer, int de = 1, int ds = 0)
    {
        gCasts();
        for (int i = ds; i < de/*trees.Count*/; i++)
        {
            addCastTree(i);
            gTreesQuestZones(i);
            DrawCastTree(i);
        }
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
            var tree = new TreeInfo(new Vector3(0,0,0), 0, maxTreeScale);
            terrain.AddTreeInstance(tree);
            terrain.Flush();
            tree = new TreeInfo(new Vector3(0.05f, 0, 0), 0, minTreeScale);
            terrain.AddTreeInstance(tree);
            terrain.Flush();
            //gTree(xTerrain, zTerrain);
        }


        // Конец зоны Goshana
    }
    private void Update()
    {
        casts = castCount;
        if (ind == 1)
        {
            gTree(xTerrain, zTerrain, 2, 1);
            ind++;
        }

        if (ind == 2)
        {
            gTree(xTerrain, zTerrain, 3, 2);
            ind++;
        }

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

        return Mathf.PerlinNoise(xCoord, yCoord);
    }
}
