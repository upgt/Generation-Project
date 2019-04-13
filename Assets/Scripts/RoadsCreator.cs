using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadsCreator : MonoBehaviour
{
    [System.Serializable]
    public class Point
    {
        public int x;
        public int z;
    }

    [System.Serializable] // задаём значения для дорог в инспекторе 
    public class Road // дорога - массив точек с координатами X Z 
    {
        public Point[] points;

        public void DrawOnAlphaMaps(float[,,] alphaMaps, int roadWidth, int roadFlexure, float roadLow, int roadTextureIndex, int texturesCount, float[,] heightMap)
        {
            for (var i = 0; i < points.Length - 1; i++)
            {
                if (points.Length == 1)
                {
                    for (int x = points[0].x - roadWidth; x < points[0].x + roadWidth; x++)
                        for (int z = points[0].z - roadWidth; z < points[0].x + roadWidth; z++)
                            for (int textureIndex = 0; textureIndex < texturesCount; textureIndex++)
                            {
                                // X альфамапы = Z глобальных координат 
                                // Y альфамапы = X глобальных координат 
                                if (textureIndex == roadTextureIndex)
                                    alphaMaps[z, x, textureIndex] = 1;
                                else alphaMaps[z, x, textureIndex] = 0;
                            }
                }


                int deltaX = Mathf.Abs(points[i].x - points[i + 1].x);
                int deltaZ = Mathf.Abs(points[i].z - points[i + 1].z);
                int minX = Mathf.Min(points[i].x, points[i + 1].x);
                int minZ = Mathf.Min(points[i].z, points[i + 1].z);
                int maxX = Mathf.Max(points[i].x, points[i + 1].x);
                int maxZ = Mathf.Max(points[i].z, points[i + 1].z);
                bool fromLeftUnderToRightUpper =
                points[i].x == minX && points[i].z == minZ ||
                points[i + 1].x == minX && points[i + 1].z == minZ;

                if (deltaZ < deltaX) // то дельта Х точно больше нуля 
                    DrawRoad(minX, deltaX, minZ, deltaZ, maxZ, fromLeftUnderToRightUpper, roadWidth, roadFlexure, roadLow, texturesCount, roadTextureIndex, alphaMaps, heightMap, true);
                else DrawRoad(minZ, deltaZ, minX, deltaX, maxX, fromLeftUnderToRightUpper, roadWidth, roadFlexure, roadLow, texturesCount, roadTextureIndex, alphaMaps, heightMap, false);
            }
        }

        private void DrawRoad(int min1, int delta1, int min2, int delta2, int max2, bool fromLeftUnderToRightUpper,
        int roadWidth, int roadFlexure, float roadLow, int texturesCount, int roadTextureIndex, float[,,] alphaMaps, float[,] heightMap, bool isCoord1X) // вынес повторяющийся код в функцию 
        {
            float[,] defaultHeightMap = (float[,])heightMap.Clone();
            for (int coord1 = min1; coord1 <= min1 + delta1; coord1++)
            {
                int currentCoord2 = fromLeftUnderToRightUpper ?
                min2 + (int)(delta2 * ((float)(coord1 - min1 + 1) / delta1)) + (int)(Mathf.Sin(2 * Mathf.PI * ((float)(coord1 - min1 + 1) / delta1)) * roadFlexure) :
                max2 - (int)(delta2 * ((float)(coord1 - min1 + 1) / delta1)) - (int)(Mathf.Sin(2 * Mathf.PI * ((float)(coord1 - min1 + 1) / delta1)) * roadFlexure);

                for (int coord2 = currentCoord2 - roadWidth; coord2 < currentCoord2 + roadWidth; coord2++)
                {
                    // отрисовка дороги 
                    for (int textureIndex = 0; textureIndex < texturesCount; textureIndex++) // цикл по текстурам точки 
                    {
                        // X альфамапы = Z глобальных координат 
                        // Y альфамапы = X глобальных координат 

                        if (isCoord1X)
                        {
                            for (int c2 = coord2 * 2; c2 < coord2 * 2 + 2; c2++) // разрешение меша 256, а разрешение альфа текстур 512, 
                                                                                 // поэтому coord1 coord2 для высот, а с1 c2 для альфа текстур.
                                for (int c1 = coord1 * 2; c1 < coord1 * 2 + 2; c1++)
                                {
                                    if (textureIndex == roadTextureIndex)
                                        alphaMaps[c2, c1, textureIndex] = 1;
                                    else alphaMaps[c2, c1, textureIndex] = 0;
                                }
                            //heightMap[coord2, coord1] -= roadLow; // тест, стереть строку 
                        }
                        else
                        {
                            for (int c2 = coord2 * 2; c2 < coord2 * 2 + 2; c2++)
                                for (int c1 = coord1 * 2; c1 < coord1 * 2 + 2; c1++)
                                {
                                    if (textureIndex == roadTextureIndex)
                                        alphaMaps[c1, c2, textureIndex] = 1;
                                    else alphaMaps[c1, c2, textureIndex] = 0;
                                }
                            //heightMap[coord1, coord2] -= roadLow; // тест, стереть строку 
                        }
                    }

                    // сглаживание рельефа дороги 
                    float mediumHeight = 0;
                    int n = 0;
                    for (int j = coord1 - roadWidth * 2; j < coord1 + roadWidth * 2; j++)
                        for (int k = coord2 - roadWidth * 2; k < coord2 + roadWidth * 2; k++)
                        {
                            mediumHeight += (isCoord1X ? defaultHeightMap[k, j] :

                            defaultHeightMap[j, k]);
                            n += 1;
                        }
                    mediumHeight = mediumHeight / n - roadLow; // итоговая средняя высота 
                    if (isCoord1X)
                        heightMap[coord2, coord1] = mediumHeight;
                    else heightMap[coord1, coord2] = mediumHeight;
                }

                //defaultHeightMap = (float[,])heightMap.Clone(); 

                // сглаживание рельефа вокруг дороги (ниже) 
                for (int coord2 = currentCoord2 - roadWidth * 2; coord2 < currentCoord2 - roadWidth; coord2++)
                {
                    float mediumHeight = 0;
                    int n = 0;
                    for (int j = coord1 - roadWidth * 2; j < coord1 + roadWidth * 2; j++)
                        for (int k = coord2 - roadWidth * 2; k < coord2 + roadWidth * 2; k++)
                        {
                            mediumHeight += (isCoord1X ? defaultHeightMap[k, j] : defaultHeightMap[j, k]);
                            n += 1;
                        }
                    mediumHeight = mediumHeight / n - roadLow; // итоговая средняя высота 

                    if (isCoord1X)
                        //heightMap[coord2, coord1] = heightMap[coord2, coord1] * 1 + (mediumHeight - roadLow) * 2 * (Mathf.Sin(2 * Mathf.PI)); 
                        heightMap[coord2, coord1] = defaultHeightMap[coord2, coord1] + (mediumHeight - defaultHeightMap[coord2, coord1]) * (1 + Mathf.Cos(Mathf.PI * (coord2 - (currentCoord2 - roadWidth * 2)) / roadWidth) * -1) / 2;
                    else //heightMap[coord1, coord2] = mediumHeight - roadLow; 
                        heightMap[coord1, coord2] = defaultHeightMap[coord1, coord2] + (mediumHeight - defaultHeightMap[coord1, coord2]) * (1 + Mathf.Cos(Mathf.PI * (coord2 - (currentCoord2 - roadWidth * 2)) / roadWidth) * -1) / 2;
                }

                // сглаживание рельефа вокруг дороги (выше) 
                for (int coord2 = currentCoord2 + roadWidth; coord2 < currentCoord2 + roadWidth * 2; coord2++)
                {
                    float mediumHeight = 0;
                    int n = 0;
                    for (int j = coord1 - roadWidth * 2; j < coord1 + roadWidth * 2; j++)
                        for (int k = coord2 - roadWidth * 2; k < coord2 + roadWidth * 2; k++)
                        {
                            mediumHeight += (isCoord1X ? defaultHeightMap[k, j] : defaultHeightMap[j, k]);
                            n += 1;
                        }
                    mediumHeight = mediumHeight / n - roadLow; // итоговая средняя высота 

                    if (isCoord1X)
                        //heightMap[coord2, coord1] = heightMap[coord2, coord1] * 1 + (mediumHeight - roadLow) * 2 * (Mathf.Sin(2 * Mathf.PI)); 
                        heightMap[coord2, coord1] = defaultHeightMap[coord2, coord1] + (mediumHeight - defaultHeightMap[coord2, coord1]) * (1 + Mathf.Cos(Mathf.PI * (coord2 - (currentCoord2 + roadWidth)) / roadWidth)) / 2;
                    else //heightMap[coord1, coord2] = mediumHeight - roadLow; 
                        heightMap[coord1, coord2] = defaultHeightMap[coord1, coord2] + (mediumHeight - defaultHeightMap[coord1, coord2]) * (1 + Mathf.Cos(Mathf.PI * (coord2 - (currentCoord2 + roadWidth)) / roadWidth)) / 2;
                }
            }
        }
    }

    public int roadTextureIndex; //индекс текстуры дороги в инспекторе (отсчёт с нуля слева направо) 
    public int roadWidth = 5; //ширина дороги/2 
    public int roadFlexure = 20; //кривизна дороги 
    public float roadLow = 0.04f; //понижение дороги 
    public Road[] roads;


    // Start is called before the first frame update 
    void Start()
    {
        TerrainData terrainData = GetComponent<Terrain>().terrainData;
        var alphaMaps = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);
        var heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);

        foreach (Road road in roads)
            road.DrawOnAlphaMaps(alphaMaps, roadWidth, roadFlexure, roadLow, roadTextureIndex, terrainData.splatPrototypes.Length, heightMap);
        terrainData.SetAlphamaps(0, 0, alphaMaps);
        terrainData.SetHeights(0, 0, heightMap);
    }

    // Update is called once per frame 
    void Update()
    {

    }
}