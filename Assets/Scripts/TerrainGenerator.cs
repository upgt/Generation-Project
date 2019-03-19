using Application;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public int depth = 200;
    public int height = 256;
    public int width = 256;

    public float scale = 20f;
    public float offsetX = 100f;
    public float offsetY = 100f;

    public int mountainHeight = 200;
    public int MountainAmount = 1;

    public float flatCoefficient = 0.2f;
    private float[,] heights;

    private void Start()
    {
        offsetX = Random.Range(0, 1000f);
        offsetY = Random.Range(0, 1000f);
    }
    private void Update()
    {
        Terrain terrain = GetComponent<Terrain>();
        terrain.terrainData = CreateTerrain(terrain.terrainData);

       //offsetX += Time.deltaTime * 2f;
    }

    private TerrainData CreateTerrain(TerrainData terrainData)
    {
        terrainData.heightmapResolution = width + 1;
        terrainData.size = new Vector3(width, depth, height);
        terrainData.SetHeights(0, 0, CreateHeights());
        return terrainData;
    }

    private float [,] CreateHeights()
    {
        heights = new float[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                heights[x, y] = CalculateHeight(x, y);
            }
        }

        AddMountain(1);
        return heights;
    }

    private float CalculateHeight(int x, int y)
    {
        float xCoord = (float)x / width * scale + offsetX;
        float yCoord = (float)y / height * scale +  offsetY;

        return Mathf.PerlinNoise(xCoord, yCoord) * flatCoefficient;
    }

    private void AddMountain(int amount)
    {
        Mountain mountain = new Mountain(mountainHeight, mountainHeight / 3 * 2);
        Peak peak = SetPeakCoordinate(mountain);
        int halfWidth = mountain.BaseWidth / 2;
        for (int i = 0; i < mountain.BaseWidth; i++)
            for (int j = 0; j < mountain.BaseWidth; j++)
            {
                heights[peak.X - halfWidth + i, peak.Y - halfWidth + j] = mountain.Heights[i, j];
            }
    }

    private Peak SetPeakCoordinate(Mountain mountain)
    {
        int x = Random.Range(mountain.BaseWidth / 2 + 5, heights.Length - mountain.BaseWidth / 2 - 5);
        int y = Random.Range(mountain.BaseWidth / 2 + 5, heights.Length - mountain.BaseWidth / 2 - 5);
        return new Peak{ X = x, Y = y };
    }
}
    //// Start is called before the first frame update
    //void Start()
    //{
    //    var terrain = GetComponent<Terrain>();
    //    terrain.terrainData.size = new Vector3(128, 50, 128);
    //    terrain.terrainData.heightmapResolution = 128;

    //    HabrHeightMap hm = new HabrHeightMap();
    //    hm.GenerateHeightMap(); //генерируем карту высот
    //    terrain.terrainData.SetHeights(0, 0, hm.HMArray); //применяем карту высот к terrain
    //}

//}

//public class HabrHeightMap
//{
    ////размеры карты (terrain)
    //public int mapSizex = 128;
    //public int mapSizey = 128;

    //public int genStep = 1024; //количество прямоугольников
    //public float zScale = 512; // коэффициент высоты карты

    ////размеры прямоугольника
    //public int recSizex = 10;
    //public int recSizey = 10;

    //public float[,] HMArray; //двумерный массив = карта высот

    //public void GenerateHeightMap()
    //{
    //    HMArray = new float[mapSizex, mapSizey];
    //    for (int i = 0; i < genStep; i++)
    //    {
    //        int x1 = Random.Range(0, 123456789) % mapSizex;
    //        int y1 = Random.Range(0, 123456789) % mapSizey;
    //        int x2 = x1 + recSizex / 4 + Random.Range(0, 123456789) % recSizex;
    //        int y2 = y1 + recSizey / 4 + Random.Range(0, 123456789) % recSizey;
    //        if (y2 > mapSizey) y2 = mapSizey;
    //        if (x2 > mapSizex) x2 = mapSizex;
    //        for (int j = x1; j < x2; j++)
    //            for (int k = y1; k < y2; k++)
    //                HMArray[j, k] += ((zScale) / (genStep) + Random.Range(0, 123456789) % 50 / 50.0f) * 0.006f;
    //        //последний коэффициент 0.006f добавил от себя, чтобы по высоте выглядело нормально
    //    }
    //}
//}

