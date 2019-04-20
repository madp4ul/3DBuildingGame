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
using _1st3DGame.Menus.GraphicsMenuOptions;
using BlockGameClasses;

namespace _1st3DGame
{
    class World
    {
        public Dictionary<ViewDistances, float> Bufferranges { get; private set; }

        //Chunkload-constants
        public float BufferRange;
        public float LoadRange { get { return BufferRange + 2f; } }
        private const int ChunkUpdateThreads = 1;
        #region buffertimes
        //21.1.15(load 5.5; buffer 5) : 
        //1thread=38sec. 
        //2threads=24sec. 
        //3threads= 20sec. 
        //4threads=15sec.
        //8threads=15sec.
        //16 threads=18 sec.
        #endregion

        public const int StartSeed = 854;
        Random Rnd;
        //RandomSeed-constants
        const int minStartPos = 10000;
        const int maxStartPos = 100000;
        //Time-constants
        const float TimeSpeedFactor = 3f;
        //Enemy-constants
        const int MaxEnemyCount = 20;
        readonly static TimeSpan EnemyCountReductionTimePerKill = TimeSpan.FromSeconds(5);
        const float RangeToDeleteEnemiesAt = 80f;
        readonly static TimeSpan WaittimeBeforeCorpsesGetDeleted = TimeSpan.FromSeconds(5);
        const float MinSpawnDistance = 30f;
        const float MaxSpawnDistance = 60f;

        public static Texture2D SunsTexture;

        #region Effects
        public static Effect BlockEffect;
        public static Effect PointSpriteEffect;
        public static Effect CharacterEffect;
        public readonly Effect BasicEffect;
        #endregion
        GraphicsDevice Device;
        public int WorldVertices { get; private set; }
        public int WorldVerticesDrawn { get; private set; }
        public int WorldIndices { get; private set; }
        //Light-constants
        Sky SkyBox;
        //Time
        public IngameDateTime WorldTime { get; private set; }
        //Chunks
        public SortedList<Point3D, Chunk> LoadedChunks { get; private set; }
        public SortedList<Point3D, Chunk> BufferedChunks { get; private set; }
        //Player and Mobs
        public Player Player1 { get; private set; }
        public List<Living> Characters { get; private set; }

        //Index for Player is 0
        private int _CamIndex;
        public int CamIndex
        {
            get { return _CamIndex; }
            set
            {
                if (value < this.Characters.Count && value >= 0)
                {
                    Characters[_CamIndex].CamActive = false;
                    Characters[value].CamActive = true;
                    _CamIndex = value;
                    _CurrentCamera = Characters[_CamIndex];
                }
            }
        }
        private MovebleCamera _CurrentCamera;
        public MovebleCamera CurrentCamera { get { return _CurrentCamera; } }

        public bool Paused = false;

        public World(GraphicsDevice device)
        {
            InitViewDistances();
            this.BufferRange = Bufferranges[ViewDistances.Normal];
            this.Device = device;
            BasicEffect = new BasicEffect(this.Device);
            this.Characters = new List<Living>();
            this.Rnd = new Random(StartSeed);

            //Random-Dependant init.
            Vector3 startPosition = GetStartPos();
            startPosition.Y = 100;
            startPosition = new Vector3(0, 30, 0);
            this.Player1 = new Player(device, startPosition, Vector3.Forward);
            this.Characters.Add(Player1);
            this.CamIndex = 0;

            WorldTime = new IngameDateTime((uint)new Random(StartSeed).Next(100), (uint)new Random(StartSeed).Next((int)IngameDateTime.TicksPerDay));

            this.SkyBox = new Sky();
            //Chunks
            LoadedChunks = new SortedList<Point3D, Chunk>();
            BufferedChunks = new SortedList<Point3D, Chunk>();
        }

        private void InitViewDistances()
        {
            this.Bufferranges = new Dictionary<ViewDistances, float>
            {
                { ViewDistances.Minimum, 3 },
                { ViewDistances.Low, 4 },
                { ViewDistances.Normal, 5 },
                { ViewDistances.High, 6 },
                { ViewDistances.VeryHigh, 7 },
                { ViewDistances.Extreme, 9 },
                { ViewDistances.Maximum, 10 }
            };
        }

        private Vector3 GetStartPos()
        {
            Random r = new Random(StartSeed);
            Vector3 startpos = new Vector3(
                (float)(r.Next(minStartPos, maxStartPos)),
                (float)(r.Next(minStartPos, maxStartPos)),
                (float)(r.Next(minStartPos, maxStartPos)));
            if (r.Next(1) == 1)
                startpos.X *= -1;
            if (r.Next(1) == 1)
                startpos.Y *= -1;
            if (r.Next(1) == 1)
                startpos.Z *= -1;
            return startpos;
        }

        // ////////////TIMESTAMPS/////////////////////
        public int generationUpdate = 0;
        public int collisionUpdate = 0;
        public int playerUpdate = 0;
        public int interactionUpdate = 0;
        public void Update(TimeSpan elapsedTime)
        {
            DateTime d1 = DateTime.Now;
            UpdateChunkGeneration();
            generationUpdate = (int)(DateTime.Now - d1).TotalMilliseconds;

            if (!Paused)
            {
                UpdateTime(elapsedTime);
                //UpdateLights();
                SkyBox.SetSunlights(this.WorldTime);

                SpawnAndDeleteEnemies();

                DateTime d3 = DateTime.Now;
                foreach (Body body in this.Characters)
                    body.Update(elapsedTime, Player1);
                playerUpdate = (int)(DateTime.Now - d3).TotalMilliseconds;

                DateTime d2 = DateTime.Now;
                WorldCollision();
                foreach (Body body in this.Characters)
                    body.ApplyMoveVector();
                collisionUpdate = (int)(DateTime.Now - d2).TotalMilliseconds;

                DateTime d4 = DateTime.Now;
                PlayerWorldInteraction();
                interactionUpdate = (int)(DateTime.Now - d4).TotalMilliseconds;
            }
        }

