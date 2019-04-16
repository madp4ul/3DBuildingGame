using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BlockGameClasses.RandomGeneration;
using BlockGameClasses;

namespace _1st3DGame
{
    class RandomGeneration : ARandomGeneration<Block>
    {
        private PerlinNoise PerlinNoise;

        //To configure
        public const int ChunksPerPlane = 10;
        private const int MaxLayerCount = 3;
        private const int MaxLayerDepthForLayer1 = 2;
        private const int MaxLayerDepthForOtherLayers = 4;
        private const int MaxPriorityVariation = 2;

        /// <summary>
        /// At what least percentage of noise the block gets filled with structures
        /// </summary>
        private const float DefaultRelativeNoiseFilledLowerBorder = 0.0f;
        /// <summary>
        /// at what biggest percentage the block gets filled with structures
        /// </summary>
        private const float DefaultRelativeNoiseFilledUpperBorder = 0.45f;
        private const float RelativeNoiseFilledVariation = 0.4f;

        #region configured by the programm
        //do not change
        /// <summary>
        /// higher layerdepth means more seeds, more seeds mean less performance (valid values go from 1 to (1 + sqrt(ChunkWidth)))
        /// on value 6 collision is not synced to visuals, dont use it!
        /// </summary>
        private int[] LayerDepths;

        /// <summary>
        /// a factor each layervalue gets multiplied with to make certain layers more important than others
        /// </summary>
        private int[] LayerPriority;

        private const int LowerRandomLimit = 0;
        private const int UpperRandomLimit = 100;
        private int RandomRange { get { return UpperRandomLimit - LowerRandomLimit; } }

        private float RelativeNoiseFilledLowerBorder;
        private float RelativeNoiseFilledUpperBorder;
        private float _MassSpanFactor;
        private float MassSpanFactor
        {
            get
            {
                return _MassSpanFactor;
            }
            set
            {
                this._MassSpanFactor = value;
                this.RelativeNoiseFilledLowerBorder = DefaultRelativeNoiseFilledLowerBorder * value;
                this.RelativeNoiseFilledUpperBorder = DefaultRelativeNoiseFilledUpperBorder * value;
            }
        }
        #endregion

        public RandomGeneration(int chunkWidth, Point3D chunkPos, int seed)
            : base(chunkWidth, chunkPos)
        {
            this.PerlinNoise = new PerlinNoise(this.ChunkWidth);
            SetGenerationVariables(seed);
            SetRandoms();
        }

        public override void Begin()
        {
            base.Begin();
            this.lowerMassBound = LowerRandomLimit + RelativeNoiseFilledLowerBorder * RandomRange;
            this.UpperMassBound = LowerRandomLimit + RelativeNoiseFilledUpperBorder * RandomRange;
        }

        public override Block GetBlock(int x, int y, int z)
        {
            return GetBlockFromNoise(GetNoise(new Point3D(x, y, z)));
        }

        #region Biom-Generation
        private void SetGenerationVariables(int seed)
        {
            Random rnd = new Random(
                (new Random((int)Math.Floor(GetSurfaceLevel(this.ChunkPosition))).Next() +
                new Random(seed).Next()) % int.MaxValue);

            //Set noise-layer count
            int layerCount = rnd.Next(2, MaxLayerCount + 1);
            if (layerCount > MaxLayerCount)
                layerCount--;

            this.LayerDepths = new int[layerCount];
            this.LayerPriority = new int[layerCount];
            //Set layerDepths and factors
            for (int layerNumber = 0; layerNumber < layerCount; layerNumber++)
            {
                if (layerNumber == 0)
                {
                    this.LayerDepths[layerNumber] = rnd.Next(1, MaxLayerDepthForLayer1 + 1);
                    if (this.LayerDepths[layerNumber] > MaxLayerDepthForOtherLayers)
                        this.LayerDepths[layerNumber]--;
                }
                else
                {
                    this.LayerDepths[layerNumber] = rnd.Next(1, MaxLayerDepthForOtherLayers);
                    if (this.LayerDepths[layerNumber] > MaxLayerDepthForOtherLayers)
                        this.LayerDepths[layerNumber]--;
                }
                this.LayerPriority[layerNumber] = rnd.Next(1, MaxPriorityVariation + 1);
                if (this.LayerPriority[layerNumber] > MaxPriorityVariation)
                    this.LayerPriority[layerNumber]--;
            }

            //Set which noises get filled wih mass
            //how much percentage of noise is mass relative to default
            this.MassSpanFactor =
                (float)rnd.NextDouble() * RelativeNoiseFilledVariation * 2 +
                (1 - RelativeNoiseFilledVariation);
        }
        #endregion

