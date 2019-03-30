
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public int depth = 20;

    protected float[,] heightMap;
    public int height = 256;
    public int width = 256;

    public float scale = 20f;
    public float offsetX = 100f;
    public float offsetY = 100f;
    Terrain terrain;

    //Владение гошана
    public Vector3[] questZones; // x = x; y = radius; z = z
    private float xTerrain = 0;
    private float zTerrain = 0;
    public int maxDist = 4;
    public int minDist = 3;
    public float maxTreeScale = 2;
    public float minTreeScale = 1;

    bool isQZones(Vector2 coord)
    {
        bool result = false;
        for (int i = 0; i < questZones.Length; i++)
        {
            float r = questZones[i].y;
            if (questZones[i].x + r > coord.x && questZones[i].x - r < coord.x
            && questZones[i].z + r > coord.y && questZones[i].z - r < coord.y)
            {
                result = true;
            }
        }
        return result;
    }
    void gTree(float xTer, float zTer)
    {
        int maxTS = (int)maxTreeScale * 100;
        int minTS = (int)minTreeScale * 100;
        System.Random rn = new System.Random();
        for (int x = 0; x < width; x += rn.Next(minDist, maxDist))
        {
            for (int z = 0; z < height; z += rn.Next(minDist, maxDist))
            {
                Vector2 coord = new Vector2(x,z);
                if(!isQZones(coord))
                {
                    var tree = new TreeInstance
                    {
                        position = new Vector3((float)x / width, heightMap[x, z], ((float)z) / height),
                        prototypeIndex = rn.Next(0, terrain.terrainData.treePrototypes.Length-1),
                        widthScale = rn.Next(minTS, maxTS) * 0.01f,
                        heightScale = rn.Next(minTS, maxTS) * 0.01f,
                        color = Color.white,
                        lightmapColor = Color.white
                    };
                    terrain.AddTreeInstance(tree);
                    terrain.Flush();
                }
            }
        }
    }


    // Конец власти Гошана


    private void Start()
    {
        terrain = GetComponent<Terrain>(); // ЭТО ОБЯЗ В НАЧАЛЕ ПЛАНЕТЫ СУКА!!!!!!!!!!!!

        offsetX = Random.Range(0, 1000f);
        offsetY = Random.Range(0, 1000f);


        terrain.terrainData = CreateTerrain(terrain.terrainData);
        // Зона Гошана



        // Работает только тогда когда в массиве деревьев есть хотя бы одно деревоVector3 position = new Vector3(xTerrain, 0, zTerrain);                     // Крайняя начальная точка ---- это нужно гашану
        if(terrain.terrainData.treePrototypes.Length > 0)
        {
            Vector3 positionEnd = new Vector3(width + xTerrain, 0, height + zTerrain);    // Крайняя конечная точка ---- это нужно гашану

            gTree(xTerrain, zTerrain);
        }
        

        // Конец зоны Goshana
    }
    private void Update()
    {


        //offsetX += Time.deltaTime * 2f;
    }

    TerrainData CreateTerrain(TerrainData terrainData)
    {
        terrainData.heightmapResolution = width + 1;
        terrainData.size = new Vector3(width, depth, height);
        heightMap = CreateHeights();
        terrainData.SetHeights((int)xTerrain, (int)zTerrain, heightMap);
        return terrainData;
    }

    float[,] CreateHeights()
    {
        float[,] heights = new float[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                heights[x, y] = CalculateHeight(x, y);
            }
        }
        return heights;
    }

    float CalculateHeight(int x, int y)
    {
        float xCoord = (float)x / width * scale + offsetX;
        float yCoord = (float)y / height * scale + offsetY;

        return Mathf.PerlinNoise(xCoord, yCoord);
    }
}
