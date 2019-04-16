using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using BlockGameClasses;

namespace _1st3DGame
{
    static class Math2
    {
        public static float  Atan2(float y, float x)
        {
            return (float)Math.Atan2(y, x);
        }
    }
    static class Extensions
    {
        public static int Get1DIndex<T>(this T[, ,] multiDimArray, int x, int y, int z)
        {
            int xLength = multiDimArray.GetLength(0);
            int yLength = multiDimArray.GetLength(1);
            int zLength = multiDimArray.GetLength(2);

            return (x + z * xLength + y * xLength * zLength);
        }

        public static Point Subtract(this Point p1, Point p2)
        {
            return new Point(p1.X - p2.X, p1.Y - p2.Y);
        }
        public static Point Add(this Point p1, Point p2)
        {
            return new Point(p2.X + p1.X, p2.Y + p1.Y);
        }

        public static bool IsSameAs(this BoundingBox b1, BoundingBox b2)
        {
            if (b1.Min == b2.Min && b1.Max == b2.Max)
                return true;
            else
                return false;
        }

        public static bool SameSign(this float f, float f2)
        {
            if (f == 0 || f2 == 0)
                return true;
            if (f < 0)
                return f2 < 0;
            else
                return f2 > 0;
        }

        /// <summary>
        /// Returns (X|Z) of inputvector
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector2 ToVector2D(this Vector3 v)
        {
            return new Vector2(v.X, v.Z);
        }

        public static Point3D ToPoint(this Vector3 v)
        {
            return new Point3D((int)v.X, (int)v.Y, (int)v.Z);
        }

        public static float Clamp(this float f, float min, float max)
        {
            if (f < min)
                return min;
            else if (f > max)
                return max;
            else
                return f;
        }

        public static Vector3 Normalized(this Vector3 v)
        {
            v.Normalize();
            return v;
        }

        public static TimeSpan TimeAgo(this DateTime dt)
        {
            return (DateTime.Now - dt);
        }

        public static Vector3 Positive(this Vector3 v)
        {
            return new Vector3(Math.Abs(v.X), Math.Abs(v.Y), Math.Abs(v.Z));
        }

        public static Vector3 Clamp(this Vector3 v, float min)
        {
            return new Vector3(
                v.X > min ? v.X : min,
                v.Y > min ? v.Y : min,
                v.Z > min ? v.Z : min);
        }

        public static float Normalize(this float f)
        {
            return f < 0 ? -1 : 1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="f"></param>
        /// <param name="factor">value from 0 to 1</param>
        /// <param name="min">border under </param>
        public static float Reduce(this float f, float factor, float min)
        {
            f *= factor;
            if (Math.Abs(f) < min)
                f = 0;
            return f;
        }

        public static BoundingBox MoveBBox(this BoundingBox bbox, Vector3 difference)
        {
            return new BoundingBox(bbox.Min += difference, bbox.Max += difference);
        }

        public static float Radius(this BoundingBox bbox)
        {
            return new Vector3(
                0.5f * (bbox.Max.X - bbox.Min.X),
                0.5f * (bbox.Max.Y - bbox.Min.Y),
                0.5f * (bbox.Max.Z - bbox.Min.Z)).Length();
        }

        public static Vector3 Center(this BoundingBox bbox)
        {
            return bbox.Min + 0.5f * (bbox.Max - bbox.Min);
        }

        public static string ToShortString(this Vector3 v)
        {
            return "x: " + Math.Round(v.X) + " y: " + Math.Round(v.Y) + " z: " + Math.Round(v.Z);
        }

        public static bool Align(this BoundingBox thisBB, BoundingBox otherBB)
        {
            if (thisBB.Min.X.Close10000(otherBB.Max.X) || otherBB.Min.X.Close10000(thisBB.Max.X))
                if (new RectangleFloats(thisBB.Min.Y, thisBB.Min.Z, thisBB.Max.Y - thisBB.Min.Y, thisBB.Max.Z - thisBB.Min.Z).IntersectsOrAligns(
                    new RectangleFloats(otherBB.Min.Y, otherBB.Min.Z, otherBB.Max.Y - otherBB.Min.Y, otherBB.Max.Z - otherBB.Min.Z)))
                    return true;
            if (thisBB.Min.Y.Close10000(otherBB.Max.Y) || otherBB.Min.Y.Close10000(thisBB.Max.Y))
                if (new RectangleFloats(thisBB.Min.X, thisBB.Min.Z, thisBB.Max.X - thisBB.Min.X, thisBB.Max.Z - thisBB.Min.Z).IntersectsOrAligns(
                    new RectangleFloats(otherBB.Min.X, otherBB.Min.Z, otherBB.Max.X - otherBB.Min.X, otherBB.Max.Z - otherBB.Min.Z)))
                    return true;
            if (thisBB.Min.Z.Close10000(otherBB.Max.Z) || otherBB.Min.Z.Close10000(thisBB.Max.Z))
                if (new RectangleFloats(thisBB.Min.Y, thisBB.Min.X, thisBB.Max.Y - thisBB.Min.Y, thisBB.Max.X - thisBB.Min.X).IntersectsOrAligns(
                    new RectangleFloats(otherBB.Min.Y, otherBB.Min.X, otherBB.Max.Y - otherBB.Min.Y, otherBB.Max.X - otherBB.Min.X)))
                    return true;
            return false;
        }

        public static bool OuterBordersConnect(this BoundingBox thisBB, BoundingBox otherBB)
        {
            if (thisBB.Min.X == otherBB.Max.X)
            {
                if (thisBB.Min.Y == otherBB.Max.Y)
                    return true;
            }
            return false;
        }

        public static bool Close10000(this float f, float number)
        {
            if (Math.Abs(f - number) < 0.0001f)
                return true;
            return false;
        }

        public static Dictionary<Chunk, float> SortByFloat(this Dictionary<Chunk, float> dic)
        {
            Dictionary<Chunk, float> dicCopy = dic;
            Dictionary<Chunk, float> newChunks = new Dictionary<Chunk, float>();
            while (dicCopy.Count > 0)
            {
                float min = float.MaxValue;
                Chunk nearChunk = null;
                for (int i = 0; i < dicCopy.Count; i++)
                    if (dicCopy.ElementAt(i).Value < min)
                    {
                        nearChunk = dicCopy.ElementAt(i).Key;
                        min = dicCopy.ElementAt(i).Value;
                    }
                dicCopy.Remove(nearChunk);
                newChunks.Add(nearChunk, min);
            }
            return newChunks;
        }

        public static bool Contains(this BoundingSphere s, Point3D p)
        {
            return (p.ToVector() - s.Center).Length() < s.Radius;
        }
    }
}