        private void UpdateTime(TimeSpan elapsedTime)
        {
            this.WorldTime.AddTime((uint)(elapsedTime.TotalMilliseconds * TimeSpeedFactor));
        }

        #region Chunk Generation and Placement

        List<Chunk> finishedLoadedChunks = new List<Chunk>();
        List<Point3D> chunksLoading = new List<Point3D>();

        List<Chunk> finishedBufferedChunks = new List<Chunk>();
        List<Point3D> chunksBuffering = new List<Point3D>();

        private void PreLoadSomeChunks()
        {
            BoundingSphere loadingArea = new BoundingSphere(
                Chunk.GetGridPosition(Player1.EyePosition).ToVector(), LoadRange * 0.5f);
            BoundingSphere emptySphere = new BoundingSphere(Chunk.GetGridPosition(Player1.EyePosition).ToVector(), 0);

            Point3D nextPos = Point3D.Empty;
            while (nextPos != null)
            {
                nextPos = GetClosestGridpositionLoaded(loadingArea, emptySphere)[0];

                if (nextPos != null)
                {
                    LoadedChunks.Add(nextPos, new Chunk(this.Device, nextPos, StartSeed));
                    //chunksLoading.Add(nextPos);
                    //LoadChunk(nextPos);
                }
            }
        }

        private void RemoveChunk(Chunk chunk)
        {
            lock (LoadedChunks)
            {
                LoadedChunks.RemoveAt(LoadedChunks.IndexOfValue(chunk));
            }
            if (chunk.ChunkAbove != null)
                chunk.ChunkAbove.ChunkBelow = null;
            if (chunk.ChunkBelow != null)
                chunk.ChunkBelow.ChunkAbove = null;
            if (chunk.ChunkInfront != null)
                chunk.ChunkInfront.ChunkBehind = null;
            if (chunk.ChunkBehind != null)
                chunk.ChunkBehind.ChunkInfront = null;
            if (chunk.ChunkLeft != null)
                chunk.ChunkLeft.ChunkRight = null;
            if (chunk.ChunkRight != null)
                chunk.ChunkRight.ChunkLeft = null;
        }

        //////////////TIMESTAMPS/////////////
        public int pointRecursion1Time;
        public int longest1;
        private void UpdateChunkGeneration()
        {

            AddFinishedLoadingChunks();
            AddFinishedBufferingChunks();

            BoundingSphere loadingArea = new BoundingSphere(
                Chunk.GetGridPosition(Player1.EyePosition).ToVector(), LoadRange);
            BoundingSphere bufferArea = new BoundingSphere(
                Chunk.GetGridPosition(Player1.EyePosition).ToVector(), BufferRange);

            DeleteChunkBufferOutOfRange(bufferArea);
            DeleteChunksOutOfRange(loadingArea);

            DateTime d1 = DateTime.Now;

            //getting the next position to load
            Point3D nextLoadingPosition = null;
            Point3D[] positions = GetClosestGridpositionLoaded(loadingArea, bufferArea);
            nextLoadingPosition = positions[0];
            //getting the next position to buffer
            Point3D nextBufferPosition = null;
            nextBufferPosition = positions[1];

            //Timestamps
            pointRecursion1Time = (int)(DateTime.Now - d1).TotalMilliseconds;
            if (pointRecursion1Time > longest1)
                longest1 = pointRecursion1Time;
            DateTime d2 = DateTime.Now;

            if (nextBufferPosition != null &&
               (chunksLoading.Count + chunksBuffering.Count) < ChunkUpdateThreads &&
                LoadedChunks.ContainsKey(nextBufferPosition) &&
                NeighboursLoaded(nextBufferPosition))
            {
                chunksBuffering.Add(nextBufferPosition);
                Task buffer = new Task(() => { BufferChunk(nextBufferPosition); });
                buffer.Start();
            }
            if (nextLoadingPosition != null && (chunksLoading.Count + chunksBuffering.Count) < ChunkUpdateThreads)
            {
                chunksLoading.Add(nextLoadingPosition);
                Task load = new Task(() => { LoadChunk(nextLoadingPosition); });
                load.Start();
            }
        }

        private void AddFinishedLoadingChunks()
        {
            // Fügt alle bis jetzt berechneten neuen Chunks hinzu
            lock (finishedLoadedChunks)
            {
                if (finishedLoadedChunks.Count > 0)
                {
                    lock (LoadedChunks)
                    {
                        foreach (Chunk chunk in finishedLoadedChunks)
                        {
                            this.LoadedChunks.Add(chunk.GridIndex, chunk);
                        }
                    }
                    //Lösche alle hinzugefügten aus der Arbeitsliste
                    chunksLoading.RemoveAll(point => finishedLoadedChunks.Exists(chunk => chunk.GridIndex == point));

                    finishedLoadedChunks.Clear();
                }
            }
        }

        private void AddFinishedBufferingChunks()
        {
            // Fügt alle bis jetzt gebufferten neuen Chunks hinzu
            lock (finishedBufferedChunks)
            {
                foreach (Chunk chunk in finishedBufferedChunks)
                {
                    this.BufferedChunks.Add(chunk.GridIndex, chunk);
                }
                //Lösche alle hinzugefügten aus der Arbeitsliste
                chunksBuffering.RemoveAll(point => finishedBufferedChunks.Exists(chunk => chunk.GridIndex == point));

                finishedBufferedChunks.Clear();
            }
        }

