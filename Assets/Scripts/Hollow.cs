using System;
using UnityEngine;

public class Hollow
{
    public int X;
    public int Y;
    public int width;

    public float[,] Heights { get; private set; }
    private float[] initBase = new float[4];

    public Hollow(int x, int y, int w, float[] initBase)
    {
        X = x;
        Y = y;
        width = w;
        Heights = SetHeights();
        this.initBase = initBase;
    }

    public void SetOnField(float[,] field)
    {
        for (int i = 0; i < width && i < field.GetLength(0); i++)
        {
            for (int j = 0; j < width && j < field.GetLength(1); j++)
            {

                if (X + i < field.GetLength(0) && Y + j < field.GetLength(1))
                    field[X + i, Y + j] += -Heights[i, j];
              
            }
        }
    }

    public void SetMask(float[,] array)
    {
        int centerX = X + width / 2;
        int centerY = Y + width / 2;
        int radius = width / 2;
        for (int i = 0; i < width && i < array.GetLength(0); i++)
        {
            for (int j = 0; j < width && j < array.GetLength(1); j++)
            {
                if (array[i, j] != 1)
                {
                    Vector2 vector = new Vector2(X + i - centerX, Y + j - centerY);
                    if (Math.Sqrt(vector.x * vector.x + vector.y * vector.y) < radius)
                        if (X + i < array.GetLength(0) && Y + j < array.GetLength(1))
                            array[X + i, Y + j] = -1;
                }
            }
        }
    }

    private float[,] SetHeights()
    {
        DiamondSquare diamondSquare = new DiamondSquare(width, width, 2, 0.2f, true);

        float[,] _heights = diamondSquare.DrawPlasma(initBase[0], initBase[1], initBase[2], initBase[3], width, width);
        return _heights;
    }
}
