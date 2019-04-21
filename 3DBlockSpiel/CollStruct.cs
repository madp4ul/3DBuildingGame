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
    struct CollStruct
    {
        public Body Pressing;
        public Body GettingPressed;

        public CollStruct(Body pressing, Body gettingPressed)
        {
            this.Pressing = pressing;
            this.GettingPressed = gettingPressed;
        }

        public static bool operator ==(CollStruct c1, CollStruct c2)
        {
            return c1.Pressing == c2.Pressing && c1.GettingPressed == c2.GettingPressed;
        }

        public static bool operator !=(CollStruct c1, CollStruct c2)
        {
            return c1.Pressing != c2.Pressing || c1.GettingPressed != c2.GettingPressed;
        }

        public override int GetHashCode()
        {
            return (int)Pressing.EyePosition.X + 1 + (int)GettingPressed.EyePosition.Z;
        }

        public override bool Equals(object obj)
        {
            if (obj is CollStruct)
            {
                return (CollStruct)obj == this;
            }
            else
            return false;
        }
    }
}
