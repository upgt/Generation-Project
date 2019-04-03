using Application;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public int depth = 200;
    public int plainDepth = 20;
    public int height = 256;
    public int width = 256;

    public float scale = 20f;
    public float offsetX = 100f;
    public float offsetY = 100f;

    public int mountainHeight = 200;
    public int mountainX = 100;
    public int mountainY = 100;
    public double angle = 30f;

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

      // offsetX += Time.deltaTime * 2f;
    }

    private TerrainData CreateTerrain(TerrainData terrainData)
    {
        terrainData.heightmapResolution = width + 1;
        terrainData.size = new Vector3(width, plainDepth, height);
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
        return heights;
    }

    private float WhiteNoise()
    {
        return Random.Range(0, plainDepth);
    }

    private float CalculateHeight(int x, int y)
    {
        float xCoord = (float)x / width * scale + offsetX;
        float yCoord = (float)y / height * scale +  offsetY;

        return Mathf.PerlinNoise(xCoord, yCoord) * flatCoefficient;
    }

    //private void AddMountain()
    //{
    //    Mountain mountain = new Mountain(mountainHeight, angle);
    //    int halfWidth = mountain.BaseWidth / 2;
    //    for (int i = 0; i < mountain.BaseWidth; i++)
    //        for (int j = 0; j < mountain.BaseWidth; j++)
    //        {
    //            if (mountain.Heights[i,j] > 0)
    //                heights[mountainX - halfWidth + i, mountainY - halfWidth + j] = mountain.Heights[i, j];
    //        }
    //}
}


