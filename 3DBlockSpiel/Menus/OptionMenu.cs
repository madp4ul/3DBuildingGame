using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace _1st3DGame.Menus
{
    class OptionMenu : Menu
    {
        public OptionMenu(SpriteBatch sb, SpriteFont font)
            : base(sb, font, 3)
        {
            this.BackgroundTex = Menu.BackgroundTexture;
            this.ButtonBackgroundTex = Menu.ButtonBackgroundTextures;

            this.textButton[0] = "Graphics";
            this.textButton[1] = "Controls";
            this.textButton[2] = "Back";

            SetDrawRectangles();
        }
    }
}
