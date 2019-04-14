﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
}

public class RoadsCreator : MonoBehaviour
{
    public int roadTextureIndex; //индекс текстуры дороги в инспекторе (отсчёт с нуля слева направо) 
    public int roadWidth = 5; //ширина дороги/2 
    public int roadFlexure = 20; //кривизна дороги 
    public float roadLow = 0.04f; //понижение дороги 
    //public bool randomRoads = false;
    public Road[] roads;
    
    public void MakeRoads()
    {
        TerrainData terrainData = GetComponent<Terrain>().terrainData;
        var alphaMaps = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);
        var heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);

        foreach (Road road in roads)
        {
            var points = road.points;
            var texturesCount = terrainData.splatPrototypes.Length;
            for (var i = 0; i < points.Length - 1; i++)
            {
                if (points.Length == 1) // если дорога состоит из одной точки
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
                    return;
                }
                
                
                float[,] defaultHeightMap = (float[,])heightMap.Clone();

                int deltaX = Mathf.Abs(points[i].x - points[i + 1].x);
                int deltaZ = Mathf.Abs(points[i].z - points[i + 1].z);
                int delta1;

                if (deltaZ < deltaX) // то дельта Х точно больше нуля 
                    delta1 = deltaX;
                else
                    delta1 = deltaZ;

                for (int a = 0; a <= delta1; a++)
                {
                    int coord1; // первая координата точки на отрезке между point i и point i+1
                    int currentCoord2; // вторая координата точки учитывая отклонение
                    bool isCoord1X;
                    {
                        int minX = Mathf.Min(points[i].x, points[i + 1].x);
                        int minZ = Mathf.Min(points[i].z, points[i + 1].z);
                        int maxX = Mathf.Max(points[i].x, points[i + 1].x);
                        int maxZ = Mathf.Max(points[i].z, points[i + 1].z);

                        bool fromLeftUnderToRightUpper =
                        points[i].x == minX && points[i].z == minZ ||
                        points[i + 1].x == minX && points[i + 1].z == minZ;

                        if (deltaZ < deltaX)
                        {
                            isCoord1X = true;
                            coord1 = a + minX;
                            int deltaCoord2 = (int)(deltaZ * ((float)(coord1 - minX) / deltaX)); // дельта второй координаты точки на отрезке между point i и point i+1 (расстояние от мин/макс)
                            int flexure = (int)(Mathf.Sin(2 * Mathf.PI * ((float)(coord1 - minX) / deltaX)) * roadFlexure); // отклонение (изгиб) дороги по синусоиде
                            currentCoord2 = fromLeftUnderToRightUpper ? // вторая координата точки учитывая отклонение
                            minZ + deltaCoord2 + flexure :
                            maxZ - deltaCoord2 - flexure;
                        }
                        else
                        {
                            isCoord1X = false;
                            coord1 = a + minZ;
                            int deltaCoord2 = (int)(deltaX * ((float)(coord1 - minZ) / deltaZ)); // дельта второй координаты точки на отрезке между point i и point i+1 (расстояние от мин/макс)
                            int flexure = (int)(Mathf.Sin(2 * Mathf.PI * ((float)(coord1 - minZ) / deltaZ)) * roadFlexure); // отклонение (изгиб) дороги по синусоиде
                            currentCoord2 = fromLeftUnderToRightUpper ? // вторая координата точки учитывая отклонение
                            minX + deltaCoord2 + flexure :
                            maxX - deltaCoord2 - flexure;
                        }
                    }
                    

                    for (int b = -roadWidth; b < roadWidth; b++)
                    {
                        int coord2 = b + currentCoord2;
                        // отрисовка дороги 
                        for (int textureIndex = 0; textureIndex < texturesCount; textureIndex++) // цикл по текстурам точки 
                        {
                            // X альфамапы = Z глобальных координат 
                            // Y альфамапы = X глобальных координат 
                            if (isCoord1X)
                            {
                                // разрешение меша 1024, а разрешение альфа текстур 512, 
                                // поэтому берём координату высоты / 2
                                if (textureIndex == roadTextureIndex)
                                    alphaMaps[coord2 / 2, coord1 / 2, textureIndex] = 1;
                                else alphaMaps[coord2 / 2, coord1 / 2, textureIndex] = 0;
                            }
                            else
                            {
                                if (textureIndex == roadTextureIndex)
                                    alphaMaps[coord1 / 2, coord2 / 2, textureIndex] = 1;
                                else alphaMaps[coord1 / 2, coord2 / 2, textureIndex] = 0;
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
                            heightMap[coord2, coord1] = defaultHeightMap[coord2, coord1] + (mediumHeight - defaultHeightMap[coord2, coord1]) * (1 + Mathf.Cos(Mathf.PI * (coord2 - (currentCoord2 - roadWidth * 2)) / roadWidth) * -1) / 2;
                        else
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
                            heightMap[coord2, coord1] = defaultHeightMap[coord2, coord1] + (mediumHeight - defaultHeightMap[coord2, coord1]) * (1 + Mathf.Cos(Mathf.PI * (coord2 - (currentCoord2 + roadWidth)) / roadWidth)) / 2;
                        else
                            heightMap[coord1, coord2] = defaultHeightMap[coord1, coord2] + (mediumHeight - defaultHeightMap[coord1, coord2]) * (1 + Mathf.Cos(Mathf.PI * (coord2 - (currentCoord2 + roadWidth)) / roadWidth)) / 2;
                    }
                }
            }
        }
        terrainData.SetAlphamaps(0, 0, alphaMaps);
        terrainData.SetHeights(0, 0, heightMap);
    }

    // Start is called before the first frame update 
    void Start()
    {
        //if(randomRoads)
        MakeRoads();
    }

    // Update is called once per frame 
    void Update()
    {

    }
}