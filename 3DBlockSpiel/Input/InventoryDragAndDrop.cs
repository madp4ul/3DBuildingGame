using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace _1st3DGame.Input
{
    static class InventoryDragAndDrop
    {
        private const float ItemPickupAcceleration = 80f;
        private const float MaxPickupMouseMovement = 2f;

        private static Inventory Inv;
        private static Menus.InventoryMenu Hud;

        private static MouseState MouseInput;
        private static Point MousePosition;
        private static Point MouseDifference;
        private static float MouseDifferenceLength;

        private static bool ItemOnMouse = false;
        private static int? OriginItemSlot = null;

        private static int LMBPressed = 0;
        private static int LMBPressedOnSlot = 0;
        private static int TicksSinceItemInkrement = 0;
        private static int RMBPressed = 0;

        public static void SetData(DataContainer data)
        {
            Inv = data.InGame.Player1.inventory;
            Hud = (Menus.InventoryMenu)data.Menus[DisplayMode.Inventory];
        }


        public static void Update(MouseState ms, Point mouseDif)
        {
            MouseInput = ms;
            MousePosition = new Point(ms.X, ms.Y);
            MouseDifference = mouseDif;
            MouseDifferenceLength = (float)Math.Sqrt(MouseDifference.X * MouseDifference.X + MouseDifference.Y * MouseDifference.Y);

            LMB();
            RMB();
        }



        private static void LMB()
        {
            //If leftmousebutton pressed
            if (MouseInput.LeftButton == ButtonState.Pressed)
            {
                //If lmb was pressed before
                if (LMBPressed > 0)
                {
                    int? inventoryIndex = GetInventorySlotIndex();
                    if (inventoryIndex != null && inventoryIndex == OriginItemSlot) //if mouse is on the same spot it was when taking an item
                    {
                        float interval = ItemPickupAcceleration / ((float)Math.Sqrt(LMBPressedOnSlot) + 1);

                        if (MouseDifferenceLength < MaxPickupMouseMovement)//if mouse has not moved too much
                        {
                            //ticks get counted, the period of ticks the code goes in this get shorter and shorter
                            if (TicksSinceItemInkrement >= (int)interval)
                            {
                                TicksSinceItemInkrement = 0;
                                int takenCount = 0;
                                Block b = Inv.GetItemAt(inventoryIndex.Value, 1, out takenCount);
                                //If Block was selected
                                if (b != null)
                                    Hud.DragAndDropBlockCount++;//add one block to mouse
                            }
                            else
                                TicksSinceItemInkrement++;
                        }
                        else
                        {
                            TicksSinceItemInkrement = 0;
                            LMBPressedOnSlot = 0;
                        }
                        LMBPressedOnSlot++;
                    }
                    else //if lmb still pressed but is not on the slot anymore
                        LMBPressedOnSlot = 0;
                }
                //if lmb pressed for the first time
                else
                {
                    int? inventoryIndex = GetInventorySlotIndex();
                    if (inventoryIndex.HasValue && !ItemOnMouse)
                    {
                        int takenCount = 0;
                        Block b = Inv.GetItemAt(inventoryIndex.Value, 1, out takenCount);
                        //If Block was selected
                        if (b != null)
                            SetDraggedItem(b, 1, inventoryIndex.Value);
                        LMBPressedOnSlot++;
                    }
                }

                LMBPressed++;
            }
            else
            {
                //If lmb was released
                if (LMBPressed > 0)
                {
                    int? inventoryIndex = GetInventorySlotIndex();
                    if (ItemOnMouse && Hud.DragAndDropBlock != null)
                    {
                        if (inventoryIndex.HasValue) // if mouse is over slot
                        {
                            bool success = Inv.Add(Hud.DragAndDropBlock,
                                Hud.DragAndDropBlockCount,
                                inventoryIndex.Value);
                            if (success) //if valid targetslot
                                SetDraggedItemToNull(Hud);
                            else //if targetslot was invalid
                            {
                                bool successOriginSpot = Inv.Add(Hud.DragAndDropBlock, Hud.DragAndDropBlockCount, OriginItemSlot.Value);
                                if (successOriginSpot) //if reset to originspot was successful
                                    SetDraggedItemToNull(Hud);
                            }
                        }
                        else // if mouse is not over slot
                        {
                            bool successOriginSpot = Inv.Add(Hud.DragAndDropBlock, Hud.DragAndDropBlockCount, OriginItemSlot.Value);
                            if (successOriginSpot) //if reset to originspot was succesful
                                SetDraggedItemToNull(Hud);
                        }
                    }
                    TicksSinceItemInkrement = 0;
                    LMBPressedOnSlot = 0;
                    LMBPressed = 0;
                }
                // else: lmb was not pressed at all!
            }
        }

        private static void RMB()
        {
            //If rightmousebutton pressed
            if (MouseInput.RightButton == ButtonState.Pressed)
            {
                //if rmb pressed for the first time
                if (RMBPressed == 0)
                {
                    int? inventoryIndex = GetInventorySlotIndex();
                    if (inventoryIndex.HasValue && !ItemOnMouse) //if mouse over slot and no DragAndDrop yet
                    {
                        int count = Inv.itemSlotCount[inventoryIndex.Value];
                        int takenCount = 0;
                        Block b = Inv.GetItemAt(inventoryIndex.Value, count, out takenCount);
                        //If Block was selected
                        if (b != null && count > 0)
                            SetDraggedItem(b, count, inventoryIndex.Value);

                    }
                }
                //else rmb pressed before

                RMBPressed++;
            }
            else
            {
                //If rmb was released
                if (RMBPressed > 0)
                {
                    int? inventoryIndex = GetInventorySlotIndex();
                    if (ItemOnMouse && Hud.DragAndDropBlock != null)
                    {
                        if ((inventoryIndex.HasValue))// mouse over slot
                        {
                            //release blocks
                            bool success = Inv.Add(Hud.DragAndDropBlock,
                                Hud.DragAndDropBlockCount,
                                inventoryIndex.Value);
                            if (success) //if valid targetslot
                                SetDraggedItemToNull(Hud);
                            else //if targetslot was invalid
                            {
                                bool successOriginSpot = Inv.Add(Hud.DragAndDropBlock, Hud.DragAndDropBlockCount, OriginItemSlot.Value);
                                if (successOriginSpot) //if reset to originspot was succesful
                                    SetDraggedItemToNull(Hud);
                            }
                        }
                        else // mouse is not over slot
                        {
                            bool successOriginSpot = Inv.Add(Hud.DragAndDropBlock, Hud.DragAndDropBlockCount, OriginItemSlot.Value);
                            if (successOriginSpot) //if reset to originspot was succesful
                                SetDraggedItemToNull(Hud);
                        }
                    }
                    RMBPressed = 0;
                }
                // else: rmb was not pressed at all!
            }
        }

        /// <summary>
        /// set amount of specific blocks to mouse
        /// </summary>
        /// <param name="b">block</param>
        /// <param name="blockCount">count of blocks</param>
        /// <param name="originalItemSlotIndex">index of itemslot the block came from</param>
        private static void SetDraggedItem(Block b, int blockCount, int originalItemSlotIndex)
        {
            if (b == null)
                throw new Exception("the block object cannot be null");
            Hud.DragAndDropBlock = b;
            Hud.DragAndDropBlockCount = blockCount;
            OriginItemSlot = originalItemSlotIndex;
            ItemOnMouse = true;
        }

        private static void SetDraggedItemToNull(Menus.InventoryMenu hud)
        {
            hud.DragAndDropBlock = null;
            hud.DragAndDropBlockCount = 0;
            OriginItemSlot = null;
            ItemOnMouse = false;
        }

        private static int? GetInventorySlotIndex()
        {
            int? slotIndex = null;
            for (int i = 0; i < Hud.rectanglesButton.Length; i++)
                if (Hud.rectanglesButton[i].Contains(MousePosition))
                {
                    slotIndex = i;
                    break;
                }
            return slotIndex;
        }
    }
}