        private void DeleteChunkBufferOutOfRange(BoundingSphere sphere)
        {

            var removeChunks = BufferedChunks.Where(c => !sphere.Contains(c.Key)).ToList();

            foreach (var chunk in removeChunks)
            {
                this.WorldVertices -= chunk.Value.VerticesCount;
                this.WorldIndices -= chunk.Value.IndicesCount;
                chunk.Value.DeleteBuffer();
                BufferedChunks.Remove(chunk.Key);
            }
        }

        private void DeleteChunksOutOfRange(BoundingSphere sphere)
        {
            var removeChunks = this.LoadedChunks.Where(c =>
                !sphere.Contains(c.Key)).ToList();

            removeChunks.ForEach(c => RemoveChunk(c.Value));
        }

        private void BufferChunk(Point3D chunkPosition)
        {
            Chunk toBuffer;
            lock (LoadedChunks)
            {
                if (LoadedChunks.ContainsKey(chunkPosition))
                {
                    toBuffer = LoadedChunks[chunkPosition];
                    SetNeighbours(toBuffer);
                }
                else toBuffer = null;
            }
            if (toBuffer != null)
            {
                toBuffer.InitializeVisuals();
                this.WorldVertices += toBuffer.VerticesCount;
                this.WorldIndices += toBuffer.IndicesCount;

                lock (finishedBufferedChunks)
                {
                    finishedBufferedChunks.Add(toBuffer);
                }
            }
        }

        private void SetNeighbours(Chunk chunk)
        {
            if (LoadedChunks.ContainsKey(chunk.GridIndex.AddX(-1)))
            {
                chunk.ChunkLeft = LoadedChunks[chunk.GridIndex.AddX(-1)];
                LoadedChunks[chunk.GridIndex.AddX(-1)].ChunkRight = chunk;
            }
            if (LoadedChunks.ContainsKey(chunk.GridIndex.AddX(1)))
            {
                chunk.ChunkRight = LoadedChunks[chunk.GridIndex.AddX(1)];
                LoadedChunks[chunk.GridIndex.AddX(1)].ChunkLeft = chunk;
            }

            if (LoadedChunks.ContainsKey(chunk.GridIndex.AddY(-1)))
            {
                chunk.ChunkBelow = LoadedChunks[chunk.GridIndex.AddY(-1)];
                LoadedChunks[chunk.GridIndex.AddY(-1)].ChunkAbove = chunk;
            }
            if (LoadedChunks.ContainsKey(chunk.GridIndex.AddY(1)))
            {
                chunk.ChunkAbove = LoadedChunks[chunk.GridIndex.AddY(1)];
                LoadedChunks[chunk.GridIndex.AddY(1)].ChunkBelow = chunk;
            }

            if (LoadedChunks.ContainsKey(chunk.GridIndex.AddZ(-1)))
            {
                chunk.ChunkBehind = LoadedChunks[chunk.GridIndex.AddZ(-1)];
                LoadedChunks[chunk.GridIndex.AddZ(-1)].ChunkInfront = chunk;
            }
            if (LoadedChunks.ContainsKey(chunk.GridIndex.AddZ(1)))
            {
                chunk.ChunkInfront = LoadedChunks[chunk.GridIndex.AddZ(1)];
                LoadedChunks[chunk.GridIndex.AddZ(1)].ChunkBehind = chunk;
            }
        }

        private bool NeighboursLoaded(Point3D chunkPos)
        {
            return
                LoadedChunks.ContainsKey(chunkPos.AddX(-1)) &&
                LoadedChunks.ContainsKey(chunkPos.AddX(1)) &&
                LoadedChunks.ContainsKey(chunkPos.AddY(-1)) &&
                LoadedChunks.ContainsKey(chunkPos.AddY(1)) &&
                LoadedChunks.ContainsKey(chunkPos.AddZ(-1)) &&
                LoadedChunks.ContainsKey(chunkPos.AddZ(1));
        }

        private void LoadChunk(Point3D chunkPosition)
        {
            Chunk loadedChunk = new Chunk(this.Device, chunkPosition, StartSeed);

            lock (finishedLoadedChunks)
            {
                finishedLoadedChunks.Add(loadedChunk);
            }
        }

