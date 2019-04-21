using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace BlockGame3D
{
    abstract class Body : MovebleCamera
    {
        #region VisualDefaults
        private const float BorderTolerance = 0f;
        private const int TexturesPerType = 6;

        public static Texture2D EnemyTex;
        private float TexPosUpper { get { return (1 * 1.0f) / ((int)EnemyTypes.TypeCount * 1.0f) + BorderTolerance; } }
        private float TexPosLower { get { return (1 * 1.0f + 1) / ((int)EnemyTypes.TypeCount * 1.0f) - BorderTolerance; } }

        private static VertexBuffer DefaultThirdPersonVBuffer = null;
        private static IndexBuffer DefaultThirdPersonIBuffer = null;
        private static bool DefaultBuffersInitialized = false;

        private static int VerticesCount;
        private static int IndicesCount;
        #endregion
        
        //Movement
        private readonly float MovementAcceleration = 12f;
        private readonly float BrakeAcceleration = 1 / 1500f;
        private readonly float GravityAcceleration = 40f;
        private readonly float JumpAcceleration = 10.5f;
        private readonly float MaxDownMovement = -40;
        private readonly float MaxFlatMovement = 6;
        public readonly float MovementStrength = 1;

        //Collision (Body-Size)
        private readonly float EyeHeight = 1.5f;
        private readonly float BodyHeight = 1.8f;
        private readonly float BodyWidth = NearPlane * 2 + 0.4f;
        public Vector3 Center { get { return this.EyePosition + Vector3.Up * (+0.5f * BodyHeight - EyeHeight); } }

        private Matrix _ScaleMatrix = Matrix.Identity;
        protected Matrix ScaleMatrix
        {
            get { return _ScaleMatrix; }
            set
            {
                _ScaleMatrix = value;
                WorldMatrix = _ScaleMatrix * _RotationMatrix * _TranslationMatrix;
            }
        }
        private Matrix _TranslationMatrix = Matrix.Identity;
        protected Matrix TranslationMatrix
        {
            get { return _TranslationMatrix; }
            set
            {
                _TranslationMatrix = value;
                WorldMatrix = _ScaleMatrix * _RotationMatrix * _TranslationMatrix;
            }
        }
        private Matrix _RotationMatrix = Matrix.Identity;
        protected Matrix RotationMatrix
        {
            get { return _RotationMatrix; }
            set
            {
                _RotationMatrix = value;
                WorldMatrix = _ScaleMatrix * _RotationMatrix * _TranslationMatrix;
            }
        }
        public Matrix WorldMatrix { get; private set; }
        public bool CamActive { get; set; }

        public bool HasMoved { get; private set; }
        //Debug
        public bool GravityOn = true;
        #region constructors
        public Body(GraphicsDevice device, Vector3 position, Vector3 lookDirection,
            float acceleration, float brakeAcceleration, float gravityAcceleration, float jumpAcceleration,
            float maxFallSpeed, float maxMovementSpeed, float movementStrength,
            float eyeHeight, float bodyHeight, float bodyWidth, float fieldOfView, float viewDistance)
            : this(device, position, lookDirection, fieldOfView, viewDistance)
        {
            this.MovementAcceleration = acceleration;
            this.BrakeAcceleration = brakeAcceleration;
            this.GravityAcceleration = gravityAcceleration;
            this.JumpAcceleration = jumpAcceleration;
            this.MaxDownMovement = maxFallSpeed;
            this.MaxFlatMovement = maxMovementSpeed;
            this.MovementStrength = movementStrength;
            this.EyeHeight = eyeHeight;
            this.BodyHeight = bodyHeight;
            this.BodyWidth = bodyWidth;
            if (BodyHeight - EyeHeight <= NearPlane)
                throw new Exception("bodyheight has to be more than " + NearPlane + " higher than eyeheight.");
            if (bodyWidth <= NearPlane * 2)
                throw new Exception("bodywidth has to be more than " + (2 * NearPlane) + ".");
            SetBBox();
            this.ScaleMatrix = Matrix.CreateScale(new Vector3(BodyWidth, BodyHeight, BodyWidth));

        }

        public Body(GraphicsDevice device, Vector3 position, Vector3 lookDiretion, float fov, float viewDistance)
            : base(device, position, lookDiretion, fov, viewDistance)
        {
            SetBBox();
            this.ScaleMatrix = Matrix.CreateScale(new Vector3(BodyWidth, BodyHeight, BodyWidth));
            this.TranslationMatrix = Matrix.CreateTranslation(this.BBox.Center());

            if (!DefaultBuffersInitialized)
                SetupVerticesIndices(device);
            OnGround = false;
        }
        #endregion

        protected void UpdateBody(TimeSpan elapsedTime)
        {
            Gravity(elapsedTime);
            MovementProcessing(elapsedTime);
        }

        public abstract void Update(TimeSpan elapsedTime, Player player1);

        #region Collision

        public BoundingBox BBox { get; private set; }
        public Vector3 EyePosition
        {
            get { return this.Position; }
            private set { this.Position = value; SetBBox(); }
        }

        /// <summary>
        /// How much velocity has been lost due to collision and in which direction
        /// </summary>
        public Vector3 CollisionStop;
        protected bool OnGround { get; private set; }

        public void ResetCollisionParameters()
        {
            this.CollisionStop = Vector3.Zero;
            OnGround = false;
        }

        private void SetBBox()
        {
            this.BBox = new BoundingBox(new Vector3(
                this.Position.X - 0.5f * BodyWidth,
                this.Position.Y - EyeHeight,
                this.Position.Z - 0.5f * BodyWidth),
                new Vector3(
                this.Position.X + 0.5f * BodyWidth,
                this.Position.Y - EyeHeight + BodyHeight,
                this.Position.Z + 0.5f * BodyWidth));
        }

        //public void Collide(IEnumerable<BoundingBox> staticBBoxes, IEnumerable<Body> nearBodies)
        //{
        //    ResetCollisionParameters();

        //    List<BoundingBox> touchBoxOverlappingBoxes = new List<BoundingBox>();
        //    List<BoundingBox> startBoxIntersectingBoxes = new List<BoundingBox>();
        //    List<BoundingBox> otherBoxes = new List<BoundingBox>();
        //    foreach (BoundingBox box in staticBBoxes)
        //    {
        //        Vector3 normal = Vector3.Zero;
        //        float collisionTime = SweptAABB(this.BBox, this.PreCollisionMovement, box, Vector3.Zero, out normal);
        //        Vector3 movementToCollision = PreCollisionMovement * collisionTime;
        //        BoundingBox touchBox = this.BBox.MoveBBox(movementToCollision);

        //        Vector3 overlap = Overlap(box, touchBox);

        //        int overhangingDimensions = 0;
        //        overhangingDimensions += overlap.X > 0 ? 1 : 0;
        //        overhangingDimensions += overlap.Y > 0 ? 1 : 0;
        //        overhangingDimensions += overlap.Z > 0 ? 1 : 0;

        //        if (box.Intersects(this.BBox))
        //            startBoxIntersectingBoxes.Add(box);
        //        else if (overhangingDimensions == 3)
        //            touchBoxOverlappingBoxes.Add(box);
        //        else
        //            otherBoxes.Add(box);
        //    }

        //    List<Vector3> modifiedMovementVectors = new List<Vector3>();
        //    //bodies can push each other into the world, so this check has to be done first
        //    foreach (Body body in nearBodies)
        //    {
        //        Collide(body);
        //        modifiedMovementVectors.Add(body.PreCollisionMovement);
        //    }
        //    //Sorted World-Blocks
        //    foreach (BoundingBox staticBox in startBoxIntersectingBoxes)
        //        Collide(staticBox);
        //    foreach (BoundingBox staticBox in touchBoxOverlappingBoxes)
        //        Collide(staticBox);
        //    foreach (BoundingBox staticBox in otherBoxes)
        //        Collide(staticBox);
        //    foreach (Body body in nearBodies)
        //        Collide(body.BBox);


        //    this.PostCollisionMovement = PreCollisionMovement;
        //}

        public void Collide(Body body, bool blockCollisionOnAlreadyCollidedSide)
        {
            float strengthDivider = this.MovementStrength + body.MovementStrength;
            float thisStrengthFactor = (this.MovementStrength / strengthDivider);
            float otherStrengthFactor = (body.MovementStrength / strengthDivider);

            Vector3 collisionMovement =
                    this.PreCollisionMovement * thisStrengthFactor +
                    body.PreCollisionMovement * otherStrengthFactor;
            if (blockCollisionOnAlreadyCollidedSide)
            {
                if (this.CollisionStop.X != 0 || body.CollisionStop.X != 0)
                    collisionMovement.X = 0;
                if (this.CollisionStop.Y != 0 || body.CollisionStop.Y != 0)
                    collisionMovement.Y = 0;
                if (this.CollisionStop.Z != 0 || body.CollisionStop.Z != 0)
                    collisionMovement.Z = 0;
            }

            Vector3 normal = Vector3.Zero;
            float collisionTime = SweptAABB(this.BBox, this.PreCollisionMovement, body.BBox, body.PreCollisionMovement);

            Vector3 thisFinal;
            Vector3 otherFinal;

            if (collisionTime < 1.0f)
            {
                collisionMovement *= (1 - collisionTime);
                //this values
                Vector3 thisMovementToCollision = this.PreCollisionMovement * collisionTime;
                float thisRemainingTime = 1.0f - collisionTime;
                Vector3 thisMovementWhileCollision = this.PreCollisionMovement * thisRemainingTime;
                BoundingBox thisTouchBox = this.BBox.MoveBBox(thisMovementToCollision);
                //other values
                Vector3 otherMovementToCollision = body.PreCollisionMovement * collisionTime;
                float otherRemainingTime = 1.0f - collisionTime;
                Vector3 otherMovementWhileCollision = body.PreCollisionMovement * thisRemainingTime;
                BoundingBox otherTouchBox = body.BBox.MoveBBox(otherMovementToCollision);

                thisMovementWhileCollision = this.SlideCollisionResponse(
                    thisMovementWhileCollision, otherMovementWhileCollision, collisionMovement, thisTouchBox, otherTouchBox);

                otherMovementWhileCollision = body.SlideCollisionResponse(
                    otherMovementWhileCollision, thisMovementWhileCollision, collisionMovement, otherTouchBox, thisTouchBox);

                thisFinal = thisMovementToCollision + thisMovementWhileCollision;
                otherFinal = otherMovementToCollision + otherMovementWhileCollision;
            }
            else
            {
                thisFinal = this.PreCollisionMovement;
                otherFinal = body.PreCollisionMovement;
            }

            //final modification of the movevector
            this.PreCollisionMovement = thisFinal;
            body.PreCollisionMovement = otherFinal;
        }

        public void Collide(BoundingBox staticBox)
        {
            Vector3 normal = Vector3.Zero;
            float collisionTime = SweptAABB(this.BBox, this.PreCollisionMovement, staticBox, Vector3.Zero);

            Vector3 movementToCollision = PreCollisionMovement * collisionTime;
            float remainingTime = 1.0f - collisionTime;
            Vector3 movementWhileCollision = PreCollisionMovement * remainingTime;
            BoundingBox touchBox = this.BBox.MoveBBox(movementToCollision);

            movementWhileCollision = SlideCollisionResponse(
                movementWhileCollision, Vector3.Zero, Vector3.Zero, touchBox, staticBox);

            Vector3 final = movementToCollision + movementWhileCollision;

            //final modification of the movevector
            this.PreCollisionMovement = final;
        }

        private Vector3 SlideCollisionResponse(Vector3 thisMovementWhileCollision, Vector3 otherMovementWhileCollision,
            Vector3 collisionMovement, BoundingBox thisTouchBox, BoundingBox otherBox)
        {
            #region set variables
            Vector3 modifiedMWCV = thisMovementWhileCollision;
            Vector3 roundingErrorCorrection = Vector3.Zero;

            Vector3 overlap = Overlap(otherBox, thisTouchBox);
            Vector3 clampedOverlap = overlap.Clamp(0);
            float aX = clampedOverlap.Y * clampedOverlap.Z;
            float aY = clampedOverlap.X * clampedOverlap.Z;
            float aZ = clampedOverlap.X * clampedOverlap.Y;
            float biggestA = Math.Max(aX, Math.Max(aY, aZ));

            int overhangingDimensions = 0;
            overhangingDimensions += overlap.X > 0 ? 1 : 0;
            overhangingDimensions += overlap.Y > 0 ? 1 : 0;
            overhangingDimensions += overlap.Z > 0 ? 1 : 0;
            #endregion

            #region rouding errors
            // if boxes are inside each other(because rounding error or glitches occurs)
            // push out the smallest overhang and move bbox
            if (overhangingDimensions == 3)
            {
                if (clampedOverlap.X < clampedOverlap.Y)
                    if (clampedOverlap.Z < clampedOverlap.X) //Z
                    {
                        Vector3 dif = new Vector3(0, 0, -PreCollisionMovement.Z.Normalize() * clampedOverlap.Z);
                        thisTouchBox = thisTouchBox.MoveBBox(dif);
                        roundingErrorCorrection += dif;
                    }
                    else //X
                    {
                        Vector3 dif = new Vector3(-PreCollisionMovement.X.Normalize() * clampedOverlap.X, 0, 0);
                        thisTouchBox = thisTouchBox.MoveBBox(dif);
                        roundingErrorCorrection += dif;
                    }
                else
                    if (clampedOverlap.Z < clampedOverlap.Y)//Z
                    {
                        Vector3 dif = new Vector3(0, 0, -PreCollisionMovement.Z.Normalize() * clampedOverlap.Z);
                        thisTouchBox = thisTouchBox.MoveBBox(dif);
                        roundingErrorCorrection += dif;
                    }
                    else//Y
                    {
                        Vector3 dif = new Vector3(0, -PreCollisionMovement.Y.Normalize() * clampedOverlap.Y, 0);
                        thisTouchBox = thisTouchBox.MoveBBox(dif);
                        roundingErrorCorrection += dif;


                    }
                overlap = Overlap(otherBox, thisTouchBox);
                clampedOverlap = overlap.Clamp(0);

                aX = clampedOverlap.Y * clampedOverlap.Z;
                aY = clampedOverlap.X * clampedOverlap.Z;
                aZ = clampedOverlap.X * clampedOverlap.Y;
                biggestA = Math.Max(aX, Math.Max(aY, aZ));
            }
            #endregion
            Vector3 differenceMovement = thisMovementWhileCollision - otherMovementWhileCollision;
            Vector3 differenceBoxCenters = otherBox.Center() - thisTouchBox.Center();

            if (biggestA > 0)
            {
                if (aX > aY)
                {
                    if (aZ > aX)
                    {
                        if (overlap.Z >= 0 && differenceMovement.Z.SameSign(differenceBoxCenters.Z))
                        {
                            //if (this.CollisionStop.Z == 0)
                            //
                            this.CollisionStop.Z += modifiedMWCV.Z - collisionMovement.Z;
                            modifiedMWCV.Z = collisionMovement.Z;
                            //}
                        }
                    }
                    else
                    {
                        if (overlap.X >= 0 && differenceMovement.X.SameSign(differenceBoxCenters.X))
                        {
                            //if (this.CollisionStop.X == 0)
                            //{
                            this.CollisionStop.X += modifiedMWCV.X - collisionMovement.X;
                            modifiedMWCV.X = collisionMovement.X;
                            //}
                        }
                    }
                }
                else
                {
                    if (aZ > aY)
                    {
                        if (overlap.Z >= 0 && differenceMovement.Z.SameSign(differenceBoxCenters.Z))
                        {
                            //if (this.CollisionStop.Z == 0)
                            //{
                            this.CollisionStop.Z += modifiedMWCV.Z - collisionMovement.Z;
                            modifiedMWCV.Z = collisionMovement.Z;
                            //}
                        }
                    }
                    else
                    {
                        if (overlap.Y >= 0 && differenceMovement.Y.SameSign(differenceBoxCenters.Y))
                        {
                            //if (this.CollisionStop.Y == 0)
                            //{
                            if (thisMovementWhileCollision.Y <= 0 && thisTouchBox.Min.Y.Close10000(otherBox.Max.Y))
                            {
                                OnGround = true;
                                UpDownMovement = 0;
                            }
                            else if (thisTouchBox.Max.Y.Close10000(otherBox.Min.Y))
                                UpDownMovement = 0;

                            this.CollisionStop.Y += modifiedMWCV.Y - collisionMovement.Y;
                            modifiedMWCV.Y = collisionMovement.Y;
                            //}
                        }
                    }
                }
            }

            #region old
            //// Y
            //if (thisTouchBox.Min.X < staticBox.Max.X && thisTouchBox.Max.X > staticBox.Min.X &&
            //    thisTouchBox.Min.Z < staticBox.Max.Z && thisTouchBox.Max.Z > staticBox.Min.Z)
            //{
            //    if (thisTouchBox.Min.Y.Close10000(staticBox.Max.Y)) // static below
            //    {
            //        modifiedMWCV.Y = 0;
            //        if (staticBox.Max.Y - thisTouchBox.Min.Y > 0)
            //            modifiedMWCV.Y += 0.00001f;
            //        if (PreCollisionMovement.Y <= 0)
            //        {
            //            UpDownMovement = 0;
            //            OnGround = true;
            //        }
            //    }
            //    else if (thisTouchBox.Max.Y.Close10000(staticBox.Min.Y)) // static above
            //    {
            //        modifiedMWCV.Y = 0;
            //        if (staticBox.Min.Y - thisTouchBox.Max.Y < 0)
            //            modifiedMWCV.Y -= 0.00001f;
            //        if (PreCollisionMovement.Y > 0)
            //            UpDownMovement = 0;
            //    }
            //}


            //// X
            //if (thisTouchBox.Min.Y < staticBox.Max.Y && thisTouchBox.Max.Y > staticBox.Min.Y &&
            //    thisTouchBox.Min.Z < staticBox.Max.Z && thisTouchBox.Max.Z > staticBox.Min.Z)
            //{
            //    if (thisTouchBox.Min.X.Close10000(staticBox.Max.X)) // static left
            //    {
            //        modifiedMWCV.X = 0;
            //        if (staticBox.Max.X - thisTouchBox.Min.X < 0)
            //            modifiedMWCV.X += 0.00001f;
            //    }
            //    else if (thisTouchBox.Max.X.Close10000(staticBox.Min.X)) // static right
            //    {
            //        modifiedMWCV.X = 0;
            //        if (staticBox.Min.X - thisTouchBox.Max.X < 0)
            //            modifiedMWCV.X -= 0.00001f;
            //    }
            //}

            //// Z
            //if (thisTouchBox.Min.Y < staticBox.Max.Y && thisTouchBox.Max.Y > staticBox.Min.Y &&
            //    thisTouchBox.Min.X < staticBox.Max.X && thisTouchBox.Max.X > staticBox.Min.X)
            //{
            //    if (thisTouchBox.Min.Z.Close10000(staticBox.Max.Z)) // static behind
            //    {
            //        modifiedMWCV.Z = 0;
            //        if (staticBox.Max.Z - thisTouchBox.Min.Z < 0)
            //            modifiedMWCV.Z += 0.00001f;
            //    }
            //    else if (thisTouchBox.Max.Z.Close10000(staticBox.Min.Z)) // static infront
            //    {
            //        modifiedMWCV.Z = 0;
            //        if (staticBox.Min.Z - thisTouchBox.Max.Z < 0)
            //            modifiedMWCV.Z -= 0.00001f;
            //    }
            //}


            //#region Slide on Corners

            #endregion
            Vector3 final = modifiedMWCV + roundingErrorCorrection;
            return final;
        }

        public static Vector3 Overlap(BoundingBox b1, BoundingBox b2)
        {
            float resultX, resultY, resultZ;
            {
                float minX = b1.Min.X > b2.Min.X ? b1.Min.X : b2.Min.X;

                if (b1.Max.X < b2.Max.X)
                    resultX = b1.Max.X - minX;
                else
                    resultX = b2.Max.X - minX;
            }
            {
                float minY = b1.Min.Y > b2.Min.Y ? b1.Min.Y : b2.Min.Y;
                if (b1.Max.Y < b2.Max.Y)
                    resultY = b1.Max.Y - minY;
                else
                    resultY = b2.Max.Y - minY;
            }
            {
                float minZ = b1.Min.Z > b2.Min.Z ? b1.Min.Z : b2.Min.Z;
                if (b1.Max.Z < b2.Max.Z)
                    resultZ = b1.Max.Z - minZ;
                else
                    resultZ = b2.Max.Z - minZ;
            }
            //if (resultX < 0)
            //    resultX = 0;
            //if (resultY < 0)
            //    resultY = 0;
            //if (resultZ < 0)
            //    resultZ = 0;
            return new Vector3(resultX, resultY, resultZ);
        }

        private static Vector3 Overhang(BoundingBox b1, Vector3 movement1, BoundingBox b2, Vector3 movement2)
        {
            Vector3 movementDif = movement2 - movement1;
            //b1 OTHER ////////// b2 THIS
            Vector3 overhang = Vector3.Zero;
            if (movementDif.X > 0.0f)
            {
                overhang.X = b1.Min.X - b2.Max.X;
            }
            else
            {
                overhang.X = b1.Max.X - b2.Min.X;
            }
            if (movementDif.Y > 0.0f)
            {
                overhang.Y = b1.Min.Y - b2.Max.Y;
            }
            else
            {
                overhang.Y = b1.Max.Y - b2.Min.Y;
            }
            if (movementDif.Z > 0.0f)
            {
                overhang.Z = b1.Min.Z - b2.Max.Z;
            }
            else
            {
                overhang.Z = b1.Max.Z - b2.Min.Z;
            }
            return overhang;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="movingBox"></param>
        /// <param name="b2"></param>
        /// <param name="normal">normal of collided surface</param>
        /// <returns></returns>
        public static float SweptAABB(BoundingBox b1, Vector3 movement1, BoundingBox b2, Vector3 movement2)
        {
            //b1 THIS ///////// b2 OTHER

            //find the distance between the objects on the near and far sides for x, y and z
            Vector3 invEntry = Overhang(b1, movement1, b2, movement2);

            Vector3 movementDif = movement2 - movement1;
            Vector3 entry = Vector3.Zero;
            #region find time of collision and time of leaving for each axis
            if (movementDif.X == 0.0f)
            {
                entry.X = float.NegativeInfinity;
            }
            else
            {
                entry.X = invEntry.X / movementDif.X;
            }
            if (movementDif.Y == 0.0f)
            {
                entry.Y = float.NegativeInfinity;
            }
            else
            {
                entry.Y = invEntry.Y / movementDif.Y;
            }
            if (movementDif.Z == 0.0f)
            {
                entry.Z = float.NegativeInfinity;
            }
            else
            {
                entry.Z = invEntry.Z / movementDif.Z;
            }
            #endregion

            //get the time of the first collision
            float entryTime = Math.Max(entry.X, Math.Max(entry.Y, entry.Z));

            // if there was no collision
            if (entryTime < 0 || entryTime >= 1)
                return 1.0f;

            //return the time of collision
            return entryTime;
        }

        #endregion

        private void MovementProcessing(TimeSpan elapsedTime)
        {
            MoveLimitSpeed();
            SetPlayerMoveVector(elapsedTime);
        }

        #region MOVEMENT PROCESSING
        public float UpDownMovement = 0;
        protected float LeftRightMovement = 0;
        protected float ForwardBackwardMovement = 0;
        protected Vector2 UnscaledFlatMovement
        {
            get
            {
                Vector2 forward = this.LookDirection.ToVector2D();
                forward.Normalize();
                Vector2 left = new Vector2(forward.Y, -forward.X);
                return forward * ForwardBackwardMovement + left * LeftRightMovement;
            }
            set
            {
                Vector3 lookDirection3 = this.LookDirection;

                Vector2 lookDirection2 = new Vector2(lookDirection3.X, lookDirection3.Z);
                lookDirection2.Normalize();
                ForwardBackwardMovement = lookDirection2.X * value.X + lookDirection2.Y * value.Y;
                LeftRightMovement = lookDirection2.Y * value.X - lookDirection2.X * value.Y;
            }
        }
        protected Vector3 MovementDirection
        {
            get
            {
                Vector2 flatMovement = UnscaledFlatMovement;
                return new Vector3(flatMovement.X, UpDownMovement, flatMovement.Y);
            }
        }
        public Vector3 PreCollisionMovement { get; private set; }
        public Vector3 PostCollisionMovement { get; private set; }

        private void Gravity(TimeSpan elapsedTime)
        {
            if (GravityOn)
            {
                float speed = (float)elapsedTime.TotalSeconds;
                if (OnGround == false)
                    UpDownMovement -= GravityAcceleration * speed;
                else if (UpDownMovement < 0)
                    UpDownMovement = 0;
            }
        }

        private void SetPlayerMoveVector(TimeSpan elapsedTime)
        {
            float speed = (float)elapsedTime.TotalSeconds;
            Vector2 unscaledFlat = UnscaledFlatMovement;
            this.PreCollisionMovement = (new Vector3(unscaledFlat.X, UpDownMovement, unscaledFlat.Y) * speed);
        }

        public void ApplyMoveVector()
        {
            this.Position += PreCollisionMovement;
            this.BBox = this.BBox.MoveBBox(PreCollisionMovement);
            this.TranslationMatrix = Matrix.CreateTranslation(this.BBox.Center());

            //this.PreCollisionMovement = Vector3.Zero;
        }

        private void MoveLimitSpeed()
        {
            // Reduce UpDown to MaximumSpeed
            if (UpDownMovement < MaxDownMovement)
                UpDownMovement = MaxDownMovement;
            //Reduce FlatMovement to MaximumSpeed
            float flatspeed = new Vector2(LeftRightMovement, ForwardBackwardMovement).Length();

            if (flatspeed > MaxFlatMovement)
            {

                LeftRightMovement *= (MaxFlatMovement / flatspeed);
                ForwardBackwardMovement *= (MaxFlatMovement / flatspeed);
            }
        }

        public void LookAround(float rotUp, float rotLeft)
        {
            RotateDirection(rotLeft, rotUp);
        }
        #endregion


        public void Draw(Effect effect, Matrix viewProjectionMatrix)
        {
            if (CamActive)
                DrawFirstPerson(effect);
            else
                DrawThirdPerson(effect, viewProjectionMatrix);
        }

        protected abstract void DrawThirdPerson(Effect effect, Matrix viewProjectionMatrix);
        #region ThirdPersonDefault
        private void SetupVerticesIndices(GraphicsDevice device)
        {
            VertexPositionIndexedNormalTexture[] identityVertices;
            int[] indices;

            Vector3 minCorner = new Vector3(-0.5f);
            identityVertices = new VertexPositionIndexedNormalTexture[24]{

            //Behind
            new VertexPositionIndexedNormalTexture(new Vector4(minCorner,5),
                new Vector2(1.0f / TexturesPerType - BorderTolerance, TexPosLower)),
            new VertexPositionIndexedNormalTexture(new Vector4(minCorner + Vector3.Up ,5),
                new Vector2(1.0f / TexturesPerType - BorderTolerance, TexPosUpper)),
            new VertexPositionIndexedNormalTexture(new Vector4(minCorner + Vector3.Up  + Vector3.Right ,5),
                new Vector2(0.0f / TexturesPerType + BorderTolerance, TexPosUpper)),
            new VertexPositionIndexedNormalTexture(new Vector4(minCorner + Vector3.Right ,5),
                new Vector2(0.0f / TexturesPerType + BorderTolerance, TexPosLower)),
            //Left
            new VertexPositionIndexedNormalTexture(new Vector4(minCorner,3),
                new Vector2(1.0f / TexturesPerType + BorderTolerance, TexPosLower)),
            new VertexPositionIndexedNormalTexture(new Vector4(minCorner + Vector3.Backward ,3),
                new Vector2(2.0f / TexturesPerType - BorderTolerance, TexPosLower)),
            new VertexPositionIndexedNormalTexture(new Vector4(minCorner + Vector3.Up  + Vector3.Backward ,3),
                new Vector2(2.0f / TexturesPerType - BorderTolerance, TexPosUpper)),
            new VertexPositionIndexedNormalTexture(new Vector4(minCorner + Vector3.Up ,3),
                new Vector2(1.0f / TexturesPerType + BorderTolerance, TexPosUpper)),
                            //Infront
            new VertexPositionIndexedNormalTexture(new Vector4(minCorner + Vector3.Backward ,2),
                new Vector2(2.0f / TexturesPerType + BorderTolerance, TexPosLower)),
            new VertexPositionIndexedNormalTexture(new Vector4(minCorner + Vector3.Backward  + Vector3.Right ,2),
                new Vector2(3.0f / TexturesPerType - BorderTolerance, TexPosLower)),
            new VertexPositionIndexedNormalTexture(new Vector4(minCorner + Vector3.Backward + Vector3.Right  + Vector3.Up ,2),
                new Vector2(3.0f / TexturesPerType - BorderTolerance, TexPosUpper)),
            new VertexPositionIndexedNormalTexture(new Vector4(minCorner + Vector3.Backward  + Vector3.Up ,2),
                new Vector2(2.0f / TexturesPerType + BorderTolerance, TexPosUpper)),

            //Right
            new VertexPositionIndexedNormalTexture(new Vector4(minCorner + Vector3.Right ,0),
                new Vector2(4.0f / TexturesPerType - BorderTolerance, TexPosLower)),
            new VertexPositionIndexedNormalTexture(new Vector4(minCorner + Vector3.Right  + Vector3.Up ,0),
                new Vector2(4.0f / TexturesPerType - BorderTolerance, TexPosUpper)),
            new VertexPositionIndexedNormalTexture(new Vector4(minCorner + Vector3.Right  + Vector3.Up  + Vector3.Backward ,0),
                new Vector2(3.0f / TexturesPerType + BorderTolerance, TexPosUpper)),
            new VertexPositionIndexedNormalTexture(new Vector4(minCorner + Vector3.Right  + Vector3.Backward ,0),
                new Vector2(3.0f / TexturesPerType + BorderTolerance, TexPosLower)),
            //Above
            new VertexPositionIndexedNormalTexture(new Vector4(minCorner + Vector3.Up,1),
                new Vector2(4.0f/TexturesPerType + BorderTolerance, TexPosLower)),
            new VertexPositionIndexedNormalTexture(new Vector4(minCorner + Vector3.Up + Vector3.Backward ,1),
                new Vector2(4.0f/TexturesPerType +BorderTolerance, TexPosUpper)),
            new VertexPositionIndexedNormalTexture(new Vector4(minCorner + Vector3.Up + Vector3.Right + Vector3.Backward ,1),
                new Vector2(5.0f / TexturesPerType - BorderTolerance, TexPosUpper)),
            new VertexPositionIndexedNormalTexture(new Vector4(minCorner + Vector3.Up + Vector3.Right ,1),
                new Vector2(5.0f / TexturesPerType - BorderTolerance, TexPosLower)),
            //Lower
            new VertexPositionIndexedNormalTexture(new Vector4(minCorner,4),
                new Vector2(5.0f / TexturesPerType + BorderTolerance, TexPosLower)),
            new VertexPositionIndexedNormalTexture(new Vector4(minCorner + Vector3.Right ,4),
                new Vector2(1 - BorderTolerance, TexPosLower)),
            new VertexPositionIndexedNormalTexture(new Vector4(minCorner + Vector3.Right + Vector3.Backward ,4),
                new Vector2(1 - BorderTolerance, TexPosUpper)),
            new VertexPositionIndexedNormalTexture(new Vector4(minCorner + Vector3.Backward ,4),
                new Vector2(5.0f / TexturesPerType + BorderTolerance, TexPosUpper))
         };
            indices = new int[36];
            for (short i = 0; i < 6; i++)
            {
                short vertexOffset = (short)(4 * i);
                short indexOffset = (short)(6 * i);

                indices[indexOffset + 0] = (short)(vertexOffset + 0);
                indices[indexOffset + 1] = (short)(vertexOffset + 1);
                indices[indexOffset + 2] = (short)(vertexOffset + 2);

                indices[indexOffset + 3] = (short)(vertexOffset + 0);
                indices[indexOffset + 4] = (short)(vertexOffset + 2);
                indices[indexOffset + 5] = (short)(vertexOffset + 3);
            }
            Body.VerticesCount = identityVertices.Length;
            Body.IndicesCount = indices.Length;
            Body.DefaultThirdPersonVBuffer = new VertexBuffer(device, VertexPositionIndexedNormalTexture.VertexDeclaration, 24, BufferUsage.WriteOnly);
            Body.DefaultThirdPersonIBuffer = new IndexBuffer(device, typeof(int), 36, BufferUsage.WriteOnly);
            Body.DefaultThirdPersonVBuffer.SetData(identityVertices);
            Body.DefaultThirdPersonIBuffer.SetData(indices);
        }
        protected void DrawThirdPersonDefault(Effect effect, Matrix viewProjectionMatrix)
        {
            effect.Parameters["WVPMatrix"].SetValue(this.WorldMatrix * viewProjectionMatrix);
            effect.Parameters["WorldInverseTranspose"].SetValue(this.RotationMatrix);

            this.Device.Indices = DefaultThirdPersonIBuffer;
            this.Device.SetVertexBuffer(Body.DefaultThirdPersonVBuffer);
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                this.Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, Body.IndicesCount / 3);
            }
        }
        #endregion

        protected abstract void DrawFirstPerson(Effect effect);

        public override string ToString()
        {
            return this.BBox.ToString();
        }
    }
}
