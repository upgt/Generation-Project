using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
            roads.StartRoads(textures, tg);
            tree.treeStart(tg, textures, roads);
        }
    }
}
