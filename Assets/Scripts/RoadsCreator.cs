using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

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
    private Ground_Controiler textures;
    private TerrainGenerator terrainGenerator;
    public int roadWidth = 5; //ширина дороги/2 
    private int roadFlexure = 20; //кривизна дороги 
    public float roadLow = 0.015f; //понижение дороги 
    public bool randomRoads = false; //создание случайных дорог вместо определенных
    public bool tracks = true; //колеи дорог
    private float tracksLow;
    public int amountOfRandomRoads = 3; //количество рандомных дорог
    private int amountOfPointsRandomRoads; //количество точек на каждую рандомную дорогу
    public Road[] roads;
    public float[,] treePlaceInfo; //информация о возможности рассадки деревьев, 0 - нельзя
    private int roadMinLength;
    Deleg func;
    //переменные которые ограничивают крутость дороги, прохождение всквозь горы
    private float tooHighMedium = 10000000f; //насколько сильным может быть перепад высот рядом с дорогой
    private float tooHighCenter = 10000000f; //насколько дорога может меняться по высоте вдоль центральной линии
    private float tooHighLeftRight = 100000000f; //насколько сильно может отличаться высота левого и бравого бока дороги

    private float[,] groundInfo;
    private float roadLowCoef;
    //private float tracksLowCoef;

    TerrainData terrainData;
    Terrain terrain;

    public float[,] roadMask; //где 1f - там НЕ рисуется дорога, где 0f - рисуется.
    bool CheckEqual(float f1, float f2)
    {
        return f1 == f2;
    }

    public void MakeRoads(Road[] roads)
    {
        terrain = GetComponent<Terrain>();
        terrainData = terrain.terrainData;
        func = new Deleg(CheckEqual);
        var heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);
        float[,] defaultHeightMap = (float[,])heightMap.Clone();

        treePlaceInfo = (float[,])heightMap.Clone();
        for (int j = 0; j < terrainData.heightmapWidth; j++)
            for (int k = 0; k < terrainData.heightmapHeight; k++)
                treePlaceInfo[j, k] = 1;

        roadMask = (float[,])treePlaceInfo.Clone();

        int tracksLen = 0; //для непостоянности колеи

        //groundInfo = terrainGenerator.globalMaskMap;//если меньше 1f, то это земля.
        
        //groundInfo = TerrainGenerator.CreateMask(groundInfo, 0, func);
        

        roadMinLength = roadWidth * 4;

        foreach (Road road in roads)
        {
            //try
            var points = road.points;
            for (var i = 0; i < points.Length - 1; i++)
            {
                int deltaX = Mathf.Abs(points[i].x - points[i + 1].x);
                int deltaZ = Mathf.Abs(points[i].z - points[i + 1].z);
                int delta1;

                if (deltaZ < deltaX) // то дельта Х точно больше нуля 
                    delta1 = deltaX;
                else
                    delta1 = deltaZ;

                //переменные для отслеживания дороги под крутым углом
                float centerHeight = -1;
                float leftHeight = 0;
                float rightHeight = 0;

                bool tooHigh = false; // верно, если дорога идёт под крутым углом или слишком крутая по бокам
                bool roadThrowWater = false; // верно, если дорога идёт всквозь водный участок
                bool tooShort = false; // верно, если дорога слишком короткая

                Point startPoint = points[i];

                //проверка чтобы отсечь короткие отрезки дороги
                if (delta1 < roadMinLength)
                    continue;

                //корректировка кривизны дороги в зависимости от её длины
                int actualRoadFlexure = roadFlexure;
                if (delta1 < 100)
                    actualRoadFlexure = (int)(roadFlexure * (float)delta1 / 100);
                if (delta1 < 80)
                    actualRoadFlexure /= 2;

                //цикл для проверки - будет ли отрезок дороги коротким из-за горы
                for (int a = 0; a < roadMinLength; a++)
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
                            if (coord1 < 0 || coord1 >= terrainData.heightmapHeight)
                                continue;
                            float deltaCoord2 = deltaZ * ((float)(coord1 - minX) / deltaX); // дельта второй координаты точки на отрезке между point i и point i+1 (расстояние от мин/макс)
                            float flexure = Mathf.Sin(2 * Mathf.PI * ((float)(coord1 - minX) / deltaX)) * actualRoadFlexure; // отклонение (изгиб) дороги по синусоиде
                            currentCoord2 = fromLeftUnderToRightUpper ? // вторая координата точки учитывая отклонение
                            minZ + deltaCoord2 + flexure :
                            maxZ - deltaCoord2 - flexure;
                            if (a == 0)
                            {
                                if (coord1 == points[i].x && (int)currentCoord2 == points[i].z)
                                    startPoint = points[i];
                                else startPoint = points[i + 1];
                            }
                        }
                        else
                        {
                            isCoord1X = false;
                            coord1 = a + minZ;
                            if (coord1 < 0 || coord1 >= terrainData.heightmapWidth)
                                continue;
                            float deltaCoord2 = deltaX * ((float)(coord1 - minZ) / deltaZ); // дельта второй координаты точки на отрезке между point i и point i+1 (расстояние от мин/макс)
                            float flexure = Mathf.Sin(2 * Mathf.PI * ((float)(coord1 - minZ) / deltaZ)) * actualRoadFlexure; // отклонение (изгиб) дороги по синусоиде
                            currentCoord2 = fromLeftUnderToRightUpper ? // вторая координата точки учитывая отклонение
                            minX + deltaCoord2 + flexure :
                            maxX - deltaCoord2 - flexure;
                            if (a == 0)
                            {
                                if (coord1 == points[i].z && (int)currentCoord2 == points[i].x)
                                    startPoint = points[i];
                                else startPoint = points[i + 1];
                            }
                        }
                    }

                    // вычисления для tooHigh
                    {
                        float minHeight = -1f;
                        float maxHeight = -1f;
                        for (int k = -roadWidth * 2; k < roadWidth * 2; k++)
                            for (int j = -roadWidth * 2; j < roadWidth * 2; j++)
                            {
                                try
                                {
                                    if (isCoord1X)
                                    {
                                        if (defaultHeightMap[(int)currentCoord2 + k, coord1 + j] > maxHeight)
                                            maxHeight = defaultHeightMap[(int)currentCoord2 + k, coord1 + j];
                                        if (minHeight == -1f || defaultHeightMap[(int)currentCoord2 + k, coord1 + j] < minHeight)
                                            minHeight = defaultHeightMap[(int)currentCoord2 + k, coord1 + j];
                                        if (defaultHeightMap[(int)currentCoord2 + k, coord1 + j] < 0.4f || defaultHeightMap[(int)currentCoord2 + k, coord1 + j] > 0.6f)
                                            roadThrowWater = true;
                                    }
                                    else
                                    {
                                        if (defaultHeightMap[coord1 + k, (int)currentCoord2 + j] > maxHeight)
                                            maxHeight = defaultHeightMap[coord1 + k, (int)currentCoord2 + j];
                                        if (minHeight == -1f || defaultHeightMap[coord1 + k, (int)currentCoord2 + j] < minHeight)
                                            minHeight = defaultHeightMap[coord1 + k, (int)currentCoord2 + j];
                                        if (defaultHeightMap[coord1 + k, (int)currentCoord2 + j] < 0.4f || defaultHeightMap[coord1 + k, (int)currentCoord2 + j] > 0.6f)
                                            roadThrowWater = true;
                                    }
                                }
                                catch (System.IndexOutOfRangeException)
                                {
                                }
                            }
                        if (Mathf.Abs(maxHeight - minHeight) > tooHighMedium)
                            tooHigh = true;
                    }
                    for (int b = -roadWidth; b < roadWidth; b++)
                    {
                        int coord2 = b + (int)currentCoord2;

                        // сглаживание рельефа дороги 
                        {
                            float mediumRoadHeight = 0;
                            int n = 0;
                            for (int j = coord1 - roadWidth * 2; j < coord1 + roadWidth * 2; j++)
                                for (int k = coord2 - roadWidth * 2; k < coord2 + roadWidth * 2; k++)
                                {
                                    try
                                    {
                                        mediumRoadHeight += (isCoord1X ?
                                            defaultHeightMap[k, j] :
                                            defaultHeightMap[j, k]);
                                        n++;
                                    }
                                    catch (System.IndexOutOfRangeException)
                                    {
                                    }
                                }
                            mediumRoadHeight = mediumRoadHeight / n - roadLowCoef; // итоговая средняя высота, учитывая понижение дороги
                            if (b == -roadWidth)
                                leftHeight = mediumRoadHeight;
                            if (b == 0)
                            {
                                if (a != 0 && Mathf.Abs(mediumRoadHeight - centerHeight) > tooHighCenter)
                                {
                                    centerHeight = mediumRoadHeight;
                                    tooHigh = true;
                                    break;
                                }
                                centerHeight = mediumRoadHeight;
                            }
                            if (b == roadWidth - 1)
                            {
                                rightHeight = mediumRoadHeight;
                                if (Mathf.Abs(leftHeight - rightHeight) > tooHighLeftRight)
                                {
                                    tooHigh = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (tooHigh || roadThrowWater)// тут если дорога слишком крутая, она обрывается слишком рано, поэтому она короткая
                    {
                        tooShort = true;
                        break; // не продолжает дальнейшую проверку
                    }
                }

                if (tooShort) //если отрезок дороги короткий, переходим к след. отрезку
                    continue;

                //обработка стартовой point
                MakePoint(startPoint, heightMap, defaultHeightMap);

                //цикл отрисовки
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
                            if (coord1 < 0 || coord1 >= terrainData.heightmapHeight)
                                continue;
                            float deltaCoord2 = deltaZ * ((float)(coord1 - minX) / deltaX); // дельта второй координаты точки на отрезке между point i и point i+1 (расстояние от мин/макс)
                            float flexure = Mathf.Sin(2 * Mathf.PI * ((float)(coord1 - minX) / deltaX)) * actualRoadFlexure; // отклонение (изгиб) дороги по синусоиде
                            currentCoord2 = fromLeftUnderToRightUpper ? // вторая координата точки учитывая отклонение
                            minZ + deltaCoord2 + flexure :
                            maxZ - deltaCoord2 - flexure;
                        }
                        else
                        {
                            isCoord1X = false;
                            coord1 = a + minZ;
                            if (coord1 < 0 || coord1 >= terrainData.heightmapWidth)
                                continue;
                            float deltaCoord2 = deltaX * ((float)(coord1 - minZ) / deltaZ); // дельта второй координаты точки на отрезке между point i и point i+1 (расстояние от мин/макс)
                            float flexure = Mathf.Sin(2 * Mathf.PI * ((float)(coord1 - minZ) / deltaZ)) * actualRoadFlexure; // отклонение (изгиб) дороги по синусоиде
                            currentCoord2 = fromLeftUnderToRightUpper ? // вторая координата точки учитывая отклонение
                            minX + deltaCoord2 + flexure :
                            maxX - deltaCoord2 - flexure;
                        }
                    }

                    // вычисления для tooHigh
                    {
                        float minHeight = -1f;
                        float maxHeight = -1f;
                        for (int k = -roadWidth * 2; k < roadWidth * 2; k++)
                            for (int j = -roadWidth * 2; j < roadWidth * 2; j++)
                            {
                                try
                                {
                                    if (isCoord1X)
                                    {
                                        if (defaultHeightMap[(int)currentCoord2 + k, coord1 + j] > maxHeight)
                                            maxHeight = defaultHeightMap[(int)currentCoord2 + k, coord1 + j];
                                        if (minHeight == -1f || defaultHeightMap[(int)currentCoord2 + k, coord1 + j] < minHeight)
                                            minHeight = defaultHeightMap[(int)currentCoord2 + k, coord1 + j];
                                        if (defaultHeightMap[(int)currentCoord2 + k, coord1 + j] < 0.4f || defaultHeightMap[(int)currentCoord2 + k, coord1 + j] > 0.6f)
                                            roadThrowWater = true;
                                    }
                                    else
                                    {
                                        if (defaultHeightMap[coord1 + k, (int)currentCoord2 + j] > maxHeight)
                                            maxHeight = defaultHeightMap[coord1 + k, (int)currentCoord2 + j];
                                        if (minHeight == -1f || defaultHeightMap[coord1 + k, (int)currentCoord2 + j] < minHeight)
                                            minHeight = defaultHeightMap[coord1 + k, (int)currentCoord2 + j];
                                        if (defaultHeightMap[coord1 + k, (int)currentCoord2 + j] < 0.4f || defaultHeightMap[coord1 + k, (int)currentCoord2 + j] > 0.6f)
                                            roadThrowWater = true;
                                    }
                                }
                                catch (System.IndexOutOfRangeException)
                                {
                                }
                            }
                        if (Mathf.Abs(maxHeight - minHeight) > tooHighMedium)
                            tooHigh = true;
                    }

                    if (tooHigh || roadThrowWater)
                    {
                        MakePoint(new Point
                        {
                            x = isCoord1X ? coord1 : (int)currentCoord2,
                            z = isCoord1X ? (int)currentCoord2 : coord1
                        },
                            heightMap, defaultHeightMap);
                        if(i!=0 &&i+1<points.Length-1)
                        {
                            if (points[i + 1] == startPoint)
                            {
                                points[i].x = isCoord1X ? coord1 : (int)currentCoord2;
                                points[i].z = isCoord1X ? (int)currentCoord2 : coord1;
                            }
                            else
                            {
                                points[i + 1].x = isCoord1X ? coord1 : (int)currentCoord2;
                                points[i + 1].z = isCoord1X ? (int)currentCoord2 : coord1;
                            }
                        }
                        break; // не продолжает создание дороги всквозь горы/воду
                    }

                    for (int b = -roadWidth; b < roadWidth; b++)
                    {
                        int coord2 = b + (int)currentCoord2;
                        if(isCoord1X)
                        {
                            if (coord2 < 0 || coord2 >= terrainData.heightmapWidth)
                                continue;
                        }
                        else
                        {
                            if (coord2 < 0 || coord2 >= terrainData.heightmapHeight)
                                continue;
                        }
                        // сглаживание рельефа дороги 
                        float mediumRoadHeight = 0;
                        int n = 0;
                        for (int j = coord1 - roadWidth * 2; j < coord1 + roadWidth * 2; j++)
                            for (int k = coord2 - roadWidth * 2; k < coord2 + roadWidth * 2; k++)
                            {
                                try
                                {
                                    if(isCoord1X)
                                    {
                                        if (defaultHeightMap[k, j] != 0)
                                        {
                                            mediumRoadHeight += defaultHeightMap[k, j];
                                            n++;
                                        }
                                    }
                                    else
                                    {
                                        if(defaultHeightMap[j, k] !=0)
                                        {
                                            mediumRoadHeight += defaultHeightMap[j, k];
                                            n++;
                                        }
                                    }
                                }
                                catch (System.IndexOutOfRangeException)
                                {
                                }
                            }
                        mediumRoadHeight = mediumRoadHeight / n - roadLowCoef; // итоговая средняя высота, учитывая понижение дороги
                        if (b == -roadWidth)
                            leftHeight = mediumRoadHeight;
                        if (b == 0)
                        {
                            if (a != 0 && Mathf.Abs(mediumRoadHeight - centerHeight) > tooHighCenter)
                            {
                                centerHeight = mediumRoadHeight;
                                tooHigh = true;
                                continue;
                            }
                            centerHeight = mediumRoadHeight;
                        }
                        if (b == roadWidth - 1)
                        {
                            rightHeight = mediumRoadHeight;
                            if (Mathf.Abs(leftHeight - rightHeight) > tooHighLeftRight)
                            {
                                tooHigh = true;
                                continue;
                            }
                        }
                        if (tracks) //если дороги с колеями
                        {
                            //по ширине дороги делает изгиб под две колеи
                            float tracksCurveCoef = 1 - Mathf.Abs(Mathf.Cos((Mathf.PI * b + currentCoord2 - (int)currentCoord2) / roadWidth));
                            //непостоянность колеи
                            float tracksSinCoef = Mathf.Abs(Mathf.Sin(Mathf.PI * tracksLen / 50));
                            mediumRoadHeight -= tracksLow * tracksCurveCoef * tracksSinCoef; //снижение - колеи дорог
                        }
                        if (isCoord1X)
                        {
                            heightMap[coord2, coord1] = mediumRoadHeight;
                            treePlaceInfo[coord2, coord1] = 0;
                            roadMask[coord2, coord1] = 0;
                        }
                        else
                        {
                            heightMap[coord1, coord2] = mediumRoadHeight;
                            treePlaceInfo[coord1, coord2] = 0;
                            roadMask[coord1, coord2] = 0;
                        }
                    }

                    if (tooHigh || roadThrowWater)
                    {
                        MakePoint(new Point
                        {
                            x = isCoord1X ? coord1 : (int)currentCoord2,
                            z = isCoord1X ? (int)currentCoord2 : coord1
                        },
                            heightMap, defaultHeightMap);
                        if (i != 0 && i + 1 < points.Length - 1)
                        {
                            if (points[i + 1] == startPoint)
                            {
                                points[i].x = isCoord1X ? coord1 : (int)currentCoord2;
                                points[i].z = isCoord1X ? (int)currentCoord2 : coord1;
                            }
                            else
                            {
                                points[i + 1].x = isCoord1X ? coord1 : (int)currentCoord2;
                                points[i + 1].z = isCoord1X ? (int)currentCoord2 : coord1;
                            }
                        }
                        break; // не продолжает создание дороги всквозь горы/воду
                    }

                    // сглаживание рельефа вокруг дороги (ниже) 
                    for (int c = 0; c < roadWidth; c++)
                    {
                        int coord2 = (int)currentCoord2 - roadWidth * 2 + c;
                        if (isCoord1X)
                        {
                            if (coord2 < 0 || coord2 >= terrainData.heightmapWidth)
                                continue;
                        }
                        else
                        {
                            if (coord2 < 0 || coord2 >= terrainData.heightmapHeight)
                                continue;
                        }
                        float coord2f = currentCoord2 - roadWidth * 2 + c; //значение float для исправления ребристости дороги
                        float mediumRoadHeight = 0;
                        int n = 0;
                        for (int j = coord1 - roadWidth * 2; j < coord1 + roadWidth * 2; j++)
                            for (int k = coord2 - roadWidth * 2; k < coord2 + roadWidth * 2; k++)
                            {
                                try
                                {
                                    if (isCoord1X)
                                    {
                                        if (defaultHeightMap[k, j] != 0)
                                        {
                                            mediumRoadHeight += defaultHeightMap[k, j];
                                            n++;
                                        }
                                    }
                                    else
                                    {
                                        if (defaultHeightMap[j, k] != 0)
                                        {
                                            mediumRoadHeight += defaultHeightMap[j, k];
                                            n++;
                                        }
                                    }
                                }
                                catch (System.IndexOutOfRangeException)
                                {
                                }
                            }
                        mediumRoadHeight = mediumRoadHeight / n - roadLowCoef; // итоговая средняя высота, учитывая понижение дороги
                        float coef = (1 + Mathf.Cos(Mathf.PI * (coord2f - (currentCoord2 - roadWidth * 2)) / roadWidth) * -1) / 2; //коэфф от 0 до 1, который сглаживает рельеф рядом с дорогой

                        if (isCoord1X)
                        {
                            mediumRoadHeight = defaultHeightMap[coord2, coord1] + (mediumRoadHeight - defaultHeightMap[coord2, coord1]) * coef;
                            if (mediumRoadHeight < heightMap[coord2, coord1])
                                heightMap[coord2, coord1] = mediumRoadHeight;
                            treePlaceInfo[coord2, coord1] = 0;
                        }
                        else
                        {
                            mediumRoadHeight = defaultHeightMap[coord1, coord2] + (mediumRoadHeight - defaultHeightMap[coord1, coord2]) * coef;
                            if (mediumRoadHeight < heightMap[coord1, coord2])
                                heightMap[coord1, coord2] = mediumRoadHeight;
                            treePlaceInfo[coord1, coord2] = 0;
                        }
                    }

                    // сглаживание рельефа вокруг дороги (выше) 
                    for (int d = 0; d < roadWidth; d++)
                    {
                        int coord2 = (int)currentCoord2 + roadWidth + d;
                        if (isCoord1X)
                        {
                            if (coord2 < 0 || coord2 >= terrainData.heightmapWidth)
                                continue;
                        }
                        else
                        {
                            if (coord2 < 0 || coord2 >= terrainData.heightmapHeight)
                                continue;
                        }
                        float coord2f = currentCoord2 + roadWidth + d;
                        float mediumRoadHeight = 0;
                        int n = 0;
                        for (int j = coord1 - roadWidth * 2; j < coord1 + roadWidth * 2; j++)
                            for (int k = coord2 - roadWidth * 2; k < coord2 + roadWidth * 2; k++)
                            {
                                try
                                {
                                    if (isCoord1X)
                                    {
                                        if (defaultHeightMap[k, j] != 0)
                                        {
                                            mediumRoadHeight += defaultHeightMap[k, j];
                                            n++;
                                        }
                                    }
                                    else
                                    {
                                        if (defaultHeightMap[j, k] != 0)
                                        {
                                            mediumRoadHeight += defaultHeightMap[j, k];
                                            n++;
                                        }
                                    }
                                }
                                catch (System.IndexOutOfRangeException)
                                {
                                }
                            }
                        mediumRoadHeight = mediumRoadHeight / n - roadLowCoef; // итоговая средняя высота, учитывая понижение дороги
                        float coef = (1 + Mathf.Cos(Mathf.PI * (coord2f - (currentCoord2 + roadWidth)) / roadWidth)) / 2; //коэфф от 0 до 1, который сглаживает рельеф рядом с дорогой

                        if (isCoord1X)
                        {
                            mediumRoadHeight = defaultHeightMap[coord2, coord1] + (mediumRoadHeight - defaultHeightMap[coord2, coord1]) * coef;
                            if (mediumRoadHeight < heightMap[coord2, coord1])
                                heightMap[coord2, coord1] = mediumRoadHeight;
                            treePlaceInfo[coord2, coord1] = 0;
                        }
                        else
                        {
                            mediumRoadHeight = defaultHeightMap[coord1, coord2] + (mediumRoadHeight - defaultHeightMap[coord1, coord2]) * coef;
                            if (mediumRoadHeight < heightMap[coord1, coord2])
                                heightMap[coord1, coord2] = mediumRoadHeight;
                            treePlaceInfo[coord1, coord2] = 0;
                        }
                    }

                    // заполнение инфы treePlaceInfo о промежуточных значениях (снизу)
                    for (int c = 0; c < roadWidth*3; c++)
                    {
                        int coord2 = (int)currentCoord2 - roadWidth * 5 + c;
                        if (isCoord1X && coord2 >= 0 && coord2 < terrainData.heightmapWidth)
                            treePlaceInfo[coord2, coord1] = 0f;
                        else if ((!isCoord1X) && coord2 >= 0 && coord2 < terrainData.heightmapHeight)
                            treePlaceInfo[coord1, coord2] = 0f;
                    }
                    for (int c = 0; c < roadWidth; c++)
                    {
                        int coord2 = (int)currentCoord2 - roadWidth * 6 + c;
                        if (isCoord1X && coord2 >= 0 && coord2 < terrainData.heightmapWidth && treePlaceInfo[coord2, coord1] > 0.5f)
                            treePlaceInfo[coord2, coord1] = 0.5f;
                        else if ((!isCoord1X) && coord2 >= 0 && coord2 < terrainData.heightmapHeight && treePlaceInfo[coord2, coord1] > 0.5f)
                            treePlaceInfo[coord1, coord2] = 0.5f;
                    }
                    // (сверху)
                    for (int d = 0; d < roadWidth*3; d++)
                    {
                        int coord2 = (int)currentCoord2 + roadWidth * 2 + d;
                        if (isCoord1X && coord2 >= 0 && coord2 < terrainData.heightmapWidth)
                            treePlaceInfo[coord2, coord1] = 0f;
                        else if ((!isCoord1X) && coord2 >= 0 && coord2 < terrainData.heightmapHeight)
                            treePlaceInfo[coord1, coord2] = 0f;
                    }
                    for (int d = 0; d < roadWidth; d++)
                    {
                        int coord2 = (int)currentCoord2 + roadWidth * 5 + d;
                        if (isCoord1X && coord2 >= 0 && coord2 < terrainData.heightmapWidth && treePlaceInfo[coord2, coord1] > 0.5f)
                            treePlaceInfo[coord2, coord1] = 0.5f;
                        else if ((!isCoord1X) && coord2 >= 0 && coord2 < terrainData.heightmapHeight && treePlaceInfo[coord2, coord1] > 0.5f)
                            treePlaceInfo[coord1, coord2] = 0.5f;
                    }

                    tracksLen++;
                }

                //обработка конечной point
                if (!tooHigh && !tooShort && !roadThrowWater)
                {
                    if (startPoint == points[i + 1])
                        MakePoint(points[i], heightMap, defaultHeightMap);
                    else MakePoint(points[i + 1], heightMap, defaultHeightMap);
                }
            }
            //catch(System.IndexOutOfRangeException)
            {
            }
        }
        roadMask = MixRoadMask(roadMask, 2);
        terrainData.SetHeights(0, 0, heightMap);
        //TestFile(treePlaceInfo, @"C:\Users\Computer\Documents\GitHub\Generation-Project\Assets\WriteAlpha0.txt");
    }

    public Road[] GetRandomRoads()
    {
        TerrainData terrainData = GetComponent<Terrain>().terrainData;
        var roads = new Road[amountOfRandomRoads];
        int border = roadWidth * 4 + roadFlexure / 2; // минимальное расстояние дорог от края карты
        if (terrainData.heightmapHeight <= border * 2 || terrainData.heightmapWidth <= border * 2)
            Debug.Log("No place for random roads, heightMap is too small. It should be more than " + border * 2 + "x" + border * 2 + ".");
        for (var road = 0; road < roads.Length; road++)
        {
            roads[road] = new Road { };
            roads[road].points = new Point[amountOfPointsRandomRoads];
            for (var point = 0; point < roads[road].points.Length; point++)
            {
                roads[road].points[point] = new Point { };
                if(point == 0 || point == roads[road].points.Length - 1)
                {
                    if (Random.value > 0.5f)
                    {
                        if (Random.value > 0.5f)
                            roads[road].points[point].x = Random.Range(-roadWidth * 4, -roadWidth * 3);
                        else roads[road].points[point].x = terrainData.heightmapHeight + Random.Range(roadWidth * 3, roadWidth * 4);
                        roads[road].points[point].z = Random.Range(-roadWidth * 4, terrainData.heightmapWidth + roadWidth * 4);
                    }
                    else
                    {
                        if (Random.value > 0.5f)
                            roads[road].points[point].z = Random.Range(-roadWidth * 4, -roadWidth * 3);
                        else roads[road].points[point].z = terrainData.heightmapWidth + Random.Range(roadWidth * 3, roadWidth * 4);
                        roads[road].points[point].x = Random.Range(-roadWidth * 4, terrainData.heightmapHeight + roadWidth * 4);
                    }
                }
                else
                {
                    roads[road].points[point].x = Random.Range(border, terrainData.heightmapHeight - border);
                    roads[road].points[point].z = Random.Range(border, terrainData.heightmapWidth - border);
                }
            }
        }
        return roads;
    }

    public float[,] MixRoadMask(float[,] roadMask, int diameterOfMixing)
    {
        // roadMask - Маска с нарисованной на ней дорогой
        // diameterOfMixing - область размером diameterOfMixing x diameterOfMixing вокруг точки будет смешиваться

        float[,] resultMask = (float[,])roadMask.Clone();
        for (var j = 1; j < terrainData.heightmapHeight - 1; j++)
            for (var k = 1; k < terrainData.heightmapWidth - 1; k++)
            {
                try
                {
                    if (roadMask[k, j] == 0f) //если в точке нарисована наша текстура для смешивания
                        if (
                            roadMask[k - 1, j] > 0f ||
                            roadMask[k, j - 1] > 0f ||
                            roadMask[k + 1, j] > 0f ||
                            roadMask[k, j + 1] > 0f)  //если точка лежит на границе разных текстур
                        {
                            int x1 = j - diameterOfMixing / 2;
                            int z1 = k - diameterOfMixing / 2;
                            for (var a = 0; a < diameterOfMixing; a++)
                                for (var b = 0; b < diameterOfMixing; b++)
                                    if (Random.value > 0.5f) //если верно, закрашиваем своей текстурой (дорогой)
                                        resultMask[z1 + b, x1 + a] = 1f;
                                    else resultMask[z1 + b, x1 + a] = 0f;
                        }
                }
                catch (System.IndexOutOfRangeException)
                {
                    Debug.Log("MixRoadMask System.IndexOutOfRangeException k = " + k.ToString() + "; j = " + j.ToString());
                }
            }
        return resultMask;
    }

    private void MakePoint(Point point, float[,] heightMap, float[,] defaultHeightMap)
    {
        for (int a = -roadWidth * 5; a < roadWidth * 5; a++)
            for (int b = -roadWidth * 5; b < roadWidth * 5; b++)
            {
                int x = point.x + a;
                int z = point.z + b;
                float len = Mathf.Sqrt(a * a + b * b) / roadWidth; //расстояние от центра

                try
                {
                    if(len<= 5)
                    {
                        if (len <= 3)
                        {
                            if (len <= 2)
                            {
                                float mediumRoadHeight = 0;
                                int n = 0;
                                for (int j = z - roadWidth * 2; j < z + roadWidth * 2; j++)
                                    for (int k = x - roadWidth * 2; k < x + roadWidth * 2; k++)
                                    {
                                        mediumRoadHeight += defaultHeightMap[z, x];
                                        n++;
                                    }

                                if (len <= 1)
                                {
                                    mediumRoadHeight = mediumRoadHeight / n - roadLowCoef; // итоговая средняя высота, учитывая понижение дороги
                                    roadMask[z, x] = 0;
                                }
                                else
                                {
                                    float coef = (1 + Mathf.Cos(Mathf.PI * (len - 1))) / 2; //коэфф от 0 до 1, который сглаживает рельеф рядом с дорогой
                                    mediumRoadHeight = mediumRoadHeight / n - roadLowCoef * coef; // итоговая средняя высота, учитывая понижение дороги
                                }
                                if (mediumRoadHeight < heightMap[z, x])
                                    heightMap[z, x] = mediumRoadHeight;
                            }
                        }
                        treePlaceInfo[z, x] = 0f;
                    }
                    
                }
                catch (System.IndexOutOfRangeException)
                {
                    //Debug.Log("MakePoint System.IndexOutOfRangeException z = " + z.ToString() + "; x = " + x.ToString());
                }
            }
        Debug.Log("Made point z = " + point.z.ToString() + "; x = " + point.x.ToString());
    }

    /*void TestFile(float[,] mask, string path)
    {
        StreamWriter sf = new StreamWriter(path);
        for (int i = 0; i < mask.GetLength(0); i++)
        {
            string text = "";
            for (int j = 0; j < mask.GetLength(1); j++)
            {
                text += mask[i, j];
            }
            sf.WriteLine(text);
        }
        sf.Close();
    }*/

    // Start is called before the first frame update 
    public void StartRoads(Ground_Controiler gc, TerrainGenerator tg)
    {
        amountOfPointsRandomRoads = Random.Range(2, 5);
        textures = gc;
        terrainGenerator = tg;
        if (roadLow < 0.1f)
            roadLow = 0.1f;
        if (roadLow > 1)
            roadLow = 1f;
        roadLowCoef = roadLow * 5 / terrainGenerator.depth;
        tracksLow = 0.5f / terrainGenerator.depth;
        if (randomRoads)
            MakeRoads(GetRandomRoads());
        else MakeRoads(roads);
        textures.AddTexture(textures.Road, textures.funk, roadMask);
    }

    // Update is called once per frame 
    void Update()
    {

    }
}