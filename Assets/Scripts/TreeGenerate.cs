using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;

namespace Assets.Scripts
{

    public delegate float Calculated(int weight, int height);

    class TreeGenerate : Generator
    {
        public float minTreeScale = 0.4f; // не более 0,9
        public int maxDist = 4;
        public int minDist = 3;
        public int Casts;
        public List<List<TreeInfo>> Trees;
        public TerrainGenerator hMap;
        public int broadSmall = 100; // 0 - только широколиственные, 100 - только мелколиственные 

        private Terrain terrain;
        private List<int> smallDeciduous;
        private List<int> broadDeciduous;
        private float[,] heightMap;
        private int height;
        private int width;
        public List<Vector3> QuestZones; // x = x; y = radius; z = z
        private float xTerrain = 0;
        private float zTerrain = 0;
        private int castCount;
        System.Random rn;
        private const float MAX_TREE_SCALE = 3;
        
        public int CastCount
        {
            get { return castCount; }
            set
            {
                castCount = value;
                if (value > 8)
                {
                    castCount = 8;
                }
                else if (value < 2)
                {
                    castCount = 2;
                }
            }
        }
        
        public Terrain Terrain
        {
            get
            {
                return terrain;
            }

            private set
            {
                terrain = value;
            }
        }

        public TreeGenerate(float[,] heightMap)
        {
            this.heightMap = heightMap;
        }

        void TestDesiduous(int protorypeLength)
        {
            for (int i = 0; i < protorypeLength; i++)
            {
                if (Terrain.terrainData.treePrototypes[i].prefab.GetComponent<deciduousTree>() == null)
                {
                    throw new ArgumentException("Tree in terrain is not desiduous");
                }
            }
        }

        void Desiduous(int protorypeLength)
        {
            smallDeciduous = new List<int>();
            broadDeciduous = new List<int>();
            for (int i = 0; i < protorypeLength; i++)
            {
                if (!Terrain.terrainData.treePrototypes[i].prefab.GetComponent<deciduousTree>().isBroadLeaved)
                {
                    smallDeciduous.Add(i);
                }
                else
                {
                    broadDeciduous.Add(i);
                }
                
            }
        }

        private void Start()
        {
            TreeInfo.maxScale = MAX_TREE_SCALE;
            TreeInfo.minScale = minTreeScale;
            Trees = new List<List<TreeInfo>>();
            Terrain = GetComponent<Terrain>();
            rn = new System.Random();
            QuestZones = new List<Vector3>();
            height = (int)Terrain.terrainData.size.x;
            width = (int)Terrain.terrainData.size.z;
            TerrainData ter = terrain.terrainData;
            heightMap = ter.GetHeights(0,0,ter.heightmapWidth, ter.heightmapHeight);
            int protorypeLength = Terrain.terrainData.treePrototypes.Length;
            TestDesiduous(protorypeLength);
            Desiduous(protorypeLength);
            // Работает только тогда когда в массиве деревьев есть хотя бы одно деревоVector3 position = new Vector3(xTerrain, 0, zTerrain);                     
            if (protorypeLength > 0)
            {
                Vector3 positionEnd = new Vector3(width + xTerrain, 0, height + zTerrain);
                GenTree(xTerrain, zTerrain);
            }
        }

        bool IsPointInZone(Vector2 point, Vector3 zone)
        {
            float r = zone.y;
            if (zone.x + r > point.x && zone.x - r < point.x
            && zone.z + r > point.y && zone.z - r < point.y)
            {
                return true;
            }
            return false;
        }

        bool IsPointInZones(Vector2 point, List<Vector3> zone)
        {
            bool isPointInZone = false;

            for (int i = 0; i < zone.Count && !isPointInZone; i++)
            {
                if (IsPointInZone(point, zone[i]))
                {
                    isPointInZone = true;
                }
            }
            return isPointInZone;
        }

        private void GenCasts()
        {
            castCount = Casts;
            Casts = castCount;
            TreeInfo.countCast = Casts;

            for (int i = 0; i < Casts; i++)
            {
                Trees.Add(new List<TreeInfo>());
            }
        }

        private void DrawTreeCast()
        {
            for (int i = 0; i < castCount; i++)
            {
                for (int j = 0; j < Trees[i].Count; j++)
                {
                    terrain.AddTreeInstance(Trees[i][j]);
                }
            }
            terrain.Flush();
        }

        void GenTreesQuestZones(int cast)
        {
            for (int i = 0; i < Trees[cast].Count; i++)
            {
                Vector3 AloneRadius = Trees[cast][i];
                AloneRadius.y = Math.Max(Trees[cast][i].aloneRadius, minDist);
                QuestZones.Add(AloneRadius);
            }
        }

