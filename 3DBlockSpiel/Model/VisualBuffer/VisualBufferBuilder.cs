using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockGame3D.Model.VisualBuffer
{
    class VisualBufferBuilder<TVertex> where TVertex : struct
    {
        private List<TVertex> Vertices;
        private List<int> Indices;

        public int VertexCount { get { return Vertices.Count; } }
        public int IndexCount { get { return Indices.Count; } }

        public VisualBufferBuilder()
        {
            Vertices = new List<TVertex>();
            Indices = new List<int>();
        }

        public void AddBuffer(IEnumerable<TVertex> vertices, IEnumerable<int> indices)
        {
            int indexOffset = Vertices.Count;

            Vertices.AddRange(vertices);
            Indices.AddRange(indices.Select(i => i + indexOffset));
        }

        public void AddBuffer(VisualBufferBuilder<TVertex> buffer)
        {
            AddBuffer(buffer.Vertices, buffer.Indices);
        }

        public VisualBuffer<TVertex> CreateBuffer(GraphicsDevice device, VertexDeclaration vertexDeclaration, BufferUsage bufferUsage)
        {
            if (Vertices.Count == 0 || Indices.Count == 0)
            {
                return null;
            }

            return new VisualBuffer<TVertex>(device, vertexDeclaration, bufferUsage, Vertices.ToArray(), Indices.ToArray());
        }
    }
}
