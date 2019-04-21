using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlockGame3D
{
    class Inventory
    {
        public const int BarSlots = 9;
        public const int Slots = BarSlots * 4;

        public Block[] itemSlotValues { get; private set; }
        public int[] itemSlotCount { get; private set; }

        private int _SelectedSlot;
        public int SelectedSlot
        {
            get { return _SelectedSlot; }
            set
            {
                if (value >= 0)
                    _SelectedSlot = value % Slots;
                else
                {
                    int multiplier = (int)((float)value / Slots) - 1;
                    _SelectedSlot = value - multiplier * Slots;
                }
            }
        }

        public Inventory()
        {
            SelectedSlot = 0;
            this.itemSlotValues = new Block[Slots];
            this.itemSlotCount = new int[Slots];
        }

        /// <summary>
        /// Adds block to inventory
        /// </summary>
        /// <param name="block"></param>
        /// <returns>success or not</returns>
        public bool Add(Block block, int itemCount, int slotIndex)
        {
            if (block == null ||
                slotIndex < 0 ||
                slotIndex >= Slots ||
                (itemSlotValues[slotIndex] != null && itemSlotValues[slotIndex].Type != block.Type))
                return false;
            else
            {
                if (itemSlotValues[slotIndex] == null)
                    itemSlotValues[slotIndex] = block;
                itemSlotCount[slotIndex] += itemCount;
                return true;
            }
        }

        public bool Add(Block block)
        {
            int? sameItemSlot = GetSlotNumber(block);
            //If no slot with this item exists
            if (sameItemSlot == null)
            {
                int? firstEmptySlot = FirstEmptySlotNumber();
                //Inventory is full
                if (firstEmptySlot == null)
                    return false;
                //Inventory is not full
                else
                {
                    this.itemSlotValues[(int)firstEmptySlot] = block;
                    this.itemSlotCount[(int)firstEmptySlot]++;
                    return true;
                }
            }
            //if slot with this item exists
            else
            {
                this.itemSlotCount[(int)sameItemSlot]++;
                return true;
            }
        }


        public Block GetItemAt(int slot, int count, out int returnedCount)
        {
            returnedCount = count;
            if (returnedCount > itemSlotCount[slot])
                returnedCount = itemSlotCount[slot];

            if ( returnedCount > 0)
            {
                itemSlotCount[slot] -= returnedCount;
                Block b = (Block)itemSlotValues[slot].Clone();
                if (itemSlotCount[slot] == 0)
                    itemSlotValues[slot] = null;
                return b;
            }
            return null;
        }

        public Block GetOneItemAtSelectedSlot()
        {
            int i = 0;
            return GetItemAt(SelectedSlot, 1,out i);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="block"></param>
        /// <returns>null if there is no slot</returns>
        private int? GetSlotNumber(Block block)
        {
            for (int i = 0; i < this.itemSlotValues.Length; i++)
                if (this.itemSlotValues[i] != null)
                    if (this.itemSlotValues[i].Type == block.Type)
                        return i;
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>null if there is no empty slot</returns>
        private int? FirstEmptySlotNumber()
        {
            for (int i = 0; i < this.itemSlotValues.Length; i++)
                if (this.itemSlotValues[i] == null)
                    return i;
            return null;
        }
    }
}
