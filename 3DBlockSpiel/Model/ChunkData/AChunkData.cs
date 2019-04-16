using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlockGameClasses.ChunkData
{
    /// <summary>
    /// this is supposed to some sort of saving-structure for the blocks(B is the block-type) contained i a chunk.
    /// </summary>
    public abstract class AChunkData<B>
    {
        public readonly Point3D Size;

        public AChunkData(Point3D size)
        {
            this.Size = size;
        }

        public B this[int x, int y, int z]
        {
            get
            {
                return GetBlock(x, y, z);
            }
            set
            {
                SetBlock(value, x, y, z);
            }
        }

        public B this[Point3D pos]
        {
            get
            {
                return GetBlock(pos);
            }
            set
            {
                SetBlock(value, pos);
            }
        }

        /// <summary>
        /// get a block by its position in chunk
        /// </summary>
        /// <param name="position">coordinates in chunk</param>
        /// <returns></returns>
        public B GetBlock(Point3D position)
        {
            return GetBlock(position.X, position.Y, position.Z);
        }

        /// <summary>
        /// get a block by its position in chunk
        /// </summary>
        /// <param name="x">pos x</param>
        /// <param name="y">pos y</param>
        /// <param name="z">pos z</param>
        /// <returns>block at this position</returns>
        public abstract B GetBlock(int x, int y, int z);

        /// <summary>
        /// set a block to this position in chunk
        /// </summary>
        /// <param name="b">block</param>
        /// <param name="position">coordinates in chunk</param>
        public void SetBlock(B b, Point3D position)
        {
            SetBlock(b, position.X, position.Y, position.Z);
        }

        /// <summary>
        /// set a block to this position in chunk
        /// </summary>
        /// <param name="b">block</param>
        /// <param name="x">pos x</param>
        /// <param name="y">pos y</param>
        /// <param name="z">pos z</param>
        public abstract void SetBlock(B b, int x, int y, int z);

        public override string ToString()
        {
            return "Array: " + this.Size.X + "*" + this.Size.Y + "*" + this.Size.Z + "(" + this.Size.X * this.Size.Y * this.Size.Z + " Elements)";
        }
    }
}
