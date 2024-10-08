﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using BlockGameClasses;
using BlockGameClasses.ChunkData;
using BlockGameClasses.RandomGeneration;
using BlockGame3D.Model.BlockStore;
using BlockGame3D.Model.VisualBuffer;

namespace BlockGame3D
{
    class Chunk : IDisposable
    {
        public const int ChunkWidth = 32;

        #region Values regarding random-generation of bioms and chunks
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

        #endregion
        #endregion

        #region static information about chunks
        public static int BlocksPerChunk { get { return ChunkWidth * ChunkWidth * ChunkWidth; } }
        public static int VerticesPerChunk { get { return BlocksPerChunk * Block.VerticesCount; } }
        public static int IndicesPerChunk { get { return BlocksPerChunk * Block.IndicesCount; } }
        public static readonly Vector3 Size = new Vector3(ChunkWidth * Block.Sidelength,
                    ChunkWidth * Block.Sidelength,
                    ChunkWidth * Block.Sidelength);
        public static readonly Vector3 Radius = new Vector3(0.5f * ChunkWidth);
        #endregion
        #region readonly values of that specific chunk
        public readonly Vector3 MinCorner;
        public readonly Matrix WorldMatrix;
        public readonly Point3D GridIndex;
        public readonly BoundingBox BBox;
        public Vector3 Center { get { return MinCorner + 0.5f * Size; } }
        #endregion

        //public Block[, ,] AllBlocks { get; private set; }
        public IBlockStore AllBlocks { get; private set; }

        #region Buffer and Surroundings
        VisualBuffer<VertexPositionIndexedNormalTexture> BufferLeft;
        VisualBuffer<VertexPositionIndexedNormalTexture> BufferRight;
        VisualBuffer<VertexPositionIndexedNormalTexture> BufferTop;
        VisualBuffer<VertexPositionIndexedNormalTexture> BufferBottom;
        VisualBuffer<VertexPositionIndexedNormalTexture> BufferFront;
        VisualBuffer<VertexPositionIndexedNormalTexture> BufferBack;


        public int VerticesCount { get; private set; }
        public int IndicesCount { get; private set; }

        public bool BufferSet { get; private set; }
        public bool SettingBuffer { get; private set; }

        readonly GraphicsDevice Device;

        public Chunk ChunkAbove = null;
        public Chunk ChunkBelow = null;
        public Chunk ChunkLeft = null;
        public Chunk ChunkRight = null;
        public Chunk ChunkInfront = null;
        public Chunk ChunkBehind = null;

        #endregion

        public Chunk(GraphicsDevice device, Point3D gridPosChunk, int seed)
        {
            BufferSet = false;
            this.GridIndex = gridPosChunk;
            this.MinCorner = new Vector3(gridPosChunk.X * Size.X, gridPosChunk.Y * Size.Y, gridPosChunk.Z * Size.Z);
            this.WorldMatrix = Matrix.CreateTranslation(MinCorner);
            //x=width,y=length,z=height

            //this.AllBlocks = new Block[ChunkWidth, ChunkWidth, ChunkWidth];
            this.AllBlocks = new SpaceEfficientBlockStore(ChunkWidth, ChunkWidth, ChunkWidth);

            this.BBox = new BoundingBox(MinCorner, MinCorner + Size);
            this.Device = device;
            //Set information
            InitializeBlocks(seed);
        }

        ~Chunk()
        {
            Dispose();
        }

        public void Dispose()
        {
            BufferLeft?.Dispose();
            BufferRight?.Dispose();
            BufferTop?.Dispose();
            BufferBottom?.Dispose();
            BufferFront?.Dispose();
            BufferBack?.Dispose();
        }

