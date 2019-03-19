using System;
using UnityEngine;

namespace Application
{
    public class Mountain
    {
        public int BaseWidth { get; set; }
        public int Height { get; set; }

        //Высоты, собственно, горы
        public float[,] Heights { get; private set; }
        private float heightStep;

        public Mountain(int height, int width)
        {
            BaseWidth = width % 2 == 0 ? width + 1 : width;     //гора с пиком, ширина всегда будет нечетным числом
            Height = height;
            heightStep = (float)Height / (BaseWidth / 2);
            Heights = new float[BaseWidth, BaseWidth];
        }

        private float[,] SetHeights(int width)
        {
            float[,] _heights = new float[width, width];
            int center = BaseWidth / 2 + 1;
            int stepJ = 0;
            int stepI = 0;
            float localHeight = Height;

            for (int i = center; i > 0; i--)
            {
                for (int j = center - stepJ; j <= center + stepJ; j++)
                {
                    _heights[i, j] = CalculateHeight(i, j, localHeight);
                    _heights[i + stepI, j] = CalculateHeight(i, j, localHeight);
                    localHeight -= heightStep;
                }
                stepI += 2;
                stepJ++;
            }

            stepI = 0; stepJ = 0;
            localHeight = Height;
            for (int j = center; j > 0; j--)
            {
                for (int i = center - stepJ; i <= center + stepJ; i++)
                {
                    _heights[j, i] = CalculateHeight(j, i, localHeight);
                    _heights[j + stepI, i] = CalculateHeight(j, i, localHeight);
                    localHeight -= heightStep;
                }
                stepJ += 2;
                stepI++;
            }

            return _heights;
        }

        private float CalculateHeight(int x, int y, float height)
        {
            float xCoord = (float)x / BaseWidth * heightStep + UnityEngine.Random.Range(0, 1000f);
            float yCoord = (float)y / BaseWidth * heightStep + UnityEngine.Random.Range(0, 1000f);
          

            return height - Mathf.PerlinNoise(xCoord, yCoord);
        }
    }

    public class Peak
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
}
