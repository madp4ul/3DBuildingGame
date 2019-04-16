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
    abstract class Menu
    {
        public static Texture2D BackgroundTexture;
        public static Texture2D ButtonBackgroundTextures;
        public static Color FontColor { get; private set; }

        public static Point ScreenResolution;

        private readonly Point ScreenMid;

        public readonly int ButtonCount;
        protected readonly int ButtonMargin;
        protected const float ButtonAspectRatio = 7f;
        protected const float ScreenHeightButtonHeightRatio = 12f;
        protected const float ButtonHeightFontHeightRatio = 1.3f;

        protected Color BackgroundColor = new Color(100, 100, 100);
        protected Color ItemBackgroundDifference = new Color(100, 100, 100);
        protected Color ChosenItemDifference = new Color(255, 255, 255);

        protected SpriteBatch SB { get; private set; }
        protected SpriteFont Font { get; private set; }
        protected GraphicsDevice device { get; private set; }
        protected BlendState BackgroundBlendstate { get; private set; }

        protected Texture2D BackgroundTex;
        protected Texture2D ButtonBackgroundTex;

        public readonly Point ButtonSize;
        public Rectangle rectangleBackground { get; protected set; }
        public Rectangle[] rectanglesButton { get; protected set; }
        public string[] textButton { get; protected set; }
        public Rectangle? SelectedButtonRectangle
        {
            get
            {
                if (SelectedButton > 0 && SelectedButton < ButtonCount)
                    return rectanglesButton[SelectedButton];
                else
                    return null;
            }
        }

        private int _SelectedButton = -1;
        public int SelectedButton
        {
            get { return _SelectedButton; }
            set { _SelectedButton = value; }
        }

        public Menu(SpriteBatch sb, SpriteFont font, int buttonCount)
        {
            this.SB = sb;
            this.Font = font;
            this.device = sb.GraphicsDevice;
            this.ButtonCount = buttonCount;
            this.textButton = new string[buttonCount];
            FontColor = Color.White;


            this.ScreenMid = new Point(ScreenResolution.X / 2, ScreenResolution.Y / 2);

            this.BackgroundBlendstate = new BlendState();
            this.BackgroundBlendstate.ColorBlendFunction = BlendFunction.ReverseSubtract;
            this.BackgroundBlendstate.ColorSourceBlend = Blend.One;
            this.BackgroundBlendstate.ColorDestinationBlend = Blend.One;

            int buttonHeight = (int)(ScreenResolution.Y / ScreenHeightButtonHeightRatio);
            ButtonSize = new Point((int)(buttonHeight * ButtonAspectRatio), buttonHeight);
            this.ButtonMargin = (int)(ButtonSize.Y * 0.7f);
        }

        public virtual void Update()
        { }

        protected virtual void SetDrawRectangles()
        {
            this.rectangleBackground = new Rectangle(0, 0, ScreenResolution.X, ScreenResolution.Y);
            this.rectanglesButton = new Rectangle[ButtonCount];

            int topButtonPos = ScreenMid.Y - (int)((ButtonCount * ButtonSize.Y + (ButtonCount - 1) * ButtonMargin) / 2f);
            for (int i = 0; i < ButtonCount; i++)
            {
                rectanglesButton[i] = new Rectangle(
                    ScreenMid.X - (int)(0.5f * ButtonSize.X),
                    topButtonPos + i * (ButtonSize.Y + ButtonMargin),
                    ButtonSize.X, ButtonSize.Y);
            }
        }

        public void Draw()
        {
            BlendState OldBS;

            OldBS = SB.GraphicsDevice.BlendState;

            SB.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            DrawBackground();
            DrawButtonBackGrounds();
            DrawMore();
            DrawButtonTexts();
            SB.End();

            SB.GraphicsDevice.BlendState = OldBS;
        }

        protected virtual void DrawMore()
        { }

        private void DrawBackground()
        {
            SB.GraphicsDevice.BlendState = BackgroundBlendstate;
            SB.Draw(BackgroundTex, rectangleBackground, BackgroundColor);
        }

        private void DrawButtonBackGrounds()
        {
            SB.GraphicsDevice.BlendState = BlendState.Additive;
            for (int i = 0; i < rectanglesButton.Length; i++)
            {
                if (i == SelectedButton)
                    SB.Draw(ButtonBackgroundTex, rectanglesButton[i], ChosenItemDifference);
                else
                    SB.Draw(ButtonBackgroundTex, rectanglesButton[i], ItemBackgroundDifference);
            }
        }

        private void DrawButtonTexts()
        {
            for (int i = 0; i < textButton.Length; i++)
                if (this.textButton[i] != null)
                {
                    int fontHeight = Font.LineSpacing;
                    float scale = ((float)ButtonSize.Y / fontHeight) / ButtonHeightFontHeightRatio;
                    Vector2 stringSize = Font.MeasureString(this.textButton[i]);
                    Vector2 buttonCenter = new Vector2(
                        rectanglesButton[i].X + rectanglesButton[i].Width / 2f,
                        rectanglesButton[i].Y + rectanglesButton[i].Height / 2f);

                    SB.DrawString(Font, this.textButton[i], buttonCenter,
                        FontColor, 0, 0.5f * stringSize, scale, SpriteEffects.None, 0);
                }
        }
    }
}
