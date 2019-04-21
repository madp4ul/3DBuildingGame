using BlockGame3D;
using BlockGame3D.Model.BlockStore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlockGameClasses.ChunkData
{
    /// <summary>
    /// a 3-dimensional array of objects that provides 3-dimensional access to its content.
    /// </summary>
    class BlockStoreArray3 : IBlockStore
    {
        private readonly Block[,,] Data;

        public Point3D Size { get; private set; }

        public BlockStoreArray3(int x, int y, int z)
            : this(new Point3D(x, y, z))
        { }

        public BlockStoreArray3(Point3D size)
        {
            Size = size.Clone();
            this.Data = new Block[size.X, size.Y, size.Z];
        }

        public Block this[int x, int y, int z]
        {
            get
            {
                return this.Data[x, y, z];
            }
            set
            {
                this.Data[x, y, z] = value;
            }
        }
    }
}
