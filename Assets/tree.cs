using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tree : MonoBehaviour
{
    public Terrain ttt;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        TreeInstance rrr = new TreeInstance
        {
            prototypeIndex = 0,
            heightScale = 30,
            widthScale = 100,
            position = new Vector3(0.5f, 0.5f, 0.5f),
            color = Color.white,
            lightmapColor = Color.white,
            rotation = 0f
        };

        ttt.terrainData.SetTreeInstance(0, rrr);
    }
}
