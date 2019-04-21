using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using BlockGame3D.Menus.InventoryStates;

namespace BlockGame3D.Menus
{
    class InventoryMenu : Menu
    {
        public static Texture2D InventoryBackgroundTexture;

        public const int DragAndDropSideLength = 50;

        public const int ChangeStateTime = 200;

        public const int RowHeight = 80;
        public const int SlotSideLengthPx = RowHeight;
        public const int SlotMargin = 2;
        public const int IconMargin = 4;
        private const int SeparatorHeight = 3;

        private InventoryShowStates _State;
        public InventoryShowStates State
        {
            get { return _State; }
            set
            {
                if (value != _State)
                {
                    StateChangedTime = DateTime.Now;
                    _State = value;
                    if (value == InventoryShowStates.ShowItembar)
                        this.SelectedSlot = this.SelectedSlot % SlotsPerRow;
                }
            }
        }
        private DateTime StateChangedTime = DateTime.Today;

        public Inventory inventory { get; private set; }

        public readonly int SlotsPerRow;
        public int SlotRows { get; private set; }

        private readonly int ItembarMinPosY;
        private int FullInvMinPosY;
        private int CurrentPos;
        private readonly int Left;

        public int SelectedSlot
        {
            get { return SelectedButton; }
            set
            {
                SelectedButton = value;
                inventory.SelectedSlot = value;
            }
        }

        public InventoryMenu(SpriteBatch sb, SpriteFont font, Inventory inv)
            : base(sb, font, Inventory.Slots)
        {
            BackgroundColor = new Color(100, 100, 100);
            ItemBackgroundDifference = new Color(50, 50, 50);
            ChosenItemDifference = new Color(190, 150, 0);

            this.ButtonBackgroundTex = this.BackgroundTex = InventoryBackgroundTexture;
            this.inventory = inv;

            this.SlotsPerRow = Inventory.BarSlots;
            this.SlotRows = (int)((float)Inventory.Slots / Inventory.BarSlots);

            this.Left = Menu.ScreenResolution.X / 2 - (int)(SlotSideLengthPx * Inventory.BarSlots / 2f);

            ItembarMinPosY = Menu.ScreenResolution.Y - RowHeight;
            FullInvMinPosY = Menu.ScreenResolution.Y - SlotSideLengthPx * SlotRows - SeparatorHeight;
            CurrentPos = ItembarMinPosY;

            SetDrawRectangles(ItembarMinPosY);
            this.SelectedSlot = 0;
        }

        public override void Update()
        {
            this.SlotRows = (int)((float)Inventory.Slots / Inventory.BarSlots);
            //this.FullInvMinPosY = Menu.ScreenResolution.Y - SlotSideLengthPx * SlotRows - SeparatorHeight;

            int mil = (int)StateChangedTime.TimeAgo().TotalMilliseconds;
            if (mil < ChangeStateTime || (CurrentPos != ItembarMinPosY && CurrentPos != FullInvMinPosY))
            {
                float relativePos = (float)mil / ChangeStateTime;
                relativePos = relativePos.Clamp(0, 1);
                int heightDif = ItembarMinPosY - FullInvMinPosY;

                int pos;
                if (State == InventoryShowStates.ShowAll)
                    pos = ItembarMinPosY - (int)(relativePos * heightDif);
                else
                    pos = FullInvMinPosY + (int)(relativePos * heightDif);
                CurrentPos = pos;
                SetDrawRectangles(pos);
            }

        }

        private Rectangle rSeparator;
        private Rectangle[] rIconBackgrounds;

        protected void SetDrawRectangles(int interpolatedHeight)
        {
            Point UpperLeft = new Point(this.Left, interpolatedHeight);

            rectangleBackground = new Rectangle(UpperLeft.X, UpperLeft.Y, SlotsPerRow * SlotSideLengthPx, SlotRows * RowHeight + SlotMargin);
            rSeparator = new Rectangle(UpperLeft.X + SlotMargin, UpperLeft.Y + RowHeight,
                SlotsPerRow * SlotSideLengthPx - SlotMargin * 2, SeparatorHeight);
            rectanglesButton = new Rectangle[Inventory.Slots];
            rIconBackgrounds = new Rectangle[Inventory.Slots];
            for (int i = 0; i < Inventory.Slots; i++)
            {
                int row = (int)((float)i / SlotsPerRow);
                int collumn = i % SlotsPerRow;

                rectanglesButton[i] = new Rectangle(
                    UpperLeft.X + SlotSideLengthPx * collumn + SlotMargin,
                    UpperLeft.Y + SlotMargin + RowHeight * row + (row > 0 ? SeparatorHeight : 0),
                    SlotSideLengthPx - SlotMargin * 2,
                    SlotSideLengthPx - SlotMargin * 2);
                rIconBackgrounds[i] = new Rectangle(
                    UpperLeft.X + SlotSideLengthPx * collumn + SlotMargin + IconMargin,
                    UpperLeft.Y + SlotMargin + IconMargin + RowHeight * row + (row > 0 ? SeparatorHeight : 0),
                    SlotSideLengthPx - (SlotMargin + IconMargin) * 2,
                    SlotSideLengthPx - (SlotMargin + IconMargin) * 2);
            }

        }

        protected override void DrawMore()
        {
            DrawSeparator();
            DrawItemPictures();
            DrawItemCounts();
            DrawDragAndDrop();
        }

        public Block DragAndDropBlock = null;
        public int DragAndDropBlockCount = 0;
        public Point MousePosition;
        private void DrawDragAndDrop()
        {
            if (DragAndDropBlock != null)
            {
                Rectangle screenPosRec = new Rectangle(MousePosition.X, MousePosition.Y, DragAndDropSideLength, DragAndDropSideLength);
                SB.Draw(Block.IconTex, screenPosRec, DragAndDropBlock.IconRectangle, Color.White);
                SB.DrawString(Font, DragAndDropBlockCount + "",
                    new Vector2(
                        MousePosition.X,
                        MousePosition.Y + DragAndDropSideLength - Font.LineSpacing),
                        Color.White);
            }
        }

        private void DrawItemCounts()
        {
            SB.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            for (int i = 0; i < Inventory.Slots; i++)
            {
                if (inventory.itemSlotCount[i] != 0)
                    SB.DrawString(Font, inventory.itemSlotCount[i] + "", new Vector2(rIconBackgrounds[i].X, rIconBackgrounds[i].Y), Color.White);
            }
        }

        private void DrawItemPictures()
        {
            SB.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            for (int i = 0; i < Inventory.Slots; i++)
            {
                if (inventory.itemSlotValues[i] != null)
                    SB.Draw(Block.IconTex, rIconBackgrounds[i], inventory.itemSlotValues[i].IconRectangle, Color.White);
            }
        }

        private void DrawSeparator()
        {
            SB.GraphicsDevice.BlendState = BlendState.Additive;
            SB.Draw(InventoryBackgroundTexture, rSeparator, ChosenItemDifference);
        }
    }
}
