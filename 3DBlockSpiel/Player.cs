using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using _1st3DGame.enums;

namespace _1st3DGame
{
    class Player : MovebleCamera
    {
        // // Movement Constants // //
        private const float MinMovementSpeed = 0.005f;
        private const float MovementAcceleration = 0.45f;
        private const float NegativeMovementAcceleration = 1.0f / (MovementAcceleration * 3f);
        private const float GravityAcceleration = 0.7f;
        private const float JumpAcceleration = 10f;
        private const float MaxDownMovement = -30;
        private const float MaxFlatMovement = 6;

        // // Collision Constants // //
        private const float BodyHeight = 1.6f;
        public const float BodyWidth = NearPlane * 2 + 0.4f;

        // // Interaction Constancs // // 
        public const float MaxDiggingRange = 50f;
        public const float MaxBuildingRange = 50f;
        public const int DiggingTimeoutMilliseconds = 150;
        public const int BuildingTimeoutMilliseconds = 200;

        public Inventory inventory;

        public Player(GraphicsDevice device, Vector3 position, Vector3 lookAt)
            : base(device, position)
        {
            LastCamPosition = this.Position;
            SetBBox();

            this.Digging = false;

            this.inventory = new Inventory();
        }

        public void Update(TimeSpan elapsedTime)
        {
            Gravity();
            MovementProcessing(elapsedTime);
        }


        #region World Interaction

        public bool Digging { get; set; }
        public bool Building { get; set; }

        #endregion

        #region Collision

        public BoundingBox BBox { get; private set; }

        public Vector3 EyePosition
        {
            get { return this.Position; }
            private set { this.Position = value; SetBBox(); }
        }

        ////////////////////////////
        #region Points for Collision-Checkup
        private Vector3 FootPosition
        {
            get { return FootPositionOf(this.EyePosition); }
            set { this.EyePosition = value + Vector3.Up * BodyHeight; }
        }
        private Vector3 FootPositionOf(Vector3 eyePosition)
        {
            return eyePosition - Vector3.Up * BodyHeight;
        }

        private Vector3 HeadPosition
        {
            get { return HeadPositionOf(this.EyePosition); }
            set { this.EyePosition = value - Vector3.Up * (NearPlane); }
        }
        private Vector3 HeadPositionOf(Vector3 eyePosition)
        {
            return eyePosition + Vector3.Up * (NearPlane);
        }

        private Vector3 BackBorderPosition
        {
            get { return BackBorderPositionOf(this.EyePosition); }
            set { this.EyePosition = value - Vector3.Backward * 0.5f * BodyWidth; }
        }
        private Vector3 BackBorderPositionOf(Vector3 eyePosition)
        {
            return eyePosition + Vector3.Backward * 0.5f * BodyWidth;
        }

        private Vector3 FrontBorderPosition
        {
            get { return FrontBorderPositionOf(this.EyePosition); }
            set { this.EyePosition = value - Vector3.Forward * 0.5f * BodyWidth; }
        }
        private Vector3 FrontBorderPositionOf(Vector3 eyePosition)
        {
            return eyePosition + Vector3.Forward * 0.5f * BodyWidth;
        }

        private Vector3 LeftBorderPosition
        {
            get { return LeftBorderPositionOf(this.EyePosition); }
            set { this.EyePosition = value - Vector3.Left * 0.5f * BodyWidth; }
        }
        private Vector3 LeftBorderPositionOf(Vector3 eyePosition)
        {
            return eyePosition + Vector3.Left * 0.5f * BodyWidth;
        }

        private Vector3 RightBorderPosition
        {
            get { return RightBorderPositionOf(this.EyePosition); }
            set { this.EyePosition = value - Vector3.Right * 0.5f * BodyWidth; }
        }
        private Vector3 RightBorderPositionOf(Vector3 eyePosition)
        {
            return eyePosition + Vector3.Right * 0.5f * BodyWidth;
        }

        private Vector3 LastCamPosition;
        #endregion

