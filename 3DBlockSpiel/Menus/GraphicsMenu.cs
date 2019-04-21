using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using BlockGame3D.Menus.GraphicsMenuOptions;

namespace BlockGame3D.Menus
{
    class GraphicsMenu : Menu
    {
        public static readonly Point[] SupportedResolutions = new Point[]
        {
            new Point(1100,900),
            new Point(0,0),
            new Point(800,600)
        };
        public int ResolutionIndex = 0;

        GraphicsOptions GraphicsOptions;
        public ViewDistances ViewDistance
        {
            get { return GraphicsOptions.ViewDistance; }
            set
            {
                GraphicsOptions.ViewDistance = value;
                this.textButton[2] = "ViewDistance: " + ViewDistanceString();
            }
        }

        public bool Fullscreen
        {
            get { return GraphicsOptions.Fullscreen; }
            set
            {
                GraphicsOptions.Fullscreen = value;
                this.textButton[1] = "Fullscreen: " + FullscreenString();
            }
        }

        public Point Resolution
        {
            get { return GraphicsOptions.Resolution; }
            set
            {
                GraphicsOptions.Resolution = value;
                this.textButton[0] = "Resolution: " + ResolutionString();
            }
        }

        public GraphicsMenu(SpriteBatch sb, SpriteFont font, GraphicsDeviceManager graphics, GraphicsOptions go)
            : base(sb, font, 4)
        {
            this.GraphicsOptions = go;
            this.BackgroundTex = Menu.BackgroundTexture;
            this.ButtonBackgroundTex = Menu.ButtonBackgroundTextures;

            this.Fullscreen = go.Fullscreen;
            this.Resolution = go.Resolution;
            this.ViewDistance = go.ViewDistance;
            if (SupportedResolutions.Any(point => point == Resolution))
            {
                for (int i = 0; i < SupportedResolutions.Length; i++)
                    if (SupportedResolutions[i] == Resolution)
                    {
                        ResolutionIndex = i;
                        break;
                    }
            }
            else
                ResolutionIndex = 1;// must be fullscreen

            this.textButton[0] = "Resolution: " + ResolutionString();
            this.textButton[1] = "Fullscreen: " + FullscreenString();
            this.textButton[2] = "ViewDistance: " + ViewDistanceString();
            this.textButton[3] = "Back";

            SetDrawRectangles();
        }

        private string ResolutionString()
        {
            return Resolution.X + "x" + Resolution.Y;
        }

        private string FullscreenString()
        {
            if (Fullscreen)
                return "On";
            else
                return "Off";
        }

        private string ViewDistanceString()
        {
            if (this.ViewDistance == ViewDistances.Minimum)
                return "Minimum";
            else if (this.ViewDistance == ViewDistances.Low)
                return "Low";
            else if (this.ViewDistance == ViewDistances.Normal)
                return "Normal";
            else if (this.ViewDistance == ViewDistances.High)
                return "High";
            else if (this.ViewDistance == ViewDistances.VeryHigh)
                return "Very High";
            else if (this.ViewDistance == ViewDistances.Extreme)
                return "Extreme";
            else //if (this.ViewDistance == ViewDistances.Maximum)
                return "Maximum";
        }


    }
}