        #region Noise-Processing, BlockSetter etc.

        /// <summary>
        /// noise supported from 0-100
        /// </summary>
        /// <param name="noise"></param>
        /// <returns></returns>
        private Block GetBlockFromNoise(float noise)
        {
            if (noise >= lowerMassBound &&
                noise <= UpperMassBound)
                return GetBlockOfFilledMass((noise - LowerRandomLimit) / (DefaultRelativeNoiseFilledUpperBorder * RandomRange));
            else
                return new Block(BlockTypes.Air);
        }

        private Block GetBlockOfFilledMass(float percentNoise)
        {
            const float percentageStone = 0.9f;

            if (percentNoise <= percentageStone * MassSpanFactor)
                return new Block(BlockTypes.Stone);
            else
                return new Block(BlockTypes.GrassDirt);
        }

        #endregion

        #region Perlin-Noise
        private float GetNoise(Point3D blockIndex)
        {
            float result = 0;
            int divider = 0;
            for (int i = 0; i < LayerDepths.Length; i++)
            {
                if (LayerPriority[i] != 0)
                {
                    result += GetLayerNoise(blockIndex, i) * LayerPriority[i];
                    divider += Math.Abs(LayerPriority[i]);
                }
            }
            return result / divider;
        }

        private float GetLayerNoise(Point3D blockIndex, int layer)
        {
            //index in random-array
            Point3D minRandomIndex = new Point3D(
                (int)((float)blockIndex.X / BlocksBetweenRandoms[layer]),
                (int)((float)blockIndex.Y / BlocksBetweenRandoms[layer]),
                (int)((float)blockIndex.Z / BlocksBetweenRandoms[layer]));

            //lerp percentages
            Vector3 percentages = new Vector3(
                (float)(blockIndex.X - (minRandomIndex.X * BlocksBetweenRandoms[layer])) / BlocksBetweenRandoms[layer],
                (float)(blockIndex.Y - (minRandomIndex.Y * BlocksBetweenRandoms[layer])) / BlocksBetweenRandoms[layer],
                (float)(blockIndex.Z - (minRandomIndex.Z * BlocksBetweenRandoms[layer])) / BlocksBetweenRandoms[layer]);

            return
                MathHelper.Lerp(//upper with lower
                //lower
                    MathHelper.Lerp(//left with right
                //left
                        MathHelper.Lerp(//front with back
                //back
                        Randoms[layer][minRandomIndex.X, minRandomIndex.Y, minRandomIndex.Z],
                //front
                        Randoms[layer][minRandomIndex.X, minRandomIndex.Y, minRandomIndex.Z + 1],
                        percentages.Z)
                    ,
                //right
                        MathHelper.Lerp(//front with back
                //back
                        Randoms[layer][minRandomIndex.X + 1, minRandomIndex.Y, minRandomIndex.Z],
                //front
                        Randoms[layer][minRandomIndex.X + 1, minRandomIndex.Y, minRandomIndex.Z + 1],
                        percentages.Z)
                    , percentages.X)
                ,
                //upper
                    MathHelper.Lerp(//left with right
                //left
                        MathHelper.Lerp(//front with back
                //back
                        Randoms[layer][minRandomIndex.X, minRandomIndex.Y + 1, minRandomIndex.Z],
                //front
                        Randoms[layer][minRandomIndex.X, minRandomIndex.Y + 1, minRandomIndex.Z + 1],
                        percentages.Z)
                    ,
                //right
                        MathHelper.Lerp(//front with back
                //back
                        Randoms[layer][minRandomIndex.X + 1, minRandomIndex.Y + 1, minRandomIndex.Z],
                //front
                        Randoms[layer][minRandomIndex.X + 1, minRandomIndex.Y + 1, minRandomIndex.Z + 1],
                        percentages.Z)
                    , percentages.X)
                , percentages.Y);
        }
        #endregion

