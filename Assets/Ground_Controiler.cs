using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;


public class Ground_Controiler : MonoBehaviour
{
    TreeGenerate height;
    [System.Serializable]
    public class GroundInfo
    {
        public TerrainLayer graund;
        public float density = 0.5f;
        public int protorype = -1;

        public static implicit operator TerrainLayer(GroundInfo a)
        {
            return a.graund;
        }

        public static implicit operator int(GroundInfo a)
        {
            return a.protorype;
        }

        public static implicit operator float(GroundInfo a)
        {
            return a.density;
        }
    }

    public List<GroundInfo> Ground;
    public float preDomTextGraund = 1;
    public List<GroundInfo> Water;
    public List<GroundInfo> Mountain;
    public List<GroundInfo> Road;
    private Terrain terrain;
    public TerrainGenerator terrainGenerator;
    TerrainData terrainData;
    System.Random rn;

    void TestLayer(List<GroundInfo> example)
    {
        for (int i = 0; i < terrainData.terrainLayers.Length; i++)
        {
            for (int j = 0; j < example.Count; j++)
            {
                if (terrainData.terrainLayers[i].GetHashCode() == example[j].graund.GetHashCode())
                {
                    example[j].protorype = i;
                }
            }
        }

        for (int j = 0; j < example.Count; j++)
        {
            if (example[j].protorype == -1)
            {
                throw new System.Exception("excessive texture at number: " + j.ToString());
            }
        }
    }
    public float CalculateHeight(int x, int y)
    {
        return 0.001f * rn.Next(0, 1000);
    }

    public Calculated funk;

    float[,] heights;
    public void StartGroundControl()
    {
        rn = new System.Random();
        funk = new Calculated(CalculateHeight);
        terrain = GetComponent<Terrain>();
        terrainData = terrain.terrainData;
        TestLayer(Ground);
        TestLayer(Water);
        TestLayer(Road);
        AddTexture(Ground, funk, terrainGenerator.maskGround);
        AddTexture(Water, funk, terrainGenerator.maskWater);
        
    }

    private void zeroAnother(float[,,] alphaMaps, int ex1, int ex2, int x, int z)
    {
        for (int i = 0; i < alphaMaps.GetLength(2); i++)
        {
            if (i != ex1 && i != ex2)
            {
                alphaMaps[x, z, i] = 0;
            }
        }
    }

    public void AddTexture(List<GroundInfo> texPrototypes, Calculated funk, float[,] mask)
    {
        float alpha;
        var alphaMaps = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);
        heights = TreeGenerate.CreateHeights(alphaMaps.GetLength(0), alphaMaps.GetLength(1), funk);
        for (int x = 0; x < alphaMaps.GetLength(0); x++)
        {
            for (int z = 0; z < alphaMaps.GetLength(1); z++)
            {
                // X альфамапы = Z глобальных координат
                // Y альфамапы = X глобальных координат
                if (mask[x / 2, z / 2] != 1)
                {
                    alpha = heights[x, z] * preDomTextGraund;
                    alphaMaps[x, z, texPrototypes[1]] = alpha;
                    alphaMaps[x, z, texPrototypes[0]] = 1 - alpha;
                    zeroAnother(alphaMaps, texPrototypes[1], texPrototypes[0], x, z);
                }
            }
        }
        terrainData.SetAlphamaps(0, 0, alphaMaps);
    }

    // Update is called once per frame
    void Update()
    {
        //float alpha;
        //var alphaMaps = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);
        //for (int x = 0; x < alphaMaps.GetLength(0); x++)
        //{
        //    for (int z = 0; z < alphaMaps.GetLength(1); z++)
        //    {
        //        // X альфамапы = Z глобальных координат
        //        // Y альфамапы = X глобальных координат
        //        alpha = heights[z, x] * preDomTextGraund;
        //        alphaMaps[z, x, Graund[1]] = alpha;
        //        alphaMaps[z, x, Graund[0]] = 1 - alpha;
        //    }
        //}
        //terrainData.SetAlphamaps(0, 0, alphaMaps);
    }

}
