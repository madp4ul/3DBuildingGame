using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlockGameClasses.ChunkData
{
    /// <summary>
    /// a 3-dimensional array of objects that provides 3-dimensional access to its content.
    /// </summary>
    public class ChunkDataArray3<T> : AChunkData<T>
    {
        private T[, ,] Data;

        public ChunkDataArray3(int x, int y, int z)
            : this(new Point3D(x, y, z))
        { }

        public ChunkDataArray3(Point3D size)
            : base(size)
        {
            this.Data = new T[size.X, size.Y, size.Z];
        }

        public override T GetBlock(int x, int y, int z)
        {
            return this.Data[x, y, z];
        }

        public override void SetBlock(T b, int x, int y, int z)
        {
            this.Data[x, y, z] = b;
        }
    }
}
