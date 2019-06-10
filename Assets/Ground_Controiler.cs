using System;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;
using System.IO;

public class Ground_Controiler : MonoBehaviour
{
    TreeGenerate height;
    [System.Serializable]
    public class GroundInfo
    {
        public TerrainLayer graund;
        //public float density = 0.5f;
        public int protorype = -1;

        public static implicit operator TerrainLayer(GroundInfo a)
        {
            return a.graund;
        }

        public static implicit operator int(GroundInfo a)
        {
            return a.protorype;
        }

        /*public static implicit operator float(GroundInfo a)
        {
            return a.density;
        }*/
    }

    public List<GroundInfo> Ground;
    public float preDomTextGraund = 1;
    public List<GroundInfo> Water;
    public List<GroundInfo> Mountain;
    public List<GroundInfo> Road;
    float[,] mask;
    private Terrain terrain;
    private TerrainGenerator terrainGenerator;
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
        ZeroingAlpha();
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
        float multX = (float)mask.GetLength(0) / alphaMaps.GetLength(0);
        float multZ = (float)mask.GetLength(1) / alphaMaps.GetLength(1);
        int X, Z;

        for (int x = 0; x < alphaMaps.GetLength(0); x++)
        {
            for (int z = 0; z < alphaMaps.GetLength(1); z++)
            {
                // X альфамапы = Z глобальных координат
                // Y альфамапы = X глобальных координат
                X = (int)(x * multX);
                Z = (int)(z * multZ);
                float mult = mask[X, Z] + step - (mask[X, Z] * 3);

                if (mult < 0 || mult > 2)
                {
                    mult = 0;
                }

                if (mult > 1)
                {
                    mult = 2 - mult;
                }

                alpha = heights[x, z] * preDomTextGraund;
                if (protIndx == protIndxTwo)
                {
                    alphaMaps[x, z, texPrototypes[protIndx]] = mult;
                }
                else
                {
                    alphaMaps[x, z, texPrototypes[protIndx]] = alpha * mult;
                    alphaMaps[x, z, texPrototypes[protIndxTwo]] = (1 - alpha) * mult;
                    Normalize(alphaMaps, x, z);
                }
            }
        }
        //TestFile(alphaMaps, texPrototypes[protIndx], @"E:\Толя проекты\TGen\Assets\WriteAlpha" + texPrototypes[protIndx].protorype.ToString() + ".txt");
        //TestFile(alphaMaps, texPrototypes[protIndxTwo], @"E:\Толя проекты\TGen\Assets\WriteAlpha" + texPrototypes[protIndxTwo].protorype.ToString() +".txt");
        terrainData.SetAlphamaps(0, 0, alphaMaps);
    }

    public void AddTexture(List<GroundInfo> texPrototypes, Calculated funk, float[,] mask)
    {
        TestLayer(texPrototypes);
        float alpha;
        var alphaMaps = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);
        heights = TreeGenerate.CreateHeights(alphaMaps.GetLength(0), alphaMaps.GetLength(1), funk);
        int prot = rn.Next(texPrototypes.Count - 1);
        int protTwo = GetNotRepeatRandParam(texPrototypes.Count - 1, prot);

        for (int x = 0; x < alphaMaps.GetLength(0); x++)
        {
            for (int z = 0; z < alphaMaps.GetLength(1); z++)
            {
                // X альфамапы = Z глобальных координат
                // Y альфамапы = X глобальных координат
                float diffuseX = x * mask.GetLength(0) / alphaMaps.GetLength(0);
                float diffuseZ = z * mask.GetLength(1) / alphaMaps.GetLength(1);
                int difX = (int)Math.Round(diffuseX, 0);
                int difZ = (int)Math.Round(diffuseZ, 0);
                if (mask[difX, difZ] != 1)
                {
                    alpha = heights[x, z] * preDomTextGraund;
                    if (texPrototypes.Count == 1)
                    {
                        alphaMaps[x, z, texPrototypes[prot]] = 1;
                        zeroAnother(alphaMaps, texPrototypes[prot], texPrototypes[prot], x, z);
                    }
                    else
                    {
                        alphaMaps[x, z, texPrototypes[prot]] = alpha;
                        alphaMaps[x, z, texPrototypes[protTwo]] = 1 - alpha;
                        zeroAnother(alphaMaps, texPrototypes[prot], texPrototypes[protTwo], x, z);
                    }
                    
                }
            }
        }
        terrainData.SetAlphamaps(0, 0, alphaMaps);
    }

    private static void Normalize(float[,,] alphaMaps, int x, int z)
    {
        float sum = 0;
        for (int i = 0; i < alphaMaps.GetLength(2); i++)
        {
            if (sum + alphaMaps[x, z, i] <= 1)
            {
                sum += alphaMaps[x, z, i];
            }
            else
            {
                alphaMaps[x, z, i] = 1 - sum;
            }
        }
    }

    void TestFile(float[,,] mask, int ma, string path)
    {
        StreamWriter sf = new StreamWriter(path);
        for (int i = 0; i < mask.GetLength(0); i++)
        {
            string text = "";
            for (int j = 0; j < mask.GetLength(1); j++)
            {
                text += mask[i, j,ma].ToString() + '\t' + '\t' + '\t';
            }
            sf.WriteLine(text);
        }
        sf.Close();
    }

    private void ZeroingAlpha()
    {
        var alphaMaps = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);
        for (int x = 0; x < alphaMaps.GetLength(0); x++)
        {
            for (int z = 0; z < alphaMaps.GetLength(1); z++)
            {
                for (int i = 0; i < alphaMaps.GetLength(2); i++)
                {
                    alphaMaps[x, z, i] = 0;
                }
            }
        }
        terrainData.SetAlphamaps(0, 0, alphaMaps);
    }

    int GetNotRepeatRandParam(int repeatParam, int max)
    {
        int result = rn.Next(max);
        if (result == repeatParam)
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
