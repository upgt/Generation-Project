using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    public class TreeInfo
    {
        public static float maxScale;
        public static float minScale;
        public static int countCast;
        public TreeInstance TreeInst { get; private set; }
        public float aloneRadius;
        public float parentRadius;
        private int castType;

        int GetTreeCast(float scale)
        {
            float castZone = maxScale - minScale;
            return 1 - (int)(countCast * (TreeInst.heightScale - minScale) / castZone);
        }

        //конструктор в котором радиус одиночества и родительский радиус зависят от размеров дерева scale
        public TreeInfo(Vector3 pos, int prototypeIndex, float scale)
        {
            TreeInst = new TreeInstance
            {
                position = pos,
                prototypeIndex = prototypeIndex,
                heightScale = scale,
                widthScale = scale,
                lightmapColor = Color.white,
                color = Color.white
            };
            aloneRadius = scale * 5f;
            parentRadius = scale * 9f;
            castType = GetTreeCast(scale);

        }

        public TreeInfo(Vector3 pos, int prototypeIndex, float scale, float aloneR = 1, float parentR = 10)
            : this(pos, prototypeIndex, scale)
        {
            aloneRadius = aloneR;
            parentRadius = parentR;
        }

        public static implicit operator Vector3(TreeInfo a)
        {
            return a.TreeInst.position;
        }

        public static implicit operator TreeInfo(Vector3 a)
        {
            return new TreeInfo(a, 0, 0);
        }

        public static implicit operator Vector2(TreeInfo a)
        {
            Vector2 result = new Vector2(a.TreeInst.position.x, a.TreeInst.position.z);
            return result;
        }

        public static implicit operator TreeInstance(TreeInfo a)
        {
            return a.TreeInst;
        }
    }
}
