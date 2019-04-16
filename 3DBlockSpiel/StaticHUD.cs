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
    class StaticHUD
    {
        SpriteBatch sb;
        SpriteFont font;
        GraphicsDevice device;
        Texture2D emptyTex;

        public StaticHUD(SpriteBatch sb, SpriteFont font)
        {
            this.device = sb.GraphicsDevice;
            this.sb = sb;
            this.font = font;

            this.emptyTex = new Texture2D(device, 1, 1);
            this.emptyTex.SetData<Color>(new Color[1] { Color.White });
        }

        public void Draw()
        {
            sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            DrawCrossHair();
            sb.End();
        }


        private void DrawCrossHair()
        {
            const int hairWidth = 2;
            const int size = 15;
            Color color = new Color(60, 60, 60);

            BlendState oldBS = sb.GraphicsDevice.BlendState;
            sb.GraphicsDevice.BlendState = BlendState.Additive;
            int x = (int)(this.device.PresentationParameters.BackBufferWidth*0.5f);
            int y = (int)(this.device.PresentationParameters.BackBufferHeight*0.5f);
            Rectangle rUp = new Rectangle(x - (int)(0.5f * hairWidth) , y - (int)(0.5f * size),
                hairWidth, size);
            Rectangle rLeft = new Rectangle(x - (int)(0.5f * size), y - (int)(0.5f * hairWidth),
                size, hairWidth);

            sb.Draw(emptyTex, rUp, color);
            sb.Draw(emptyTex, rLeft, color);
            sb.GraphicsDevice.BlendState = oldBS;
        }

    }
}
