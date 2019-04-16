using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlockGameClasses.RandomGeneration
{
    public abstract class ARandomGeneration<B> : IRandomGenerierung<B>
    {
        public readonly int ChunkWidth = 32;
        public readonly Point3D ChunkPosition;

        public float lowerMassBound { get; set; }
        public float UpperMassBound { get; set; }

        public ARandomGeneration(int chunkWidth, Point3D chunkPos)
        {
            this.ChunkWidth = chunkWidth;
            this.ChunkPosition = chunkPos;
        }

        public virtual void Begin()
        {

        }

        public virtual void End()
        {

        }

        public abstract B GetBlock(int x, int y, int z);

        public B GetBlock(Point3D position)
        {
            return GetBlock(position.X, position.Y, position.Z);
        }



    }
}
