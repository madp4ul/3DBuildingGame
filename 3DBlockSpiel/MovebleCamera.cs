using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Diagnostics;

namespace _1st3DGame
{
    class MovebleCamera
    {
        public readonly GraphicsDevice Device;

        private readonly float FOV = 85f;
        public const float NearPlane = 0.1f;
        public readonly float Farplane = 530.0f;

        private Matrix RotationMatrix =
                Matrix.CreateRotationX(0) *
                Matrix.CreateRotationY(0);
        public Matrix ViewMatrix { get; private set; }
        public Matrix ProjectionMatrix { get; private set; }
        public BoundingFrustum FieldOfView { get { return new BoundingFrustum(ViewMatrix * ProjectionMatrix); } }

        private Vector3 _Position;
        public Vector3 Position
        {
            get { return _Position; }
            set
            {
                _Position = value;
                UpdateViewMatrix(RotationMatrix);
            }
        }
        private readonly Vector3 LookAt = Vector3.Forward;
        private readonly Vector3 OriginalUp = Vector3.Up;
        public Vector3 Up { get; private set; }

        public float RotationDegreesLeft { get; private set; }
        public float RotationDegreesUp { get; private set; }

        /// <summary>
        /// Normal of cam direction
        /// </summary>
        public Vector3 LookDirection { get { return Vector3.TransformNormal(LookAt, RotationMatrix); } }
        public Ray LookRay { get { return new Ray(Position, LookDirection); } }

        public MovebleCamera(GraphicsDevice device, Vector3 position)
        {
            this.Device = device;

            RotationDegreesLeft = 0;
            RotationDegreesUp = 0;

            this.Position = position;
            //this.LookAt = 

            this.ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(FOV), device.Viewport.AspectRatio, NearPlane, Farplane);
            this.ViewMatrix = Matrix.CreateLookAt(position, this.LookAt, OriginalUp);
        }

        public MovebleCamera(GraphicsDevice device, Vector3 position,Vector3 rotation, float fov, float viewdistance)
        {
            this.Device = device; 
            RotationDegreesLeft = 0;
            RotationDegreesUp = 0;
            this.Position = position;
            rotation.Normalize();
            RotateDirection( new Vector3(rotation.X + position.X, rotation.Y + position.Y, rotation.Z + position.Z));
            this.FOV = fov;
            this.Farplane = viewdistance;

            this.ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(FOV), device.Viewport.AspectRatio, NearPlane, Farplane);
            this.ViewMatrix = Matrix.CreateLookAt(position, this.LookAt, OriginalUp);
        }

        public void RotateDirection(float xRot, float yRot)
        {
            RotationDegreesLeft += xRot;
            RotationDegreesUp += yRot;
            ApplyLookDirection();
        }

        protected void RotateDirection(Vector3 newLookAt)
        {
            float rotLeft = MathHelper.Pi + Math2.Atan2(newLookAt.X - Position.X, newLookAt.Z - Position.Z);

            Vector3 nla = Vector3.Transform(newLookAt - Position, Matrix.CreateRotationY(-rotLeft + MathHelper.Pi)) + Position;
            float rotUp = Math2.Atan2(nla.Y - Position.Y, nla.Z - Position.Z);

            RotationDegreesLeft = rotLeft;
            RotationDegreesUp = rotUp;

            ApplyLookDirection();

        }

        private void ApplyLookDirection()
        {
            if (RotationDegreesUp > MathHelper.PiOver2 - 0.01f)
                RotationDegreesUp = MathHelper.PiOver2 - 0.01f;
            else if (RotationDegreesUp < -MathHelper.PiOver2 + 0.01f)
                RotationDegreesUp = -MathHelper.PiOver2 + 0.01f;


            this.RotationMatrix =
                Matrix.CreateRotationX(RotationDegreesUp) *
                Matrix.CreateRotationY(RotationDegreesLeft);

            UpdateViewMatrix(RotationMatrix);
        }

        private void UpdateViewMatrix(Matrix rotationMatrix)
        {
            Vector3 rotatedTarget = Vector3.Transform(LookAt, rotationMatrix);
            Vector3 finalTarget = rotatedTarget + Position;

            this.Up = Vector3.Transform(OriginalUp, rotationMatrix);
            if (this.Up == Vector3.Zero || finalTarget == Vector3.Zero)
            { }
            this.ViewMatrix = Matrix.CreateLookAt(Position, finalTarget, this.Up);
        }
    }
}
