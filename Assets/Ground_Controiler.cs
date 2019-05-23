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
    float[,] mask;
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
    public void StartGroundControl(TerrainGenerator TG)
    {
        terrainGenerator = TG;
        mask = TG.maskWater;
        rn = new System.Random();
        funk = new Calculated(CalculateHeight);
        terrain = GetComponent<Terrain>();
        terrainData = terrain.terrainData;
        AddTexture(Ground, funk, 2);
        AddTexture(Water, funk, 1);
        AddTexture(Mountain, funk, 3);
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

    public void AddTexture(List<GroundInfo> texPrototypes, Calculated funk, int step)
    {
        TestLayer(texPrototypes);
        float alpha;
        var alphaMaps = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);
        heights = TreeGenerate.CreateHeights(alphaMaps.GetLength(0), alphaMaps.GetLength(1), funk);
        int protIndx = rn.Next(texPrototypes.Count - 1);
        int protIndxTwo = GetNotRepeatRandParam(protIndx, texPrototypes.Count - 1);

        for (int x = 0; x < alphaMaps.GetLength(0); x++)
        {
            for (int z = 0; z < alphaMaps.GetLength(1); z++)
            {
                // X альфамапы = Z глобальных координат
                // Y альфамапы = X глобальных координат
                float mult = mask[x / 2, z / 2] + step - (mask[x / 2, z / 2] * 3);

                if (mult < 0 || mult > 2)
                {
                    mult = 0;
                }

                if (mult > 1)
                {
                    mult = 2 - mult;
                }

                alpha = heights[x, z] * preDomTextGraund;
                if(protIndx == protIndxTwo)
                {
                    alphaMaps[x, z, texPrototypes[protIndx]] = mult;
                }
                else
                {
                    alphaMaps[x, z, texPrototypes[protIndx]] = alpha * mult;
                    alphaMaps[x, z, texPrototypes[protIndxTwo]] = (1 - alpha) * mult;
                }
            }
        }
        terrainData.SetAlphamaps(0, 0, alphaMaps);
    }

    int GetNotRepeatRandParam(int repeatParam, int max)
    {
        int result = rn.Next(max);
        if(result == repeatParam)
        {
            result++;
        }
        if (result > max)
        {
            return 0;
        }
        return result;
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
