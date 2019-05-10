using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ground_Controiler : MonoBehaviour
{
    public List<TerrainLayer> Graund;
    public List<TerrainLayer> Water;
    public Terrain terrain;
    // Start is called before the first frame update
    void Start()
    {
        terrain = GetComponent<Terrain>();
        var terrainData = terrain.terrainData;
        var alphaMaps = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);

       // terrainData.splatPrototypes.;
        for (int x = 0; x < alphaMaps.GetLength(0); x++)
            for (int z = 0; z < alphaMaps.GetLength(1); z++)
                for (int textureIndex = 0; textureIndex < 1; textureIndex++)
                {
                    // X альфамапы = Z глобальных координат
                    // Y альфамапы = X глобальных координат
                    if (textureIndex == 1)
                        alphaMaps[z, x, textureIndex] = 1;
                    else alphaMaps[z, x, textureIndex] = 0;
                }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