        int GenIndexByParents(int castIndex, Vector2 currentChildIndx)
        {
            List<int> prIndexCollection = SearchParrent(castIndex, currentChildIndx);

            int chance = rn.Next(1, 99);

            if (prIndexCollection.Count == 0)
            {
                int numTreeType;
                if (chance > broadSmall)
                {
                    numTreeType = rn.Next(0, broadDeciduous.Count);
                    //broad
                    return broadDeciduous[numTreeType];
                }
                else
                {
                    numTreeType = rn.Next(0, smallDeciduous.Count);
                    //small
                    return smallDeciduous[numTreeType];
                }
            }
            ///
            
            int countChanse = 0;
            int j, smallChance;
            List<int> smallLeafParrentPrototypeIndexs = new List<int>();
            List<int> broadLeafParrentPrototypeIndexs = new List<int>();

            for (int i = 0; i < prIndexCollection.Count; i++)
            {
                j = prIndexCollection[i];
                bool isBroadLeaf = Terrain.terrainData.treePrototypes[j].prefab.GetComponent<deciduousTree>().isBroadLeaved;
                if (isBroadLeaf)
                {
                    countChanse += 100 - broadSmall;
                    broadLeafParrentPrototypeIndexs.Add(j);
                }
                else
                {
                    countChanse += broadSmall;
                    smallLeafParrentPrototypeIndexs.Add(j);
                }
            }
            smallChance = (broadSmall * 100 * smallLeafParrentPrototypeIndexs.Count) / countChanse;

            if (chance > smallChance)
            {
                //broad
                return broadLeafParrentPrototypeIndexs[rn.Next(broadLeafParrentPrototypeIndexs.Count)];
            }
            else
            {
                //small
                return smallLeafParrentPrototypeIndexs[rn.Next(smallLeafParrentPrototypeIndexs.Count)];
            }
        }

        private List<int> SearchParrent(int castIndex, Vector2 currentChildIndx)
        {
            List<int> prIndexCollection = new List<int>();
            if (castIndex > 0)
            {
                for (int i = 0; i < Trees[0].Count; i++)
                {
                    Vector3 param = Trees[0][i];
                    param.y = Trees[0][i].parentRadius;
                    if (IsPointInZone(currentChildIndx, param))
                    {
                        prIndexCollection.Add(Trees[0][i].TreeInst.prototypeIndex);
                    }
                }
            }
            return prIndexCollection;
        }


        void AddTreeCast(int castIndx, float[,] noise)
        {
            float cTS = (MAX_TREE_SCALE - minTreeScale) / castCount;
            int reverseCastIndex = castCount - castIndx - 1;
            float minCastParam = (cTS * reverseCastIndex) + minTreeScale;
            float maxCastParam = (cTS * (reverseCastIndex + 1)) + minTreeScale;

            for (int x = 0; x < width / minDist; x++)
            {
                for (int z = 0; z < height / minDist; z++)
                {

                    int locNoise = 2 * (rn.Next(minDist * 2) - minDist);
                    float xNoise = Mathf.Abs(x + locNoise*0.1f);
                    float zNoise = Mathf.Abs(z + locNoise*0.1f);
                    if (zNoise < height / minDist && xNoise < width / minDist)
                    {
                        var alphaMaps = terrain.terrainData.GetAlphamaps(0, 0, terrain.terrainData.alphamapWidth, terrain.terrainData.alphamapHeight);
                        int cTextureOnTerH = terrain.terrainData.alphamapHeight / height;
                        int cTextureOnTerW = terrain.terrainData.alphamapWidth / width;
                        int iTextureGraund = 1; /// индекс текстуры земли КОСТЫЛЬ

                        int xCoord = minDist * (int)xNoise;
                        if (xCoord > 255)
                        {
                            xCoord = 255;
                        }
                        Vector2 coord = new Vector2(xCoord, zNoise * minDist);
                        float xD = xNoise * minDist / width;
                        float zD = zNoise * minDist / height;

                        if (noise[(int)xNoise, (int)zNoise] > minCastParam && noise[(int)xNoise, (int)zNoise] <= maxCastParam)
                        {
                            if (!IsPointInZones(coord, QuestZones) &&
                                (alphaMaps[(int)zNoise * minDist * cTextureOnTerW, (int)xNoise * minDist * cTextureOnTerH, iTextureGraund] < 1))
                            {
                                var position = new Vector3(xD, heightMap[xCoord, z], zD);
                                int prototypeIndex = GenIndexByParents(castIndx, coord);
                                var tree = new TreeInfo(position, prototypeIndex, noise[(int)xNoise, (int)zNoise]);
                                Trees[castIndx].Add(tree);
                            }
                        }

                    }
                }
            }
        }

        void GenTree(float xTer, float zTer)
        {
            GenCasts();
            Calculated calculated = new Calculated(CalculatePerlin);
            float[,] whiteNoise = CreateHeights(width / minDist, height / minDist, calculated);
            for (int i = 0; i < castCount; i++)
            {
                AddTreeCast(i, whiteNoise);
                GenTreesQuestZones(i);
            }
            DrawTreeCast();
        }

        public override float[,] CreateHeights(int w, int h, Calculated calculate)
        {
            float[,] heights = new float[w, h];
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    heights[x, y] = calculate.Invoke(x, y);
                }
            }
            return heights;
        }

        public override float CalculateHeight(int x, int y)
        {
            return 0.001f * rn.Next(0, 1000);
        }

        public float CalculatePerlin(int x, int y)
        {
            float xCoord = (float)x * minDist / (width / minDist) * 20 + 100;
            float yCoord = (float)y * minDist  / (height / minDist) * 20 + 100;
            return Mathf.PerlinNoise(xCoord,yCoord);
        }


        private void Update()
        {
            Casts = castCount;
        }
    }
}
