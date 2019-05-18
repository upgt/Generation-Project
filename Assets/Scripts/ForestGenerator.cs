using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets.Texture;
namespace Assets.Scripts
{
    
    class ForestGenerator : MonoBehaviour
    {
        public Ground_Controiler textures;
        public RoadsCreator roads;
        public TreeGenerate tree;
        public TerrainGenerator tg;
        void Start()
        {
            tg.StartTG();
            textures.StartGroundControl(tg);
            tree.treeStart(tg,textures);
           // roads.StartRoads();
        }
    }
}
