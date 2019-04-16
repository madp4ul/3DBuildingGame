using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace _1st3DGame
{

    class RectangleFloats
    {

        public float X, Y, Width, Height;
        public float Right { get { return X + Width; } }
        public float Left { get { return X; } }
        public float Top { get { return Y; } }
        public float Bottom { get { return Y + Height; } }

        public Vector2[] Corners { get { return new Vector2[] { new Vector2(X, Y), new Vector2(X + Width, Y), new Vector2(X, Y + Height), new Vector2(X + Width, Y + Height) }; } }

        public RectangleFloats(float x, float y, float width, float height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }

        public RectangleFloats(float x, float y, float x2, float y2,bool unusedValue)
            : this(x, y, x2 - x, y2 - y)
        { }

        public bool IntersectsOrAligns(RectangleFloats rect)
        {
            if (rect.X <= this.X + this.Width && this.X <= rect.X + rect.Width && rect.Y <= this.Y + this.Height)
                return this.Y <= rect.Y + rect.Height;
            else
                return false;
        }

        public bool Intersects(RectangleFloats rect)
        {
            if (rect.X < this.X + this.Width && this.X < rect.X + rect.Width && rect.Y < this.Y + this.Height)
                return this.Y < rect.Y + rect.Height;
            else
                return false;
        }

        public bool Contains(Vector2 point)
        {
            if (point.X > this.X &&
                point.X < this.X + this.Width &&
                point.Y > this.Y &&
                point.Y < this.Y + this.Height)
                return true;
            return false;
        }
        public bool Contains(RectangleFloats otherR)
        {
            if (otherR.X > this.X &&
                otherR.Width + otherR.X < this.Width + this.X &&
                otherR.Y > this.Y &&
                otherR.Y + otherR.Height < this.Y + this.Height)
                return true;

            return false;
        }

        public override string ToString()
        {
            return "X: " + this.X + ", Width: " + this.Width + ", Y: " + this.Y + ", Height: " + this.Height;
        }
    }
}
