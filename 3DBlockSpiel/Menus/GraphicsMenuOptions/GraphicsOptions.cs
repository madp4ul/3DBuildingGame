using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace _1st3DGame.Menus.GraphicsMenuOptions
{
    public class GraphicsOptions
    {
        public ViewDistances ViewDistance;
        public Point Resolution;
        public bool Fullscreen;

        protected GraphicsOptions() { }

        public static GraphicsOptions Default
        {
            get
            {
                return new GraphicsOptions()
                    {
                        ViewDistance = ViewDistances.Normal,
                        Resolution = new Point(1024, 768),
                        Fullscreen = false
                    };
            }
        }
    }
}