        #region Biom-Generation
        private void SetGenerationVariables(int seed)
        {
            Random rnd = new Random(
                (new Random((int)Math.Floor(GetSurfaceLevel(this.GridIndex))).Next() +
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
        }
        #endregion

        #region Randomgeneration
        private void InitializeBlocks(int seed)
        {
            RandomGeneration rg = new RandomGeneration(ChunkWidth, this.GridIndex, seed);
            FillWithBlocks(rg);
        }

        private void FillWithBlocks(IRandomGenerierung<Block> generator)
        {
            generator.Begin();
            for (int x = 0; x < ChunkWidth; x++)
                for (int y = ChunkWidth - 1; y >= 0; y--)
                    for (int z = 0; z < ChunkWidth; z++)
                    {
                        Block b = generator.GetBlock(x, y, z);
                        if (b.Type != BlockTypes.Air)
                        { }
                        this.AllBlocks[x, y, z] = b;
                    }
            generator.End();
        }
        #endregion

        #region Block-Management-Methods
        public Block ChangeBlock(Point3D index, Block newBlock)
        {
            if (WithinChunk(index.X, index.Y, index.Z))
            {
                Block oldBlock = this.AllBlocks[index.X, index.Y, index.Z];
                this.AllBlocks[index.X, index.Y, index.Z] = newBlock;
                SetHidden(index.X, index.Y, index.Z);

                if (WithinChunk(index.X - 1, index.Y, index.Z))
                    SetHidden(index.X - 1, index.Y, index.Z);
                else
                {
                    Point3D coord = ChunkLeft.ConvertBlockIndex(GridIndex, index.AddX(-1));
                    ChunkLeft.SetHidden(coord.X, coord.Y, coord.Z);
                    ChunkLeft.SetBuffers();
                }
                if (WithinChunk(index.X + 1, index.Y, index.Z))
                    SetHidden(index.X + 1, index.Y, index.Z);
                else
                {
                    Point3D coord = ChunkRight.ConvertBlockIndex(GridIndex, index.AddX(1));
                    ChunkRight.SetHidden(coord.X, coord.Y, coord.Z);
                    ChunkRight.SetBuffers();
                }

                if (WithinChunk(index.X, index.Y - 1, index.Z))
                    SetHidden(index.X, index.Y - 1, index.Z);
                else
                {
                    Point3D coord = ChunkBelow.ConvertBlockIndex(GridIndex, index.AddY(-1));
                    ChunkBelow.SetHidden(coord.X, coord.Y, coord.Z);
                    ChunkBelow.SetBuffers();
                }
                if (WithinChunk(index.X, index.Y + 1, index.Z))
                    SetHidden(index.X, index.Y + 1, index.Z);
                else
                {
                    Point3D coord = ChunkAbove.ConvertBlockIndex(GridIndex, index.AddY(1));
                    ChunkAbove.SetHidden(coord.X, coord.Y, coord.Z);
                    ChunkAbove.SetBuffers();
                }

                if (WithinChunk(index.X, index.Y, index.Z - 1))
                    SetHidden(index.X, index.Y, index.Z - 1);
                else
                {
                    Point3D coord = ChunkBehind.ConvertBlockIndex(GridIndex, index.AddZ(-1));
                    ChunkBehind.SetHidden(coord.X, coord.Y, coord.Z);
                    ChunkBehind.SetBuffers();
                }
                if (WithinChunk(index.X, index.Y, index.Z + 1))
                    SetHidden(index.X, index.Y, index.Z + 1);
                else
                {
                    Point3D coord = ChunkInfront.ConvertBlockIndex(GridIndex, index.AddZ(1));
                    ChunkInfront.SetHidden(coord.X, coord.Y, coord.Z);
                    ChunkInfront.SetBuffers();
                }

                SetBuffers();
                return oldBlock;
            }
            return null;
        }

        private void SetAllHidden()
        {
            for (int x = 0; x < this.AllBlocks.Size.X; x++)
                for (int y = 0; y < this.AllBlocks.Size.Y; y++)
                    for (int z = 0; z < this.AllBlocks.Size.Z; z++)
                        if (AllBlocks[x, y, z].Visible)
                            SetHidden(x, y, z);
        }

        private void SetHidden(int x, int y, int z)
        {
            bool notHidden =
                !HoldsVisibleBlock(x - 1, y, z) ||
                !HoldsVisibleBlock(x + 1, y, z) ||
                !HoldsVisibleBlock(x, y - 1, z) ||
                !HoldsVisibleBlock(x, y + 1, z) ||
                !HoldsVisibleBlock(x, y, z - 1) ||
                !HoldsVisibleBlock(x, y, z + 1);

            this.AllBlocks[x, y, z] = new Block(this.AllBlocks[x, y, z].Type) { NotHidden = notHidden };
        }

        /// <summary>
        /// returns if the block at the given position is visible. if the blockindex is not within this chunk it looks for the block in the surrounding chunks
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        private bool HoldsVisibleBlock(int x, int y, int z)
        {
            //if Not within this chunk the chunks nearby have to be checked for visible blocks
            if (!WithinChunk(x, y, z))
            {
                //left or right
                if (x < 0 && y >= 0 && y < ChunkWidth && z >= 0 && z < ChunkWidth)
                {
                    if (ChunkLeft != null)
                    {
                        Point3D leftIndex = ChunkLeft.ConvertBlockIndex(GridIndex, new Point3D(x, y, z));
                        return ChunkLeft.AllBlocks[leftIndex.X, leftIndex.Y, leftIndex.Z].Visible;
                    }
                }
                else if (x >= ChunkWidth && y >= 0 && y < ChunkWidth && z >= 0 && z < ChunkWidth)
                {
                    if (ChunkRight != null)
                    {
                        Point3D rightIndex = ChunkRight.ConvertBlockIndex(GridIndex, new Point3D(x, y, z));
                        return ChunkRight.AllBlocks[rightIndex.X, rightIndex.Y, rightIndex.Z].Visible;
                    }
                }
                //below or above 
                else if (x >= 0 && x < ChunkWidth && y < 0 && z >= 0 && z < ChunkWidth)
                {
                    if (ChunkBelow != null)
                    {
                        Point3D belowIndex = ChunkBelow.ConvertBlockIndex(GridIndex, new Point3D(x, y, z));
                        return ChunkBelow.AllBlocks[belowIndex.X, belowIndex.Y, belowIndex.Z].Visible;
                    }
                }
                else if (x >= 0 && x < ChunkWidth && y >= ChunkWidth && z >= 0 && z < ChunkWidth)
                {
                    if (ChunkAbove != null)
                    {
                        Point3D aboveIndex = ChunkAbove.ConvertBlockIndex(GridIndex, new Point3D(x, y, z));
                        return ChunkAbove.AllBlocks[aboveIndex.X, aboveIndex.Y, aboveIndex.Z].Visible;
                    }
                }
                //behind or infront
                else if (x >= 0 && x < ChunkWidth && y >= 0 && y < ChunkWidth && z < 0)
                {
                    if (ChunkBehind != null)
                    {
                        Point3D behindIndex = ChunkBehind.ConvertBlockIndex(GridIndex, new Point3D(x, y, z));
                        return ChunkBehind.AllBlocks[behindIndex.X, behindIndex.Y, behindIndex.Z].Visible;
                    }
                }
                else if (x >= 0 && x < ChunkWidth && y >= 0 && y < ChunkWidth && z >= ChunkWidth)
                {
                    if (ChunkInfront != null)
                    {
                        Point3D infrontIndex = ChunkInfront.ConvertBlockIndex(GridIndex, new Point3D(x, y, z));
                        return ChunkInfront.AllBlocks[infrontIndex.X, infrontIndex.Y, infrontIndex.Z].Visible;
                    }
                }
                return false;
            }
            else
                return this.AllBlocks[x, y, z].Visible;
        }

        public bool WithinChunk(int x, int y, int z)
        {
            return !(x < 0 || y < 0 || z < 0 ||
                x >= this.AllBlocks.Size.X ||
                y >= this.AllBlocks.Size.Y ||
                z >= this.AllBlocks.Size.Z);
        }

        private Vector3 BlockMinCorner(int x, int y, int z)
        {
            return this.MinCorner + new Vector3(x * Block.Sidelength, y * Block.Sidelength, z * Block.Sidelength);
        }

        public Point3D BlockIndex(Vector3 position)
        {
            Point3D index = Point3D.Empty;
            Vector3 posInChunk = position - MinCorner;
            index.X = (int)(posInChunk.X / Block.Sidelength);
            index.Y = (int)(posInChunk.Y / Block.Sidelength);
            index.Z = (int)(posInChunk.Z / Block.Sidelength);
            return index;
        }

        public Point3D ConvertBlockIndex(Point3D chunkIndex, Point3D blockIndex)
        {

            Point3D chunkIndexOffset = new Point3D(
                this.GridIndex.X - chunkIndex.X,
                this.GridIndex.Y - chunkIndex.Y,
                this.GridIndex.Z - chunkIndex.Z);

            return new Point3D(
                blockIndex.X - chunkIndexOffset.X * ChunkWidth,
                blockIndex.Y - chunkIndexOffset.Y * ChunkWidth,
                blockIndex.Z - chunkIndexOffset.Z * ChunkWidth);
        }
        #endregion

        #region Player-Collision
        public List<BoundingBox> GetIntersectingBlockBoxes(BoundingBox bbox)
        {
            Point3D[] intersectingBlockIndices = GetIntersectingBlockIndices(bbox);
            List<BoundingBox> boxes = new List<BoundingBox>();

            for (int i = 0; i < intersectingBlockIndices.Length; i++)
                if (AllBlocks[
                    intersectingBlockIndices[i].X,
                    intersectingBlockIndices[i].Y,
                    intersectingBlockIndices[i].Z]
                    .CollidesWithPlayer)
                    boxes.Add(GetBlockBBox(
                        intersectingBlockIndices[i].X,
                        intersectingBlockIndices[i].Y,
                        intersectingBlockIndices[i].Z));
            return boxes;
        }

        private Point3D[] GetIntersectingBlockIndices(BoundingBox bbox)
        {
            Point3D minIndex = BlockIndex(bbox.Min - new Vector3(0.1f));
            Point3D maxIndex = BlockIndex(bbox.Max + new Vector3(0.1f));
            if (minIndex.X < 0) minIndex.X = 0;
            if (minIndex.Y < 0) minIndex.Y = 0;
            if (minIndex.Z < 0) minIndex.Z = 0;
            if (maxIndex.X >= this.AllBlocks.Size.X) maxIndex.X = this.AllBlocks.Size.X - 1;
            if (maxIndex.Y >= this.AllBlocks.Size.Y) maxIndex.Y = this.AllBlocks.Size.Y - 1;
            if (maxIndex.Z >= this.AllBlocks.Size.Z) maxIndex.Z = this.AllBlocks.Size.Z - 1;

            Point3D[] indices = new Point3D[(maxIndex.X - minIndex.X + 1) * (maxIndex.Y - minIndex.Y + 1) * (maxIndex.Z - minIndex.Z + 1)];
            int index = 0;
            for (int x = minIndex.X; x <= maxIndex.X; x++)
                for (int y = minIndex.Y; y <= maxIndex.Y; y++)
                    for (int z = minIndex.Z; z <= maxIndex.Z; z++)
                    {
                        indices[index] = new Point3D(x, y, z);
                        index++;
                    }
            return indices;
        }

        public BoundingBox GetBlockBBox(int x, int y, int z)
        {
            Vector3 minCorner = BlockMinCorner(x, y, z);
            return new BoundingBox(minCorner, minCorner + new Vector3(Block.Sidelength));
        }
        #endregion

        #region Ray-Intersection

        /// <summary>
        /// Get the first block with is colliding or not colliding
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="maxDistance"></param>
        /// <param name="lookForCollidingBlock"></param>
        /// <returns></returns>
        public Point3D GetFirstIntersectedBlockIndex(Ray ray, float maxDistance, bool lookForCollidingBlock, out Vector3? intersection)
        {
            intersection = null;
            Point3D startIndex;
            float? dist = null;

            dist = (float)ray.Intersects(this.BBox);

            if (dist != null)
            {
                if ((float)dist == 0)
                    startIndex = new Point3D(BlockIndex(ray.Position));
                else
                    startIndex = new Point3D(BlockIndex((float)dist * ray.Direction + ray.Position));

                if (startIndex.X >= ChunkWidth)
                    startIndex.X--;
                if (startIndex.Y >= ChunkWidth)
                    startIndex.Y--;
                if (startIndex.Z >= ChunkWidth)
                    startIndex.Z--;

                Point3D blockPos = IntersectedCollidingBlockIndexRecursion(
                    ray, startIndex, maxDistance, lookForCollidingBlock, out float? distance);

                if (distance.HasValue)
                {
                    intersection = ray.Position + distance * ray.Direction;
                }
                return blockPos;
            }
            return null;
        }
        private Point3D IntersectedCollidingBlockIndexRecursion(Ray ray, Point3D startpoint, float maxDistance, bool lookForCollidingBlock, out float? distance)
        {
            ///////////////////////////////1
            distance = null;
            Point3D currentIndex = startpoint;
            bool nextIndexWithinChunk = true;
            while (nextIndexWithinChunk)
            {
                //If this block is *the answer*
                if (this.AllBlocks[currentIndex.X, currentIndex.Y, currentIndex.Z].CollidesWithPlayer == lookForCollidingBlock)
                {
                    BoundingBox bbox = GetBlockBBox(currentIndex.X, currentIndex.Y, currentIndex.Z);
                    distance = ray.Intersects(bbox);
                    return currentIndex;
                }
                bool nextIndexFound = false;

                //X Positive
                if (ray.Direction.X > 0)
                {
                    if (WithinChunk(currentIndex.X + 1, currentIndex.Y, currentIndex.Z))
                    {
                        BoundingBox bbox = GetBlockBBox(currentIndex.X + 1, currentIndex.Y, currentIndex.Z);
                        if ((ray.Position - bbox.Center()).Length() < maxDistance && ray.Intersects(bbox) != null)
                        {
                            currentIndex = currentIndex.AddX(1);
                            nextIndexFound = true;
                        }
                    }
                }//X Negative
                else if (ray.Direction.X < 0)
                {
                    if (WithinChunk(currentIndex.X - 1, currentIndex.Y, currentIndex.Z))
                    {
                        BoundingBox bbox = GetBlockBBox(currentIndex.X - 1, currentIndex.Y, currentIndex.Z);
                        if ((ray.Position - bbox.Center()).Length() < maxDistance && ray.Intersects(bbox) != null)
                        {
                            currentIndex = currentIndex.AddX(-1);
                            nextIndexFound = true;
                        }
                    }
                }
                //if next index has not yet been found
                if (!nextIndexFound)
                {
                    //Y Positive
                    if (ray.Direction.Y > 0)
                    {
                        if (WithinChunk(currentIndex.X, currentIndex.Y + 1, currentIndex.Z))
                        {
                            BoundingBox bbox = GetBlockBBox(currentIndex.X, currentIndex.Y + 1, currentIndex.Z);
                            if ((ray.Position - bbox.Center()).Length() < maxDistance && ray.Intersects(bbox) != null)
                            {
                                currentIndex = currentIndex.AddY(1);
                                nextIndexFound = true;
                            }
                        }
                    }//Y Negative
                    else if (ray.Direction.Y < 0)
                    {
                        if (WithinChunk(currentIndex.X, currentIndex.Y - 1, currentIndex.Z))
                        {
                            BoundingBox bbox = GetBlockBBox(currentIndex.X, currentIndex.Y - 1, currentIndex.Z);
                            if ((ray.Position - bbox.Center()).Length() < maxDistance && ray.Intersects(bbox) != null)
                            {
                                currentIndex = currentIndex.AddY(-1);
                                nextIndexFound = true;
                            }
                        }

                    }
                    //if next index has not yet been found
                    if (!nextIndexFound)
                    {
                        //Z Positive
                        if (ray.Direction.Z > 0)
                        {
                            if (WithinChunk(currentIndex.X, currentIndex.Y, currentIndex.Z + 1))
                            {
                                BoundingBox bbox = GetBlockBBox(currentIndex.X, currentIndex.Y, currentIndex.Z + 1);
                                if ((ray.Position - bbox.Center()).Length() < maxDistance && ray.Intersects(bbox) != null)
                                {
                                    currentIndex = currentIndex.AddZ(1);
                                    nextIndexFound = true;
                                }
                            }
                        }//Z Negative
                        else if (ray.Direction.Z < 0)
                        {
                            if (WithinChunk(currentIndex.X, currentIndex.Y, currentIndex.Z - 1))
                            {
                                BoundingBox bbox = GetBlockBBox(currentIndex.X, currentIndex.Y, currentIndex.Z - 1);
                                if ((ray.Position - bbox.Center()).Length() < maxDistance && ray.Intersects(bbox) != null)
                                {
                                    currentIndex = currentIndex.AddZ(-1);
                                    nextIndexFound = true;
                                }
                            }
                        }
                        //if no nextIndex has been found at all
                        if (!nextIndexFound)
                        {
                            nextIndexWithinChunk = false;
                        }
                    }
                }
            }
            return null;

            #region same with recursion
            /////////////////////////////////OLD
            //distance = null;
            //if (this.AllBlocks[startpoint.X, startpoint.Y, startpoint.Z].CollidesWithPlayer == lookForCollidingBlock)
            //{
            //    BoundingBox bbox = GetBlockBBox(startpoint.X, startpoint.Y, startpoint.Z);
            //    distance = ray.Intersects(bbox);
            //    return startpoint;
            //}

            ////X Positive
            //if (ray.Direction.X > 0)
            //{
            //    if (WithinChunk(startpoint.X + 1, startpoint.Y, startpoint.Z))
            //    {
            //        BoundingBox bbox = GetBlockBBox(startpoint.X + 1, startpoint.Y, startpoint.Z);
            //        if ((ray.Position - bbox.Center()).Length() < maxDistance && ray.Intersects(bbox) != null)
            //            return IntersectedCollidingBlockIndexRecursion(ray, startpoint.AddX(1), maxDistance, lookForCollidingBlock, out distance);
            //    }
            //}//X Negative
            //else if (ray.Direction.X < 0)
            //{
            //    if (WithinChunk(startpoint.X - 1, startpoint.Y, startpoint.Z))
            //    {
            //        BoundingBox bbox = GetBlockBBox(startpoint.X - 1, startpoint.Y, startpoint.Z);
            //        if ((ray.Position - bbox.Center()).Length() < maxDistance && ray.Intersects(bbox) != null)
            //            return IntersectedCollidingBlockIndexRecursion(ray, startpoint.AddX(-1), maxDistance, lookForCollidingBlock, out distance);
            //    }
            //}

            ////Y Positive
            //if (ray.Direction.Y > 0)
            //{
            //    if (WithinChunk(startpoint.X, startpoint.Y + 1, startpoint.Z))
            //    {
            //        BoundingBox bbox = GetBlockBBox(startpoint.X, startpoint.Y + 1, startpoint.Z);
            //        if ((ray.Position - bbox.Center()).Length() < maxDistance && ray.Intersects(bbox) != null)
            //            return IntersectedCollidingBlockIndexRecursion(ray, startpoint.AddY(1), maxDistance, lookForCollidingBlock, out distance);
            //    }
            //}//Y Negative
            //else if (ray.Direction.Y < 0)
            //{
            //    if (WithinChunk(startpoint.X, startpoint.Y - 1, startpoint.Z))
            //    {
            //        BoundingBox bbox = GetBlockBBox(startpoint.X, startpoint.Y - 1, startpoint.Z);
            //        if ((ray.Position - bbox.Center()).Length() < maxDistance && ray.Intersects(bbox) != null)
            //            return IntersectedCollidingBlockIndexRecursion(ray, startpoint.AddY(-1), maxDistance, lookForCollidingBlock, out distance);
            //    }
            //}

            ////Z Positive
            //if (ray.Direction.Z > 0)
            //{
            //    if (WithinChunk(startpoint.X, startpoint.Y, startpoint.Z + 1))
            //    {
            //        BoundingBox bbox = GetBlockBBox(startpoint.X, startpoint.Y, startpoint.Z + 1);
            //        if ((ray.Position - bbox.Center()).Length() < maxDistance && ray.Intersects(bbox) != null)
            //            return IntersectedCollidingBlockIndexRecursion(ray, startpoint.AddZ(1), maxDistance, lookForCollidingBlock, out distance);
            //    }
            //}//Z Negative
            //else if (ray.Direction.Z < 0)
            //{
            //    if (WithinChunk(startpoint.X, startpoint.Y, startpoint.Z - 1))
            //    {
            //        BoundingBox bbox = GetBlockBBox(startpoint.X, startpoint.Y, startpoint.Z - 1);
            //        if ((ray.Position - bbox.Center()).Length() < maxDistance && ray.Intersects(bbox) != null)
            //            return IntersectedCollidingBlockIndexRecursion(ray, startpoint.AddZ(-1), maxDistance, lookForCollidingBlock, out distance);
            //    }
            //}
            //return null;
            #endregion
        }

        #endregion

        #region Visualization (Buffer/Vertices etc)
        public void InitializeVisuals()
        {
            this.SettingBuffer = true;
            if (ChunkLeft == null || ChunkRight == null || ChunkInfront == null || ChunkBehind == null || ChunkAbove == null || ChunkBelow == null)
                throw new Exception("At least one of the surrounding chunks was unknown. All of them must be set before visuals are initialized");

            SetReactiveBlocks();
            SetAllHidden();
            SetBuffers();
            this.SettingBuffer = false;
        }

        private void SetReactiveBlocks()
        {
            for (int x = 0; x < AllBlocks.Size.X; x++)
                for (int y = 0; y < AllBlocks.Size.Y; y++)
                    for (int z = 0; z < AllBlocks.Size.Z; z++)
                    {
                        //TEST
                        if (ChunkAbove == null)
                        {
                            try
                            {
                                SetDirt(new Point3D(x, y, z));
                            }
                            catch (ArgumentNullException)
                            {
                                break;
                                //throw;
                            }
                        }
                        else
                            SetDirt(new Point3D(x, y, z));

                    }
        }

        private void SetDirt(Point3D blockIndex)
        {
            if (AllBlocks[blockIndex.X, blockIndex.Y, blockIndex.Z].Type == BlockTypes.GrassDirt)
            {
                if (blockIndex.Y == ChunkWidth - 1)
                {
                    Point3D indexAbove = ChunkAbove.ConvertBlockIndex(GridIndex, blockIndex.AddY(1));
                    if (ChunkAbove.AllBlocks[indexAbove.X, indexAbove.Y, indexAbove.Z].Visible)
                        AllBlocks[blockIndex.X, blockIndex.Y, blockIndex.Z] = new Block(BlockTypes.Dirt);
                }
                else if (AllBlocks[blockIndex.X, blockIndex.Y + 1, blockIndex.Z].Visible)
                    AllBlocks[blockIndex.X, blockIndex.Y, blockIndex.Z] = new Block(BlockTypes.Dirt);
            }
        }

        public void SetBuffers()
        {
            var builderLeft = new VisualBufferBuilder<VertexPositionIndexedNormalTexture>();
            var builderRight = new VisualBufferBuilder<VertexPositionIndexedNormalTexture>();
            var builderTop = new VisualBufferBuilder<VertexPositionIndexedNormalTexture>();
            var builderBottom = new VisualBufferBuilder<VertexPositionIndexedNormalTexture>();
            var builderFront = new VisualBufferBuilder<VertexPositionIndexedNormalTexture>();
            var builderBack = new VisualBufferBuilder<VertexPositionIndexedNormalTexture>();

            for (int x = 0; x < this.AllBlocks.Size.X; x++)
                for (int y = 0; y < this.AllBlocks.Size.Y; y++)
                    for (int z = 0; z < this.AllBlocks.Size.Z; z++)
                    {
                        Block current = AllBlocks[x, y, z];
                        Vector3 blockMinCorner = new Vector3(x, y, z) * Block.Sidelength;

                        if (current.NotHidden && current.Visible)
                        {
                            if (!HoldsVisibleBlock(x - 1, y, z))//left
                            {
                                builderLeft.AddBuffer(current.VerticesXNegative(blockMinCorner), current.Indices());
                            }
                            if (!HoldsVisibleBlock(x + 1, y, z))//right
                            {
                                builderRight.AddBuffer(current.VerticesXPositive(blockMinCorner), current.Indices());
                            }
                            if (!HoldsVisibleBlock(x, y - 1, z))//below
                            {
                                builderBottom.AddBuffer(current.VerticesYNegative(blockMinCorner), current.Indices());
                            }
                            if (!HoldsVisibleBlock(x, y + 1, z))//above
                            {
                                builderTop.AddBuffer(current.VerticesYPositive(blockMinCorner), current.Indices());
                            }
                            if (!HoldsVisibleBlock(x, y, z - 1))//infront
                            {
                                builderFront.AddBuffer(current.VerticesZNegative(blockMinCorner), current.Indices());
                            }
                            if (!HoldsVisibleBlock(x, y, z + 1))//behind
                            {
                                builderBack.AddBuffer(current.VerticesZPositive(blockMinCorner), current.Indices());
                            }
                        }
                    }
            this.VerticesCount = builderLeft.VertexCount + builderRight.VertexCount +
                builderTop.VertexCount + builderBottom.VertexCount +
                builderFront.VertexCount + builderBack.VertexCount;
            this.IndicesCount = builderLeft.IndexCount + builderRight.IndexCount +
                builderTop.IndexCount + builderBottom.IndexCount +
                builderFront.IndexCount + builderBack.IndexCount;

            //Function that gets a delegate to select the builder and one to store the created buffer and then creates a buffer from the builder
            Action<Func<VisualBufferBuilder<VertexPositionIndexedNormalTexture>>, Action<VisualBuffer<VertexPositionIndexedNormalTexture>>> createBuffer =
                (Func<VisualBufferBuilder<VertexPositionIndexedNormalTexture>> selectBuilder, Action<VisualBuffer<VertexPositionIndexedNormalTexture>> setBuffer) =>
            {
                setBuffer(selectBuilder().CreateBuffer(Device, VertexPositionIndexedNormalTexture.VertexDeclaration, BufferUsage.WriteOnly));
            };

            createBuffer(() => builderLeft, vb => BufferLeft = vb);
            createBuffer(() => builderRight, vb => BufferRight = vb);
            createBuffer(() => builderTop, vb => BufferTop = vb);
            createBuffer(() => builderBottom, vb => BufferBottom = vb);
            createBuffer(() => builderFront, vb => BufferFront = vb);
            createBuffer(() => builderBack, vb => BufferBack = vb);

            BufferSet = true;
        }

        public void DeleteBuffer()
        {
            Dispose();

            this.BufferLeft = null;
            this.BufferRight = null;
            this.BufferTop = null;
            this.BufferBottom = null;
            this.BufferFront = null;
            this.BufferBack = null;

            this.BufferSet = false;
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cameraGridPos"></param>
        /// <param name="effect"></param>
        /// <param name="viewMatrix"></param>
        /// <returns>drawn vertices</returns>
        public int Draw(Point3D cameraGridPos, Effect effect, Matrix viewProjectionMatrix)
        {
            return DrawBlocks(cameraGridPos, effect, viewProjectionMatrix);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cameraGridPos"></param>
        /// <param name="effect"></param>
        /// <param name="viewProjection"></param>
        /// <returns>drawn vertices</returns>
        public int DrawBlocks(Point3D cameraGridPos, Effect effect, Matrix viewProjectionMatrix)
        {
            FindSitesToDraw(cameraGridPos,
                out bool drawLeft, out bool drawRight, out bool drawAbove,
                out bool drawBelow, out bool drawInfront, out bool drawBehind);

            Matrix wvpMatrix = this.WorldMatrix * viewProjectionMatrix;

            effect.Parameters["WVPMatrix"].SetValue(wvpMatrix);
            effect.Parameters["WorldInverseTranspose"].SetValue(Matrix.Identity);

            int drawnVertices = 0;

            if (drawLeft)
                drawnVertices += DrawBuffer(BufferLeft, effect);
            if (drawRight)
                drawnVertices += DrawBuffer(BufferRight, effect);

            if (drawAbove)
                drawnVertices += DrawBuffer(BufferTop, effect);

            if (drawBelow)
                drawnVertices += DrawBuffer(BufferBottom, effect);

            if (drawInfront)
                drawnVertices += DrawBuffer(BufferFront, effect);

            if (drawBehind)
                drawnVertices += DrawBuffer(BufferBack, effect);


            return drawnVertices;
        }

        private int DrawBuffer(VisualBuffer<VertexPositionIndexedNormalTexture> buffer, Effect effect)
        {
            int drawnVertices = 0;

            if (buffer != null)
            {
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    buffer.SetForDevice(Device);
                    this.Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, buffer.Indices.IndexCount / 3);

                    drawnVertices += buffer.Vertices.VertexCount;
                }
            }

            return drawnVertices;
        }

        private void FindSitesToDraw(Point3D cameraPos,
            out bool drawLeft, out bool drawRight, out bool drawAbove,
            out bool drawBelow, out bool drawInfront, out bool drawBehind)
        {
            Point3D posDif = this.GridIndex - cameraPos;

            if (posDif.X < 0)
            {
                drawRight = true;
                drawLeft = false;
            }
            else if (posDif.X == 0)
            {
                drawLeft = true;
                drawRight = true;
            }
            else
            {
                drawLeft = true;
                drawRight = false;
            }

            if (posDif.Y < 0)
            {
                drawAbove = true;
                drawBelow = false;
            }
            else if (posDif.Y == 0)
            {
                drawBelow = true;
                drawAbove = true;
            }
            else
            {
                drawBelow = true;
                drawAbove = false;
            }

            if (posDif.Z < 0)
            {
                drawBehind = true;
                drawInfront = false;
            }
            else if (posDif.Z == 0)
            {
                drawBehind = true;
                drawInfront = true;
            }
            else
            {
                drawBehind = false;
                drawInfront = true;
            }
        }

        public override string ToString()
        {
            return "Chunk: " + this.GridIndex.ToString();
        }

        public static Point3D GetGridPosition(Vector3 position)
        {
            position.X /= Size.X;
            position.Y /= Size.Y;
            position.Z /= Size.Z;
            return new Point3D(
                position.X >= 0 ? (int)position.X : (int)position.X - 1,
                position.Y >= 0 ? (int)position.Y : (int)position.Y - 1,
                position.Z >= 0 ? (int)position.Z : (int)position.Z - 1);
        }

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
