using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class DiamondSquare
{
    private float R; // Коэффициент скалистости
    private float GRAIN = 8; // Коэффициент зернистости
    private bool Mountain; // Делать ли гору

    private float WH;
    private float[,] heights;

    private static readonly float minWHM = 100 / 0.2f;
    private static readonly float maxWHM = 100 / 0.5f;
    private float dispMin;
    private float dispMax;

    public DiamondSquare(int width, int height, float grain, float r, bool mountain)
    {
        R = r;
        GRAIN = grain;
        WH = (float)width + height;
        heights = new float[width, height];
        Mountain = mountain;

        dispMin = width / minWHM;
        dispMax = width / maxWHM;
    }

    // Считаем рандомный коэффициент смещения для высоты
    float DisplaceField(float num)
    {
        float max = num / WH * GRAIN * 0.25f;
        return UnityEngine.Random.Range(-0.5f, 0.5f) * max;
    }
    // Считаем рандомный коэффициент смещения для высоты
    float Displace(float num)
    {
        float max = num / WH * GRAIN;
        return UnityEngine.Random.Range(-0.5f, 0.5f) * max;
    }
    // Считаем рандомный коэффициент смещения для высоты
    float DisplaceMountain(float num)
    {
        float max = num / WH * GRAIN;
        return UnityEngine.Random.Range(dispMin, dispMax) * max;
    }

    // Вызов функции отрисовки с параметрами
    public float[,] DrawPlasma(float w, float h)
    {
        float c1 = UnityEngine.Random.Range(0.2f, 0.25f);
        float c2 = UnityEngine.Random.Range(0.2f, 0.25f);
        float c3 = UnityEngine.Random.Range(0.2f, 0.25f);
        float c4 = UnityEngine.Random.Range(0.2f, 0.25f);

        Divide(0.0f, 0.0f, w, h, c1, c2, c3, c4);

        return heights;
    }

    // Вызов функции отрисовки с параметрами
    public float[,] DrawPlasma(float c1, float c2, float c3, float c4, float w, float h)
    {
        float _c1 = c1;
        float _c2 = c2;
        float _c3 = c3;
        float _c4 = c4;

        if (!Mountain)
            Divide(0.0f, 0.0f, w, h, _c1, _c2, _c3, _c4);
        else 
            DivideMountain(0.0f, 0.0f, w, h, _c1, _c2, _c3, _c4);

        return heights;
    }

    // Сама рекурсивная функция отрисовки
    void Divide(float x, float y, float w, float h, float c1, float c2, float c3, float c4)
    {

        float newWidth = w * 0.5f;
        float newHeight = h * 0.5f;

        if (w < 1.0f && h < 1.0f)
        {
            float c = (c1 + c2 + c3 + c4) * 0.25f;
            heights[(int)x, (int)y] = c * R;
        }
        else
        {
            float middle = (c1 + c2 + c3 + c4) * 0.25f + DisplaceField(newWidth + newHeight);
            float edge1 = (c1 + c2) * 0.5f;
            float edge2 = (c2 + c3) * 0.5f;
            float edge3 = (c3 + c4) * 0.5f;
            float edge4 = (c4 + c1) * 0.5f;

            Divide(x, y, newWidth, newHeight, c1, edge1, middle, edge4);
            Divide(x + newWidth, y, newWidth, newHeight, edge1, c2, edge2, middle);
            Divide(x + newWidth, y + newHeight, newWidth, newHeight, middle, edge2, c3, edge3);
            Divide(x, y + newHeight, newWidth, newHeight, edge4, middle, edge3, c4);
        }
    }
    void DivideMountain(float x, float y, float w, float h, float c1, float c2, float c3, float c4)
    {

        float newWidth = w * 0.5f;
        float newHeight = h * 0.5f;

        if (w < 1.0f && h < 1.0f)
        {
            float c = (c1 + c2 + c3 + c4) * 0.25f;
            heights[(int)x, (int)y] = c * R;
        }
        else
        {
            float middle = (c1 + c2 + c3 + c4) * 0.25f + DisplaceMountain(newWidth + newHeight);
            float edge1 = (c1 + c2) * 0.5f;
            float edge2 = (c2 + c3) * 0.5f;
            float edge3 = (c3 + c4) * 0.5f;
            float edge4 = (c4 + c1) * 0.5f;

            DivideMountain2(x, y, newWidth, newHeight, c1, edge1, middle, edge4);
            DivideMountain2(x + newWidth, y, newWidth, newHeight, edge1, c2, edge2, middle);
            DivideMountain2(x + newWidth, y + newHeight, newWidth, newHeight, middle, edge2, c3, edge3);
            DivideMountain2(x, y + newHeight, newWidth, newHeight, edge4, middle, edge3, c4);
        }
    }
    void DivideMountain2(float x, float y, float w, float h, float c1, float c2, float c3, float c4)
    {

        float newWidth = w * 0.5f;
        float newHeight = h * 0.5f;

        if (w < 1.0f && h < 1.0f)
        {
            float c = (c1 + c2 + c3 + c4) * 0.25f;
            heights[(int)x, (int)y] = c * R;
        }
        else
        {
            float middle = (c1 + c2 + c3 + c4) * 0.25f + Displace(newWidth + newHeight);
            float edge1 = (c1 + c2) * 0.5f;
            float edge2 = (c2 + c3) * 0.5f;
            float edge3 = (c3 + c4) * 0.5f;
            float edge4 = (c4 + c1) * 0.5f;

            DivideMountain2(x, y, newWidth, newHeight, c1, edge1, middle, edge4);
            DivideMountain2(x + newWidth, y, newWidth, newHeight, edge1, c2, edge2, middle);
            DivideMountain2(x + newWidth, y + newHeight, newWidth, newHeight, middle, edge2, c3, edge3);
            DivideMountain2(x, y + newHeight, newWidth, newHeight, edge4, middle, edge3, c4);
        }
    }
}
