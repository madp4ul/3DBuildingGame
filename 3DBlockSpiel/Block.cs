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
    class Block : ICloneable
    {
        #region BlockType-Methods

        //public static Block CreateAir()
        //{
        //    return new Block(BlockTypes.Air);
        //}
        //public static Block CreateDirt()
        //{
        //    return new Block(BlockTypes.Dirt);
        //}
        //public static Block CreateStone()
        //{
        //    return new Block(BlockTypes.Stone);
        //}
        #endregion

        public const int Sidelength = 1;
        private const int CornerCount = 8;
        private const int SideCount = 6;
        public const int VerticesCount = CornerCount * 3;
        public const int IndicesCount = SideCount * 6;

        public static readonly float Radius =  new Vector3(0.5f * Sidelength).Length();

        public static Texture2D BlockTex;

        private const float BorderTolerance = 0.016f;
        private float TexPosUpper { get { return ((int)this.Type * 1.0f) / ((int)BlockTypes.TypeCount * 1.0f) + BorderTolerance; } }
        private float TexPosLower { get { return ((int)this.Type * 1.0f + 1) / ((int)BlockTypes.TypeCount * 1.0f) - BorderTolerance; } }

        

        public BlockTypes Type { get; protected set; }
        public bool CollidesWithPlayer { get { return Blocks.CollidesWithPlayer[(int)this.Type]; } }
        public bool Visible { get { return Blocks.Visible[(int)this.Type]; } }
        public bool NotHidden;

        public Block(BlockTypes type)
        {
            this.Type = type;
        }

        #region ReturnVertices
        public VertexPositionNormalTexture[] VerticesYNegative(Vector3 minCorner)
        {
            return new VertexPositionNormalTexture[]{
                            new VertexPositionNormalTexture(minCorner,
                Vector3.Down, new Vector2(2.0f / 3.0f + BorderTolerance, TexPosLower)),
            new VertexPositionNormalTexture(minCorner + Vector3.Right * Sidelength,
                Vector3.Down, new Vector2(1 - BorderTolerance, TexPosLower)),
            new VertexPositionNormalTexture(minCorner + Vector3.Right * Sidelength + Vector3.Backward * Sidelength,
                Vector3.Down, new Vector2(1 - BorderTolerance, TexPosUpper)),
            new VertexPositionNormalTexture(minCorner + Vector3.Backward * Sidelength,
                Vector3.Down, new Vector2(2.0f / 3.0f + BorderTolerance, TexPosUpper))
            };
        }
        public VertexPositionNormalTexture[] VerticesZNegative(Vector3 minCorner)
        {
            return new VertexPositionNormalTexture[]{
            new VertexPositionNormalTexture(minCorner,
                Vector3.Forward, new Vector2(2.0f / 3.0f - BorderTolerance, TexPosLower)),
            new VertexPositionNormalTexture(minCorner + Vector3.Up * Sidelength,
                Vector3.Forward, new Vector2(2.0f / 3.0f - BorderTolerance, TexPosUpper)),
            new VertexPositionNormalTexture(minCorner + Vector3.Up * Sidelength + Vector3.Right * Sidelength,
                Vector3.Forward, new Vector2(1.0f / 3.0f + BorderTolerance, TexPosUpper)),
            new VertexPositionNormalTexture(minCorner + Vector3.Right * Sidelength,
                Vector3.Forward, new Vector2(1.0f / 3.0f + BorderTolerance, TexPosLower))
            };
        }
        public VertexPositionNormalTexture[] VerticesXNegative(Vector3 minCorner)
        {
            return new VertexPositionNormalTexture[]{
            new VertexPositionNormalTexture(minCorner,
                Vector3.Left, new Vector2(1.0f / 3.0f + BorderTolerance, TexPosLower)),
            new VertexPositionNormalTexture(minCorner + Vector3.Backward * Sidelength,
                Vector3.Left, new Vector2(2.0f / 3.0f - BorderTolerance, TexPosLower)),
            new VertexPositionNormalTexture(minCorner + Vector3.Up * Sidelength + Vector3.Backward * Sidelength,
                Vector3.Left, new Vector2(2.0f / 3.0f - BorderTolerance, TexPosUpper)),
            new VertexPositionNormalTexture(minCorner + Vector3.Up * Sidelength,
                Vector3.Left, new Vector2(1.0f / 3.0f + BorderTolerance, TexPosUpper))
            };
        }
        public VertexPositionNormalTexture[] VerticesXPositive(Vector3 minCorner)
        {
            return new VertexPositionNormalTexture[]{
            new VertexPositionNormalTexture(minCorner + Vector3.Right * Sidelength,
                Vector3.Right, new Vector2(2.0f / 3.0f - BorderTolerance, TexPosLower)),
            new VertexPositionNormalTexture(minCorner + Vector3.Right * Sidelength + Vector3.Up * Sidelength,
                Vector3.Right, new Vector2(2.0f / 3.0f - BorderTolerance, TexPosUpper)),
            new VertexPositionNormalTexture(minCorner + Vector3.Right * Sidelength + Vector3.Up * Sidelength + Vector3.Backward * Sidelength,
                Vector3.Right, new Vector2(1.0f / 3.0f + BorderTolerance, TexPosUpper)),
            new VertexPositionNormalTexture(minCorner + Vector3.Right * Sidelength + Vector3.Backward * Sidelength,
                Vector3.Right, new Vector2(1.0f / 3.0f + BorderTolerance, TexPosLower))
            };
        }
        public VertexPositionNormalTexture[] VerticesZPositive(Vector3 minCorner)
        {
            return new VertexPositionNormalTexture[]{
            new VertexPositionNormalTexture(minCorner + Vector3.Backward * Sidelength,
                Vector3.Backward, new Vector2(1.0f / 3.0f + BorderTolerance, TexPosLower)),
            new VertexPositionNormalTexture(minCorner + Vector3.Backward * Sidelength + Vector3.Right * Sidelength,
                Vector3.Backward, new Vector2(2.0f / 3.0f - BorderTolerance, TexPosLower)),
            new VertexPositionNormalTexture(minCorner + Vector3.Backward * Sidelength + Vector3.Right * Sidelength + Vector3.Up * Sidelength,
                Vector3.Backward, new Vector2(2.0f / 3.0f - BorderTolerance, TexPosUpper)),
            new VertexPositionNormalTexture(minCorner + Vector3.Backward * Sidelength + Vector3.Up * Sidelength,
                Vector3.Backward, new Vector2(1.0f / 3.0f + BorderTolerance, TexPosUpper))
            };
        }
        public VertexPositionNormalTexture[] VerticesYPositive(Vector3 minCorner)
        {
            return new VertexPositionNormalTexture[]{
            new VertexPositionNormalTexture(minCorner + Vector3.Up * Sidelength,
                Vector3.Up, new Vector2(BorderTolerance, TexPosLower)),
            new VertexPositionNormalTexture(minCorner + Vector3.Up * Sidelength + Vector3.Backward * Sidelength,
                Vector3.Up, new Vector2(BorderTolerance, TexPosUpper)),
            new VertexPositionNormalTexture(minCorner + Vector3.Up * Sidelength + Vector3.Right * Sidelength + Vector3.Backward * Sidelength,
                Vector3.Up, new Vector2(1.0f / 3.0f - BorderTolerance, TexPosUpper)),
            new VertexPositionNormalTexture(minCorner + Vector3.Up * Sidelength + Vector3.Right * Sidelength,
                Vector3.Up, new Vector2(1.0f / 3.0f - BorderTolerance, TexPosLower))
            };
        }
        #endregion
        public int[] Indices(int offSet)
        {
            return new int[]
            {
                offSet + 0,
                offSet + 1,
                offSet + 2,

                offSet + 0,
                offSet + 2,
                offSet + 3
            };
        }

        //private void SetIndices()
        //{
        //    Indices = new int[SideCount * 6];
        //    for (int i = 0; i < SideCount; i++)
        //    {
        //        int index = SideCount * i;
        //        Indices[index++] = i * 4 + 0;
        //        Indices[index++] = i * 4 + 1;
        //        Indices[index++] = i * 4 + 2;

        //        Indices[index++] = i * 4 + 0;
        //        Indices[index++] = i * 4 + 2;
        //        Indices[index++] = i * 4 + 3;
        //    }
        //}

        public override string ToString()
        {
            return this.Type.ToString();
        }



        public object Clone()
        {
            return new Block(this.Type);
        }

        public override bool Equals(object obj)
        {
            if (obj is Block)
            {
                return ((Block)obj).Type == this.Type;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return this.Type.GetHashCode() + 13;
        }
    }
}
