using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace _1st3DGame
{
    enum BlockTypes : byte
    {
        TypeCount = 3,
        Air = 0,
        Dirt = 1,
        Stone = 2
    }

    static class Blocks
    {
        public static bool[] CollidesWithPlayer = new bool[(int)BlockTypes.TypeCount]
        {
            false,
            true,
            true
           
        };

        public static bool[] Visible = new bool[(int)BlockTypes.TypeCount]
        {
            false,
            true,
            true
        };
    }
}
