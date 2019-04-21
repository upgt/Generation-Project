using System;
using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts;

public class TerrainGenerator : MonoBehaviour
{
    public int depth = 20;
    //public int plainDepth = 20;
    protected float[,] heightMap;
    public int height = 256;
    public int width = 256;

    public float scale = 20f;
    public float offsetX = 100f;
    public float offsetY = 100f;
    private Terrain terrain;

    public float flatCoefficient = 0.2f;    //сглаживание шума
    public float[] noiseCoefficients;
    public float exponent = 1f;
    private float[,] heights;

    public List<List<TreeInfo>> Trees = new List<List<TreeInfo>>();
    public List<Vector3> QuestZones; // x = x; y = radius; z = z
    private float xTerrain = 0;
    private float zTerrain = 0;
    public int Casts;
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
    private const float MAX_TREE_SCALE = 1;
    public float minTreeScale = 0.4f; // не более 0,9

    bool IsPointInZone(Vector2 point, Vector3 zone)
    {
        float r = zone.y;
        if (zone.x + r > point.x && zone.x - r < point.x
        && zone.z + r > point.y && zone.z - r < point.y)
        {
            return true;
        }
        return false;
    }

    bool IsPointInZones(Vector2 point, List<Vector3> zone)
    {
        bool isPointInZone = false;

        for (int i = 0; i < zone.Count && !isPointInZone; i++)
        {
            if (IsPointInZone(point, zone[i]))
            {
                isPointInZone = true;
            }
        }
        return isPointInZone;
    }

    private void GenCasts()
    {
        castCount = Casts;
        Casts = castCount;
        TreeInfo.countCast = Casts;

        for (int i = 0; i < Casts; i++)
        {
            Trees.Add(new List<TreeInfo>());
        }
    }

    private void DrawTreeCast()
    {
        for (int i = 0; i < castCount; i++)
        {
            for (int j = 0; j < Trees[i].Count; j++)
            {
                terrain.AddTreeInstance(Trees[i][j]);
            }
        }
        terrain.Flush();
    }

    void GenTreesQuestZones(int cast)
    {
        for (int i = 0; i < Trees[cast].Count; i++)
        {
            Vector3 AloneRadius = Trees[cast][i];
            AloneRadius.y = Math.Max(Trees[cast][i].aloneRadius, minDist);
            QuestZones.Add(AloneRadius);
        }
    }

    int GenIndexByParents(int castIndex, Vector2 currentChildIndx)
    {
        List<int> prIndexCollection = new List<int>();
        if (castIndex > 1)
        {
            for (int i = 0; i < Trees[0].Count; i++)
            {
                Vector3 param = Trees[0][i];
                param.y = Trees[0][i].parentRadius;
                if (IsPointInZone(currentChildIndx, param))
                {
                    prIndexCollection.Add(Trees[0][i].TreeInst.prototypeIndex);
                }
            }
        }

        if (prIndexCollection.Count == 0)
        {
            return rn.Next(0, terrain.terrainData.treePrototypes.Length);
        }

        return prIndexCollection[rn.Next(prIndexCollection.Count)];
    }



    void AddTreeCast(int castIndx, float[,] noise)
    {
        float cTS = (MAX_TREE_SCALE - minTreeScale) / castCount;
        int reverseCastIndex = castCount - castIndx - 1;
        float minCastParam = (cTS * reverseCastIndex) + minTreeScale;
        float maxCastParam = (cTS * (reverseCastIndex + 1)) + minTreeScale;

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
                    float xD = xNoise * minDist / width;
                    float zD = zNoise * minDist / height;

                    if (noise[(int)xNoise, (int)zNoise] > minCastParam && noise[(int)xNoise, (int)zNoise] <= maxCastParam)
                    {
                        if (!IsPointInZones(coord, QuestZones) && 
                            (alphaMaps[(int)zNoise * minDist * cTextureOnTerW, (int)xNoise * minDist * cTextureOnTerH, iTextureGraund] < 1))
                        {
                            var position = new Vector3(xD, heightMap[xCoord, z], zD);
                            int prototypeIndex = GenIndexByParents(castIndx, coord);
                            var tree = new TreeInfo(position, prototypeIndex, noise[(int)xNoise, (int)zNoise]);
                            Trees[castIndx].Add(tree);
                        }
                    }

                }
            }
        }
    }

    void GenTree(float xTer, float zTer)
    {
        GenCasts();
        Calculated calculated = new Calculated(CalculateHeight);
        float[,] noise = CreateHeights(width / minDist, height / minDist, calculated);
        calculated = new Calculated(CalculateNoise);
        float[,] whiteNoise = CreateHeights(width / minDist, height / minDist, calculated);
        for (int i = 0; i < castCount; i++)
        {
            AddTreeCast(i, whiteNoise);
            GenTreesQuestZones(i);
        }
        DrawTreeCast();
    }
    
    private void Start()
    {
        TreeInfo.maxScale = MAX_TREE_SCALE;
        TreeInfo.minScale = minTreeScale;
        
        terrain = GetComponent<Terrain>(); 

        offsetX = UnityEngine.Random.Range(0, 1000f);
        offsetY = UnityEngine.Random.Range(0, 1000f);

        terrain.terrainData = CreateTerrain(terrain.terrainData);
        
        // Работает только тогда когда в массиве деревьев есть хотя бы одно деревоVector3 position = new Vector3(xTerrain, 0, zTerrain);                     
        if (terrain.terrainData.treePrototypes.Length > 0)
        {
            Vector3 positionEnd = new Vector3(width + xTerrain, 0, height + zTerrain);   
            GenTree(xTerrain, zTerrain);
        }
    }
    private void Update()
    {
        Casts = castCount;
        terrain.terrainData = CreateTerrain(terrain.terrainData);
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


    private float CalculateHeight(int x, int y)
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