using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace BlockGameClasses
{
    public class Point3D : IComparable<Point3D>
    {
        public int X;
        public int Y;
        public int Z;

        public static Point3D Empty { get { return new Point3D(0, 0, 0); } }

        public Point3D(int x, int y, int z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public Point3D(Point3D p)
        {
            this.X = p.X;
            this.Y = p.Y;
            this.Z = p.Z;
        }

        /// <summary>
        /// returns a 2D-point with the dimensions NOT given as parameter, e.g. dimension = 1 -> new point(X,Z)
        /// </summary>
        /// <param name="dimension"></param>
        /// <returns></returns>
        public Point FlattenDimension(byte dimension)
        {
            if (dimension == 0)
                return new Point(this.Y, this.Z);
            else if (dimension == 1)
                return new Point(X, Z);
            else if (dimension == 2)
                return new Point(X, Y);
            else return Point.Zero;
        }

        public Point3D AddX(int add)
        {
            return new Point3D(this.X + add, this.Y, this.Z);
        }
        public Point3D AddY(int add)
        {
            return new Point3D(this.X, this.Y + add, this.Z);
        }
        public Point3D AddZ(int add)
        {
            return new Point3D(this.X, this.Y, this.Z + add);
        }

        public float Length()
        {
            return (float)Math.Sqrt(X * X + Y * Y + Z * Z);
        }

        public Vector3 ToVector()
        {
            return new Vector3(X, Y, Z);
        }

        public static Point3D operator +(Point3D p1, Point3D p2)
        {
            return new Point3D(p1.X + p2.X, p1.Y + p2.Y, p1.Z + p2.Z);
        }

        public static Point3D operator -(Point3D p1, Point3D p2)
        {
            return new Point3D(p1.X - p2.X, p1.Y - p2.Y, p1.Z - p2.Z);
        }

        public int CompareTo(Point3D p2)
        {
            if (p2 == null)
                return 1;
            if (X != p2.X)
                return this.X - p2.X;
            else if (Y != p2.Y)
                return Y - p2.Y;
            else if (Z != p2.Z)
                return Z - p2.Z;
            else
                return 0;
        }

        public static bool operator ==(Point3D p1, Point3D p2)
        {
            if ((object)p1 == null ^ (object)p2 == null)
                return false;
            else if ((object)p1 == null && (object)p2 == null)
                return true;
            return p1.X == p2.X && p1.Y == p2.Y && p1.Z == p2.Z;
        }
        public static bool operator !=(Point3D p1, Point3D p2)
        {
            return !(p1 == p2);
        }
        public override bool Equals(object obj)
        {
            if (obj is Point3D)
                return this == (Point3D)obj;
            return false;
        }
        public override int GetHashCode()
        {
            return X + Y + Z;
        }

        public override string ToString()
        {
            return "(" + X + "|" + Y + "|" + Z + ")";
        }
    }
}
