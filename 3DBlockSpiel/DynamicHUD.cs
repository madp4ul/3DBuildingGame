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
    class DynamicHUD
    {
        SpriteBatch sb;
        SpriteFont font;
        GraphicsDevice device;
        Texture2D emptyTex;

        public DynamicHUD(SpriteBatch sb, SpriteFont font)
        {
            this.device = sb.GraphicsDevice;
            this.sb = sb;
            this.font = font;

            this.emptyTex = new Texture2D(device, 1, 1);
            this.emptyTex.SetData<Color>(new Color[1] { Color.White });
        }

        public void Update()
        {

        }

        public void Draw()
        {
            sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            sb.End();
        }
    }
}
