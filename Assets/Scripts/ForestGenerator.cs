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
        void Start()
        {
            textures.StartGroundControl();
            roads.StartRoads();
        }
    }
}
