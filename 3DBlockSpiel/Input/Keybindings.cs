using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace _1st3DGame.Input
{
    public class Keybindings
    {
        #region WorldInput

        public float WorldMouseSensitivity;
        //Move
        public Keys WorldWalkForward, WorldWalkLeft, WorldWalkRight, WorldWalkBackward, WorldJump;
        //ChangeMode
        public Keys WorldOpenInventory, WorldOpenMenu;
        //Select Itembar-Slot
        public Keys WorldSelectItembarSlot0, WorldSelectItembarSlot1, WorldSelectItembarSlot2,
            WorldSelectItembarSlot3, WorldSelectItembarSlot4, WorldSelectItembarSlot5,
            WorldSelectItembarSlot6, WorldSelectItembarSlot7, WorldSelectItembarSlot8;

        internal readonly Keys WorldDebugOnOff, WorldDebugNextCam, WorldDebugLastCam;

        #endregion
        #region MenuInput
        //ChangeMode

        internal readonly Keys MenuUp, MenuDown, MenuSelect;
        #endregion

        protected Keybindings()
        {
            WorldMouseSensitivity = 0.06f;
            WorldWalkForward = Keys.W;
            WorldWalkLeft = Keys.A;
            WorldWalkRight = Keys.D;
            WorldWalkBackward = Keys.S;
            WorldJump = Keys.Space;
            WorldOpenInventory = Keys.Tab;
            WorldOpenMenu = Keys.Escape;

            WorldSelectItembarSlot0 = Keys.D1;
            WorldSelectItembarSlot1 = Keys.D2;
            WorldSelectItembarSlot2 = Keys.D3;
            WorldSelectItembarSlot3 = Keys.D4;
            WorldSelectItembarSlot4 = Keys.D5;
            WorldSelectItembarSlot5 = Keys.D6;
            WorldSelectItembarSlot6 = Keys.D7;
            WorldSelectItembarSlot7 = Keys.D8;
            WorldSelectItembarSlot8 = Keys.D9;

            WorldDebugOnOff = Keys.F2;
            WorldDebugNextCam = Keys.PageUp;
            WorldDebugLastCam = Keys.PageDown;

            MenuSelect = Keys.Enter;
            MenuDown = WorldWalkBackward;
            MenuUp = WorldWalkForward;

        }

        public static Keybindings Default
        {
            get
            {
                return new Keybindings();
            }
        }
    }
}
