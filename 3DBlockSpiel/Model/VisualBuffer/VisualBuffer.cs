using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockGame3D.Model.VisualBuffer
{
    class VisualBuffer<TVertex> : IDisposable where TVertex : struct
    {
        public readonly VertexBuffer Vertices;
        public readonly IndexBuffer Indices;

        public VisualBuffer(GraphicsDevice device, VertexDeclaration vertexDeclaration, BufferUsage bufferUsage, TVertex[] vertices, int[] indices)
        {
            Vertices = new VertexBuffer(device, VertexPositionIndexedNormalTexture.VertexDeclaration, vertices.Length, bufferUsage);
            Indices = new IndexBuffer(device, typeof(int), indices.Length, bufferUsage);

            Vertices.SetData(vertices);
            Indices.SetData(indices);
        }

        public void Dispose()
        {
            if (!Vertices.IsDisposed)
            {
                Vertices.Dispose();

            }
            if (!Indices.IsDisposed)
            {
                Indices.Dispose();
            }
        }

        public void SetForDevice(GraphicsDevice device)
        {
            device.Indices = Indices;
            device.SetVertexBuffer(Vertices);
        }
    }
}
