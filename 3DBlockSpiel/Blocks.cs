using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace BlockGame3D
{
    enum BlockTypes : byte
    {
        TypeCount = 4,
        Air = 0,
        GrassDirt = 1,
        Stone = 2,
        Dirt = 3
    }

    static class Blocks
    {
        public static bool[] CollidesWithPlayer = new bool[(int)BlockTypes.TypeCount]
        {
            false,
            true,
            true,
            true
        };

        public static bool[] Visible = new bool[(int)BlockTypes.TypeCount]
        {
            false,
            true,
            true,
            true
        };

        public static Block[] OnPickup = new Block[(int)BlockTypes.TypeCount]
        {
            new Block(BlockTypes.Air),
            new Block(BlockTypes.Dirt),
            new Block(BlockTypes.Stone),
            new Block(BlockTypes.Dirt)
        };

        public static int[] Armor = new int[(int)BlockTypes.TypeCount]
        {
            0,
            10,
            30,
            10
        };

        
    }
}