        #region Random-Management

        int[][, ,] Randoms;
        int[] RandomsPerSide;
        int[] BlocksBetweenRandoms;
        private void SetRandoms()
        {
            int[][, ,] seeds = SetSeeds();
            Randoms = new int[seeds.Length][, ,];
            RandomsPerSide = new int[seeds.Length];
            BlocksBetweenRandoms = new int[seeds.Length];
            for (int i = 0; i < seeds.Length; i++)
            {
                int[] seedLength = new int[] { seeds[i].GetLength(0), seeds[i].GetLength(1), seeds[i].GetLength(2) };
                RandomsPerSide[i] = seedLength[0];
                BlocksBetweenRandoms[i] = ChunkWidth / (RandomsPerSide[i] - 1);
                Randoms[i] = new int[seedLength[0], seedLength[1], seedLength[2]];

                for (int x = 0; x < seedLength[0]; x++)
                    for (int y = 0; y < seedLength[1]; y++)
                        for (int z = 0; z < seedLength[2]; z++)
                            Randoms[i][x, y, z] = new Random(seeds[i][x, y, z]).Next(LowerRandomLimit, UpperRandomLimit);
            }
        }

        private int[][, ,] SetSeeds()
        {
            //14 guarantees regular planes, dont change
            const int seedOffset = 14;

            int[][, ,] InnerSeeds = new int[LayerDepths.Length][, ,];
            int maxSeedCount = (int)Math.Pow(2, LayerDepths.Max() - 1) + 1;
            int depthOfHighestPriorityPlane = (int)Math.Pow(2, LayerDepths.Max() - 1);

            for (int i = 0; i < LayerDepths.Length; i++)
            {
                int seedCount = (int)Math.Pow(2, LayerDepths[i] - 1) + 1;
                int seedStep = (maxSeedCount - 1) / (seedCount - 1);
                InnerSeeds[i] = new int[seedCount, seedCount, seedCount];

                for (int x = 0; x < seedCount; x++)
                    for (int y = 0; y < seedCount; y++)
                        for (int z = 0; z < seedCount; z++)
                        {
                            //scaled GridIndex (used as chunkseed later)
                            int xComp = (int)(ChunkPosition.X * (maxSeedCount - 1) + x * seedStep) % int.MaxValue;
                            int yComp = (int)(ChunkPosition.Y * (maxSeedCount - 1) + y * seedStep) % int.MaxValue;
                            int zComp = (int)(ChunkPosition.Z * (maxSeedCount - 1) + z * seedStep) % int.MaxValue;

                            InnerSeeds[i][x, y, z] = (int)(
                                CoordinateSeedCombiner(xComp, yComp, zComp, i, depthOfHighestPriorityPlane) +
                                seedOffset
                                ) % int.MaxValue;
                        }
            }
            return InnerSeeds;
        }

        /// <summary>
        /// combines the three components of the chunkcoordinate to a single int
        /// </summary>
        /// <param name="x">x</param>
        /// <param name="y">y</param>
        /// <param name="z">z</param>
        /// <param name="layerNumber">index in layerArray of the current layer</param>
        /// <param name="twoPowHighestLayerDepthMinusOne">2^(layer with highest priority)</param>
        /// <returns>random seed</returns>
        private int CoordinateSeedCombiner(int x, int y, int z, int layerNumber, int twoPowHighestLayerDepthMinusOne)
        {
            //chunksPerForcedPlane has to be multiplied by 2^(id of layer with highest priority)
            int randomFactorX = new Random(x).Next(11);
            int randomFactorZ = new Random(z).Next(2437);
            return (int)(((x * randomFactorZ + z * randomFactorX) * (layerNumber + 1) *
                (y % (ChunksPerPlane * twoPowHighestLayerDepthMinusOne))) % int.MaxValue);
        }
        #endregion

        public static float GetSurfaceLevel(Point3D chunkPosition)
        {
            if (chunkPosition != null)
                return ((float)chunkPosition.Y / ChunksPerPlane);
            else
                return 0;
        }
        public static int GetSurfaceLevelInt(Point3D chunkPosition)
        {
            if (chunkPosition != null)
                return (int)Math.Floor(GetSurfaceLevel(chunkPosition));
            else
                return 0;
        }
    }
}