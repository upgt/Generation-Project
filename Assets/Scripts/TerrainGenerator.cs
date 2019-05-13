using System;
using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts;

public abstract class Generator : MonoBehaviour
{
    public virtual Terrain Terrain { get; set; }

    public abstract float[,] CreateHeights(int w, int h, Calculated calculate);

    public abstract float CalculateHeight(int x, int y);
}
public class TerrainGenerator : Generator
{
    // секс переменные
    private System.Random rn;
    private List<float> maskCollect;
    private int smothAngle = 1;
    public float[,] maskWMap;
    float[,] globalMaskMap;
    float groundRelief = 0.8f;

    public float[,] maskWater;
    public float[,] maskGround;

    // секс закончился


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

    public static implicit operator float[,] (TerrainGenerator t)
    {
        return t.heightMap;
    }

    private void Awake()
    {
        rn = new System.Random();
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


    //функции секса
    private float GetMaxParam(float[,] mask)
    {
        float result = 0;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                result = Mathf.Max(mask[i, j], result);
            }
        }
        return result;
    }

    private float GetMinParam(float[,] mask)
    {
        float result = 1;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                result = Mathf.Min(mask[i, j], result);
            }
        }
        return result;
    }

    private float[,] InterpolatedMask(float[,] mask)
    {
        float[,] maskTerrain = new float[width, height];
        float minParam = GetMinParam(mask);
        float maxParam = GetMaxParam(mask) - minParam;
        float param;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                param = (mask[i, j] - minParam) / maxParam;
                maskTerrain[i, j] = (float)Math.Round(param, 1);
                if (maskTerrain[i, j] * 10 % 3 != 0)
                {
                    param = (maskTerrain[i, j] * 10 % 3) * 0.1f;
                    maskTerrain[i, j] = maskTerrain[i, j] - param;
                }
            }
        }
        return maskTerrain;
    }

    float[,] mask;
    float[,] CreateMask(float[,] mask, float waterParam)
    {
        this.mask = new float[width, height];
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (mask[i, j] == waterParam)
                {
                    this.mask[i, j] = 0;
                }
                else
                {
                    this.mask[i, j] = 1;
                }
            }
        }
        return this.mask;
    }

    private float gbs = 1;
    float[,] SmoothZone(float[,] mask, float param)
    {
        float radSmoothAngle = (param + smothAngle) * Mathf.PI / 180;
        float angle = 0;
        float smoothCoef;
        angle = Mathf.Acos(gbs);
        angle += radSmoothAngle;
        smoothCoef = (Mathf.Cos(angle) + 1) / 2;

        if (param == 0)
        {
            gbs = 1;
        }

        for (int i = 1; i < width - 1; i++)
        {
            for (int j = 1; j < height - 1; j++)
            {
                if (mask[i, j] == gbs)
                {
                    if (mask[i + 1, j] == 0)
                    {
                        mask[i + 1, j] = smoothCoef;
                    }
                    if (mask[i, j + 1] == 0)
                    {
                        mask[i, j + 1] = smoothCoef;
                    }
                    if (mask[i - 1, j] == 0)
                    {
                        mask[i - 1, j] = smoothCoef;
                    }
                    if (mask[i, j - 1] == 0)
                    {
                        mask[i, j - 1] = smoothCoef;
                    }
                }
            }
        }
        gbs = smoothCoef;
        return mask;
    }

  
    float[,] MergerMask(float[,] map, float[,] mask, float param)
    {
        float[,] result = map;
        for (int i = 1; i < width - 1; i++)
        {
            for (int j = 1; j < height - 1; j++)
            {
                if (mask[i, j] != 1)
                {
                    result[i, j] += mask[i, j] * param;
                }
            }
        }
        return result;
    }

    bool IsUnique(float param, List<float> list)
    {
        bool result = true;
        for(int i = 0; i < list.Count; i++)
        {
            if(list[i] == param)
            {
                result = false;
            }
        }
        return result;
    }

    List<float> levelMap;
    void LevelMap(float[,] map)
    {
        levelMap = new List<float>();
        for (int i = 1; i < width - 1; i++)
        {
            for (int j = 1; j < height - 1; j++)
            {
                if(IsUnique(map[i,j], levelMap))
                {
                    levelMap.Add(map[i, j]);
                }
            }
        }
        levelMap.Sort();
    }
    // функции секса умерли

    TerrainData CreateTerrain(TerrainData terrainData)
    {
        // зона секса
        TerrainGenerator example = new TerrainGenerator();
        example.depth = depth;
        example.scale = scale;
        example.noiseCoefficients = noiseCoefficients;
        example.flatCoefficient = flatCoefficient;

        depth = 20;
        scale = 2;
        offsetX = rn.Next(0, 10000);
        offsetY = rn.Next(0, 10000);
        flatCoefficient = 0.03f;
        noiseCoefficients = new float[3];
        noiseCoefficients[0] = 1;
        noiseCoefficients[1] = 0;
        noiseCoefficients[2] = 0;
        exponent = 3.3f;
        terrainData.heightmapResolution = width + 1;
        terrainData.size = new Vector3(width, depth, height);
        globalMaskMap = CreateHeights();
        globalMaskMap = InterpolatedMask(globalMaskMap); // убойная хуйня так-же пригодиться для наложения текстур!
        LevelMap(globalMaskMap);
        maskWMap = CreateMask(globalMaskMap, levelMap[0]);
        for (int i = 0; i < 90 / smothAngle; i++)
        {
            maskWMap = SmoothZone(maskWMap, i * smothAngle);
        }
        maskWater = maskWMap;
        globalMaskMap = MergerMask(globalMaskMap, maskWMap, levelMap[1]);

        // делаем землю
        maskWMap = CreateMask(globalMaskMap, levelMap[1]);

        depth = 55;
        scale = 4;
        offsetX = rn.Next(0, 10000);
        offsetY = rn.Next(0, 10000);
        flatCoefficient = 0.7f;
        noiseCoefficients = new float[3];
        noiseCoefficients[0] = 1;
        noiseCoefficients[1] = 2;
        noiseCoefficients[2] = 3;
        exponent = 1.5f;
        heightMap = CreateHeights();
        maskWMap = MergerMask(heightMap, maskWMap, 1);
        maskGround = maskWMap;
        globalMaskMap = MergerMask(globalMaskMap, maskWMap, levelMap[1] * groundRelief);
        //depth = example.depth;
        //scale = example.scale;
        //exponent = example.exponent;
        //noiseCoefficients = example.noiseCoefficients;



        // конец сексу
        terrainData.heightmapResolution = width + 1;
        terrainData.size = new Vector3(width, depth, height);
        heightMap = CreateHeights();
        terrainData.SetHeights((int)xTerrain, (int)zTerrain, globalMaskMap/*heightMap*/);
        return terrainData;
    }
    private void Update()
    {
        Terrain.terrainData.SetHeights((int)xTerrain, (int)zTerrain, globalMaskMap/*heightMap*/);
        if (Input.GetKey(KeyCode.Y))
        {
            Terrain.terrainData.SetHeights((int)xTerrain, (int)zTerrain, maskWMap/*heightMap*/);
        }
        if (Input.GetKey(KeyCode.U))
        {
            Terrain.terrainData.SetHeights((int)xTerrain, (int)zTerrain, heightMap);
        }
        //Terrain.terrainData = CreateTerrain(Terrain.terrainData);
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