        /// <summary>
        /// when using this method it is important that no chunk outside the loadsphere exists
        /// </summary>
        /// <param name="loadSphere"></param>
        /// <param name="bufferSphere"></param>
        /// <returns>array[0] is loadpos, array[1] is bufferpos</returns>
        private Point3D[] GetClosestGridpositionLoaded(BoundingSphere loadSphere, BoundingSphere bufferSphere)
        {
            BoundingBox box = BoundingBox.CreateFromSphere(loadSphere);

            Point3D min = new Point3D((int)box.Min.X, (int)box.Min.Y, (int)box.Min.Z);
            Point3D max = new Point3D((int)box.Max.X + 1, (int)box.Max.Y + 1, (int)box.Max.Z + 1);

            float closestLoadDistance = float.MaxValue;
            Point3D loadResult = null;
            float closestBufferDistance = float.MaxValue;
            Point3D bufferResult = null;

            int loadListIndex = 0;
            int bufferListIndex = 0;
            for (int x = min.X; x <= max.X; x++)
                for (int y = min.Y; y <= max.Y; y++)
                    for (int z = min.Z; z <= max.Z; z++)
                    {
                        Point3D point = new Point3D(x, y, z);
                        float distance;
                        // if within LOADsphere
                        if ((distance = (point.ToVector() - loadSphere.Center).Length()) < loadSphere.Radius)
                        {
                            // chunk already exists in loadedChunks
                            if (LoadedChunks.Count > loadListIndex && LoadedChunks.Keys[loadListIndex] == point)
                            {
                                loadListIndex++;
                            }
                            else
                            {
                                if (closestLoadDistance > distance && !chunksLoading.Contains(point))
                                {
                                    closestLoadDistance = distance;
                                    loadResult = point;
                                }
                            }
                            //if within BUFFERsphere
                            if (distance < bufferSphere.Radius)
                            {
                                // chunk already exists in bufferedChunks
                                if (BufferedChunks.Count > bufferListIndex && BufferedChunks.Keys[bufferListIndex] == point)
                                {
                                    bufferListIndex++;
                                }
                                else
                                {
                                    if (closestBufferDistance > distance && !chunksBuffering.Contains(point))
                                    {
                                        closestBufferDistance = distance;
                                        bufferResult = point;
                                    }
                                }

                            }
                        }
                    }
            if (loadResult != null)
            {
                float minLoadDis = (new Vector3(
                    loadResult.X * Chunk.ChunkWidth,
                    loadResult.Y * Chunk.ChunkWidth,
                    loadResult.Z * Chunk.ChunkWidth) -
                    Player1.EyePosition).Length() - Chunk.Radius.Length();
                if (minLoadDis > MaxSpawnDistance)
                {
                    MaxSpawnDistanceSphereLoaded = true;
                }
                else
                    MaxSpawnDistanceSphereLoaded = false;
            }

            return new Point3D[] { loadResult, bufferResult };
        }

        #endregion

        #region Collision
        //private void WorldCollision()
        //{
        //    Dictionary<Body, BoundingBox> broadBoxes = new Dictionary<Body, BoundingBox>();
        //    foreach (Body col in this.Bodies)
        //        broadBoxes.Add(col, BoundingBox.CreateMerged(col.BBox, col.BBox.MoveBBox(col.PreCollisionMovement)));

        //    Dictionary<Body, List<BoundingBox>> staticColBoxes = new Dictionary<Body, List<BoundingBox>>();
        //    Dictionary<Body, List<Body>> bodiesColliding = new Dictionary<Body, List<Body>>();
        //    foreach (KeyValuePair<Body, BoundingBox> kv in broadBoxes)
        //    {
        //        BoundingBox b = kv.Key.BBox;
        //        if (float.IsNaN(b.Min.X) ||//DEBUG
        //            float.IsNaN(b.Min.Y) ||
        //            float.IsNaN(b.Min.Z) ||
        //            float.IsNaN(b.Max.X) ||
        //            float.IsNaN(b.Max.Y) ||
        //            float.IsNaN(b.Max.Z))
        //        { }

        //        staticColBoxes.Add(kv.Key, new List<BoundingBox>());
        //        bodiesColliding.Add(kv.Key, new List<Body>());
        //    }

        //    for (int i = 0; i < broadBoxes.Count; i++)
        //    {
        //        BoundingBox broadBox = broadBoxes.ElementAt(i).Value;
        //        foreach (Chunk chunk in this.LoadedChunks.Values)
        //            if (broadBox.Intersects(chunk.BBox))
        //                staticColBoxes.ElementAt(i).Value.AddRange(chunk.GetIntersectingBlockBoxes(broadBox));

        //        for (int j = 0; j < broadBoxes.Count; j++)
        //        {
        //            BoundingBox otherBroadBox = broadBoxes.ElementAt(j).Value;
        //            if (broadBox.Intersects(otherBroadBox) &&
        //                !broadBox.IsSameAs(otherBroadBox))
        //                bodiesColliding.ElementAt(i).Value.Add(this.Bodies[j]);
        //        }
        //    }
        //    for (int i = 0; i < broadBoxes.Count; i++)
        //    {
        //        Body col = broadBoxes.ElementAt(i).Key;
        //        col.Collide(staticColBoxes[col], bodiesColliding[col]);
        //    }
        //}

        public Vector3 PlayerCollBodies;
        public Vector3 PlayerCollStatic;
        private void WorldCollision()
        {
            PrepareAllBodiesForCollision();

            //SortedCollideAllBodiesWithEachOther(false);
            CollideAllBodiesWithEachOther(false);
            this.PlayerCollBodies = Player1.CollisionStop;

            //foreach (Body body in this.Bodies)
            //    body.CollisionStop = Vector3.Zero;
            Player1.CollisionStop = Vector3.Zero;

            CollideAllBodiesWithStaticBoxes();
            this.PlayerCollStatic = Player1.CollisionStop;

            //SortedCollideAllBodiesWithEachOther(true);
            CollideAllBodiesWithEachOther(true);

        }
        private void PrepareAllBodiesForCollision()
        {
            foreach (Body body in this.Characters)
                body.ResetCollisionParameters();
        }

        //private void SortedCollideAllBodiesWithEachOther(bool blockCollisionOnAlreadyCollidedSide)
        //{
        //    //Sort bodies
        //    List<Body> bodiesLeft = this.Bodies.ToList();
        //    bool foundPressingBodies = true;
        //    while (foundPressingBodies && bodiesLeft.Count > 0)
        //    {
        //        List<CollStruct> pressingBodies = FindAllPressingOnlyBodies(bodiesLeft);
        //        foundPressingBodies = pressingBodies.Count > 0;
        //        foreach (CollStruct col in pressingBodies)
        //            col.Pressing.Collide(col.GettingPressed, blockCollisionOnAlreadyCollidedSide);
        //        foreach (CollStruct col in pressingBodies)
        //            bodiesLeft.Remove(col.Pressing);
        //    }
        //}

