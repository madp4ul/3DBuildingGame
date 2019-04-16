using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace _1st3DGame
{
    class Chunk
    {
        public const int ChunkWidth = 32;

        private const int SeedOffset = 1;

        private const int SeedLayer = 2;
        private const int RandomRange = 100;

        public static int BlocksPerChunk { get { return ChunkWidth * ChunkWidth * ChunkWidth; } }
        public static int VerticesPerChunk { get { return BlocksPerChunk * Block.VerticesCount; } }
        public static int IndicesPerChunk { get { return BlocksPerChunk * Block.IndicesCount; } }
        public static Vector3 Size
        {
            get
            {
                return new Vector3(ChunkWidth * Block.Sidelength,
                    ChunkWidth * Block.Sidelength,
                    ChunkWidth * Block.Sidelength);
            }
        }

        public readonly Vector3 minCorner;
        public readonly Point3D GridIndex;
        public readonly BoundingBox BBox;
        public Vector3 Center { get { return minCorner + 0.5f * Size; } }

        public Block[,,] AllBlocks { get; private set; }

        VertexBuffer VBuffer;
        int VerticesCount = 0;
        int IndicesCount = 0;
        IndexBuffer IBuffer;
        GraphicsDevice Device;

        private bool ContainVisibleBlocks;

        public Chunk(GraphicsDevice device, Point3D gridPosChunk)
        {
            this.GridIndex = gridPosChunk;
            this.minCorner = new Vector3(gridPosChunk.X * Size.X, gridPosChunk.Y * Size.Y, gridPosChunk.Z * Size.Z);
            //x=width,y=length,z=height
            this.AllBlocks = new Block[ChunkWidth, ChunkWidth, ChunkWidth];
            this.BBox = new BoundingBox(minCorner, minCorner + Size);

            this.Device = device;

            SetRandoms();

            FillWithBlocks();
            SetAllHidden();

            SetBuffers();
        }

        /// <summary>
        /// noise supported from 0-100
        /// </summary>
        /// <param name="noise"></param>
        /// <returns></returns>
        private Block GetBlock(float noise)
        {
            if (noise <= 38)
                return new Block(BlockTypes.Stone);
            else if (noise > 38 && noise <= 41)
                return new Block(BlockTypes.Dirt);
            else
                return new Block(BlockTypes.Air);
        }

        private void FillWithBlocks()
        {
            for (int x = 0; x < ChunkWidth; x++)
                for (int y = 0; y < ChunkWidth; y++)
                    for (int z = 0; z < ChunkWidth; z++)
                        this.AllBlocks[x, y, z] = GetBlock(GetNoise(new Point3D(x, y, z)));
        }

        #region Perlin-Noise
        private float GetNoise(Point3D blockIndex)
        {
            float[] noiseLayers = new float[SeedLayer];
            for (int i = 0; i < SeedLayer; i++)
                noiseLayers[i] = GetLayerNoise(blockIndex, i);

            float result = 0;
            int divider = 0;
            for (int i = 0; i < SeedLayer; i++)
            {
                result += noiseLayers[i] * ((float)SeedLayer / (float)(i + 1));
                divider += (i + 1);
            }
            result /= divider;

            return result;
            //return GetLayerNoise(blockIndex, 0);
        }

        private float GetLayerNoise(Point3D blockIndex, int layer)
        {
            int randomsPerSide = Randoms[layer].GetLength(0);
            int blocksBetweenRandoms = ChunkWidth / (randomsPerSide - 1);

            //index in random-array
            Point3D minRandomIndex = new Point3D(
                (int)((float)blockIndex.X / (float)blocksBetweenRandoms),
                (int)((float)blockIndex.Y / (float)blocksBetweenRandoms),
                (int)((float)blockIndex.Z / (float)blocksBetweenRandoms));

            //lerp percentages
            Vector3 percentages = new Vector3(
                (float)(blockIndex.X - (minRandomIndex.X * blocksBetweenRandoms)) / blocksBetweenRandoms,
                (float)(blockIndex.Y - (minRandomIndex.Y * blocksBetweenRandoms)) / blocksBetweenRandoms,
                (float)(blockIndex.Z - (minRandomIndex.Z * blocksBetweenRandoms)) / blocksBetweenRandoms);

            float lerped =
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
            return lerped;
        }


        #endregion

        #region Seed-management

        int[][,,] Randoms;
        private void SetRandoms()
        {
            int[][,,] seeds = SetSeeds();
            Randoms = new int[seeds.Length][,,];
            for (int i = 0; i < seeds.Length; i++)
            {
                Randoms[i] = new int[seeds[i].GetLength(0), seeds[i].GetLength(1), seeds[i].GetLength(2)];
                for (int x = 0; x < seeds[i].GetLength(0); x++)
                    for (int y = 0; y < seeds[i].GetLength(1); y++)
                        for (int z = 0; z < seeds[i].GetLength(2); z++)
                            Randoms[i][x, y, z] = new Random(seeds[i][x, y, z]).Next(RandomRange);
            }
        }

        private int[][,,] SetSeeds()
        {
            const int offsetPerLayer = 13;

            int[][,,] InnerSeeds = new int[SeedLayer][,,];
            int maxSeedCount = (int)Math.Pow(2, SeedLayer - 1) + 1;


            for (int i = 0; i < SeedLayer; i++)
            {
                int seedCount = (int)Math.Pow(2, i) + 1;
                int seedStep = (maxSeedCount - 1) / (seedCount - 1);
                InnerSeeds[i] = new int[seedCount, seedCount, seedCount];

                for (int x = 0; x < seedCount; x++)
                    for (int y = 0; y < seedCount; y++)
                        for (int z = 0; z < seedCount; z++)
                        {
                            //components
                            int xComp = (GridIndex.X * (maxSeedCount - 1) + x * seedStep) % int.MaxValue;
                            int yComp = (GridIndex.Y * (maxSeedCount - 1) + y * seedStep) % int.MaxValue;
                            int zComp = (GridIndex.Z * (maxSeedCount - 1) + z * seedStep) % int.MaxValue;

                            InnerSeeds[i][x, y, z] = (int)(
                                SeedCombiner(xComp, yComp, zComp) +
                                offsetPerLayer * i +
                                SeedOffset
                                ) % int.MaxValue
                                ;
                        }
            }
            return InnerSeeds;
        }

        private long SeedCombiner(int x, int y, int z)
        {
            return (x * y) % (z == 0 ? 1 : z);
            //return (
            //    x +
            //    y % (z == 0 ? 1 :
            //    z));
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
                if (WithinChunk(index.X + 1, index.Y, index.Z))
                    SetHidden(index.X + 1, index.Y, index.Z);

                if (WithinChunk(index.X, index.Y - 1, index.Z))
                    SetHidden(index.X, index.Y - 1, index.Z);
                if (WithinChunk(index.X, index.Y + 1, index.Z))
                    SetHidden(index.X, index.Y + 1, index.Z);

                if (WithinChunk(index.X, index.Y, index.Z - 1))
                    SetHidden(index.X, index.Y, index.Z - 1);
                if (WithinChunk(index.X, index.Y, index.Z + 1))
                    SetHidden(index.X, index.Y, index.Z + 1);

                SetBuffers();
                return oldBlock;
            }
            return null;
        }

        private void SetAllHidden()
        {
            for (int x = 0; x < this.AllBlocks.GetLength(0); x++)
                for (int y = 0; y < this.AllBlocks.GetLength(1); y++)
                    for (int z = 0; z < this.AllBlocks.GetLength(2); z++)
                        if (AllBlocks[x, y, z].Visible)
                            SetHidden(x, y, z);
        }

        private void SetHidden(int x, int y, int z)
        {
            if (
                !HoldsVisibleBlock(x - 1, y, z) ||
                !HoldsVisibleBlock(x + 1, y, z) ||
                !HoldsVisibleBlock(x, y - 1, z) ||
                !HoldsVisibleBlock(x, y + 1, z) ||
                !HoldsVisibleBlock(x, y, z - 1) ||
                !HoldsVisibleBlock(x, y, z + 1)
                )
                this.AllBlocks[x, y, z].NotHidden = true;
            else
                this.AllBlocks[x, y, z].NotHidden = false;
        }

        private bool HoldsVisibleBlock(int x, int y, int z)
        {
            if (!WithinChunk(x, y, z))
                return false;
            else
                return this.AllBlocks[x, y, z].Visible;
        }

        public bool WithinChunk(int x, int y, int z)
        {
            return !(x < 0 || y < 0 || z < 0 ||
                x >= this.AllBlocks.GetLength(0) ||
                y >= this.AllBlocks.GetLength(1) ||
                z >= this.AllBlocks.GetLength(2));
        }

        private Vector3 BlockMinCorner(int x, int y, int z)
        {
            return this.minCorner + new Vector3(x * Block.Sidelength, y * Block.Sidelength, z * Block.Sidelength);
        }

        public Point3D BlockIndex(Vector3 position)
        {
            Point3D index = Point3D.Empty;
            Vector3 chunkSize = Chunk.Size;
            Vector3 posInChunk = position - minCorner;
            index.X = (int)(posInChunk.X * Block.Sidelength);
            index.Y = (int)(posInChunk.Y * Block.Sidelength);
            index.Z = (int)(posInChunk.Z * Block.Sidelength);
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

        public BoundingBox GetBlockBBox(int x, int y, int z)
        {
            Vector3 minCorner = BlockMinCorner(x, y, z);
            return new BoundingBox(minCorner, minCorner + new Vector3(Block.Sidelength));
        }
        #endregion

        #region Player-Collision
        public void ChunkCollision(Player player1)
        {
            Point3D[] intersectingBlocks = GetIntersectingBlockIndices(player1.BBox);
            if (player1.UpDownMovement < 0)
            {

            }
            for (int i = 0; i < intersectingBlocks.GetLength(0); i++)
            {
                if (AllBlocks[intersectingBlocks[i].X, intersectingBlocks[i].Y, intersectingBlocks[i].Z].CollidesWithPlayer)
                    player1.Collision(GetBlockBBox(intersectingBlocks[i].X, intersectingBlocks[i].Y, intersectingBlocks[i].Z));
            }
        }

        public Point3D[] GetIntersectingBlockIndices(BoundingBox bbox)
        {
            Point3D minIndex = BlockIndex(bbox.Min - new Vector3(0.1f));
            Point3D maxIndex = BlockIndex(bbox.Max + new Vector3(0.1f));
            if (minIndex.X < 0) minIndex.X = 0;
            if (minIndex.Y < 0) minIndex.Y = 0;
            if (minIndex.Z < 0) minIndex.Z = 0;
            if (maxIndex.X >= this.AllBlocks.GetLength(0)) maxIndex.X = this.AllBlocks.GetLength(0) - 1;
            if (maxIndex.Y >= this.AllBlocks.GetLength(1)) maxIndex.Y = this.AllBlocks.GetLength(1) - 1;
            if (maxIndex.Z >= this.AllBlocks.GetLength(2)) maxIndex.Z = this.AllBlocks.GetLength(2) - 1;

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

                float? distance;
                Point3D blockPos = IntersectedCollidingBlockIndexRecursion(
                    ray, startIndex, maxDistance, lookForCollidingBlock, out distance);

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
            distance = null;
            if (this.AllBlocks[startpoint.X, startpoint.Y, startpoint.Z].CollidesWithPlayer == lookForCollidingBlock)
            {
                BoundingBox bbox = GetBlockBBox(startpoint.X, startpoint.Y, startpoint.Z);
                distance = ray.Intersects(bbox);
                return startpoint;
            }

            //X Positive
            if (ray.Direction.X > 0)
            {
                if (WithinChunk(startpoint.X + 1, startpoint.Y, startpoint.Z))
                {
                    BoundingBox bbox = GetBlockBBox(startpoint.X + 1, startpoint.Y, startpoint.Z);
                    if ((ray.Position - bbox.Center()).Length() < maxDistance && ray.Intersects(bbox) != null)
                        return IntersectedCollidingBlockIndexRecursion(ray, startpoint.AddX(1), maxDistance, lookForCollidingBlock, out distance);
                }
            }//X Negative
            else if (ray.Direction.X < 0)
            {
                if (WithinChunk(startpoint.X - 1, startpoint.Y, startpoint.Z))
                {
                    BoundingBox bbox = GetBlockBBox(startpoint.X - 1, startpoint.Y, startpoint.Z);
                    if ((ray.Position - bbox.Center()).Length() < maxDistance && ray.Intersects(bbox) != null)
                        return IntersectedCollidingBlockIndexRecursion(ray, startpoint.AddX(-1), maxDistance, lookForCollidingBlock, out distance);
                }
            }

            //Y Positive
            if (ray.Direction.Y > 0)
            {
                if (WithinChunk(startpoint.X, startpoint.Y + 1, startpoint.Z))
                {
                    BoundingBox bbox = GetBlockBBox(startpoint.X, startpoint.Y + 1, startpoint.Z);
                    if ((ray.Position - bbox.Center()).Length() < maxDistance && ray.Intersects(bbox) != null)
                        return IntersectedCollidingBlockIndexRecursion(ray, startpoint.AddY(1), maxDistance, lookForCollidingBlock, out distance);
                }
            }//Y Negative
            else if (ray.Direction.Y < 0)
            {
                if (WithinChunk(startpoint.X, startpoint.Y - 1, startpoint.Z))
                {
                    BoundingBox bbox = GetBlockBBox(startpoint.X, startpoint.Y - 1, startpoint.Z);
                    if ((ray.Position - bbox.Center()).Length() < maxDistance && ray.Intersects(bbox) != null)
                        return IntersectedCollidingBlockIndexRecursion(ray, startpoint.AddY(-1), maxDistance, lookForCollidingBlock, out distance);
                }
            }

            //Z Positive
            if (ray.Direction.Z > 0)
            {
                if (WithinChunk(startpoint.X, startpoint.Y, startpoint.Z + 1))
                {
                    BoundingBox bbox = GetBlockBBox(startpoint.X, startpoint.Y, startpoint.Z + 1);
                    if ((ray.Position - bbox.Center()).Length() < maxDistance && ray.Intersects(bbox) != null)
                        return IntersectedCollidingBlockIndexRecursion(ray, startpoint.AddZ(1), maxDistance, lookForCollidingBlock, out distance);
                }
            }//Z Negative
            else if (ray.Direction.Z < 0)
            {
                if (WithinChunk(startpoint.X, startpoint.Y, startpoint.Z - 1))
                {
                    BoundingBox bbox = GetBlockBBox(startpoint.X, startpoint.Y, startpoint.Z - 1);
                    if ((ray.Position - bbox.Center()).Length() < maxDistance && ray.Intersects(bbox) != null)
                        return IntersectedCollidingBlockIndexRecursion(ray, startpoint.AddZ(-1), maxDistance, lookForCollidingBlock, out distance);
                }
            }
            return null;
        }

        #endregion

        #region Visualization (Buffer/Vertices etc)
        public void SetBuffers()
        {
            List<VertexPositionNormalTexture> vertices;
            List<int> indices;
            SetVerticesIndices(out vertices, out indices);

            if (indices.Count == 0 || vertices.Count == 0)
                this.ContainVisibleBlocks = false;
            else
            {
                this.VBuffer = new VertexBuffer(this.Device, VertexPositionNormalTexture.VertexDeclaration,
                   vertices.Count, BufferUsage.WriteOnly);
                this.IBuffer = new IndexBuffer(this.Device, typeof(int), indices.Count, BufferUsage.WriteOnly);
                this.VBuffer.SetData<VertexPositionNormalTexture>(vertices.ToArray());
                this.IBuffer.SetData<int>(indices.ToArray());
                this.ContainVisibleBlocks = true;
            }
        }

        private void SetVerticesIndices(out List<VertexPositionNormalTexture> vertices, out List<int> indices)
        {
            vertices = new List<VertexPositionNormalTexture>();
            indices = new List<int>();

            for (int x = 0; x < this.AllBlocks.GetLength(0); x++)
                for (int y = 0; y < this.AllBlocks.GetLength(1); y++)
                    for (int z = 0; z < this.AllBlocks.GetLength(2); z++)
                    {
                        if (AllBlocks[x, y, z].NotHidden && AllBlocks[x, y, z].Visible)
                        {
                            if (!HoldsVisibleBlock(x - 1, y, z))
                            {
                                indices.AddRange(AllBlocks[x, y, z].Indices(vertices.Count));
                                vertices.AddRange(AllBlocks[x, y, z].VerticesXNegative(BlockMinCorner(x, y, z)));
                            }
                            if (!HoldsVisibleBlock(x + 1, y, z))
                            {
                                indices.AddRange(AllBlocks[x, y, z].Indices(vertices.Count));
                                vertices.AddRange(AllBlocks[x, y, z].VerticesXPositive(BlockMinCorner(x, y, z)));
                            }
                            if (!HoldsVisibleBlock(x, y - 1, z))
                            {
                                indices.AddRange(AllBlocks[x, y, z].Indices(vertices.Count));
                                vertices.AddRange(AllBlocks[x, y, z].VerticesYNegative(BlockMinCorner(x, y, z)));
                            }
                            if (!HoldsVisibleBlock(x, y + 1, z))
                            {
                                indices.AddRange(AllBlocks[x, y, z].Indices(vertices.Count));
                                vertices.AddRange(AllBlocks[x, y, z].VerticesYPositive(BlockMinCorner(x, y, z)));
                            }
                            if (!HoldsVisibleBlock(x, y, z - 1))
                            {
                                indices.AddRange(AllBlocks[x, y, z].Indices(vertices.Count));
                                vertices.AddRange(AllBlocks[x, y, z].VerticesZNegative(BlockMinCorner(x, y, z)));
                            }
                            if (!HoldsVisibleBlock(x, y, z + 1))
                            {
                                indices.AddRange(AllBlocks[x, y, z].Indices(vertices.Count));
                                vertices.AddRange(AllBlocks[x, y, z].VerticesZPositive(BlockMinCorner(x, y, z)));
                            }
                        }
                    }
            this.VerticesCount = vertices.Count;
            this.IndicesCount = indices.Count;
        }
        #endregion

        public void Draw(Effect effect)
        {
            DrawBlocks(effect);
        }

        public void DrawBlocks(Effect effect)
        {
            if (ContainVisibleBlocks)
            {
                effect.Parameters["xTexture"].SetValue(Block.BlockTex);
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                    pass.Apply();

                this.Device.Indices = this.IBuffer;
                this.Device.SetVertexBuffer(this.VBuffer);
                this.Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, this.VerticesCount, 0, this.IndicesCount / 3);
            }
        }

        public override string ToString()
        {
            return this.GridIndex.ToString();
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
    }
}
