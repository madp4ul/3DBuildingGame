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
    class Enemy : Living
    {
        #region Visuals


        #endregion

        #region Body-Constants
        //Movement
        protected const float MinMovementSpeed = 0.3f;
        private const float MovementAcceleration = 5f;
        private const float BrakeAcceleration = 1 / 2f;
        private const float GravityAcceleration = 35f;
        private const float JumpAcceleration = 10.5f;
        private const float MaxDownMovement = -39;
        private const float MaxFlatMovement = 2.6f;
        private const float EnemyMovementStrength = 0.2f;
        //Collision (Body-Size)
        private const float EyeHeight = 0.7f;
        private const float BodyHeight = 0.8f;
        private const float BodyWidth = NearPlane * 2 + 0.6f;
        //EyeSight
        private const float FOV = 65f;
        private const float ViewDistance = 10f;
        #endregion
        private const float HearDistance = 3f;

        private const float MaxVisualUpRotation = MathHelper.PiOver4 / 8f;
        public Vector3? TargetPos = null;

        public EnemyTypes Type { get; private set; }

        public Enemy(GraphicsDevice device, Vector3 position, Vector3 lookDirection, int surfaceLayer)
            : base(device, position, lookDirection,
            MovementAcceleration, BrakeAcceleration, GravityAcceleration, JumpAcceleration,
            MaxDownMovement, MaxFlatMovement, EnemyMovementStrength, EyeHeight, BodyHeight, BodyWidth, FOV, ViewDistance)
        {
            this.Type = EnemyTypes.Slime;
            //this.ScaleMatrix = Matrix.CreateScale(1);//BodyWidth, BodyHeight, BodyWidth);

            this.RotationMatrix = Matrix.CreateRotationY(RotationDegreesLeft);
        }

        public override void Update(TimeSpan elapsedTime, Player player1)
        {
            UpdateAI(elapsedTime, player1);

            UpdateBody(elapsedTime);
        }

        #region AI
        public void UpdateAI(TimeSpan elapsedTime, Player player)
        {
            if (!IsDead)
                LookOutForPlayer(player);
            else
                this.TargetPos = null;

            if (this.TargetPos != null)
                LookToTarget();
            AccelerateToTarget(player.BBox, elapsedTime);
        }

        private void LookOutForPlayer(Player player)
        {
            BoundingBox playerBox = player.BBox;
            BoundingFrustum fov = this.FieldOfView;
            if ((playerBox.Center() - this.EyePosition).Length() < HearDistance ||
                playerBox.Intersects(fov))
            {
                this.TargetPos = playerBox.Center();
            }
            else
                this.TargetPos = null;
        }

        private void LookToTarget()
        {
            this.RotateDirection(this.TargetPos.Value);
            float rotUp = Math.Abs(this.RotationDegreesUp) < MaxVisualUpRotation ? this.RotationDegreesUp :
                this.RotationDegreesUp < 0 ? -MaxVisualUpRotation : MaxVisualUpRotation;

            this.RotationMatrix = Matrix.CreateRotationY(this.RotationDegreesLeft);
        }

        private void AccelerateToTarget(BoundingBox targetBox, TimeSpan elapsedTime)
        {
            if (this.TargetPos.HasValue)
            {
                float speed = (float)elapsedTime.TotalSeconds;
                Vector3 move = (this.TargetPos.Value - this.EyePosition);
                Vector2 moveFlat = new Vector2(move.X, move.Z);
                if (moveFlat.Length() > this.BBox.Radius())
                {
                    moveFlat.Normalize();
                    this.UnscaledFlatMovement += moveFlat * MovementAcceleration * speed;
                }
                else
                    this.UnscaledFlatMovement = new Vector2(
                        UnscaledFlatMovement.X.Reduce(BrakeAcceleration, MinMovementSpeed),
                        UnscaledFlatMovement.Y.Reduce(BrakeAcceleration, MinMovementSpeed));

                if (OnGround && (this.CollisionStop.X != 0 || this.CollisionStop.Z != 0) && !this.BBox.Intersects(targetBox))
                {
                    this.UpDownMovement += JumpAcceleration;
                }
            }
            else
                this.UnscaledFlatMovement = new Vector2(
                    UnscaledFlatMovement.X.Reduce(BrakeAcceleration, MinMovementSpeed),
                    UnscaledFlatMovement.Y.Reduce(BrakeAcceleration, MinMovementSpeed));
        }

        #endregion

        #region Visuals
 
        #endregion

        protected override void DrawThirdPerson(Effect effect, Matrix viewProjectionMatrix)
        {
            DrawThirdPersonDefault(effect, viewProjectionMatrix);
        }
        protected override void DrawFirstPerson(Effect effect)
        { }

        public static BoundingBox GetBoundingBox(Vector3 position)
        {
            return new BoundingBox(new Vector3(
                position.X - 0.5f * BodyWidth,
                position.Y - EyeHeight,
                position.Z - 0.5f * BodyWidth),
                new Vector3(
                position.X + 0.5f * BodyWidth,
                position.Y - EyeHeight + BodyHeight,
                position.Z + 0.5f * BodyWidth));
        }
    }
}
