using System.Collections;
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
    public float roadLow = 0.015f; //понижение дороги 
    public bool randomRoads = false; //создание случайных дорог вместо определенных
    public bool tracks = true; //колеи дорог
    private float tracksLow = 0.0055f;
    public Road[] roads;
    
    public void MakeRoads(Road[] roads)
    {
        TerrainData terrainData = GetComponent<Terrain>().terrainData;
        var alphaMaps = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);
        var heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);
        var texturesCount = terrainData.splatPrototypes.Length;

        CleanAlphaMaps(alphaMaps, terrainData.alphamapWidth, terrainData.alphamapHeight, texturesCount);

        foreach (Road road in roads)
        {
            var points = road.points;
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
                    float currentCoord2; // вторая координата точки учитывая отклонение
                    bool isCoord1X; // если true, то min1 = minX, delta1 = deltaX и т.д.. Если false, то min1 = minY и т.д.

                    // рассчёты трёх предыдущих переменных:
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
                            float deltaCoord2 = deltaZ * ((float)(coord1 - minX) / deltaX); // дельта второй координаты точки на отрезке между point i и point i+1 (расстояние от мин/макс)
                            float flexure = Mathf.Sin(2 * Mathf.PI * ((float)(coord1 - minX) / deltaX)) * roadFlexure; // отклонение (изгиб) дороги по синусоиде
                            currentCoord2 = fromLeftUnderToRightUpper ? // вторая координата точки учитывая отклонение
                            minZ + deltaCoord2 + flexure :
                            maxZ - deltaCoord2 - flexure;
                        }
                        else
                        {
                            isCoord1X = false;
                            coord1 = a + minZ;
                            float deltaCoord2 = deltaX * ((float)(coord1 - minZ) / deltaZ); // дельта второй координаты точки на отрезке между point i и point i+1 (расстояние от мин/макс)
                            float flexure = Mathf.Sin(2 * Mathf.PI * ((float)(coord1 - minZ) / deltaZ)) * roadFlexure; // отклонение (изгиб) дороги по синусоиде
                            currentCoord2 = fromLeftUnderToRightUpper ? // вторая координата точки учитывая отклонение
                            minX + deltaCoord2 + flexure :
                            maxX - deltaCoord2 - flexure;
                        }
                    }
                    

                    for (int b = -roadWidth; b < roadWidth; b++)
                    {
                        int coord2 = b + (int)currentCoord2;
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
                        {
                            float mediumRoadHeight = 0;
                            int n = 0;
                            for (int j = coord1 - roadWidth * 2; j < coord1 + roadWidth * 2; j++)
                                for (int k = coord2 - roadWidth * 2; k < coord2 + roadWidth * 2; k++)
                                {
                                    mediumRoadHeight += (isCoord1X ?
                                        defaultHeightMap[k, j] :
                                        defaultHeightMap[j, k]);
                                    n++;
                                }
                            mediumRoadHeight = mediumRoadHeight / n - roadLow; // итоговая средняя высота, учитывая понижение дороги
                            if (tracks) //если дороги с колеями
                                mediumRoadHeight -= tracksLow *(1 - Mathf.Abs(Mathf.Cos((Mathf.PI * b + currentCoord2 - (int)currentCoord2)/ roadWidth))); //снижение - колеи дорог
                            if (isCoord1X)
                                heightMap[coord2, coord1] = mediumRoadHeight;
                            else heightMap[coord1, coord2] = mediumRoadHeight;
                        }
                    }

                    // сглаживание рельефа вокруг дороги (ниже) 
                    for (int c = 0; c < roadWidth; c++)
                    {
                        int coord2 = (int)currentCoord2 - roadWidth * 2 + c;
                        float coord2f = currentCoord2 - roadWidth * 2 + c; //значение float для исправления ребристости дороги
                        float mediumRoadHeight = 0;
                        int n = 0;
                        for (int j = coord1 - roadWidth * 2; j < coord1 + roadWidth * 2; j++)
                            for (int k = coord2 - roadWidth * 2; k < coord2 + roadWidth * 2; k++)
                            {
                                mediumRoadHeight += (isCoord1X ? 
                                    defaultHeightMap[k, j] : 
                                    defaultHeightMap[j, k]);
                                n++;
                            }
                        mediumRoadHeight = mediumRoadHeight / n - roadLow; // итоговая средняя высота, учитывая понижение дороги
                        float deltaHeight; // разница по высоте между дорогой и оригинальной землёй
                        float coef = (1 + Mathf.Cos(Mathf.PI * (coord2f - (currentCoord2 - roadWidth * 2)) / roadWidth) * -1) / 2; //коэфф от 0 до 1, который сглаживает рельеф рядом с дорогой

                        if (isCoord1X)
                        {
                            deltaHeight = mediumRoadHeight - defaultHeightMap[coord2, coord1];
                            heightMap[coord2, coord1] = defaultHeightMap[coord2, coord1] + deltaHeight * coef;
                        }
                        else
                        {
                            deltaHeight = mediumRoadHeight - defaultHeightMap[coord1, coord2];
                            heightMap[coord1, coord2] = defaultHeightMap[coord1, coord2] + deltaHeight * coef;
                        }
                    }

                    // сглаживание рельефа вокруг дороги (выше) 
                    for (int d = 0; d < roadWidth; d++)
                    {
                        int coord2 = (int)currentCoord2 + roadWidth + d;
                        float coord2f = currentCoord2 + roadWidth + d;
                        float mediumRoadHeight = 0;
                        int n = 0;
                        for (int j = coord1 - roadWidth * 2; j < coord1 + roadWidth * 2; j++)
                            for (int k = coord2 - roadWidth * 2; k < coord2 + roadWidth * 2; k++)
                            {
                                mediumRoadHeight += (isCoord1X ? 
                                    defaultHeightMap[k, j] : 
                                    defaultHeightMap[j, k]);
                                n++;
                            }
                        mediumRoadHeight = mediumRoadHeight / n - roadLow; // итоговая средняя высота, учитывая понижение дороги
                        float deltaHeight; // разница по высоте между дорогой и оригинальной землёй
                        float coef = (1 + Mathf.Cos(Mathf.PI * (coord2f - (currentCoord2 + roadWidth)) / roadWidth)) / 2; //коэфф от 0 до 1, который сглаживает рельеф рядом с дорогой

                        if (isCoord1X)
                        {
                            deltaHeight = mediumRoadHeight - defaultHeightMap[coord2, coord1];
                            heightMap[coord2, coord1] = defaultHeightMap[coord2, coord1] + deltaHeight * coef;
                        }
                        else
                        {
                            deltaHeight = mediumRoadHeight - defaultHeightMap[coord1, coord2];
                            heightMap[coord1, coord2] = defaultHeightMap[coord1, coord2] + deltaHeight * coef;
                        }
                    }
                }
            }
        }
        terrainData.SetAlphamaps(0, 0, alphaMaps);
        terrainData.SetHeights(0, 0, heightMap);
    }

    public Road[] GetRandomRoads()
    {
        var roads = new Road[Random.Range(1, 5)]; //от 1 до 4 дорог
        for(var road=0; road<roads.Length;road++)
        {
            roads[road] = new Road { };
            roads[road].points = new Point[Random.Range(2, 5)]; //от 2 до 4 точек у каждой дороги
            for (var point = 0; point < roads[road].points.Length; point++)
            {
                roads[road].points[point] = new Point { };
                roads[road].points[point].x = Random.Range(30, 994);
                roads[road].points[point].z = Random.Range(30, 994);
            }
        }
        return roads;
    }

    public void CleanAlphaMaps(float[,,] alphaMaps, int alphamapWidth, int alphamapHeight, int texturesCount) //очистить альфамапы от текстуры дороги
    {
        for (var x = 0; x < alphamapWidth; x++)
            for (var y = 0; y < alphamapHeight; y++)
                for (int textureIndex = 0; textureIndex < texturesCount; textureIndex++)
                    if (textureIndex == 0)
                        alphaMaps[x, y, textureIndex] = 1f; //закрашиваем всю карту нулевой текстурой (травой)
                    else alphaMaps[x, y, textureIndex] = 0f;
    }

    // Start is called before the first frame update 
    void Start()
    {
        if (randomRoads)
            MakeRoads(GetRandomRoads());
        else MakeRoads(roads);
    }

    // Update is called once per frame 
    void Update()
    {

    }
}