using System;
using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts;
using System.IO;
using Application;

public delegate bool Deleg(float f1, float f2);

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
    public int smothAngle = 1; // пляжный спад или резкий обрыв на воде 
    public float[,] maskWMap;
    public float[,] globalMaskMap;
    float groundRelief = 0.8f; // Холмистость рельефа земли

    public float[,] maskWater;
    public float[,] maskGround;

    // секс закончился

    
    public float[,] heightMap;
    public int size = 256;

    private float scale;
    private float offsetX;
    private float offsetY;

    private float flatCoefficient;    //сглаживание шума
    private float[] noiseCoefficients;
    private float exponent;
    private float[,] heights;

    //Для новой генерации______________________________________//
    public float grain = 8; // Коэффициент зернистости
    public float r;

    public int mountainsNumber;
    private List<Mountain> mountains = new List<Mountain>();

    public int hollowsNumber;
    private List<Hollow> hollows = new List<Hollow>();
    //__________________________________________________________//

    private float xTerrain = 0;
    private float zTerrain = 0;

    public static implicit operator float[,] (TerrainGenerator t)
    {
        return t.heights;
    }

    public void StartTG()
    {
        rn = new System.Random();
        Terrain = GetComponent<Terrain>();

        offsetX = UnityEngine.Random.Range(0, 1000f);
        offsetY = UnityEngine.Random.Range(0, 1000f);

        Terrain.terrainData = CreateTerrain(Terrain.terrainData);

        // Работает только тогда когда в массиве деревьев есть хотя бы одно деревоVector3 position = new Vector3(xTerrain, 0, zTerrain);                     
        if (Terrain.terrainData.treePrototypes.Length > 0)
        {
            Vector3 positionEnd = new Vector3(size + xTerrain, 0, size + zTerrain);
        }
    }


    //функции секса
    private float GetMaxParam(float[,] mask)
    {
        float result = 0;
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                result = Mathf.Max(mask[i, j], result);
            }
        }
        return result;
    }

    private float GetMinParam(float[,] mask)
    {
        float result = 1;
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                result = Mathf.Min(mask[i, j], result);
            }
        }
        return result;
    }

    private float[,] InterpolatedMask(float[,] mask)
    {
        float[,] maskTerrain = new float[size, size];
        float minParam = GetMinParam(mask);
        float maxParam = GetMaxParam(mask) - minParam;
        float param;
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                param = (mask[i, j] - minParam) / maxParam;
                maskTerrain[i, j] = (float)Math.Round(param, 3);
                if(hollowsNumber == 0)
                {
                    maskTerrain[i, j] = maskTerrain[i, j] + 0.5f * (1 - maskTerrain[i, j]);
                }

                if (mountainsNumber == 0)
                {
                    maskTerrain[i, j] = maskTerrain[i, j] + 0.5f * (0 - maskTerrain[i, j]);
                }

                if (mountainsNumber == 0 && hollowsNumber == 0)
                {
                    maskTerrain[i, j] += 0.25f;
                }
            }
        }
        return maskTerrain;
    }

    public static float[,] CreateMask(float[,] mask, float waterParam, Deleg func)
    {
        float[,] result = new float[mask.GetLength(0), mask.GetLength(1)];
        for (int i = 0; i < mask.GetLength(0); i++)
        {
            for (int j = 0; j < mask.GetLength(1); j++)
            {
                if (func(mask[i, j] ,waterParam))
                {
                    result[i, j] = 0;
                }
                else
                {
                    result[i, j] = 1;
                }
            }
        }
        return result;
    }

    bool CheckEqual(float f1, float f2)
    {
        return f1 == f2;
    }
    

    private float downCoef = 1;
    float[,] SmoothZone(float[,] mask, float param)
    {
        float radSmoothAngle = (param + smothAngle) * Mathf.PI / 180;
        float angle = 0;
        float smoothCoef;
        angle = Mathf.Acos(downCoef);
        angle += radSmoothAngle;
        smoothCoef = (Mathf.Cos(angle) + 1) / 2;

        if (param == 0)
        {
            downCoef = 1;
        }

        for (int i = 1; i < size - 1; i++)
        {
            for (int j = 1; j < size - 1; j++)
            {
                if (mask[i, j] == downCoef)
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
        downCoef = smoothCoef;
        return mask;
    }


    float[,] MergerMask(float[,] map, float[,] mask, float param)
    {
        float[,] result = map;
        for (int i = 1; i < size - 1; i++)
        {
            for (int j = 1; j < size - 1; j++)
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
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == param)
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
        for (int i = 1; i < size - 1; i++)
        {
            for (int j = 1; j < size - 1; j++)
            {
                if (IsUnique(map[i, j], levelMap))
                {
                    levelMap.Add(map[i, j]);
                }
            }
        }
        levelMap.Sort();
    }

    private void RandomizeOffset()
    {
        offsetX = rn.Next(0, 10000);
        offsetY = rn.Next(0, 10000);
    }

    private void SetDefaultNoiseCoefs(float coreCoef, float c1 = 0, float c2 = 0)
    {
        noiseCoefficients = new float[3];
        noiseCoefficients[0] = coreCoef;
        noiseCoefficients[1] = c1;
        noiseCoefficients[2] = c2;
    }
    // функции секса умерли

    TerrainData CreateTerrain(TerrainData terrainData)
    {
        Deleg func = new Deleg(CheckEqual);
        // делаем секс
        terrainData.heightmapResolution = size + 1;
        terrainData.size = new Vector3(size, size, size);

        SetDefaultNoiseCoefs(1);
        SetTerrainValues(2, 0.03f, 3.3f);

        //создаем равнину
        DiamondSquare diamondSquare = new DiamondSquare(size, size, grain, r, false);
        heights = diamondSquare.DrawPlasma(size, size);
        globalMaskMap = new float[size, size];
        SetOnes(globalMaskMap);

        RandomField(heights);
        LevelMap(globalMaskMap);

        maskWater = InterpolatedMask(heights);
        //TestFile(maskWater);
        terrainData.SetHeights((int)xTerrain, (int)zTerrain, heights);
        return terrainData;
    }

    /*public static void TestFile(float[,] mask)
    {
        StreamWriter sf = new StreamWriter(@"E:\Толя проекты\TGen\Assets\WriteText.txt");
        for (int i = 0; i < mask.GetLength(0); i++)
        {
            string text = "";
            for (int j = 0; j < mask.GetLength(1); j++)
            {
                text += mask[i, j].ToString() + '\t';
            }
            sf.WriteLine(text);
        }
        sf.Close();
    }*/

    private void RandomField(float[,] field)
    {
        for (int i = 0; i < mountainsNumber; i++)
        {
            int w = UnityEngine.Random.Range(size / 4, size / 2);
            int x = UnityEngine.Random.Range(0, size - w);
            int y = UnityEngine.Random.Range(0, size - w);
            mountains.Add(new Mountain(x, y, w, InitMountainBase(x, y, w)));
            for (int j = 0; j < 3; j++)
            {
                int _x = SelectPoint(x, w * 0.5f);
                int _y = SelectPoint(y, w * 0.5f);
                int _w = UnityEngine.Random.Range((int)(w * 0.3f), (int)(w * 0.5f));
                mountains.Add(new Mountain(_x, _y, _w, InitMountainBase(_x, _y, _w)));
            }
        }
        foreach (Mountain e in mountains)
        {
            e.SetMask(globalMaskMap);
            e.SetOnField(field);
        }

        for (int i = 0; i < hollowsNumber; i++)
        {
            int w = UnityEngine.Random.Range(size / 4, size / 2);
            int x = UnityEngine.Random.Range(0, size - w);
            int y = UnityEngine.Random.Range(0, size - w);
            hollows.Add(new Hollow(x, y, w, InitMountainBase(x, y, w)));
            for (int j = 0; j < 2; j++)
            {
                int _x = SelectPoint(x, w * 0.5f);
                int _y = SelectPoint(y, w * 0.5f);
                int _w = UnityEngine.Random.Range((int)(w * 0.5f), w);
                hollows.Add(new Hollow(_x, _y, _w, InitMountainBase(_x, _y, _w)));
            }
        }
        foreach (Hollow e in hollows)
        {
            e.SetMask(globalMaskMap);
            e.SetOnField(field);
        }
    }


    private int SelectPoint(int a, float offset)
    {
        int result = UnityEngine.Random.Range(a, a + (int)offset);
        return result >= 0 ? result : 0;
    }


    private void SetOnes(float[,] array)
    {
        for (int i = 0; i < array.GetLength(0); i++)
            for (int j = 0; j < array.GetLength(1); j++)
                array[i, j] = 0;
    }
    private void SetNull(float[,] array)
    {
        for (int i = 0; i < array.GetLength(0); i++)
            for (int j = 0; j < array.GetLength(1); j++)
                array[i, j] = 0f;
    }

    private float[] InitMountainBase(int x, int y, int w)
    {
        float[] mBase = new float[4];

        if (PointInField(x, y))
            mBase[3] = heights[x, y];
        else
            mBase[3] = heights[0, 0];

        if (PointInField(x + w, y))
            mBase[2] = heights[x + w, y];
        else
            mBase[2] = heights[size - 1, 0];

        if (PointInField(x, y + w))
            mBase[1] = heights[x, y + w];
        else
            mBase[1] = heights[0, size - 1];

        if (PointInField(x + w, y + w))
            mBase[0] = heights[x + w, y + w];
        else
            mBase[0] = heights[size - 1, size - 1];

        return mBase;
    }

    private bool PointInField(int x, int y)
    {
        return false || (x >= 0 && x < heights.GetLength(0) && y >= 0 && y < heights.GetLength(1));
    }

    private void SetTerrainValues(float scale, float flat, float exp)
    {
        this.scale = scale;
        RandomizeOffset();
        flatCoefficient = flat;
        exponent = exp;
    }

    private void Update()
    {
        //Terrain.terrainData.SetHeights((int)xTerrain, (int)zTerrain, globalMaskMap/*heightMap*/);
        //Terrain.terrainData = CreateTerrain(Terrain.terrainData);
    }

    /*По координатам карты (для карты)*/
    float[,] CreateHeights()
    {
        Calculated calculated = new Calculated(CalculateHeight);
        return CreateHeights(size, size, calculated);
    }

    public override float CalculateHeight(int x, int y)
    {
        float xCoord = (float)x / size * scale + offsetX;
        float yCoord = (float)y / size * scale + offsetY;

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