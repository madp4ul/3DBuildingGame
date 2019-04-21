using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace BlockGame3D.Hud_Elements
{
    class CrossHair
    {
        const int hairWidth = 2;
        const int size = 15;
        Color color = new Color(60, 60, 60);
        SpriteBatch SB;
        GraphicsDevice device;
        readonly Point ScreenMid;
        Rectangle rUp, rLeft;

        public CrossHair(SpriteBatch sb)
        {
            SB = sb;
            device = sb.GraphicsDevice;
            ScreenMid = new Point((int)(this.device.PresentationParameters.BackBufferWidth * 0.5f),
            (int)(this.device.PresentationParameters.BackBufferHeight * 0.5f));
            rUp = new Rectangle(ScreenMid.X - (int)(0.5f * hairWidth), ScreenMid.Y - (int)(0.5f * size),
                hairWidth, size);
            rLeft = new Rectangle(ScreenMid.X - (int)(0.5f * size), ScreenMid.Y - (int)(0.5f * hairWidth),
                size, hairWidth);
        }

        public void Draw()
        {

            BlendState oldBS = SB.GraphicsDevice.BlendState;
            SB.GraphicsDevice.BlendState = BlendState.Additive;
            SB.Draw(StaticHUD.EmptyTex, rUp, color);
            SB.Draw(StaticHUD.EmptyTex, rLeft, color);
            SB.GraphicsDevice.BlendState = oldBS;
        }
    }
}