        private bool OnGround = false;
        private bool OnForwardWall = false;
        private bool OnBackwardWall = false;
        private bool OnLeftWall = false;
        private bool OnRightWall = false;
        public void ResetCollisionParameters()
        {
            OnGround = false;
            OnForwardWall = false;
            OnBackwardWall = false;
            OnLeftWall = false;
            OnRightWall = false;
        }

        private void SetBBox()
        {
            this.BBox = new BoundingBox(new Vector3(
                this.Position.X - 0.5f * BodyWidth,
                this.Position.Y - BodyHeight,
                this.Position.Z - 0.5f * BodyWidth),
                new Vector3(
                this.Position.X + 0.5f * BodyWidth,
                this.Position.Y + NearPlane,
                this.Position.Z + 0.5f * BodyWidth));
        }

        public void Collision(BoundingBox bbox)
        {
            // IF ALIGN
            if (this.BBox.Align(bbox))
            {
                //If collision on feet
                if (bbox.Max.Y.Close1000(this.BBox.Min.Y))
                {
                    OnGround = true;
                    if (UpDownMovement < 0)
                        UpDownMovement = 0;
                }
                //Forward coll
                else if (bbox.Max.Z.Close1000(this.BBox.Min.Z))
                {
                    OnForwardWall = true;
                }
                //Backward coll
                else if (bbox.Min.Z.Close1000(this.BBox.Max.Z))
                {
                    OnBackwardWall = true;
                }
                //Left coll
                else if (bbox.Max.X.Close1000(this.BBox.Min.X))
                {
                    OnLeftWall = true;
                }
                ////Right coll
                else if (bbox.Min.X.Close1000(this.BBox.Max.X))
                {
                    OnRightWall = true;
                }
            }
            //IF INTERSECT
            else if (this.BBox.Intersects(bbox))
            {
                //Box Under the Player
                if (FootPositionOf(LastCamPosition).Y > bbox.Max.Y)
                {
                    this.FootPosition = new Vector3(this.FootPosition.X, bbox.Max.Y, this.FootPosition.Z);
                    if (UpDownMovement < 0)
                        UpDownMovement = 0;
                    OnGround = true;
                }
                //Box above the Player
                else if (HeadPositionOf(LastCamPosition).Y < bbox.Min.Y)
                {
                    this.HeadPosition = new Vector3(this.HeadPosition.X, bbox.Min.Y, this.HeadPosition.Z);
                    if (UpDownMovement > 0)
                        UpDownMovement = 0;
                }
                //Box infront of the Player
                else if (FrontBorderPositionOf(LastCamPosition).Z > bbox.Max.Z)
                {
                    this.FrontBorderPosition = new Vector3(this.FrontBorderPosition.X, this.FrontBorderPosition.Y, bbox.Max.Z);
                    if (UnscaledFlatMovement.Y < 0)
                    {
                        OnForwardWall = true;
                        ForwardBackwardMovement = 0;
                    }
                }
                //Box behind the Player
                else if (BackBorderPositionOf(LastCamPosition).Z < bbox.Min.Z)
                {
                    this.BackBorderPosition = new Vector3(this.BackBorderPosition.X, this.BackBorderPosition.Y, bbox.Min.Z);
                    if (UnscaledFlatMovement.Y > 0)
                    {
                        OnBackwardWall = true;
                        ForwardBackwardMovement = 0;
                    }
                }
                // Box left of the player
                else if (LeftBorderPositionOf(LastCamPosition).X > bbox.Max.X)
                {
                    this.LeftBorderPosition = new Vector3(bbox.Max.X, this.LeftBorderPosition.Y, this.LeftBorderPosition.Z);
                    if (UnscaledFlatMovement.X < 0)
                    {
                        OnLeftWall = true;
                        LeftRightMovement = 0;
                    }
                }
                // Box right of the player
                else if (RightBorderPositionOf(LastCamPosition).X < bbox.Min.X)
                {
                    this.RightBorderPosition = new Vector3(bbox.Min.X, this.RightBorderPosition.Y, this.RightBorderPosition.Z);
                    if (UnscaledFlatMovement.X > 0)
                    {
                        OnRightWall = true;
                        LeftRightMovement = 0;
                    }
                }
            }
            //IF NO CONTACT
            else
            { }
        }
        #endregion

