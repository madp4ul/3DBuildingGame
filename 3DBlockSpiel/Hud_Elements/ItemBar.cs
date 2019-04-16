using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace _1st3DGame.Hud_Elements
{
    class ItemBar
    {
        const int Height = 80;
        const int SlotMargin = 2;
        const int IconMargin = 4;
        int Width;
        int ShownItems;

        Color color = new Color(60, 60, 60);
        SpriteBatch SB;
        SpriteFont Font;
        GraphicsDevice device;

        Point UpperLeft;
        BlendState BackgroundBlendstate = new BlendState();
        Color BackgroundColor = new Color(100, 100, 100);
        Color ItemBackgroundDifference = new Color(50, 50, 50);
        Color ChosenItemDifference = new Color(190, 150, 0);
        Rectangle rBackground;
        Rectangle[] rItemSlotBackgrounds;
        Rectangle[] rIconBackgrounds;

        Inventory Items;


        public ItemBar(SpriteBatch sb,SpriteFont font, Inventory inventory)
        {
            ShownItems = Inventory.BarSlots;
            SB = sb;
            Font = font;
            device = sb.GraphicsDevice;
            BackgroundBlendstate.ColorBlendFunction = BlendFunction.ReverseSubtract;
            BackgroundBlendstate.ColorSourceBlend = Blend.One;
            BackgroundBlendstate.ColorDestinationBlend = Blend.One;

            Width = Height * Inventory.BarSlots;
            Point LowestScreenMid = new Point(device.PresentationParameters.BackBufferWidth / 2, device.PresentationParameters.BackBufferHeight);
            UpperLeft = LowestScreenMid.Subtract(new Point((int)(Width / 2.0f), Height));
            rBackground = new Rectangle(UpperLeft.X, UpperLeft.Y, Width, Height);
            rItemSlotBackgrounds = new Rectangle[Inventory.BarSlots];
            rIconBackgrounds = new Rectangle[Inventory.BarSlots];
            int itemSlotWidth = (int)(((float)Width) / Inventory.BarSlots);
            for (int i = 0; i < Inventory.BarSlots; i++)
            {
                rItemSlotBackgrounds[i] = new Rectangle(UpperLeft.X + itemSlotWidth * i + SlotMargin, UpperLeft.Y + SlotMargin,
                                                    itemSlotWidth - SlotMargin * 2, itemSlotWidth - SlotMargin * 2);
                rIconBackgrounds[i] = new Rectangle(UpperLeft.X + itemSlotWidth * i + SlotMargin + IconMargin,
                    UpperLeft.Y + SlotMargin + IconMargin, itemSlotWidth - (SlotMargin + IconMargin) * 2, itemSlotWidth - (SlotMargin + IconMargin) * 2);

            }

            Items = inventory;
        }

        BlendState OldBS;
        public void Draw()
        {
            OldBS = SB.GraphicsDevice.BlendState;

            DrawBackground();
            DrawItemBackGrounds();
            DrawItemPictures();
            DrawItemCounts();

            SB.GraphicsDevice.BlendState = OldBS;
        }

        private void DrawItemCounts()
        {
            SB.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            for (int i = 0; i < ShownItems; i++)
            {
                if (Items.itemSlotCount[i] != 0)
                    SB.DrawString(Font, Items.itemSlotCount[i]+"", new Vector2(rIconBackgrounds[i].X, rIconBackgrounds[i].Y),Color.White);
            }
        }

        private void DrawItemPictures()
        {
            SB.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            for (int i = 0; i < ShownItems; i++)
            {
                if (Items.itemSlotValues[i] != null)
                    SB.Draw(Block.IconTex, rIconBackgrounds[i], Items.itemSlotValues[i].IconRectangle, Color.White);
            }
        }

        private void DrawItemBackGrounds()
        {
            SB.GraphicsDevice.BlendState = BlendState.Additive;
            for (int i = 0; i < ShownItems; i++)
            {
                if(i == Items.SelectedSlot)
                    SB.Draw(StaticHUD.EmptyTex, rItemSlotBackgrounds[i], ChosenItemDifference);
                else
                    SB.Draw(StaticHUD.EmptyTex, rItemSlotBackgrounds[i], ItemBackgroundDifference);
            }
        }

        private void DrawBackground()
        {
            SB.GraphicsDevice.BlendState = BackgroundBlendstate;
            SB.Draw(StaticHUD.EmptyTex, rBackground, BackgroundColor);
        }
    }
}
