using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlockGameClasses.ChunkData
{
    /// <summary>
    /// a 1-dimensional array of objects that provides 3-dimensional access to its content.
    /// </summary>
    public class ChunkDataArray1<T> : AChunkData<T>
    {
        private T[] Data;

        public ChunkDataArray1(int x, int y, int z)
            : this(new Point3D(x, y, z))
        { }

        public ChunkDataArray1(Point3D chunkSize)
            : base(chunkSize)
        {
            this.Data = new T[chunkSize.X * chunkSize.Y * chunkSize.Z];
        }

        public override T GetBlock(int x, int y, int z)
        {
            return this.Data[x * (Size.Z * Size.Y) + y * Size.Z + z];
        }

        public override void SetBlock(T b, int x, int y, int z)
        {
            this.Data[x * (Size.Z * Size.Y) + y * Size.Z + z] = b;
        }
    }
}
