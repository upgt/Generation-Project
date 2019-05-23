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
    public Ground_Controiler textures;
    private int roadTextureIndex; //индекс текстуры дороги в инспекторе (отсчёт с нуля слева направо) 
    public int roadWidth = 5; //ширина дороги/2 
    public int roadFlexure = 20; //кривизна дороги 
    public float roadLow = 0.015f; //понижение дороги 
    public bool randomRoads = false; //создание случайных дорог вместо определенных
    public bool tracks = true; //колеи дорог
    private float tracksLow = 0.0055f;
    public Road[] roads;
    public float[,] treePlaceInfo; //информация о возможности рассадки деревьев, 0 - нельзя
    private int roadMinLength = 10;

    //переменные которые ограничивают крутость дороги, прохождение всквозь горы
    private float tooHighMedium = 0.20f; //насколько сильным может быть перепад высот рядом с дорогой
    private float tooHighCenter = 0.0055f; //насколько дорога может меняться по высоте вдоль центральной линии
    private float tooHighLeftRight = 0.019f; //насколько сильно может отличаться высота левого и бравого бока дороги

    TerrainData terrainData;

    //int coefTexture = terrainData.baseMapResolution / terrainData.heightmapWidth;
    int coefTexture = 2;

    public float[,] roadMask;

    public void MakeRoads(Road[] roads)
    {
        terrainData = GetComponent<Terrain>().terrainData;
        var alphaMaps = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);
        var heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);
        var texturesCount = terrainData.splatPrototypes.Length;
        float[,] defaultHeightMap = (float[,])heightMap.Clone();

        treePlaceInfo = (float[,])heightMap.Clone();
        for (int j = 0; j < terrainData.heightmapWidth; j++)
            for (int k = 0; k < terrainData.heightmapHeight; k++)
                treePlaceInfo[j, k] = 1;

        

        float[,,] everythingMap = (float[,,])alphaMaps.Clone();

        int tracksLen = 0; //для непостоянности колеи

        //float[,] groundInfo = textures.terrainGenerator.maskGround;//если меньше 1f, то это земля.
        

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
                bool tooShort = false; // верно, если дорога слишком короткая

                Point startPoint = points[i];

                //проверка чтобы отсечь короткие отрезки дороги
                //if (delta1 < roadMinLength)
                    //continue;
                //цикл для проверки - будет ли отрезок дороги коротким из-за горы
                for (int a = 0; a < roadMinLength; a++)// 100 - минимальная длина дороги (не по диагонали, надо переделать)
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
                            if(a == 0)
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
                            float deltaCoord2 = deltaX * ((float)(coord1 - minZ) / deltaZ); // дельта второй координаты точки на отрезке между point i и point i+1 (расстояние от мин/макс)
                            float flexure = Mathf.Sin(2 * Mathf.PI * ((float)(coord1 - minZ) / deltaZ)) * roadFlexure; // отклонение (изгиб) дороги по синусоиде
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
                                if (isCoord1X)
                                {
                                    if (defaultHeightMap[(int)currentCoord2 + k, coord1 + j] > maxHeight)
                                        maxHeight = defaultHeightMap[(int)currentCoord2 + k, coord1 + j];
                                    if (minHeight == -1f || defaultHeightMap[(int)currentCoord2 + k, coord1 + j] < minHeight)
                                        minHeight = defaultHeightMap[(int)currentCoord2 + k, coord1 + j];
                                }
                                else
                                {
                                    if (defaultHeightMap[coord1 + k, (int)currentCoord2 + j] > maxHeight)
                                        maxHeight = defaultHeightMap[coord1 + k, (int)currentCoord2 + j];
                                    if (minHeight == -1f || defaultHeightMap[coord1 + k, (int)currentCoord2 + j] < minHeight)
                                        minHeight = defaultHeightMap[coord1 + k, (int)currentCoord2 + j];
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
                                    mediumRoadHeight += (isCoord1X ?
                                        defaultHeightMap[k, j] :
                                        defaultHeightMap[j, k]);
                                    n++;
                                }
                            mediumRoadHeight = mediumRoadHeight / n - roadLow; // итоговая средняя высота, учитывая понижение дороги
                            if (b == -roadWidth)
                                leftHeight = mediumRoadHeight;
                            if (b == 0)
                            {
                                if (a != 0 && Mathf.Abs(mediumRoadHeight - centerHeight) > tooHighCenter)
                                {
                                    centerHeight = mediumRoadHeight;
                                    tooHigh = true;
                                    //break;
                                }
                                centerHeight = mediumRoadHeight;
                            }
                            if (b == roadWidth - 1)
                            {
                                rightHeight = mediumRoadHeight;
                                if (Mathf.Abs(leftHeight - rightHeight) > tooHighLeftRight)
                                {
                                    tooHigh = true;
                                    //break;
                                }
                            }
                        }
                    }

                    if (tooHigh)// тут если дорога слишком крутая, она обрывается слишком рано, поэтому она короткая
                    {
                        tooShort = true;
                        //break; // не продолжает дальнейшую проверку
                    }
                }

                //if (tooShort) //если отрезок дороги короткий, переходим к след. отрезку
                    //continue;

                //обработка стартовой point
                MakePoint(startPoint, texturesCount, alphaMaps, heightMap, defaultHeightMap);

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
                    
                    // вычисления для tooHigh
                    {
                        float minHeight = -1f;
                        float maxHeight = -1f;
                        for (int k = -roadWidth * 2; k < roadWidth * 2; k++)
                            for (int j = -roadWidth * 2; j < roadWidth * 2; j++)
                            {
                                if (isCoord1X)
                                {
                                    if (defaultHeightMap[(int)currentCoord2 + k, coord1 + j] > maxHeight)
                                        maxHeight = defaultHeightMap[(int)currentCoord2 + k, coord1 + j];
                                    if (minHeight == -1f || defaultHeightMap[(int)currentCoord2 + k, coord1 + j] < minHeight)
                                        minHeight = defaultHeightMap[(int)currentCoord2 + k, coord1 + j];
                                }
                                else
                                {
                                    if (defaultHeightMap[coord1 + k, (int)currentCoord2 + j] > maxHeight)
                                        maxHeight = defaultHeightMap[coord1 + k, (int)currentCoord2 + j];
                                    if (minHeight == -1f || defaultHeightMap[coord1 + k, (int)currentCoord2 + j] < minHeight)
                                        minHeight = defaultHeightMap[coord1 + k, (int)currentCoord2 + j];
                                }
                            }
                        if (Mathf.Abs(maxHeight - minHeight) > tooHighMedium)
                            tooHigh = true;
                    }

                    if (tooHigh)
                    {
                        MakePoint(new Point {
                            x = isCoord1X ? coord1 : (int)currentCoord2,
                            z = isCoord1X ? (int)currentCoord2 : coord1}, 
                            texturesCount, alphaMaps, heightMap, defaultHeightMap);
                        //break; // не продолжает создание дороги всквозь горы
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
                                    alphaMaps[coord2 * coefTexture, coord1 * coefTexture, textureIndex] = 1;
                                else alphaMaps[coord2 * coefTexture, coord1 * coefTexture, textureIndex] = 0;
                            }
                            else
                            {
                                if (textureIndex == roadTextureIndex)
                                    alphaMaps[coord1 * coefTexture, coord2 * coefTexture, textureIndex] = 1;
                                else alphaMaps[coord1 * coefTexture, coord2 * coefTexture, textureIndex] = 0;
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
                            if (b == -roadWidth)
                                leftHeight = mediumRoadHeight;
                            if (b == 0)
                            {
                                if (a != 0 && Mathf.Abs(mediumRoadHeight - centerHeight) > tooHighCenter)
                                {
                                    centerHeight = mediumRoadHeight;
                                    tooHigh = true;
                                    //continue;
                                }
                                centerHeight = mediumRoadHeight;
                            }
                            if (b == roadWidth - 1)
                            {
                                rightHeight = mediumRoadHeight;
                                if(Mathf.Abs(leftHeight-rightHeight) > tooHighLeftRight)
                                {
                                    tooHigh = true;
                                    //continue;
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
                            }
                            else
                            {
                                heightMap[coord1, coord2] = mediumRoadHeight;
                                treePlaceInfo[coord1, coord2] = 0;
                            }
                        }
                    }

                    roadMask = (float[,])treePlaceInfo.Clone();

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
                        float coef = (1 + Mathf.Cos(Mathf.PI * (coord2f - (currentCoord2 - roadWidth * 2)) / roadWidth) * -1) / 2; //коэфф от 0 до 1, который сглаживает рельеф рядом с дорогой

                        if (isCoord1X)
                        {
                            mediumRoadHeight = defaultHeightMap[coord2, coord1] + (mediumRoadHeight - defaultHeightMap[coord2, coord1]) * coef;
                            if(mediumRoadHeight < heightMap[coord2, coord1])
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
                    for (int c = 0; c < roadWidth; c++)
                    {
                        int coord2 = (int)currentCoord2 - roadWidth * 3 + c;
                        if (isCoord1X)
                            treePlaceInfo[coord2, coord1] = 0.5f;
                        else
                            treePlaceInfo[coord1, coord2] = 0.5f;
                    }
                    // (сверху)
                    for (int d = 0; d < roadWidth; d++)
                    {
                        int coord2 = (int)currentCoord2 + roadWidth * 2 + d;
                        if (isCoord1X)
                            treePlaceInfo[coord2, coord1] = 0.5f;
                        else
                            treePlaceInfo[coord1, coord2] = 0.5f;
                    }

                    tracksLen++;
                }

                //обработка конечной point
                if (!tooHigh)
                    MakePoint(points[i+1], texturesCount, alphaMaps, heightMap, defaultHeightMap);
            }
        }
        //alphaMaps = MixAlphaMaps(alphaMaps, textures.Ground[0], roadTextureIndex, 2);
        terrainData.SetAlphamaps(0, 0, alphaMaps);
        terrainData.SetHeights(0, 0, heightMap);
    }

    public Road[] GetRandomRoads()
    {
        TerrainData terrainData = GetComponent<Terrain>().terrainData;
        var roads = new Road[Random.Range(1, 5)]; //от 1 до 4 дорог
        int border = roadWidth * 4 + roadFlexure / 2; // минимальное расстояние дорог от края карты
        if (terrainData.heightmapHeight <= border * 2 || terrainData.heightmapWidth <= border * 2)
            Debug.Log("No place for random roads, heightMap is too small. It should be more than " + border * 2 + "x" + border * 2 + ".");
        for (var road=0; road<roads.Length;road++)
        {
            roads[road] = new Road { };
            roads[road].points = new Point[Random.Range(2, 5)]; //от 2 до 4 точек у каждой дороги
            for (var point = 0; point < roads[road].points.Length; point++)
            {
                roads[road].points[point] = new Point { };
                roads[road].points[point].x = Random.Range(border, terrainData.heightmapHeight - border);
                roads[road].points[point].z = Random.Range(border, terrainData.heightmapWidth - border);
            }
        }
        return roads;
    }

    public float[,,] MixAlphaMaps(float[,,] roadMap, float currentIndex, int textureMixIndex, int diameterOfMixing)
    {
        // roadMap - карта с нарисованной на ней дорогой (лужей)
        // everythingMap - карта ДО момента рисования дороги (лужи)
        // textureMixIndex - индекс текстуры дороги (лужи)
        // diameterOfMixing - область размером diameterOfMixing x diameterOfMixing вокруг точки будет смешиваться

        float[,,] resultMap = (float[,,])roadMap.Clone();
        for (var x = 1; x < terrainData.alphamapHeight - 1; x++)
            for (var z = 1; z < terrainData.alphamapWidth - 1; z++)
            {
                if (roadMap[z, x, textureMixIndex] == 1f) //если в точке нарисована наша текстура для смешивания
                    if (
                        roadMap[z - 1, x, textureMixIndex] < 1f ||
                        roadMap[z, x - 1, textureMixIndex] < 1f ||
                        roadMap[z + 1, x, textureMixIndex] < 1f ||
                        roadMap[z, x + 1, textureMixIndex] < 1f)  //если точка лежит на границе разных текстур
                    {
                        int x1 = x - diameterOfMixing / 2;
                        int z1 = z - diameterOfMixing / 2;
                        for (var j = 0; j < diameterOfMixing; j++)
                            for (var k = 0; k < diameterOfMixing; k++)
                                if (Random.value > 0.5f) //если верно, закрашиваем своей текстурой (дорогой/лужей)
                                    for (int textureIndex = 0; textureIndex < terrainData.splatPrototypes.Length; textureIndex++)
                                    {
                                        if (textureIndex == textureMixIndex)
                                            resultMap[z1 + k, x1 + j, textureIndex] = 1f;
                                        else resultMap[z1 + k, x1 + j, textureIndex] = 0f;
                                    }
                                else //иначе закрашиваем тем, что было ДО дороги (лужи)
                                    for (int textureIndex = 0; textureIndex < terrainData.splatPrototypes.Length; textureIndex++)
                                        resultMap[z1 + k, x1 + j, textureIndex] = currentIndex;
                    }
            }
        return resultMap;
    }

    private void CleanAlphaMaps(float[,,] alphaMaps, int alphamapWidth, int alphamapHeight, int texturesCount) //очистить альфамапы от текстуры дороги
    {
        for (var x = 0; x < alphamapWidth; x++)
            for (var y = 0; y < alphamapHeight; y++)
                for (int textureIndex = 0; textureIndex < texturesCount; textureIndex++)
                    if (textureIndex == 0)
                        alphaMaps[x, y, textureIndex] = 1f; //закрашиваем всю карту нулевой текстурой (травой)
                    else alphaMaps[x, y, textureIndex] = 0f;
    }

    private void MakePoint(Point point, int texturesCount, float[,,] alphaMaps, float[,] heightMap, float[,] defaultHeightMap)
    {
        for (int a = -roadWidth * 3; a < roadWidth * 3; a++)
            for (int b = -roadWidth * 3; b < roadWidth * 3; b++)
            {
                int x = point.x + a;
                int z = point.z + b;
                float len = Mathf.Sqrt(a * a + b * b) / roadWidth; //расстояние от центра

                try
                {
                    if (len <= 3)
                    {
                        if (len <= 2)
                        {
                            if (len <= 1)
                            {
                                // отрисовка дороги 
                                for (int textureIndex = 0; textureIndex < texturesCount; textureIndex++) // цикл по текстурам точки 
                                {
                                    // X альфамапы = Z глобальных координат 
                                    // Y альфамапы = X глобальных координат
                                    // разрешение меша 1024, а разрешение альфа текстур 512, 
                                    // поэтому берём координату высоты / 2
                                    if (textureIndex == roadTextureIndex)
                                    {
                                        alphaMaps[z * coefTexture, x * coefTexture, textureIndex] = 1;
                                        if (roadMask == null)
                                            roadMask = (float[,])treePlaceInfo.Clone();
                                        roadMask[z * coefTexture, x * coefTexture] = 0;
                                    } 
                                    else alphaMaps[z * coefTexture, x * coefTexture, textureIndex] = 0;
                                }

                                float mediumRoadHeight = 0;
                                int n = 0;
                                for (int j = z - roadWidth * 2; j < z + roadWidth * 2; j++)
                                    for (int k = x - roadWidth * 2; k < x + roadWidth * 2; k++)
                                    {
                                        mediumRoadHeight += defaultHeightMap[z, x];
                                        n++;
                                    }
                                mediumRoadHeight = mediumRoadHeight / n - roadLow; // итоговая средняя высота, учитывая понижение дороги

                                if (mediumRoadHeight < heightMap[z, x])
                                    heightMap[z, x] = mediumRoadHeight;
                            }
                            else
                            {
                                float mediumRoadHeight = 0;
                                int n = 0;
                                for (int j = z - roadWidth * 2; j < z + roadWidth * 2; j++)
                                    for (int k = x - roadWidth * 2; k < x + roadWidth * 2; k++)
                                    {
                                        mediumRoadHeight += defaultHeightMap[z, x];
                                        n++;
                                    }
                                float coef = (1 + Mathf.Cos(Mathf.PI * (len - 1))) / 2; //коэфф от 0 до 1, который сглаживает рельеф рядом с дорогой
                                mediumRoadHeight = mediumRoadHeight / n - roadLow * coef; // итоговая средняя высота, учитывая понижение дороги

                                if (mediumRoadHeight < heightMap[z, x])
                                    heightMap[z, x] = mediumRoadHeight;
                            }
                            treePlaceInfo[z, x] = 0f;
                        }
                        else treePlaceInfo[z, x] = 0.5f;
                    }
                }
                catch(System.IndexOutOfRangeException)
                {
                    Debug.Log("MakePoint System.IndexOutOfRangeException z = " + z.ToString() + "; x = " + x.ToString());
                }
            }
    }

    // Start is called before the first frame update 
    public void StartRoads()
    {
        roadTextureIndex = textures.Road[Random.Range(0, textures.Road.Count - 1)];
        if (randomRoads)
            MakeRoads(GetRandomRoads());
        else MakeRoads(roads);
       // textures.AddTexture(textures.Road, textures.funk, roadMask);
    }

    // Update is called once per frame 
    void Update()
    {

    }
}