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
        public const int Sidelength = 1;
        private const int CornerCount = 8;
        private const int SideCount = 6;
        public const int VerticesCount = CornerCount * 3;
        public const int IndicesCount = SideCount * 6;

        public static readonly float Radius = new Vector3(0.5f * Sidelength).Length();

        public static Texture2D BlockTex;
        public static Texture2D BlockBumpMap;
        public static Texture2D IconTex;
        public Rectangle IconRectangle
        {
            get
            {
                return new Rectangle(
                    0, IconTex.Width * (int)Type, IconTex.Width, IconTex.Width);
            }
        }

        private const float TexturesPerBlock = 4;
        private const float BorderTolerance = 0.026f;
        private float TexPosUpper { get { return ((int)this.Type * 1.0f) / ((int)BlockTypes.TypeCount * 1.0f) + BorderTolerance; } }
        private float TexPosLower { get { return ((int)this.Type * 1.0f + 1) / ((int)BlockTypes.TypeCount * 1.0f) - BorderTolerance; } }



        public BlockTypes Type { get; protected set; }

        public int Armor { get { return Blocks.Armor[(int)this.Type]; } }
        public Block Pickup { get { return Blocks.OnPickup[(int)this.Type]; } }
        public bool CollidesWithPlayer { get { return Blocks.CollidesWithPlayer[(int)this.Type]; } }
        public bool Visible { get { return Blocks.Visible[(int)this.Type]; } }

        public bool NotHidden;

        public Block(BlockTypes type)
        {
            this.Type = type;
        }

        #region ReturnVertices
        public VertexPositionIndexedNormalTexture[] VerticesYNegative(Vector3 minCorner)
        {
            return new VertexPositionIndexedNormalTexture[]{
                new VertexPositionIndexedNormalTexture(new Vector4(minCorner,4),
                    new Vector2(2.0f / TexturesPerBlock + BorderTolerance, TexPosLower)),
                new VertexPositionIndexedNormalTexture(new Vector4(minCorner + Vector3.Right * Sidelength,4),
                    new Vector2(3.0f / TexturesPerBlock - BorderTolerance, TexPosLower)),
                new VertexPositionIndexedNormalTexture(new Vector4(minCorner + Vector3.Right * Sidelength + Vector3.Backward * Sidelength,4),
                    new Vector2(3.0f / TexturesPerBlock - BorderTolerance, TexPosUpper)),
                new VertexPositionIndexedNormalTexture(new Vector4(minCorner + Vector3.Backward * Sidelength,4),
                    new Vector2(2.0f / TexturesPerBlock + BorderTolerance, TexPosUpper))
            };
        }
        public VertexPositionIndexedNormalTexture[] VerticesZNegative(Vector3 minCorner)
        {
            return new VertexPositionIndexedNormalTexture[]{
            new VertexPositionIndexedNormalTexture(new Vector4(minCorner,5),
                new Vector2(2.0f / TexturesPerBlock - BorderTolerance, TexPosLower)),
            new VertexPositionIndexedNormalTexture(new Vector4(minCorner + Vector3.Up * Sidelength,5),
                new Vector2(2.0f / TexturesPerBlock - BorderTolerance, TexPosUpper)),
            new VertexPositionIndexedNormalTexture(new Vector4(minCorner + Vector3.Up * Sidelength + Vector3.Right * Sidelength,5),
                new Vector2(1.0f / TexturesPerBlock + BorderTolerance, TexPosUpper)),
            new VertexPositionIndexedNormalTexture(new Vector4(minCorner + Vector3.Right * Sidelength,5),
                new Vector2(1.0f / TexturesPerBlock + BorderTolerance, TexPosLower))
            };
        }
        public VertexPositionIndexedNormalTexture[] VerticesXNegative(Vector3 minCorner)
        {
            return new VertexPositionIndexedNormalTexture[]{
            new VertexPositionIndexedNormalTexture(new Vector4(minCorner,3),
                new Vector2(1.0f / TexturesPerBlock + BorderTolerance, TexPosLower)),
            new VertexPositionIndexedNormalTexture(new Vector4(minCorner + Vector3.Backward * Sidelength,3),
                new Vector2(2.0f / TexturesPerBlock - BorderTolerance, TexPosLower)),
            new VertexPositionIndexedNormalTexture(new Vector4(minCorner + Vector3.Up * Sidelength + Vector3.Backward * Sidelength,3),
                new Vector2(2.0f / TexturesPerBlock - BorderTolerance, TexPosUpper)),
            new VertexPositionIndexedNormalTexture(new Vector4(minCorner + Vector3.Up * Sidelength,3),
                new Vector2(1.0f / TexturesPerBlock + BorderTolerance, TexPosUpper))
            };
        }
        public VertexPositionIndexedNormalTexture[] VerticesXPositive(Vector3 minCorner)
        {
            return new VertexPositionIndexedNormalTexture[]{
            new VertexPositionIndexedNormalTexture(new Vector4(minCorner + Vector3.Right * Sidelength,0),
                new Vector2(2.0f / TexturesPerBlock - BorderTolerance, TexPosLower)),
            new VertexPositionIndexedNormalTexture(new Vector4(minCorner + Vector3.Right * Sidelength + Vector3.Up * Sidelength,0),
                new Vector2(2.0f / TexturesPerBlock - BorderTolerance, TexPosUpper)),
            new VertexPositionIndexedNormalTexture(new Vector4(minCorner + Vector3.Right * Sidelength + Vector3.Up * Sidelength + Vector3.Backward * Sidelength,0),
                new Vector2(1.0f / TexturesPerBlock + BorderTolerance, TexPosUpper)),
            new VertexPositionIndexedNormalTexture(new Vector4(minCorner + Vector3.Right * Sidelength + Vector3.Backward * Sidelength,0),
                new Vector2(1.0f / TexturesPerBlock + BorderTolerance, TexPosLower))
            };
        }
        public VertexPositionIndexedNormalTexture[] VerticesZPositive(Vector3 minCorner)
        {
            return new VertexPositionIndexedNormalTexture[]{
            new VertexPositionIndexedNormalTexture(new Vector4(minCorner + Vector3.Backward * Sidelength,2),
                new Vector2(1.0f / TexturesPerBlock + BorderTolerance, TexPosLower)),
            new VertexPositionIndexedNormalTexture(new Vector4(minCorner + Vector3.Backward * Sidelength + Vector3.Right * Sidelength,2),
                new Vector2(2.0f / TexturesPerBlock - BorderTolerance, TexPosLower)),
            new VertexPositionIndexedNormalTexture(new Vector4(minCorner + Vector3.Backward * Sidelength + Vector3.Right * Sidelength + Vector3.Up * Sidelength,2),
                new Vector2(2.0f / TexturesPerBlock - BorderTolerance, TexPosUpper)),
            new VertexPositionIndexedNormalTexture(new Vector4(minCorner + Vector3.Backward * Sidelength + Vector3.Up * Sidelength,2),
                new Vector2(1.0f / TexturesPerBlock + BorderTolerance, TexPosUpper))
            };
        }
        public VertexPositionIndexedNormalTexture[] VerticesYPositive(Vector3 minCorner)
        {
            return new VertexPositionIndexedNormalTexture[]{
            new VertexPositionIndexedNormalTexture(new Vector4(minCorner + Vector3.Up * Sidelength,1),
                new Vector2(BorderTolerance, TexPosUpper)),
            new VertexPositionIndexedNormalTexture(new Vector4(minCorner + Vector3.Up * Sidelength + Vector3.Backward * Sidelength,1),
                new Vector2(BorderTolerance, TexPosLower)),
            new VertexPositionIndexedNormalTexture(new Vector4(minCorner + Vector3.Up * Sidelength + Vector3.Right * Sidelength + Vector3.Backward * Sidelength,1),
                new Vector2(1.0f / TexturesPerBlock - BorderTolerance, TexPosLower)),
            new VertexPositionIndexedNormalTexture(new Vector4(minCorner + Vector3.Up * Sidelength + Vector3.Right * Sidelength,1),
                new Vector2(1.0f / TexturesPerBlock - BorderTolerance, TexPosUpper))
            };
        }
        //public VertexPositionNormalTexture[] VerticesYNegative(Vector3 minCorner)
        //{
        //    return new VertexPositionNormalTexture[]{
        //                    new VertexPositionNormalTexture(minCorner,
        //        Vector3.Down, new Vector2(2.0f / 3.0f + BorderTolerance, TexPosLower)),
        //    new VertexPositionNormalTexture(minCorner + Vector3.Right * Sidelength,
        //        Vector3.Down, new Vector2(1 - BorderTolerance, TexPosLower)),
        //    new VertexPositionNormalTexture(minCorner + Vector3.Right * Sidelength + Vector3.Backward * Sidelength,
        //        Vector3.Down, new Vector2(1 - BorderTolerance, TexPosUpper)),
        //    new VertexPositionNormalTexture(minCorner + Vector3.Backward * Sidelength,
        //        Vector3.Down, new Vector2(2.0f / 3.0f + BorderTolerance, TexPosUpper))
        //    };
        //}
        //public VertexPositionNormalTexture[] VerticesZNegative(Vector3 minCorner)
        //{
        //    return new VertexPositionNormalTexture[]{
        //    new VertexPositionNormalTexture(minCorner,
        //        Vector3.Forward, new Vector2(2.0f / 3.0f - BorderTolerance, TexPosLower)),
        //    new VertexPositionNormalTexture(minCorner + Vector3.Up * Sidelength,
        //        Vector3.Forward, new Vector2(2.0f / 3.0f - BorderTolerance, TexPosUpper)),
        //    new VertexPositionNormalTexture(minCorner + Vector3.Up * Sidelength + Vector3.Right * Sidelength,
        //        Vector3.Forward, new Vector2(1.0f / 3.0f + BorderTolerance, TexPosUpper)),
        //    new VertexPositionNormalTexture(minCorner + Vector3.Right * Sidelength,
        //        Vector3.Forward, new Vector2(1.0f / 3.0f + BorderTolerance, TexPosLower))
        //    };
        //}
        //public VertexPositionNormalTexture[] VerticesXNegative(Vector3 minCorner)
        //{
        //    return new VertexPositionNormalTexture[]{
        //    new VertexPositionNormalTexture(minCorner,
        //        Vector3.Left, new Vector2(1.0f / 3.0f + BorderTolerance, TexPosLower)),
        //    new VertexPositionNormalTexture(minCorner + Vector3.Backward * Sidelength,
        //        Vector3.Left, new Vector2(2.0f / 3.0f - BorderTolerance, TexPosLower)),
        //    new VertexPositionNormalTexture(minCorner + Vector3.Up * Sidelength + Vector3.Backward * Sidelength,
        //        Vector3.Left, new Vector2(2.0f / 3.0f - BorderTolerance, TexPosUpper)),
        //    new VertexPositionNormalTexture(minCorner + Vector3.Up * Sidelength,
        //        Vector3.Left, new Vector2(1.0f / 3.0f + BorderTolerance, TexPosUpper))
        //    };
        //}
        //public VertexPositionNormalTexture[] VerticesXPositive(Vector3 minCorner)
        //{
        //    return new VertexPositionNormalTexture[]{
        //    new VertexPositionNormalTexture(minCorner + Vector3.Right * Sidelength,
        //        Vector3.Right, new Vector2(2.0f / 3.0f - BorderTolerance, TexPosLower)),
        //    new VertexPositionNormalTexture(minCorner + Vector3.Right * Sidelength + Vector3.Up * Sidelength,
        //        Vector3.Right, new Vector2(2.0f / 3.0f - BorderTolerance, TexPosUpper)),
        //    new VertexPositionNormalTexture(minCorner + Vector3.Right * Sidelength + Vector3.Up * Sidelength + Vector3.Backward * Sidelength,
        //        Vector3.Right, new Vector2(1.0f / 3.0f + BorderTolerance, TexPosUpper)),
        //    new VertexPositionNormalTexture(minCorner + Vector3.Right * Sidelength + Vector3.Backward * Sidelength,
        //        Vector3.Right, new Vector2(1.0f / 3.0f + BorderTolerance, TexPosLower))
        //    };
        //}
        //public VertexPositionNormalTexture[] VerticesZPositive(Vector3 minCorner)
        //{
        //    return new VertexPositionNormalTexture[]{
        //    new VertexPositionNormalTexture(minCorner + Vector3.Backward * Sidelength,
        //        Vector3.Backward, new Vector2(1.0f / 3.0f + BorderTolerance, TexPosLower)),
        //    new VertexPositionNormalTexture(minCorner + Vector3.Backward * Sidelength + Vector3.Right * Sidelength,
        //        Vector3.Backward, new Vector2(2.0f / 3.0f - BorderTolerance, TexPosLower)),
        //    new VertexPositionNormalTexture(minCorner + Vector3.Backward * Sidelength + Vector3.Right * Sidelength + Vector3.Up * Sidelength,
        //        Vector3.Backward, new Vector2(2.0f / 3.0f - BorderTolerance, TexPosUpper)),
        //    new VertexPositionNormalTexture(minCorner + Vector3.Backward * Sidelength + Vector3.Up * Sidelength,
        //        Vector3.Backward, new Vector2(1.0f / 3.0f + BorderTolerance, TexPosUpper))
        //    };
        //}
        //public VertexPositionNormalTexture[] VerticesYPositive(Vector3 minCorner)
        //{
        //    return new VertexPositionNormalTexture[]{
        //    new VertexPositionNormalTexture(minCorner + Vector3.Up * Sidelength,
        //        Vector3.Up, new Vector2(BorderTolerance, TexPosLower)),
        //    new VertexPositionNormalTexture(minCorner + Vector3.Up * Sidelength + Vector3.Backward * Sidelength,
        //        Vector3.Up, new Vector2(BorderTolerance, TexPosUpper)),
        //    new VertexPositionNormalTexture(minCorner + Vector3.Up * Sidelength + Vector3.Right * Sidelength + Vector3.Backward * Sidelength,
        //        Vector3.Up, new Vector2(1.0f / 3.0f - BorderTolerance, TexPosUpper)),
        //    new VertexPositionNormalTexture(minCorner + Vector3.Up * Sidelength + Vector3.Right * Sidelength,
        //        Vector3.Up, new Vector2(1.0f / 3.0f - BorderTolerance, TexPosLower))
        //    };
        //}
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

        public override string ToString()
        {
            return this.Type.ToString();
        }

        public object Clone()
        {
            return new Block(this.Type);
        }

        //public static bool operator ==(Block b1, Block b2)
        //{
        //    if (b1.Type == b2.Type)
        //    {
        //        return true;
        //    }
        //    else return false;
        //}

        //public static bool operator !=(Block b1, Block b2)
        //{
        //    if (b1.Type == b2.Type)
        //    {
        //        return false;
        //    }
        //    else return true;
        //}

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