        private void CollideAllBodiesWithEachOther(bool blockCollisionOnAlreadyCollidedSide)
        {
            for (int i = 0; i < this.Characters.Count; i++)
                for (int j = 0; j < this.Characters.Count; j++)
                    if (i != j)
                    {
                        BoundingBox iBP = GetBroadPhase(this.Characters[i]);
                        BoundingBox jBP = GetBroadPhase(this.Characters[j]);
                        if (iBP.Intersects(jBP))
                            this.Characters[i].Collide(this.Characters[j], blockCollisionOnAlreadyCollidedSide);
                    }
        }

        private List<CollStruct> FindAllPressingOnlyBodies(List<Body> bodies)
        {
            List<CollStruct> intersections = new List<CollStruct>();
            for (int i = 0; i < bodies.Count; i++)
                for (int j = 0; j < bodies.Count; j++)
                    if (i != j)
                    {
                        Body bodyI = bodies[i];
                        Body bodyJ = bodies[j];
                        BoundingBox iBP = GetBroadPhase(bodyI);
                        BoundingBox jBP = GetBroadPhase(bodyJ);
                        if (iBP.Intersects(jBP))
                        {
                            float collTime = Body.SweptAABB(bodyI.BBox, bodyI.PreCollisionMovement,
                            bodyJ.BBox, bodyJ.PreCollisionMovement);
                            if (collTime >= 0 && collTime < 1)
                            {
                                BoundingBox iTouchBox = bodyI.BBox.MoveBBox(bodyI.PreCollisionMovement * collTime);
                                BoundingBox jTouchBox = bodyJ.BBox.MoveBBox(bodyJ.PreCollisionMovement * collTime);

                                Vector3 overlap = Body.Overlap(iTouchBox, jTouchBox);
                                Vector3 clampedOverlap = overlap.Clamp(0);
                                float aX = clampedOverlap.Y * clampedOverlap.Z;
                                float aY = clampedOverlap.X * clampedOverlap.Z;
                                float aZ = clampedOverlap.X * clampedOverlap.Y;
                                float biggestA = Math.Max(aX, Math.Max(aY, aZ));

                                float strengthDivider = bodyI.MovementStrength + bodyJ.MovementStrength;
                                float thisStrengthFactor = (bodyI.MovementStrength / strengthDivider);
                                float otherStrengthFactor = (bodyJ.MovementStrength / strengthDivider);

                                Vector3 collisionMovement =
                                        bodyI.PreCollisionMovement * thisStrengthFactor +
                                        bodyJ.PreCollisionMovement * otherStrengthFactor;

                                bool iIsPressing = false;
                                if (biggestA > 0)
                                {
                                    if (aX > aY)
                                    {
                                        if (aZ > aX)
                                        {
                                            if (bodyI.PreCollisionMovement.Z.SameSign(bodyJ.PreCollisionMovement.Z))
                                                iIsPressing = Math.Abs(bodyI.PreCollisionMovement.Z) > Math.Abs(bodyJ.PreCollisionMovement.Z);
                                            else
                                            {
                                                iIsPressing = bodyI.PreCollisionMovement.Z.SameSign(collisionMovement.Z);
                                            }
                                        }
                                        else
                                        {
                                            if (bodyI.PreCollisionMovement.X.SameSign(bodyJ.PreCollisionMovement.X))
                                                iIsPressing = Math.Abs(bodyI.PreCollisionMovement.X) > Math.Abs(bodyJ.PreCollisionMovement.X);
                                            else
                                            {
                                                iIsPressing = bodyI.PreCollisionMovement.X.SameSign(collisionMovement.X);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (aZ > aY)
                                        {
                                            if (bodyI.PreCollisionMovement.Z.SameSign(bodyJ.PreCollisionMovement.Z))
                                                iIsPressing = Math.Abs(bodyI.PreCollisionMovement.Z) > Math.Abs(bodyJ.PreCollisionMovement.Z);
                                            else
                                            {
                                                iIsPressing = bodyI.PreCollisionMovement.Z.SameSign(collisionMovement.Z);
                                            }
                                        }
                                        else
                                        {
                                            if (bodyI.PreCollisionMovement.Y.SameSign(bodyJ.PreCollisionMovement.Y))
                                                iIsPressing = Math.Abs(bodyI.PreCollisionMovement.Y) > Math.Abs(bodyJ.PreCollisionMovement.Y);
                                            else
                                            {
                                                iIsPressing = bodyI.PreCollisionMovement.Y.SameSign(collisionMovement.Y);
                                            }
                                        }
                                    }
                                }

                                if (iIsPressing)
                                    intersections.Add(new CollStruct(bodyI, bodyJ));
                                else
                                    intersections.Add(new CollStruct(bodyJ, bodyI));
                            }
                            //intersections.Add();
                        }
                    }

            //intersections = intersections.Distinct().ToList();
            List<CollStruct> pressing = new List<CollStruct>();
            foreach (CollStruct col in intersections)
                if (!intersections.Any(c => c.GettingPressed == col.Pressing))
                {
                    pressing.Add(col);
                }

            return pressing;
        }

        private void CollideAllBodiesWithStaticBoxes()
        {
            for (int i = 0; i < this.Characters.Count; i++)
            {
                Body cur = this.Characters[i];
                BoundingBox BP = GetBroadPhase(cur);
                List<BoundingBox> staticBoxes = GetIntersectingBlockBoxes(BP);

                List<BoundingBox> touchBoxOverlappingBoxes = new List<BoundingBox>();
                List<BoundingBox> startBoxIntersectingBoxes = new List<BoundingBox>();
                List<BoundingBox> otherBoxes = new List<BoundingBox>();
                foreach (BoundingBox box in staticBoxes)
                {
                    Vector3 normal = Vector3.Zero;
                    float collisionTime = Body.SweptAABB(cur.BBox, cur.PreCollisionMovement, box, Vector3.Zero);
                    Vector3 movementToCollision = cur.PreCollisionMovement * collisionTime;
                    BoundingBox touchBox = cur.BBox.MoveBBox(movementToCollision);

                    Vector3 overlap = Body.Overlap(box, touchBox);

                    int overhangingDimensions = 0;
                    overhangingDimensions += overlap.X > 0 ? 1 : 0;
                    overhangingDimensions += overlap.Y > 0 ? 1 : 0;
                    overhangingDimensions += overlap.Z > 0 ? 1 : 0;

                    if (box.Intersects(cur.BBox))
                        startBoxIntersectingBoxes.Add(box);
                    else if (overhangingDimensions == 3)
                        touchBoxOverlappingBoxes.Add(box);
                    else
                        otherBoxes.Add(box);
                }

                foreach (BoundingBox staticBox in startBoxIntersectingBoxes)
                    cur.Collide(staticBox);
                foreach (BoundingBox staticBox in touchBoxOverlappingBoxes)
                    cur.Collide(staticBox);
                foreach (BoundingBox staticBox in otherBoxes)
                    cur.Collide(staticBox);
            }
        }


        private BoundingBox GetBroadPhase(Body body)
        {
            return BoundingBox.CreateMerged(body.BBox, body.BBox.MoveBBox(body.PreCollisionMovement));
        }

        private List<BoundingBox> GetIntersectingBlockBoxes(BoundingBox bbox)
        {
            List<BoundingBox> blockBoxes = new List<BoundingBox>();
            Point3D minChunkIndex = Chunk.GetGridPosition(bbox.Min);
            Point3D maxChunkIndex = Chunk.GetGridPosition(bbox.Max);

            for (int x = minChunkIndex.X; x <= maxChunkIndex.X; x++)
                for (int y = minChunkIndex.Y; y <= maxChunkIndex.Y; y++)
                    for (int z = minChunkIndex.Z; z <= maxChunkIndex.Z; z++)
                    {
                        Point3D chunkIndex = new Point3D(x, y, z);
                        if (this.LoadedChunks.ContainsKey(chunkIndex))
                            blockBoxes.AddRange(this.LoadedChunks[chunkIndex].GetIntersectingBlockBoxes(bbox));
                    }

            return blockBoxes;
        }
        #endregion

        #region ManageEnemies
        bool MaxSpawnDistanceSphereLoaded = false;

        private void SpawnAndDeleteEnemies()
        {
            List<Body> enemiesToDelete = new List<Body>();
            foreach (Living body in this.Characters)
                if (body is Enemy &&
                    ((body.EyePosition - Player1.EyePosition).Length() > RangeToDeleteEnemiesAt ||
                    (body.IsDead && (DateTime.Now - body.TimeDied) > WaittimeBeforeCorpsesGetDeleted)))
                    enemiesToDelete.Add(body);
            foreach (Living body in enemiesToDelete)
                this.Characters.Remove(body);

            if (this.Characters.Count < MaxEnemyCount && MaxSpawnDistanceSphereLoaded)
            {
                if (this.Characters.Count > 20)
                { }

                //find direction with least enemies (XZ: ++, --,-+,+-)
                int[] EnemyCount = new int[4];

                foreach (Body enemy in this.Characters)
                    if (enemy is Enemy)
                    {
                        Vector2 direction = new Vector2(enemy.EyePosition.X - Player1.EyePosition.X,
                            enemy.EyePosition.Z - Player1.EyePosition.Z);
                        if (direction.X >= 0 && direction.Y >= 0)
                            EnemyCount[0]++;
                        else if (direction.X < 0 && direction.Y < 0)
                            EnemyCount[1]++;
                        else if (direction.X < 0)
                            EnemyCount[2]++;
                        else if (direction.Y < 0)
                            EnemyCount[3]++;
                    }
                int lowestCount = int.MaxValue;
                int indexOfSideWithLowestCount = 0;
                for (int i = 0; i < EnemyCount.Length; i++)
                    if (EnemyCount[i] < lowestCount)
                    {
                        indexOfSideWithLowestCount = i;
                        lowestCount = EnemyCount[i];
                    }

                Vector2 leastEnemies;
                if (indexOfSideWithLowestCount == 0)
                    leastEnemies = Vector2.One;
                else if (indexOfSideWithLowestCount == 1)
                    leastEnemies = -Vector2.One;
                else if (indexOfSideWithLowestCount == 2)
                    leastEnemies = new Vector2(-1, 1);
                else// ==3
                    leastEnemies = new Vector2(1, -1);

                SpawnEnemy(leastEnemies);
            }
        }

        private void SpawnEnemy(Vector2 direction)
        {
            //Position
            float xPos = (float)Rnd.NextDouble();
            float yPos = (float)Rnd.NextDouble();
            float zPos = (float)Rnd.NextDouble();
            float distance = MinSpawnDistance + (float)Rnd.NextDouble() * (MaxSpawnDistance - MinSpawnDistance);
            Random rnd2 = new Random();
            int signedYPos = rnd2.Next(1, 99);
            if (signedYPos >= 50)
                yPos *= -1;
            if (direction.X < 0)
                xPos *= -1;
            if (direction.Y < 0)
                zPos *= -1;

            float xRot = (float)Rnd.NextDouble();
            float yRot = (float)Rnd.NextDouble();
            float zRot = (float)Rnd.NextDouble();
            int signedXRot = rnd2.Next(1, 99);
            int signedYRot = rnd2.Next(1, 99);
            int signedZRot = rnd2.Next(1, 99);
            if (signedXRot >= 50)
                xRot *= -1;
            if (signedYRot >= 50)
                yRot *= -1;
            if (signedZRot >= 50)
                zRot *= -1;
            Vector3 lookDirection = new Vector3(xRot, yRot, zRot);

            Vector3 relativeSpawnPosition = new Vector3(xPos, yPos, zPos);
            relativeSpawnPosition = relativeSpawnPosition.Normalized() * distance;
            Vector3 worldPosition = relativeSpawnPosition + Player1.EyePosition;
            BoundingBox bbox = Enemy.GetBoundingBox(worldPosition);
            float disToPlayer = MinSpawnDistance + (MaxSpawnDistance - MinSpawnDistance) * 0.5f;
            if (GetIntersectingBlockBoxes(bbox).Count > 0)
                while (disToPlayer < MaxSpawnDistance && disToPlayer > MinSpawnDistance)
                {
                    bbox = Enemy.GetBoundingBox(worldPosition);
                    if (Player1.FieldOfView.Intersects(bbox))
                    {

                        //if no block in line of sight
                        bool hiddenBehindBlock = false;
                        float distancePlayerEnemy = (bbox.Center() - Player1.BBox.Center()).Length();
                        Ray playerToEnemy = new Ray(Player1.EyePosition, relativeSpawnPosition.Normalized());
                        Dictionary<Chunk, float> chunks = IntersectingChunks(playerToEnemy);
                        foreach (KeyValuePair<Chunk, float> kv in chunks)
                        {
                            if (kv.Value < distancePlayerEnemy)
                            {
                                Point3D index = kv.Key.GetFirstIntersectedBlockIndex(playerToEnemy, distancePlayerEnemy, true, out Vector3? intersectionPoint);
                                if (index != null)
                                {
                                    hiddenBehindBlock = true;
                                    break;
                                }
                            }
                            else
                                break;
                        }
                        if (!hiddenBehindBlock)
                            break;
                    }
                    if (GetIntersectingBlockBoxes(bbox).Count > 0)
                    {
                        relativeSpawnPosition.Y += Block.Sidelength;
                        worldPosition = relativeSpawnPosition + Player1.EyePosition;
                        disToPlayer = (bbox.Center() - Player1.BBox.Center()).Length();
                    }
                    else
                    {
                        this.Characters.Add(new Enemy(
                            this.Device, worldPosition, lookDirection, (Chunk.GetSurfaceLevelInt(Chunk.GetGridPosition(worldPosition)))));

                        break;
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
            Ray playerRay = Player1.LookRay;
            Dictionary<Chunk, float> inChunks = IntersectingChunks(playerRay);
            Vector3? intersection = null;
            Point3D positionToDig = null;

            float BodyIntersectionRange = float.MaxValue;
            foreach (Body body in this.Characters)
                if (!(body is Player))
                {
                    float? curIntersection = body.BBox.Intersects(playerRay);
                    if (curIntersection.HasValue && curIntersection.Value < BodyIntersectionRange)
                        BodyIntersectionRange = curIntersection.Value;
                }

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
                for (int i = 0; i < inChunks2.Count; i++)
                {
                    Point3D positionToBuild = inChunks2.ElementAt(i).Key.GetFirstIntersectedBlockIndex(
                        backRay, Block.Radius * 2, false, out Vector3? airIntersectionPoint);

                    if (positionToBuild != null)
                    {
                        BoundingBox blockBox = inChunks2.ElementAt(i).Key.GetBlockBBox(positionToBuild.X, positionToBuild.Y, positionToBuild.Z);
                        if (!this.Characters.Any(body => (!(body is Player) && body.BBox.Intersects(blockBox))))
                            if (Player1.BBox.Intersects(blockBox))
                            {
                                if (blockBox.Align(Player1.BBox) && Player1.BBox.Contains(blockBox.Center()) == ContainmentType.Disjoint)
                                {
                                    Block newB = Player1.inventory.GetOneItemAtSelectedSlot();
                                    if (newB != null)
                                        inChunks2.ElementAt(i).Key.ChangeBlock(positionToBuild, newB);
                                }
                            }
                            else
                            {
                                Block newB = Player1.inventory.GetOneItemAtSelectedSlot();
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

            float BodyIntersectionRange = float.MaxValue;
            Living intersectedLiving = null;
            foreach (Living body in this.Characters)
                if (!(body is Player))
                {
                    float? curIntersection = body.BBox.Intersects(playerRay);
                    if (curIntersection.HasValue && curIntersection.Value < BodyIntersectionRange)
                    {
                        BodyIntersectionRange = curIntersection.Value;
                        intersectedLiving = body;
                    }
                }

            for (int i = 0; i < inChunks.Count; i++)
            {
                Point3D positionToDig = inChunks.ElementAt(i).Key.GetFirstIntersectedBlockIndex(playerRay, Player.MaxDiggingRange, true, out intersection);
                if (positionToDig != null)
                {
                    BoundingBox blockBox = inChunks.ElementAt(i).Key.GetBlockBBox(positionToDig.X, positionToDig.Y, positionToDig.Z);

                    if ((blockBox.Center() - Player1.BBox.Center()).Length() < BodyIntersectionRange)
                    {
                        if (inChunks.ElementAt(i).Key.AllBlocks[positionToDig.X, positionToDig.Y, positionToDig.Z]
                            .Armor <= Player1.DigStrength)
                        {
                            Block b = inChunks.ElementAt(i).Key.ChangeBlock(positionToDig, new Block(BlockTypes.Air));
                            if (b != null)
                                Player1.inventory.Add(b.Pickup);
                        }
                    }
                    else
                    {
                        intersectedLiving.GetDamaged(Player1.Damage);
                    }
                    break;
                }
            }
        }

        private Dictionary<Chunk, float> IntersectingChunks(Ray ray)
        {
            Dictionary<Chunk, float> dDistanceChunk = new Dictionary<Chunk, float>();
            //Collect intersected Chunks
            for (int i = 0; i < this.BufferedChunks.Count; i++)
            {
                float? f = ray.Intersects(BufferedChunks.Values[i].BBox);
                if (f != null)
                    if ((float)f >= 0 && (float)f < Player.MaxDiggingRange)
                        dDistanceChunk.Add(BufferedChunks.Values[i], (float)f);
            }
            dDistanceChunk = dDistanceChunk.SortByFloat();
            return dDistanceChunk;
        }
        #endregion

        public int drawTime;
        public void Draw()
        {
            DateTime dt = DateTime.Now;
            Matrix viewProjectionMatrix = CurrentCamera.ViewMatrix * CurrentCamera.ProjectionMatrix;

            DrawSuns();
            DrawChunks(viewProjectionMatrix);
            DrawBodies(viewProjectionMatrix);
            if (DrawDebugInfo)
                DrawOrientation();

            drawTime = (int)(DateTime.Now - dt).TotalMilliseconds;
        }

        private void DrawChunks(Matrix viewProjectionMatrix)
        {
            Effect effect = BlockEffect;

            effect.CurrentTechnique = effect.Techniques["BlockShader"];
            effect.Parameters["Texture"].SetValue(Block.BlockTex);
            effect.Parameters["NormalMap"].SetValue(Block.BlockBumpMap);

            effect.Parameters["DiffuseLightDirections"].SetValue(SkyBox.LightDirections);
            effect.Parameters["DiffuseColors"].SetValue(SkyBox.LightColors);
            effect.Parameters["DiffuseIntensities"].SetValue(SkyBox.LightIntensities);

            this.WorldVerticesDrawn = 0;
            BoundingFrustum fov = Characters[CamIndex].FieldOfView;
            foreach (Chunk chunk in this.BufferedChunks.Values)
            {
                if (chunk.BBox.Intersects(fov))
                {
                    int drawnVertices = chunk.Draw(Chunk.GetGridPosition(CurrentCamera.Position),
                        effect, viewProjectionMatrix);
                    this.WorldVerticesDrawn += drawnVertices;
                }
            }


        }

        private void DrawSuns()
        {
            Effect effect = PointSpriteEffect;

            VertexPositionTexture[] sunVertices = SkyBox.GetSunVertices(CurrentCamera.Position);// GetLightsVertices();

            effect.CurrentTechnique = effect.Techniques["PointSprite"];

            effect.Parameters["World"].SetValue(Matrix.Identity);
            effect.Parameters["View"].SetValue(CurrentCamera.ViewMatrix);
            effect.Parameters["Projection"].SetValue(CurrentCamera.ProjectionMatrix);

            effect.Parameters["Texture"].SetValue(SunsTexture);
            effect.Parameters["CamPos"].SetValue(CurrentCamera.Position);
            effect.Parameters["CamUp"].SetValue(Vector3.Up);

            for (int i = 0; i < Sky.SunCount; i++)
                if (SkyBox.Suns[i].LightIntensity > 0)
                {
                    effect.Parameters["PointSpriteSize"].SetValue(SkyBox.Suns[i].OriginalLightSize);
                    effect.Parameters["SpriteColor"].SetValue(SkyBox.Suns[i].LightColor);
                    foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        this.Device.DrawUserPrimitives(PrimitiveType.TriangleList, sunVertices, i * 6, 2);
                    }
                }
        }

        private void DrawBodies(Matrix viewProjectionMatrix)
        {
            Effect effect = CharacterEffect;

            effect.CurrentTechnique = effect.Techniques["CharacterShader"];
            effect.Parameters["Texture"].SetValue(Enemy.EnemyTex);
            effect.Parameters["DiffuseLightDirections"].SetValue(SkyBox.LightDirections);
            effect.Parameters["DiffuseColors"].SetValue(SkyBox.LightColors);
            effect.Parameters["DiffuseIntensities"].SetValue(SkyBox.LightIntensities);

            foreach (Body body in this.Characters)
                body.Draw(effect, viewProjectionMatrix);
        }

        private static readonly VertexPositionColor[] OrientationLines = new VertexPositionColor[]{
                new VertexPositionColor(Vector3.Zero,Color.Red),
                new VertexPositionColor(Vector3.Right,Color.Red),
                new VertexPositionColor(Vector3.Zero,Color.Green),
                new VertexPositionColor(Vector3.Up,Color.Green),
                new VertexPositionColor(Vector3.Zero,Color.Blue),
                new VertexPositionColor(Vector3.Forward,Color.Blue)
            };
        static readonly RasterizerState noCulling = new RasterizerState()
        {
            CullMode = CullMode.None
        };
        private void DrawOrientation()
        {
            BlendState previousBS = Device.BlendState;
            Device.BlendState = BlendState.Additive;

            BasicEffect effect = (BasicEffect)BasicEffect;
            effect.VertexColorEnabled = true;

            effect.Projection = CurrentCamera.ProjectionMatrix;
            effect.View = CurrentCamera.ViewMatrix;
            effect.World = Matrix.CreateScale(0.02f) *
                Matrix.CreateTranslation(CurrentCamera.Position + CurrentCamera.LookDirection * (MovebleCamera.NearPlane + 0.1f));

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                this.Device.DrawUserPrimitives(PrimitiveType.LineList, OrientationLines, 0, 3);
            }


            Device.BlendState = previousBS;
        }

        public bool DrawDebugInfo = true;

    }
}
