using System;
using UnityEngine;

namespace Application
{
    public class Mountain
    {
        public int X;
        public int Y;
        public int width;

        public float[,] Heights { get; private set; }
        private float[] initBase = new float[4];
        public Mountain(int x, int y, int w, float[] initBase)
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
                        field[X + i, Y + j] += Heights[i, j];
                }
            }
        }

        public void SetMask(float[,] field)
        {

            int centerX = X + width / 2;
            int centerY = Y + width / 2;
            int radius = width / 2;
            for (int i = 0; i < Heights.GetLength(0) && i < field.GetLength(0); i++)
            {
                for (int j = 0; j < Heights.GetLength(1) && j < field.GetLength(1); j++)
                {
                    Vector2 vector = new Vector2(X + i - centerX, Y + j - centerY);
                    if (Math.Sqrt(vector.x * vector.x + vector.y * vector.y) < radius)
                        if (X + i < field.GetLength(0) && Y + j < field.GetLength(1))
                        field[X + i,Y + j] = 1;
                }
            }
        }

        private float[,] SetHeights()
        {
            DiamondSquare diamondSquare = new DiamondSquare(width, width, 1, 0.2f, true);

            float[,] _heights = diamondSquare.DrawPlasma(initBase[0], initBase[1], initBase[2], initBase[3], width, width);
            return _heights;
        }
    }

}
