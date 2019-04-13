using System;
using UnityEngine;

namespace Application
{
    public class Mountain
    {
        public int BaseWidth { get; set; }
        public int Height { get; set; }
        public double Angle { get; set; }
        public float[,] Heights { get; private set; }

        public Mountain(int height, double angle)
        {
            Angle = angle;
            Height = height;
            BaseWidth =(int)(Math.Tan(Angle/2) * 2 * Height);
            Heights = SetHeights();
        }

        private float[,] SetHeights()
        {
            float[,] _heights = new float[BaseWidth, BaseWidth];
            int radius = BaseWidth / 2;
            Vector center = new Vector(radius, radius);
            for (int i = 0; i < BaseWidth; i++)
            {
                for (int j = 0; j < BaseWidth; j++)
                {
                    if (Vector.GetLength(Vector.Add(center, new Vector(i, j))) <= radius)
                        _heights[i, j] = CalculateHeight(i, j);
                    else
                        _heights[i, j] = 0;
                }
            }

            return _heights;
        }

        private float CalculateHeight(int x, int y)
        {
            float xCoord = (float)x / BaseWidth + UnityEngine.Random.Range(0, 1000f);
            float yCoord = (float)y / BaseWidth + UnityEngine.Random.Range(0, 1000f);

            float zCoord = (float)(Math.Cos(Angle) * Math.Sqrt(x * x + y * y) / Math.Sin(Angle));

            return zCoord + Mathf.PerlinNoise(xCoord, yCoord) * 0.2f;
        }

    }

    public class Peak
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
}
