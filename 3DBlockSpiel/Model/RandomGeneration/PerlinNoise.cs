using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlockGameClasses.ChunkData;
using Microsoft.Xna.Framework;

namespace BlockGameClasses.RandomGeneration
{
   public class PerlinNoise
    {
       public readonly int ChunkSize;
       public int[, ,] CurrentValues { get; set; }

       public PerlinNoise(int chunkSize)
       {
           this.ChunkSize = chunkSize;
       }

       
       /// <summary>
       /// get the noisevalue on the blockindex
       /// CurrentValues needs to be set.
       /// </summary>
       /// <param name="blockIndex">position in chunk</param>
       /// <returns>noise</returns>
       public float GetNoise(Point3D blockIndex)
       {
           //blocks between two lerped informations
           int BlocksBetweenRandoms = this.ChunkSize / (CurrentValues.GetLength(0) - 1);

           //index in random-array
           Point3D minRandomIndex = new Point3D(
               (int)((float)blockIndex.X / BlocksBetweenRandoms),
               (int)((float)blockIndex.Y / BlocksBetweenRandoms),
               (int)((float)blockIndex.Z / BlocksBetweenRandoms));

           //lerped percentages, how many percent the blockindex has of its maxRandomIndex towards the minRandomIndex
           Vector3 percentages = new Vector3(
               (float)(blockIndex.X - (minRandomIndex.X * BlocksBetweenRandoms)) / BlocksBetweenRandoms,
               (float)(blockIndex.Y - (minRandomIndex.Y * BlocksBetweenRandoms)) / BlocksBetweenRandoms,
               (float)(blockIndex.Z - (minRandomIndex.Z * BlocksBetweenRandoms)) / BlocksBetweenRandoms);

           return
                MathHelper.Lerp(//upper with lower
               //lower
                    MathHelper.Lerp(//left with right
               //left
                        MathHelper.Lerp(//front with back
               //back
                        CurrentValues[minRandomIndex.X, minRandomIndex.Y, minRandomIndex.Z],
               //front
                        CurrentValues[minRandomIndex.X, minRandomIndex.Y, minRandomIndex.Z + 1],
                        percentages.Z)
                    ,
               //right
                        MathHelper.Lerp(//front with back
               //back
                        CurrentValues[minRandomIndex.X + 1, minRandomIndex.Y, minRandomIndex.Z],
               //front
                        CurrentValues[minRandomIndex.X + 1, minRandomIndex.Y, minRandomIndex.Z + 1],
                        percentages.Z)
                    , percentages.X)
                ,
               //upper
                    MathHelper.Lerp(//left with right
               //left
                        MathHelper.Lerp(//front with back
               //back
                        CurrentValues[minRandomIndex.X, minRandomIndex.Y + 1, minRandomIndex.Z],
               //front
                        CurrentValues[minRandomIndex.X, minRandomIndex.Y + 1, minRandomIndex.Z + 1],
                        percentages.Z)
                    ,
               //right
                        MathHelper.Lerp(//front with back
               //back
                        CurrentValues[minRandomIndex.X + 1, minRandomIndex.Y + 1, minRandomIndex.Z],
               //front
                        CurrentValues[minRandomIndex.X + 1, minRandomIndex.Y + 1, minRandomIndex.Z + 1],
                        percentages.Z)
                    , percentages.X)
                , percentages.Y);
       }
    }
}
