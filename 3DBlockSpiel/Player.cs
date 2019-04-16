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
    class Player : Living
    {

        // // Movement Constants // //
        protected static readonly float MinMovementSpeed = 0.3f;
        protected static readonly float MovementAcceleration = 10f;
        protected static readonly float BrakeAcceleration = 1 / 1500f;
        protected static readonly float GravityAcceleration = 40f;
        protected static readonly float JumpAcceleration = 10.5f;
        protected static readonly float MaxDownMovement = -40;
        protected static readonly float MaxFlatMovement = 4;
        protected static readonly float PlayerMovementStrength = 1;

        // // Collision Constants // //
        private const float EyeHeight = 1.6f;
        private const float BodyHeight = 1.9f;
        public const float BodyWidth = NearPlane * 2 + 0.4f;

        // // Interaction Constancs // // 
        public const float MaxDiggingRange = 50f;
        public const float MaxBuildingRange = 50f;
        public const int DiggingTimeoutMilliseconds = 90;
        public const int BuildingTimeoutMilliseconds = 100;
        private const float HandDigStrength = 150f;

        //EyeSight - Constants
        public const float FOV = 85f;
        public const float ViewDistance = 600f;
        
        public float DigStrength = HandDigStrength;
        public Inventory inventory;

        public Player(GraphicsDevice device, Vector3 position,Vector3 lookDirection)
            : base(device, position, lookDirection,
            MovementAcceleration, BrakeAcceleration, GravityAcceleration, JumpAcceleration,
            MaxDownMovement, MaxFlatMovement, PlayerMovementStrength,
            EyeHeight, BodyHeight, BodyWidth,FOV,ViewDistance)
        {
            this.inventory = new Inventory();
            this.Digging = false;
        }

        public override void Update(TimeSpan elapsedTime,Player player1)
        {
            MoveInputProcessing(elapsedTime);
            UpdateBody(elapsedTime);
        }

        public bool Digging { get; set; }
        public bool Building { get; set; }

        private void MoveInputProcessing(TimeSpan elapsedTime)
        {
            float speed = (float)elapsedTime.TotalSeconds;//elapsedTime.Milliseconds / 1000.0f;
            //Accelerate Left Right
            if (MoveLeft != null)
            {
                if (MoveLeft == true)
                    LeftRightMovement += MovementAcceleration * speed;
                else
                    LeftRightMovement -= MovementAcceleration * speed;
                MoveLeft = null;
            }
            //Brake Left Right
            else if (LeftRightMovement != 0)
                LeftRightMovement = LeftRightMovement.Reduce((float)Math.Pow(BrakeAcceleration, speed), MinMovementSpeed);
            //Accelerate Forward Backward
            if (MoveForward != null)
            {
                if (MoveForward == true)
                    ForwardBackwardMovement += MovementAcceleration * speed;
                else
                    ForwardBackwardMovement -= MovementAcceleration * speed;
                MoveForward = null;
            }
            //Brake Forward Backward
            else if (ForwardBackwardMovement != 0)
                ForwardBackwardMovement = ForwardBackwardMovement.Reduce((float)Math.Pow(BrakeAcceleration, speed), MinMovementSpeed);
            //Jump
            if (MoveUp != null)
            {
                if (MoveUp == true)
                {
                    if (OnGround)
                        UpDownMovement = JumpAcceleration;
                }
                MoveUp = null;
            }
        }

        private bool? MoveLeft = null;
        private bool? MoveForward = null;
        private bool? MoveUp = null;
        public void MoveInput(MoveDirection direction)
        {
            if (direction == MoveDirection.Forward)
                MoveForward = true;
            else if (direction == MoveDirection.Backwards)
                MoveForward = false;
            else if (direction == MoveDirection.Left)
                MoveLeft = true;
            else if (direction == MoveDirection.Right)
                MoveLeft = false;
            else if (direction == MoveDirection.Jump)
                MoveUp = true;
        }

        protected override void DrawFirstPerson(Effect effect)
        {
            
        }

        protected override void DrawThirdPerson(Effect effect, Matrix viewProjectionMatrix)
        {
            DrawThirdPersonDefault(effect, viewProjectionMatrix);
        }
    }
}
