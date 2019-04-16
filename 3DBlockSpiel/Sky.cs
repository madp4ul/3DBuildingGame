using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _1st3DGame
{
    class Sky
    {
        public const int SunCount = 4;

        public Sun[] Suns { get; private set; }

        public Vector3[] LightDirections
        {
            get
            {
                Vector3[] directions = new Vector3[SunCount];
                for (int i = 0; i < SunCount; i++)
                    directions[i] = this.Suns[i].LightDirection;
                return directions;
            }
        }
        public Vector4[] LightColors
        {
            get
            {
                Vector4[] directions = new Vector4[SunCount];
                for (int i = 0; i < SunCount; i++)
                    directions[i] = this.Suns[i].LightColor;
                return directions;
            }
        }
        public float[] LightIntensities
        {
            get
            {
                float[] directions = new float[SunCount];
                for (int i = 0; i < SunCount; i++)
                    directions[i] = this.Suns[i].LightIntensity;
                return directions;
            }
        }

        public Sky()
        {
            this.Suns = new Sun[SunCount];

            InitializeSuns();
        }

        protected virtual void InitializeSuns()
        {
            this.Suns[0] = new Sun(new Vector3(11f / 8f, -1f, 0).Normalized(),
                new Vector4(1),
                1f, 300, 160, 1, 0.59f);

            this.Suns[1] = new Sun(new Vector3(11f / 8f, -1f, 0).Normalized(),
                new Vector4(255f / 255f, 255f / 255f, 68f / 255f, 1),
                1, 260, 90, 0.57f, 1);

            this.Suns[2] = new Sun(new Vector3(11f / 8f, -1f, 0).Normalized(),
                new Vector4(0f / 255f, 181f / 255f, 255f / 255f, 1),
                1, 250, 80, 0.11f, 1);

            this.Suns[3] = new Sun(new Vector3(11f / 8f, -1f, 0).Normalized(),
                new Vector4(204f / 255f, 194f / 255f, 0f / 255f, 1),
                1, 240, 70, 0.32f, 3f / 8f);
        }

        public void SetSunlights(IngameDateTime time)
        {
            //refresh sun positions
            for (int i = 0; i < SunCount; i++)
                this.Suns[i].SetCurrentValues(time);
            //post-processing
            for (int i = 0; i < SunCount; i++)
                SolarEclipse(i);//solar eclipses

        }

        private void SolarEclipse(int thisSunsIndex)
        {
            Sun currentSun = this.Suns[thisSunsIndex];
            float shineFactor = 1;
            for (int i = 0; i < SunCount; i++)
                if (currentSun.OriginalLightDistance > this.Suns[i].OriginalLightDistance && this.Suns[i].OriginalLightIntensity > 0)
                {
                    float dif = 1 - Vector3.Dot(currentSun.LightDirection, this.Suns[i].LightDirection);
                    float thisViewSize = currentSun.OriginalLightSize / currentSun.OriginalLightDistance;
                    float otherViewSize = this.Suns[i].OriginalLightSize / this.Suns[i].OriginalLightDistance;
                    float maxDifferenceForSE = 0.02f * (thisViewSize + otherViewSize) / MathHelper.PiOver2;
                    if (dif < maxDifferenceForSE)
                    {
                        float minShineFactor = 1 - otherViewSize / thisViewSize;
                        if (minShineFactor < 0)
                            minShineFactor = 0;
                        shineFactor = minShineFactor + (shineFactor - minShineFactor) * (dif / maxDifferenceForSE);
                    }
                }
            currentSun.LightIntensity *= shineFactor;
        }

        public VertexPositionTexture[] GetSunVertices(Vector3 cameraPosition)
        {
            VertexPositionTexture[] sunVertices = new VertexPositionTexture[6 * SunCount];
            for (int i = 0; i < SunCount; i++)
            {
                Matrix m = Matrix.CreateTranslation(-this.Suns[i].LightDirection * this.Suns[i].OriginalLightDistance) *
                Matrix.CreateTranslation(cameraPosition);

                int j = i * 6;
                sunVertices[j++] = new VertexPositionTexture(Vector3.Transform(Vector3.Zero, m), new Vector2(1, 0));
                sunVertices[j++] = new VertexPositionTexture(Vector3.Transform(Vector3.Zero, m), new Vector2(0, 0));
                sunVertices[j++] = new VertexPositionTexture(Vector3.Transform(Vector3.Zero, m), new Vector2(1, 1));

                sunVertices[j++] = new VertexPositionTexture(Vector3.Transform(Vector3.Zero, m), new Vector2(0, 0));
                sunVertices[j++] = new VertexPositionTexture(Vector3.Transform(Vector3.Zero, m), new Vector2(0, 1));
                sunVertices[j++] = new VertexPositionTexture(Vector3.Transform(Vector3.Zero, m), new Vector2(1, 1));
            }
            return sunVertices;
        }
    }
}
