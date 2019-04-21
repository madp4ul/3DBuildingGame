using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using BlockGame3D.Hud_Elements;

namespace BlockGame3D
{
    class StaticHUD
    {
        public static Texture2D EmptyTex;

        #region Hud-specific variables
        SpriteBatch sb;
        SpriteFont font;
        GraphicsDevice device;
        #endregion
        CrossHair CH;

        public StaticHUD(SpriteBatch sb, SpriteFont font,World inGameData)
        {
            this.device = sb.GraphicsDevice;
            this.sb = sb;
            this.font = font;

            CH = new CrossHair(sb);
        }

        public void Draw()
        {
            sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            CH.Draw();
            //IB.Draw();
            sb.End();
        }
    }
}
