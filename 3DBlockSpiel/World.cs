using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Threading.Tasks;
using System.Threading;

namespace _1st3DGame
{
    class World
    {
        //Absolute border: 4.24 chunks
        public const float RenderRange = 3.24f;
        public const int StartSeed = 1337;
        const int minStartPos = 10000;
        const int maxStartPos = 100000;
        GraphicsDevice Device;


        public List<Chunk> Chunks { get; private set; }
        public Player Player1;

        public World(GraphicsDevice device)
        {
            this.Device = device;
            Vector3 startPosition = GetStartPos();
            Player1 = new Player(device, Vector3.Zero, Vector3.Zero + Vector3.Forward);
            Chunks = new List<Chunk>();
            UpdateChunkGeneration();
        }

        private Vector3 GetStartPos()
        {
            Random r = new Random(StartSeed);
            Vector3 startpos = new Vector3(
                (float)(r.Next(minStartPos,maxStartPos)),
                (float)(r.Next(minStartPos,maxStartPos)),
                (float)(r.Next(minStartPos,maxStartPos)));
            if (r.Next(1) == 1)
                startpos.X *= -1;
            if (r.Next(1) == 1)
                startpos.Y *= -1;
            if (r.Next(1) == 1)
                startpos.Z *= -1;
            return startpos;
        }

        public void Update(TimeSpan elapsedTime)
        {
            UpdateChunkGeneration();
            WorldCollision(Player1);
            Player1.Update(elapsedTime);
            PlayerWorldInteraction();

        }

        #region Chunk Generation and Placement

        List<Chunk> finishedChunks = new List<Chunk>();
        List<Point3D> chunksInArbeit = new List<Point3D>();

        private void UpdateChunkGeneration()
        {
            // Fügt alle bis jetzt berechneten neuen Chunks hinzu
            lock (finishedChunks)
            {
                if (finishedChunks.Count > 0)
                {
                    this.Chunks.AddRange(finishedChunks);
                    //Lösche alle hinzugefügten aus der Arbeitsliste
                    chunksInArbeit.RemoveAll(point => finishedChunks.Exists(chunk => chunk.GridIndex == point));

                    finishedChunks.Clear();
                }
            }

            BoundingSphere renderArea = new BoundingSphere(
                Chunk.GetGridPosition(Player1.EyePosition).ToVector(), RenderRange);

            DeleteChunksOutOfRange(renderArea);

            //TODO: multithreading
            //start here
            Point3D chunkPosition = GetOneEmptyGridPosition(renderArea);

            //or here
            if (chunkPosition != null && chunksInArbeit.Count < 1)
            {
                chunksInArbeit.Add(chunkPosition);

                Task task = new Task(() => { CreateChunk(chunkPosition); });
                task.Start();
            }
            //end other thread
        }

        private void CreateChunk(Point3D nextChunkPosition)
        {
            Chunk generatedChunk = new Chunk(this.Device, nextChunkPosition);

            lock (finishedChunks)
            {
                finishedChunks.Add(generatedChunk);
            }
        }

        private void DeleteChunksOutOfRange(BoundingSphere sphere)
        {
            for (int i = 0; i < this.Chunks.Count; i++)
                if (!sphere.Contains(Chunks[i].GridIndex))
                {
                    Chunks.RemoveAt(i);
                    i--;
                }
        }

        private Point3D GetOneEmptyGridPosition(BoundingSphere sphere)
        {
            List<Point3D> indices = new List<Point3D>();
            return EmptyGridPositionRecursion(sphere, sphere.Center.ToPoint(), ref indices);
        }

        private Point3D EmptyGridPositionRecursion(BoundingSphere sphere, Point3D gridPos, ref List<Point3D> checkedPositions)
        {
            checkedPositions.Add(gridPos);

            if (!Chunks.Any(x => x.GridIndex == gridPos) && !chunksInArbeit.Contains(gridPos))
            {
                return gridPos;
            }


            //sammle Nachbarpunkte
            Point3D[] nachbarn = new Point3D[]{ 
                gridPos.AddX(1), // x + 1
                gridPos.AddX(-1), //x - 1
                gridPos.AddY(1), //y + 1
                gridPos.AddY(-1), //y - 1
                gridPos.AddZ(1), //z + 1
                gridPos.AddZ(-1), //z - 1
            };

            foreach (Point3D point in nachbarn)
            {
                if (!checkedPositions.Contains(point) && sphere.Contains(point))
                {
                    Point3D emptyPoint = EmptyGridPositionRecursion(sphere, point.AddX(0), ref checkedPositions);

                    if (emptyPoint != null)
                    {
                        return emptyPoint;
                    }
                }
            }

            return null;
        }

        #endregion

        #region Collision
        public Point3D debugPlayerIndex = null;
        public Point3D PlayerChunkIndex = null;
        private void WorldCollision(Player player1)
        {
            player1.ResetCollisionParameters();
            foreach (Chunk chunk in this.Chunks)
            {
                if (chunk != null)
                    if (chunk.BBox.Intersects(player1.BBox) || chunk.BBox.Align(player1.BBox))
                    {
                        chunk.ChunkCollision(player1);
                        debugPlayerIndex = new Point3D(chunk.BlockIndex(player1.EyePosition));
                        PlayerChunkIndex = chunk.GridIndex;
                    }

            }
        }
        #endregion

