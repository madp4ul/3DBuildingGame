using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlockGameClasses;

namespace _1st3DGame.Model.BlockStore
{
    class SpaceEfficientBlockStore : IBlockStore
    {
        static readonly int BitPerBlockType;
        //Add a bit to store "NotHidden"
        static int BitPerBlock { get { return BitPerBlockType + 1; } }

        static SpaceEfficientBlockStore()
        {
            int typeCount = (int)BlockTypes.TypeCount;

            BitPerBlockType = 1;
            while (Math.Pow(2, BitPerBlockType) < (typeCount - 0.001))
            {
                BitPerBlockType++;
            }
        }

        readonly BitArray Data;

        public Point3D Size { get; private set; }

        public SpaceEfficientBlockStore(int x, int y, int z)
        {
            Size = new Point3D(x, y, z);

            //Add a byte due to possible cutting off in division
            int bitCount = x * y * z * BitPerBlock;
            Data = new BitArray(bitCount);
        }

        public Block this[int x, int y, int z]
        {
            get
            {
                return DeserializeBlockAt(x, y, z);
            }
            set
            {
                SerializeBlockAt(x, y, z, value);
            }
        }

        private Block DeserializeBlockAt(int x, int y, int z)
        {
            int readStart = GetBitPos(x, y, z);
            int readEnd = readStart + BitPerBlockType;

            int result = 0;
            int multiplier = 1;
            for (int i = readStart; i < readEnd; i++)
            {
                result += multiplier * (Data[i] ? 1 : 0);

                multiplier *= 2;
            }

            return new Block((BlockTypes)result)
            {
                NotHidden = Data[readEnd]
            };
        }

        private void SerializeBlockAt(int x, int y, int z, Block block)
        {
            int value = (int)block.Type;

            int writeStart = GetBitPos(x, y, z);
            int writeEnd = writeStart + BitPerBlockType;

            for (int i = writeStart; i < writeEnd; i++)
            {
                Data[i] = value % 2 == 1;
                value /= 2;
            }

            Data[writeEnd] = block.NotHidden;
        }

        private int GetBitPos(int x, int y, int z)
        {
            return (x * (Size.Z * Size.Y) + y * Size.Z + z) * BitPerBlock;
        }
    }
}
