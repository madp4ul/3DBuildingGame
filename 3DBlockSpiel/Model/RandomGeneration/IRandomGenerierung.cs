using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlockGameClasses.RandomGeneration
{
    public interface IRandomGenerierung<B>
    {
        float lowerMassBound { get; set; }
        float UpperMassBound { get; set; }


        void Begin();

        B GetBlock(int x, int y, int z);

        void End();

        B GetBlock(Point3D position);
    }
}
