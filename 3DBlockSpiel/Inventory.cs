using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace _1st3DGame
{
    class Inventory
    {
        private const int Slots = 20;

        public Block[] itemSlotValues { get; private set; }
        public int[] itemSlotCount { get; private set; }

        public Inventory()
        {
            this.itemSlotValues = new Block[Slots];
            this.itemSlotCount = new int[Slots];
        }

        /// <summary>
        /// Adds block to inventory
        /// </summary>
        /// <param name="block"></param>
        /// <returns>success or not</returns>
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

        public Block GetItemAt(int slot)
        {
            if (itemSlotCount[slot] > 0)
            {
                itemSlotCount[slot]--;
                Block b = (Block)itemSlotValues[slot].Clone();
                if (itemSlotCount[slot] == 0)
                    itemSlotValues[slot] = null;
                return b;
            }
            return null;
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
