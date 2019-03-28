using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadsCreator : MonoBehaviour {
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

        public void DrawOnAlphaMaps(float[,,] alphaMaps, int roadWidth, int roadTextureIndex, int texturesCount)
        {
            for (var i = 0; i < points.Length - 1; i++)
            {
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
                    DrawRoad(minX, deltaX, minZ, deltaZ, maxZ, fromLeftUnderToRightUpper, roadWidth, texturesCount, roadTextureIndex, alphaMaps, true);
                else DrawRoad(minZ, deltaZ, minX, deltaX, maxX, fromLeftUnderToRightUpper, roadWidth, texturesCount, roadTextureIndex, alphaMaps, false);
            }
        }

        private void DrawRoad(int min1, int delta1, int min2, int delta2, int max2, bool fromLeftUnderToRightUpper, 
            int roadWidth, int texturesCount, int roadTextureIndex, float[,,]alphaMaps, bool isCoord1X) // вынес повторяющийся код в функцию
        {
            for (int coord1 = min1; coord1 <= min1 + delta1; coord1++)
            {
                int currentCoord2 = fromLeftUnderToRightUpper ?
                    min2 + (int)(delta2 * ((float)(coord1 - min1 + 1) / delta1)) :
                    max2 - (int)(delta2 * ((float)(coord1 - min1 + 1) / delta1));
                for (int coord2 = currentCoord2 - roadWidth; coord2 < currentCoord2 + roadWidth; coord2++)
                {
                    for (int textureIndex = 0; textureIndex < texturesCount; textureIndex++)
                    {
                        // X альфамапы = Z глобальных координат
                        // Y альфамапы = X глобальных координат
                        if(isCoord1X)
                        {
                            if (textureIndex == roadTextureIndex)
                                alphaMaps[coord2, coord1, textureIndex] = 1;
                            else alphaMaps[coord2, coord1, textureIndex] = 0;
                        }
                        else if (textureIndex == roadTextureIndex)
                            alphaMaps[coord1, coord2, textureIndex] = 1;
                        else alphaMaps[coord1, coord2, textureIndex] = 0;
                    }
                }
            }
        }
    }

    public int roadTextureIndex; //индекс текстуры дороги в инспекторе (отсчёт с нуля слева направо)
    public int roadWidth = 5;
    public Road[] roads;


    // Start is called before the first frame update
    void Start()
    {
        TerrainData terrainData = GetComponent<Terrain>().terrainData;
        var alphaMaps = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);

        foreach (Road road in roads)
            road.DrawOnAlphaMaps(alphaMaps, roadWidth, roadTextureIndex, terrainData.splatPrototypes.Length);
        terrainData.SetAlphamaps(0, 0, alphaMaps);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
