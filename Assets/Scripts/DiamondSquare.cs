using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public class Square
//{
//    public float c1;
//    public float c2;
//    public float c3;
//    public float c4;

//    public Square()
//    {
//        c1 = Random.value;
//        c2 = Random.value;
//        c3 = Random.value;
//        c4 = Random.value;
//    }

//    public Square(float a, float b, float c, float d)
//    {
//        c1 = a;
//        c2 = b; 
//        c3 = c; 
//        c4 = d;
//    }

//    public float AverageValue()
//    {
//        return (c1 + c2 + c3 + c4) * 0.25f;
//    }
//}

public class DiamondSquare : MonoBehaviour
{
    private float R; // Коэффициент скалистости
    private int GRAIN = 8; // Коэффициент зернистости
    private bool FLAT = false; // Делать ли равнины

    private int width = 2048;
    private int height = 2048;
    private float WH;
    private Color32[] cols;


    public DiamondSquare(int width, int height, int grain, float r)
    {
        R = r;
        GRAIN = grain;
        this.width = width;
        this.height = height;
        WH = (float)width + height;
        cols = new Color32[width * height];
    }


    // Считаем рандомный коэффициент смещения для высоты
    float Displace(float num)
    {
        float max = num / WH * GRAIN;
        return Random.Range(-0.5f, 0.5f) * max;
    }

    // Вызов функции отрисовки с параметрами
    public Color32[] DrawPlasma(float w, float h)
    {
        float c1 = Random.value;
        float c2 = Random.value;
        float c3 = Random.value;
        float c4 = Random.value;

        Divide(0.0f, 0.0f, w, h, c1, c2, c3, c4);

        return cols;
    }

    // Сама рекурсивная функция отрисовки
    void Divide(float x, float y, float w, float h, float c1, float c2, float c3, float c4)
    {

        float newWidth = w * 0.5f;
        float newHeight = h * 0.5f;

        if (w < 1.0f && h < 1.0f)
        {
            float c = (c1 + c2 + c3 + c4) * 0.25f;
            cols[(int)x + (int)y * width] = new Color(c, c, c);
        }
        else
        {
            float middle = (c1 + c2 + c3 + c4) * 0.25f + Displace(newWidth + newHeight);
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
            Divide(x, y, newWidth, newHeight, c1, edge1, middle, edge4);
            Divide(x + newWidth, y, newWidth, newHeight, edge1, c2, edge2, middle);
            Divide(x + newWidth, y + newHeight, newWidth, newHeight, middle, edge2, c3, edge3);
            Divide(x, y + newHeight, newWidth, newHeight, edge4, middle, edge3, c4);
        }
    }
}