        #region Player-World Interaction

        DateTime LastClick = DateTime.Now;
        private void PlayerWorldInteraction()
        {
            if (this.Player1.Digging && (DateTime.Now - LastClick) > TimeSpan.FromMilliseconds(Player.DiggingTimeoutMilliseconds))
            {
                DigIntersectedBlock();
                LastClick = DateTime.Now;
            }
            else if (this.Player1.Building && (DateTime.Now - LastClick) > TimeSpan.FromMilliseconds(Player.BuildingTimeoutMilliseconds))
            {
                BuildIntersectedBlock();
                LastClick = DateTime.Now;
            }
        }

        private void BuildIntersectedBlock()
        {
            int slot = 0;

            Ray playerRay = Player1.LookRay;
            Dictionary<Chunk, float> inChunks = IntersectingChunks(playerRay);
            Vector3? intersection = null;
            Point3D positionToDig = null;

            for (int i = 0; i < inChunks.Count; i++)
            {
                positionToDig = inChunks.ElementAt(i).Key.GetFirstIntersectedBlockIndex(
                    playerRay, Player.MaxBuildingRange, true, out intersection);
                if (positionToDig != null)
                    break;
            }
            if (intersection != null)
            {
                Ray backRay = new Ray((Vector3)intersection, -playerRay.Direction);
                Dictionary<Chunk, float> inChunks2 = IntersectingChunks(backRay);
                Vector3? airIntersectionPoint;
                for (int i = 0; i < inChunks2.Count; i++)
                {
                    Point3D positionToBuild = inChunks2.ElementAt(i).Key.GetFirstIntersectedBlockIndex(
                        backRay, Block.Radius * 2, false, out airIntersectionPoint);

                    if (positionToBuild != null)
                    {
                        BoundingBox blockBox = inChunks2.ElementAt(i).Key.GetBlockBBox(positionToBuild.X, positionToBuild.Y, positionToBuild.Z);
                        if (blockBox.Intersects(Player1.BBox))
                        {
                            if (blockBox.Align(Player1.BBox) && Player1.BBox.Contains(blockBox.Center()) == ContainmentType.Disjoint)
                            {
                                Block newB = Player1.inventory.GetItemAt(slot);
                                if (newB != null)
                                    inChunks2.ElementAt(i).Key.ChangeBlock(positionToBuild, newB);
                            }
                        }
                        else
                        {
                            Block newB = Player1.inventory.GetItemAt(slot);
                            if (newB != null)
                                inChunks2.ElementAt(i).Key.ChangeBlock(positionToBuild, newB);
                        }
                        break;
                    }
                }
            }
        }

        private void DigIntersectedBlock()
        {
            Ray playerRay = Player1.LookRay;
            Dictionary<Chunk, float> inChunks = IntersectingChunks(playerRay);
            Vector3? intersection = null;
            for (int i = 0; i < inChunks.Count; i++)
            {
                Point3D positionToDig = inChunks.ElementAt(i).Key.GetFirstIntersectedBlockIndex(playerRay, Player.MaxDiggingRange, true, out intersection);
                if (positionToDig != null)
                {
                    Block b = inChunks.ElementAt(i).Key.ChangeBlock(positionToDig, new Block(BlockTypes.Air));
                    if (b != null)
                        Player1.inventory.Add(b);
                    break;
                }
            }
        }

        private Dictionary<Chunk, float> IntersectingChunks(Ray ray)
        {
            Dictionary<Chunk, float> dDistanceChunk = new Dictionary<Chunk, float>();
            //Collect intersected Chunks
            for (int i = 0; i < this.Chunks.Count; i++)
            {
                float? f = ray.Intersects(Chunks[i].BBox);
                if (f != null)
                    if ((float)f >= 0 && (float)f < Player.MaxDiggingRange)
                        dDistanceChunk.Add(Chunks[i], (float)f);
            }
            dDistanceChunk = dDistanceChunk.SortByFloat();
            return dDistanceChunk;
        }
        #endregion

        public void Draw(Effect effect)
        {
            BoundingFrustum fov = new BoundingFrustum(
                Player1.ViewMatrix *
                Player1.ProjectionMatrix);
            //Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(05), this.Device.Viewport.AspectRatio, 0.1f, 600f));
            effect.CurrentTechnique = effect.Techniques["Textured"];
            effect.Parameters["xWorld"].SetValue(Matrix.Identity);
            effect.Parameters["xView"].SetValue(Player1.ViewMatrix);
            effect.Parameters["xProjection"].SetValue(Player1.ProjectionMatrix);
            effect.Parameters["xEnableLighting"].SetValue(true);
            effect.Parameters["xLightDirection"].SetValue(-Vector3.One);
            effect.Parameters["xAmbient"].SetValue(0.4f);

            foreach (Chunk chunk in this.Chunks)
            {
                if (chunk != null)
                {
                    if (chunk.BBox.Intersects(fov))
                        chunk.Draw(effect);
                }
            }

        }

        public bool DrawDebugInfo = true;

    }
}
