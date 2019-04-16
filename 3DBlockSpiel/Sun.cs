using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace _1st3DGame
{
    class Sun
    {
        public readonly Vector3 OriginalLightDirection;
        public readonly Vector4 OriginalLightColor;

        public readonly float OriginalLightIntensity;
        public readonly float OriginalLightDistance;
        public readonly float OriginalLightSize;
        public readonly float OriginalLightSpeedLeft;
        public readonly float OriginalLightRotLimitUpDown;

        public Vector3 LightDirection { get; private set; }
        public Vector4 LightColor { get; private set; }
        public float LightIntensity { get; set; }

        public Sun(Vector3 origDirection, Vector4 origColor,float origIntensity,
            float origDistance,float origSize, float origSpeedLeft,float origRotLimitUpDown)
        {
            this.OriginalLightDirection = origDirection;
            this.OriginalLightColor = origColor;
            this.OriginalLightIntensity = origIntensity;
            this.OriginalLightDistance = origDistance;
            this.OriginalLightSize = origSize;
            this.OriginalLightSpeedLeft = origSpeedLeft;
            this.OriginalLightRotLimitUpDown = origRotLimitUpDown;
        }

        public void SetCurrentValues(IngameDateTime time)
        {
            LightDirection = GetLightDirection(time);                
            LightColor = GetLightColor(time);
            LightIntensity = GetLightIntensity(time);
        }

        protected virtual Vector3 GetLightDirection(IngameDateTime time)
        {
            float rotLeft = (OriginalLightSpeedLeft * MathHelper.TwoPi * time.DateTime) % MathHelper.TwoPi;
            float rotUp = OriginalLightRotLimitUpDown * (float)Math.Sin(rotLeft);
            return Vector3.TransformNormal(OriginalLightDirection, Matrix.CreateRotationY(rotLeft) * Matrix.CreateRotationZ(rotUp));
        }
        protected virtual Vector4 GetLightColor(IngameDateTime time)
        {
            return OriginalLightColor;
        }
        protected virtual float GetLightIntensity(IngameDateTime time)
        {
            return OriginalLightIntensity;
        }
    }
}
