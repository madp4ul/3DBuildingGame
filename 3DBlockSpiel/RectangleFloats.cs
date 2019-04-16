using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace _1st3DGame
{
    class RectangleFloats
    {
        public float X, Y, Width, Height;
        public Vector2[] Corners { get { return new Vector2[] { new Vector2(X, Y), new Vector2(X + Width, Y), new Vector2(X, Y + Height), new Vector2(X + Width, Y + Height) }; } }

        public RectangleFloats(float x, float y, float width, float height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }

        public bool Intersects(RectangleFloats otherR)
        {
            if (Contains(otherR) || otherR.Contains(this))
                return true;

            Vector2[] corners = this.Corners;
            foreach (Vector2 corner in corners)
                if (otherR.Contains(corner))
                    return true;
            corners = otherR.Corners;
            foreach (Vector2 corner in corners)
                if (this.Contains(corner))
                    return true;

            bool allCornersBetweenX = true;
            bool allCornersBetweenY = true;
            foreach (Vector2 corner in corners)
            {
                if (corner.X < this.X || corner.X > this.X + this.Width)
                    allCornersBetweenX = false;
                if (corner.Y < this.Y || corner.Y > this.Y + this.Height)
                    allCornersBetweenY = false;
            }
            if (allCornersBetweenX)
            {
                bool oneAboveY = false; bool oneBelowY = false;
                foreach (Vector2 corner in corners)
                    if (corner.Y > this.Y)
                        oneAboveY = true;
                    else
                        oneBelowY = true;
                if (oneAboveY && oneBelowY)
                    return true;
            }
            else if (allCornersBetweenY)
            {
                bool oneAboveX = false; bool oneBelowX = false;
                foreach (Vector2 corner in corners)
                    if (corner.X > this.X)
                        oneAboveX = true;
                    else
                        oneBelowX = true;
                if (oneAboveX && oneBelowX)
                    return true;
            }
            return false;
        }

        public bool Contains(Vector2 point)
        {
            if (point.X >= this.X &&
                point.X <= this.X + this.Width &&
                point.Y >= this.Y &&
                point.Y <= this.Y + this.Height)
                return true;
            return false;
        }
        public bool Contains(RectangleFloats otherR)
        {
            if (otherR.X >= this.X &&
                otherR.Width + otherR.X <= this.Width + this.X &&
                otherR.Y >= this.Y &&
                otherR.Y + otherR.Height <= this.Y + this.Height)
                return true;

            return false;
        }
    }
}
