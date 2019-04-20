using BlockGameClasses;

namespace _1st3DGame.Model.BlockStore
{
    interface IBlockStore
    {
        Point3D Size { get; }

        Block this[int x, int y, int z] { get; set; }
    }
}
