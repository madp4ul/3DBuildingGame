using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Xna.Framework.Graphics;

namespace _1st3DGame
{
    struct VertexPositionIndexedNormalTexture
    {
        public Vector4 PositionAndNormalIndex;
        public NormalizedShort2 TextureCoordinate;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="positionNormal">1,2,3 make position, 4 makes index of normal in an array of normals in the shader</param>
        /// <param name="textureCoodinate">texcord</param>
        public VertexPositionIndexedNormalTexture(Vector4 positionNormal, Vector2 textureCoodinate)
        {
            this.PositionAndNormalIndex = positionNormal;
            this.TextureCoordinate = new NormalizedShort2(textureCoodinate);
        }

        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
            (
                new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.Position, 0),
                new VertexElement(16, VertexElementFormat.NormalizedShort2, VertexElementUsage.TextureCoordinate, 0)
            );

        public override string ToString()
        {
            return "Pos+N: " + PositionAndNormalIndex.ToString() + "Tex: " + TextureCoordinate.ToVector2().ToString();
        }
    }
}