        private void MovementProcessing(TimeSpan elapsedTime)
        {
            MoveInputProcessing();
            MoveLimitSpeed();
            MovePlayer(elapsedTime);
        }

        #region MOVEMENT PROCESSING

        public float UpDownMovement = 0;
        private float LeftRightMovement = 0;
        private float ForwardBackwardMovement = 0;
        private Vector2 UnscaledFlatMovement
        {
            get
            {
                Vector2 forward = this.LookDirection.ToVector2D();
                forward.Normalize();
                Vector2 left = new Vector2(forward.Y, -forward.X);
                return forward * ForwardBackwardMovement + left * LeftRightMovement;
            }
            set { }
        }

        private void Gravity()
        {
            if (OnGround == false)
                UpDownMovement -= GravityAcceleration;
        }

        private void MovePlayer(TimeSpan elapsedTime)
        {
            float speed = elapsedTime.Milliseconds / 1000.0f;
            Vector2 unscaledFlat = UnscaledFlatMovement;
            Vector3 posDif = (new Vector3(unscaledFlat.X, UpDownMovement, unscaledFlat.Y) * speed);

            //Collision
            if (OnForwardWall && posDif.Z < 0)
                posDif.Z = 0;
            if (OnBackwardWall && posDif.Z > 0)
                posDif.Z = 0;

            if (OnLeftWall && posDif.X < 0)
                posDif.X = 0;
            if (OnRightWall && posDif.X > 0)
                posDif.X = 0;

            this.LastCamPosition = Position;
            this.Position += posDif;
            this.BBox = this.BBox.MoveBBox(posDif);

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

        private void MoveInputProcessing()
        {
            //Accelerate Left Right
            if (MoveLeft != null)
            {
                if (MoveLeft == true)
                    LeftRightMovement += MovementAcceleration;
                else
                    LeftRightMovement -= MovementAcceleration;
                MoveLeft = null;
            }
            //Brake Left Right
            else if (LeftRightMovement != 0)
                LeftRightMovement = LeftRightMovement.Reduce(NegativeMovementAcceleration, MinMovementSpeed);
            //Accelerate Forward Backward
            if (MoveForward != null)
            {
                if (MoveForward == true)
                    ForwardBackwardMovement += MovementAcceleration;
                else
                    ForwardBackwardMovement -= MovementAcceleration;
                MoveForward = null;
            }
            //Brake Forward Backward
            else if (ForwardBackwardMovement != 0)
                ForwardBackwardMovement = ForwardBackwardMovement.Reduce(NegativeMovementAcceleration, MinMovementSpeed);
            //Jump
            if (MoveUp != null)
            {
                if (MoveUp == true)
                {
                    if (OnGround)
                        UpDownMovement += JumpAcceleration;
                }
                MoveUp = null;
            }
        }
        #endregion
        #region MOVEMENT INPUT
        private bool? MoveLeft = null;
        private bool? MoveForward = null;
        private bool? MoveUp = null;
        public void MoveInput(MoveDirection direction)
        {
            if (direction == enums.MoveDirection.Forward)
                MoveForward = true;
            else if (direction == enums.MoveDirection.Backwards)
                MoveForward = false;
            else if (direction == enums.MoveDirection.Left)
                MoveLeft = true;
            else if (direction == enums.MoveDirection.Right)
                MoveLeft = false;
            else if (direction == enums.MoveDirection.Jump)
                MoveUp = true;
        }


        public void LookAround(float rotUp,float rotLeft)
        {
            MoveDirection(rotLeft,rotUp);
        }
        #endregion

        public override string ToString()
        {
            return this.BBox.ToString();
        }
    }
}
