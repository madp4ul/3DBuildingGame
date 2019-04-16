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

        /// <summary>
        /// Returns (X|Z) of inputvector
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector2 ToVector2D(this Vector3 v)
        {
            return new Vector2(v.X, v.Z);
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

        public static Vector3 Center(this BoundingBox bbox)
        {
            return bbox.Min + 0.5f * (bbox.Max - bbox.Min);
        }

        public static bool Align(this BoundingBox thisBB, BoundingBox otherBB)
        {
            if (thisBB.Min.X.Close1000(otherBB.Max.X) || otherBB.Min.X.Close1000(thisBB.Max.X))
                if (new RectangleFloats(thisBB.Min.Y, thisBB.Min.Z, thisBB.Max.Y - thisBB.Min.Y, thisBB.Max.Z - thisBB.Min.Z).Intersects(
                    new RectangleFloats(otherBB.Min.Y, otherBB.Min.Z, otherBB.Max.Y - otherBB.Min.Y, otherBB.Max.Z - otherBB.Min.Z)))
                    return true;
            if (thisBB.Min.Y.Close1000(otherBB.Max.Y) || otherBB.Min.Y.Close1000(thisBB.Max.Y))
                if (new RectangleFloats(thisBB.Min.X, thisBB.Min.Z, thisBB.Max.X - thisBB.Min.X, thisBB.Max.Z - thisBB.Min.Z).Intersects(
                    new RectangleFloats(otherBB.Min.X, otherBB.Min.Z, otherBB.Max.X - otherBB.Min.X, otherBB.Max.Z - otherBB.Min.Z)))
                    return true;
            if (thisBB.Min.Z.Close1000(otherBB.Max.Z) || otherBB.Min.Z.Close1000(thisBB.Max.Z))
                if (new RectangleFloats(thisBB.Min.Y, thisBB.Min.X, thisBB.Max.Y - thisBB.Min.Y, thisBB.Max.X - thisBB.Min.X).Intersects(
                    new RectangleFloats(otherBB.Min.Y, otherBB.Min.X, otherBB.Max.Y - otherBB.Min.Y, otherBB.Max.X - otherBB.Min.X)))
                    return true;
            return false;
        }

        public static bool Close1000(this float f, float number)
        {
            if (Math.Abs(f - number) < 0.001f)
                return true;
            return false;
        }

        public static Point3D ToPoint(this Vector3 v)
        {
            return new Point3D((int)v.X, (int)v.Y, (int)v.Z);
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
