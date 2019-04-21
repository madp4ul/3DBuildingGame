using BlockGame3D;
using BlockGame3D.Model.BlockStore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlockGameClasses.ChunkData
{
    /// <summary>
    /// a 1-dimensional array of objects that provides 3-dimensional access to its content.
    /// </summary>
    class BlockStoreArray1 : IBlockStore
    {
        private readonly Block[] Data;

        public Point3D Size { get; private set; }

        public BlockStoreArray1(int x, int y, int z)
            : this(new Point3D(x, y, z))
        { }

        public BlockStoreArray1(Point3D chunkSize)
        {
            this.Size = chunkSize.Clone();
            this.Data = new Block[chunkSize.X * chunkSize.Y * chunkSize.Z];
        }

        public Block this[int x, int y, int z]
        {
            get
            {
                return this.Data[x * (Size.Z * Size.Y) + y * Size.Z + z];
            }
            set
            {
                this.Data[x * (Size.Z * Size.Y) + y * Size.Z + z] = value;
            }
        }
    }
}
