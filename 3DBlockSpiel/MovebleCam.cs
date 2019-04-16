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
    //OLD AND NOT IN USE
    class MovebleCam
    {
        protected const float NearPlane = 0.1f;
        protected const float FarPlane = 600f;
        protected const float FOVDegrees = 85f;

        private Vector3 _CamPosition;
        public Vector3 CamPosition
        {
            get { return _CamPosition; }
            protected set
            {
                this._CamlookAt = value + this.CamLookDirection;
                this._CamPosition = value;
                //this.viewMatrix = Matrix.CreateLookAt(this.CamPosition, this.CamLookAt, this.CamUp);
            }
        }
        private Vector3 _CamlookAt;
        public Vector3 CamLookAt
        {
            get { return _CamlookAt; }
            private set
            {
                this._CamlookAt = value;
                //this.viewMatrix = Matrix.CreateLookAt(this.CamPosition, this.CamLookAt, this.CamUp);
            }
        }
        public Vector3 CamLookDirection
        {
            get { return this.CamLookAt - this.CamPosition; }
            private set { this.CamLookAt = value + this.CamPosition; }
        }
        private Vector3 NormalizedDirection
        {
            get { Vector3 nD = CamLookDirection; nD.Normalize(); return nD; }
        }
        public Ray CamLookRay { get { return new Ray(this.CamPosition, this.NormalizedDirection); } }
        public Vector3 CamUp { get; private set; }

        public Matrix viewMatrix { get; private set; }
        public Matrix projectionMatrix { get; private set; }

        public MovebleCam(GraphicsDevice device, Vector3 position, Vector3 lookAt)
        {
            //this.Device = device;
            this.CamUp = new Vector3(0, 1, 0);
            this.CamPosition = position;
            this.CamLookAt = lookAt;
            this.viewMatrix = Matrix.CreateLookAt(this.CamPosition, this.CamLookAt, this.CamUp);
            this.projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
               MathHelper.ToRadians(FOVDegrees), device.Viewport.AspectRatio, NearPlane, FarPlane);
        }

        float rotLeft = MathHelper.PiOver2;
        float rotUp = MathHelper.Pi / 10.0f;
        protected void MoveDirection(float rotLeftRight, float rotUpDown)
        {
            rotLeft += rotLeftRight;
            rotUp += rotUpDown;
            Matrix cameraRotation = Matrix.CreateRotationX(rotUp) * Matrix.CreateRotationY(rotLeft);

            Vector3 rotatedTarget = Vector3.Transform(CamLookAt, cameraRotation);
            Vector3 finalTarget = rotatedTarget;

            Vector3 rotatedUp = Vector3.Transform(CamUp, cameraRotation);

            this.viewMatrix = Matrix.CreateLookAt(CamPosition, finalTarget, rotatedUp);
        }

        //protected void MoveDirection(Quaternion rotLeftRight,Quaternion rotUpDown)
        //{
        //    this.CamLookDirection = Vector3.Transform(
        //        this.CamLookDirection, Matrix.CreateFromQuaternion(rotLeftRight*rotUpDown));
        //    this.CamUp = Vector3.Up;
        //    this.CamUp = Vector3.Transform(CamUp,
        //        Matrix.CreateFromQuaternion(rotUpDown));
            
        //}

        public Vector3 axisUpDown { get { return new Vector3(-this.CamLookDirection.Z, 0, this.CamLookDirection.X); } }
        public Vector3 axisLeftRight { get { return Vector3.Up; } }
    }
}
