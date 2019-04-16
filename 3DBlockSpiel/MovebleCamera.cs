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
    class MovebleCamera
    {
        private const float FOV = 85f;
        public const float NearPlane = 0.1f;
        public const float Farplane = 330.0f;

        private Matrix RotationMatrix =
                Matrix.CreateRotationX(0) *
                Matrix.CreateRotationY(0);
        public Matrix ViewMatrix { get; private set; }
        public Matrix ProjectionMatrix { get; private set; }

        private Vector3 _Position;
        public Vector3 Position
        {
            get { return _Position; }
            set
            {
                _Position = value;
                UpdateViewMatrix();
            }
        }
        private readonly Vector3 LookAt;
        private readonly Vector3 Up = Vector3.Up;

        private float RotationDegreesLeft;
        private float RotationDegreesUp;

        public Vector3 LookDirection { get { return Vector3.TransformNormal(LookAt, RotationMatrix); } }
        public Ray LookRay { get { return new Ray(Position, LookDirection); } }

        public MovebleCamera(GraphicsDevice device, Vector3 position)
        {
            RotationDegreesLeft = 0;
            RotationDegreesUp = 0;

            this.Position = position;
            this.LookAt = Vector3.Forward * 100f;

            this.ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(FOV), device.Viewport.AspectRatio, NearPlane, Farplane);
            this.ViewMatrix = Matrix.CreateLookAt(position, this.LookAt, Up);
        }

        public void MoveDirection(float xRot, float yRot)
        {
            RotationDegreesLeft += xRot;
            RotationDegreesUp += yRot;
            if (RotationDegreesUp > MathHelper.PiOver2 - 0.01f)
                RotationDegreesUp = MathHelper.PiOver2 - 0.01f;
            else if (RotationDegreesUp < -MathHelper.PiOver2 + 0.01f)
                RotationDegreesUp = -MathHelper.PiOver2 + 0.01f;

            this.RotationMatrix =
                Matrix.CreateRotationX(RotationDegreesUp) *
                Matrix.CreateRotationY(RotationDegreesLeft);

            UpdateViewMatrix();
        }

        private void UpdateViewMatrix()
        {
            Vector3 rotatedTarget = Vector3.Transform(LookAt, RotationMatrix);
            Vector3 finalTarget = rotatedTarget + Position;

            Vector3 rotatedUp = Vector3.Transform(Up, RotationMatrix);

            this.ViewMatrix = Matrix.CreateLookAt(Position, finalTarget, rotatedUp);
        }
    }
}
