//using System;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{

    //public int depth = 200;
    //public int plainDepth = 20;
    //public int height = 256;
    //public int width = 256;

    //public float scale = 20f;
    //public float offsetX = 100f;
    //public float offsetY = 100f;

    ////public int mountainHeight = 200;
    ////public int mountainX = 100;
    ////public int mountainY = 100;
    ////public double angle = 30f;

    //public float flatCoefficient = 0.2f;    //сглаживание шума
    //public float[] noiseCoefficients;
    //public float exponent = 1f;
    //private float[,] heights;

    public float R; // Коэффициент скалистости
    public int GRAIN = 8; // Коэффициент зернистости
    public bool FLAT = false; // Делать ли равнины
    //public Material material;

    public int width = 1024;
    public int height = 1024;
    public int depth = 20;
    private float WH;
    private Color32[] cols;
    private Texture2D texture;

    private void Start()
    {
        int resolution = width;
        WH = (float)width + height;

        // Задаём карту высот
        Terrain terrain = FindObjectOfType<Terrain>();
        float[,] heights = new float[resolution, resolution];

        // Создаём карту высот
        texture = new Texture2D(width, height);
        cols = new Color32[width * height];
        drawPlasma(width, height);
        texture.SetPixels32(cols);
        texture.Apply();

        // Используем шейдер (смотри пункт 3 во 2 части)
        //material.SetTexture("_HeightTex", texture);

        // Задаём высоту вершинам по карте высот
        for (int i = 0; i < resolution; i++)
        {
            for (int k = 0; k < resolution; k++)
            {
                heights[i, k] = texture.GetPixel(i, k).grayscale * R;
            }
        }

        // Применяем изменения
        terrain.terrainData.size = new Vector3(width, depth, height);
        terrain.terrainData.heightmapResolution = resolution;
        terrain.terrainData.SetHeights(0, 0, heights);

        //offsetX = UnityEngine.Random.Range(0, 1000f);
        //offsetY = UnityEngine.Random.Range(0, 1000f);
        //Terrain terrain = GetComponent<Terrain>();
        //terrain.terrainData = CreateTerrain(terrain.terrainData);

    }

    // Считаем рандомный коэффициент смещения для высоты
    float displace(float num)
    {
        float max = num / WH * GRAIN;
        return Random.Range(-0.5f, 0.5f) * max;
    }

    // Вызов функции отрисовки с параметрами
    void drawPlasma(float w, float h)
    {
        float c1, c2, c3, c4;

        c1 = Random.value;
        c2 = Random.value;
        c3 = Random.value;
        c4 = Random.value;

        divide(0.0f, 0.0f, w, h, c1, c2, c3, c4);
    }

    // Сама рекурсивная функция отрисовки
    void divide(float x, float y, float w, float h, float c1, float c2, float c3, float c4)
    {

        float newWidth = w * 0.5f;
        float newHeight = h * 0.5f;

        if(w < 1.0f && h < 1.0f)
        {
            float c = (c1 + c2 + c3 + c4) * 0.25f;
            cols[(int)x + (int)y * width] = new Color(c, c, c);
        }
        else
        {
            float middle = (c1 + c2 + c3 + c4) * 0.25f + displace(newWidth + newHeight);
            float edge1 = (c1 + c2) * 0.5f;
            float edge2 = (c2 + c3) * 0.5f;
            float edge3 = (c3 + c4) * 0.5f;
            float edge4 = (c4 + c1) * 0.5f;

            if (!FLAT)
            {
                if (middle <= 0)
                {
                    middle = 0;
                }
                else if (middle > 1.0f)
                {
                    middle = 1.0f;
                }
            }
            divide(x, y, newWidth, newHeight, c1, edge1, middle, edge4);
            divide(x + newWidth, y, newWidth, newHeight, edge1, c2, edge2, middle);
            divide(x + newWidth, y + newHeight, newWidth, newHeight, middle, edge2, c3, edge3);
            divide(x, y + newHeight, newWidth, newHeight, edge4, middle, edge3, c4);
        }
    }
}







    //private void Update()
    //{
    //    //Terrain terrain = GetComponent<Terrain>();
    //    //terrain.terrainData = CreateTerrain(terrain.terrainData);

    //}

    //private TerrainData CreateTerrain(TerrainData terrainData)
    //{
    //    terrainData.heightmapResolution = width + 1;
    //    terrainData.size = new Vector3(width, plainDepth, height);
    //    terrainData.SetHeights(0, 0, CreateHeights());              //генерация шумом перлина
    //    return terrainData;
    //}

    //private float[,] CreateHeights()
    //{
    //    heights = new float[width, height];
    //    for (int x = 0; x < width; x++)
    //    {
    //        for (int y = 0; y < height; y++)
    //        {
    //            heights[x, y] = CalculateHeight(x, y);
    //        }
    //    }
    //    return heights;
    //}

    //private float CalculateHeight(int x, int y)
    //{
    //    float xCoord = (float)x / width * scale + offsetX;
    //    float yCoord = (float)y / height * scale +  offsetY;

    //    var _height = (Mathf.PerlinNoise(noiseCoefficients[0] * xCoord, noiseCoefficients[0] * yCoord)
    //         + 0.5f * Mathf.PerlinNoise(noiseCoefficients[1] * xCoord, noiseCoefficients[1] * yCoord)
    //         + 0.25f * Mathf.PerlinNoise(noiseCoefficients[2] * xCoord, noiseCoefficients[2] * yCoord)) * flatCoefficient;

    //    _height = (float)Math.Pow(_height, exponent);

    //    return _height;
    //}



