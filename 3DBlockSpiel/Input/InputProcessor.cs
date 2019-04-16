using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using _1st3DGame.Menus;

namespace _1st3DGame.Input
{
    static class InputProcessor
    {
        private const int DefaultKeyTimeOut = 100;
        /// <summary>
        /// bigger value means slower acc
        /// </summary>

        private static Game1 Game;

        private static KeyboardState KeyboardInput;
        private static MouseState MouseInput;

        private static Point MousePosition;
        private static Point MouseDifference;

        private static Point ScreenMid;
        /// <summary>
        /// Mousewheel difference since start of the game
        /// </summary>
        private static int MousewheelPosition;
        /// <summary>
        /// Mousewheeldifference between to updates
        /// </summary>
        private static int MousewheelDifference;
        private static float MouseDifferenceLength;

        private static bool LMBDownLastFrame = false;
        private static bool RMBDownLastFrame = false;
        static Keybindings Keybindings;

        public static void SetData(Game1 game, KeyboardState ks, MouseState ms, Point windowSize, Keybindings kb)
        {
            Game = game;
            InventoryDragAndDrop.SetData(Game.GameData);

            Keybindings = kb;

            MousePosition = new Point(ms.X, ms.Y);

            ScreenMid = new Point(windowSize.X / 2, windowSize.Y / 2);
        }

        private static void PrepareInternalData(KeyboardState ks, MouseState ms)
        {
            //stuff about last frame
            LMBDownLastFrame = MouseInput.LeftButton == ButtonState.Pressed;
            RMBDownLastFrame = MouseInput.RightButton == ButtonState.Pressed;

            //refresh to current frame
            KeyboardInput = ks;
            MouseInput = ms;

            //stuff about current frame
            Point newMousePosition = new Point(MouseInput.X, MouseInput.Y);
            MouseDifference = MousePosition.Subtract(newMousePosition);
            MousePosition = newMousePosition;
            MouseDifferenceLength = (float)Math.Sqrt(MouseDifference.X * MouseDifference.X + MouseDifference.Y * MouseDifference.Y);

            int newMousewheelPosition = ms.ScrollWheelValue;
            MousewheelDifference = newMousewheelPosition - MousewheelPosition;
            MousewheelPosition = newMousewheelPosition;
        }

        public static void UpdateInput(KeyboardState ks, MouseState ms)
        {
            PrepareInternalData(ks, ms);

            if (Game.GameData.GameDislayMode == DisplayMode.World)
                WorldInput();
            else if (Game.GameData.GameDislayMode == DisplayMode.Inventory)
                InventoryInput();
            else if (Game.GameData.GameDislayMode == DisplayMode.PauseMenu)
                PauseMenuInput();
            else if (Game.GameData.GameDislayMode == DisplayMode.OptionMenu)
                OptionsMenuInput();
            else if (Game.GameData.GameDislayMode == DisplayMode.GraphicsMenu)
                GraphicsMenuInput();

        }

        #region WorldInput
        private static void WorldInput()
        {
            SetSelectedItembarSlot();
            ScrollSelectedItemBarSlot();
            WalkPlayer();
            LookAroundPlayer();
            InteractWithWorld();
            ChangeDebugOptions();
            ChangeDisplayModeFromWorld();
        }

        private static void LookAroundPlayer()
        {
            float rotLeft = MouseDifference.X * Keybindings.WorldMouseSensitivity / 60;
            float rotUp = MouseDifference.Y * Keybindings.WorldMouseSensitivity / 60;

            if (!(rotLeft == 0 && rotUp == 0))
                Game.GameData.InGame.Player1.LookAround(rotUp, rotLeft);

            Mouse.SetPosition(ScreenMid.X, ScreenMid.Y);
            MousePosition = ScreenMid;

            //Mouse.SetPosition(MousePosition.X + MouseDifference.X, MousePosition.Y + MouseDifference.Y);
        }

        private static void WalkPlayer()
        {
            /////// Walk
            //Forward
            if (KeyboardInput.IsKeyDown(Keybindings.WorldWalkForward) &&
                !KeyboardInput.IsKeyDown(Keybindings.WorldWalkBackward))
                Game.GameData.InGame.Player1.MoveInput(MoveDirection.Forward);

            //Or Backward
            else if (KeyboardInput.IsKeyDown(Keybindings.WorldWalkBackward) &&
                !KeyboardInput.IsKeyDown(Keybindings.WorldWalkForward))
                Game.GameData.InGame.Player1.MoveInput(MoveDirection.Backwards);

            //Left
            if (KeyboardInput.IsKeyDown(Keybindings.WorldWalkLeft) &&
                !KeyboardInput.IsKeyDown(Keybindings.WorldWalkRight))
                Game.GameData.InGame.Player1.MoveInput(MoveDirection.Left);

            //Or Right
            else if (KeyboardInput.IsKeyDown(Keybindings.WorldWalkRight) &&
                !KeyboardInput.IsKeyDown(Keybindings.WorldWalkLeft))
                Game.GameData.InGame.Player1.MoveInput(MoveDirection.Right);

            if (KeyboardInput.IsKeyDown(Keybindings.WorldJump))
                Game.GameData.InGame.Player1.MoveInput(MoveDirection.Jump);

        }

        private static void InteractWithWorld()
        {
            Game.GameData.InGame.Player1.Digging = MouseInput.LeftButton == ButtonState.Pressed;
            Game.GameData.InGame.Player1.Building = MouseInput.RightButton == ButtonState.Pressed;
        }

        private static void SetSelectedItembarSlot()
        {
            if (KeyboardInput.IsKeyDown(Keybindings.WorldSelectItembarSlot0))
                ((Menus.InventoryMenu)Game.GameData.Menus[DisplayMode.Inventory]).SelectedSlot = 0;
            else if (KeyboardInput.IsKeyDown(Keybindings.WorldSelectItembarSlot1))
                ((Menus.InventoryMenu)Game.GameData.Menus[DisplayMode.Inventory]).SelectedSlot = 1;
            else if (KeyboardInput.IsKeyDown(Keybindings.WorldSelectItembarSlot2))
                ((Menus.InventoryMenu)Game.GameData.Menus[DisplayMode.Inventory]).SelectedSlot = 2;
            else if (KeyboardInput.IsKeyDown(Keybindings.WorldSelectItembarSlot3))
                ((Menus.InventoryMenu)Game.GameData.Menus[DisplayMode.Inventory]).SelectedSlot = 3;
            else if (KeyboardInput.IsKeyDown(Keybindings.WorldSelectItembarSlot4))
                ((Menus.InventoryMenu)Game.GameData.Menus[DisplayMode.Inventory]).SelectedSlot = 4;
            else if (KeyboardInput.IsKeyDown(Keybindings.WorldSelectItembarSlot5))
                ((Menus.InventoryMenu)Game.GameData.Menus[DisplayMode.Inventory]).SelectedSlot = 5;
            else if (KeyboardInput.IsKeyDown(Keybindings.WorldSelectItembarSlot6))
                ((Menus.InventoryMenu)Game.GameData.Menus[DisplayMode.Inventory]).SelectedSlot = 6;
            else if (KeyboardInput.IsKeyDown(Keybindings.WorldSelectItembarSlot7))
                ((Menus.InventoryMenu)Game.GameData.Menus[DisplayMode.Inventory]).SelectedSlot = 7;
            else if (KeyboardInput.IsKeyDown(Keybindings.WorldSelectItembarSlot8))
                ((Menus.InventoryMenu)Game.GameData.Menus[DisplayMode.Inventory]).SelectedSlot = 8;
        }

        private static void ScrollSelectedItemBarSlot()
        {
            int posDif = MousewheelDifference > 0 ? 1 : MousewheelDifference < 0 ? -1 : 0;
            if (posDif != 0)
            {
                Menus.InventoryMenu vis = ((Menus.InventoryMenu)Game.GameData.Menus[DisplayMode.Inventory]);
                vis.SelectedSlot = (vis.SelectedSlot - posDif) % Inventory.BarSlots;
            }
            //Data.InGame.Player1.inventory.SelectedSlot = (Data.InGame.Player1.inventory.SelectedSlot - posDif) % Inventory.BarSlots;
        }

        private static bool WorldDebugOnOffLastDown = false;
        private static bool WorldDebugNextCamLastDown = false;
        private static bool WorldDebugLastCamLastDown = false;
        private static void ChangeDebugOptions()
        {
            if (KeyboardInput.IsKeyDown(Keybindings.WorldDebugOnOff) &&
                !WorldDebugOnOffLastDown)
            {
                Game.GameData.InGame.DrawDebugInfo = !Game.GameData.InGame.DrawDebugInfo;
                WorldDebugOnOffLastDown = true;
            }
            else if (WorldDebugOnOffLastDown && KeyboardInput.IsKeyUp(Keybindings.WorldDebugOnOff))
                WorldDebugOnOffLastDown = false;

            if (KeyboardInput.IsKeyDown(Keybindings.WorldDebugNextCam) &&
                !WorldDebugNextCamLastDown)
            {
                Game.GameData.InGame.CamIndex++;
                WorldDebugNextCamLastDown = true;
            }
            else if (WorldDebugNextCamLastDown && KeyboardInput.IsKeyUp(Keybindings.WorldDebugNextCam))
                WorldDebugNextCamLastDown = false;

            if (KeyboardInput.IsKeyDown(Keybindings.WorldDebugLastCam) &&
                !WorldDebugLastCamLastDown)
            {
                Game.GameData.InGame.CamIndex--;
                WorldDebugLastCamLastDown = true;
            }
            else if (WorldDebugLastCamLastDown && KeyboardInput.IsKeyUp(Keybindings.WorldDebugLastCam))
                WorldDebugLastCamLastDown = false;

        }

        private static bool WorldOpenMenuLastDown = false;
        private static bool WorldOpenInventoryLastDown = false;
        private static void ChangeDisplayModeFromWorld()
        {
            if (KeyboardInput.IsKeyDown(Keybindings.WorldOpenMenu) &&
                !WorldOpenMenuLastDown)
            {
                Game.GameData.GameDislayMode = DisplayMode.PauseMenu;
                WorldOpenMenuLastDown = true;
            }
            else if (KeyboardInput.IsKeyUp(Keybindings.WorldOpenMenu) && WorldOpenMenuLastDown)
                WorldOpenMenuLastDown = false;


            if (KeyboardInput.IsKeyDown(Keybindings.WorldOpenInventory) &&
                !WorldOpenInventoryLastDown)
            {
                Game.GameData.GameDislayMode = DisplayMode.Inventory;
                WorldOpenInventoryLastDown = true;
            }
            else if (KeyboardInput.IsKeyUp(Keybindings.WorldOpenInventory) && WorldOpenInventoryLastDown)
                WorldOpenInventoryLastDown = false;
        }
        #endregion

        #region InventoryInput
        private static void InventoryInput()
        {
            SetSelectedItembarSlot();
            ScrollSelectedItemBarSlot();
            //ScrollSelectedInventorySlot();
            InvDragAndDrop();
            WalkPlayer();
            ChangedisplayModeFromInventory();
        }


        private static void InvDragAndDrop()
        {
            Inventory inv = Game.GameData.InGame.Player1.inventory;
            Menus.InventoryMenu vis = ((Menus.InventoryMenu)Game.GameData.Menus[DisplayMode.Inventory]);
            vis.MousePosition = MousePosition;

            InventoryDragAndDrop.Update(MouseInput, MouseDifference);
        }




        private static void ScrollSelectedInventorySlot()
        {
            int posDif = MousewheelDifference > 0 ? 1 : MousewheelDifference < 0 ? -1 : 0;
            if (posDif != 0)
                ((Menus.InventoryMenu)Game.GameData.Menus[DisplayMode.Inventory]).SelectedSlot -= posDif;
        }

        private static void ChangedisplayModeFromInventory()
        {
            if (KeyboardInput.IsKeyDown(Keybindings.WorldOpenMenu) &&
                !WorldOpenMenuLastDown)
            {
                Game.GameData.GameDislayMode = DisplayMode.PauseMenu;
                WorldOpenMenuLastDown = true;
            }
            else if (KeyboardInput.IsKeyUp(Keybindings.WorldOpenMenu) && WorldOpenMenuLastDown)
                WorldOpenMenuLastDown = false;

            if (KeyboardInput.IsKeyDown(Keybindings.WorldOpenInventory) &&
                !WorldOpenInventoryLastDown)
            {
                Game.GameData.GameDislayMode = DisplayMode.World;
                WorldOpenInventoryLastDown = true;
            }
            else if (KeyboardInput.IsKeyUp(Keybindings.WorldOpenInventory) && WorldOpenInventoryLastDown)
                WorldOpenInventoryLastDown = false;
        }

        #endregion

        #region GenaralMenuInput
        private static void MenuInput(Action buttonActions)
        {
            MenuKeyNavigateButtons();
            MenuMouseNavigateButtons();
            MenuSelectButtons(buttonActions);
        }

        private static bool ButtonMouseSelected = false;

        private static bool LeaveButtonLastUp = false;

        private static bool NavigateMenuLastDownDown = false;
        private static bool NavigateMenuLastUpDown = false;
        private static void MenuKeyNavigateButtons()
        {
            if (KeyboardInput.IsKeyDown(Keybindings.MenuDown) &&
                !KeyboardInput.IsKeyDown(Keybindings.MenuUp) &&
                !NavigateMenuLastDownDown)
            {
                Game.GameData.CurrentMenu.SelectedButton++;
                Game.GameData.CurrentMenu.SelectedButton = Game.GameData.CurrentMenu.SelectedButton % Game.GameData.CurrentMenu.ButtonCount;
                NavigateMenuLastDownDown = true;
                ButtonMouseSelected = false;
            }
            else if (KeyboardInput.IsKeyUp(Keybindings.MenuDown) && NavigateMenuLastDownDown)
                NavigateMenuLastDownDown = false;

            if (KeyboardInput.IsKeyDown(Keybindings.MenuUp) &&
                !KeyboardInput.IsKeyDown(Keybindings.MenuDown) &&
                !NavigateMenuLastUpDown)
            {

                if (Game.GameData.CurrentMenu.SelectedButton <= 0)
                    Game.GameData.CurrentMenu.SelectedButton += Game.GameData.CurrentMenu.ButtonCount - Game.GameData.CurrentMenu.SelectedButton;

                Game.GameData.CurrentMenu.SelectedButton--;
                Game.GameData.CurrentMenu.SelectedButton = Game.GameData.CurrentMenu.SelectedButton % Game.GameData.CurrentMenu.ButtonCount;

                NavigateMenuLastUpDown = true;
                ButtonMouseSelected = false;
            }
            else if (KeyboardInput.IsKeyUp(Keybindings.MenuUp) && NavigateMenuLastUpDown)
                NavigateMenuLastUpDown = false;

        }

        private static void MenuMouseNavigateButtons()
        {
            if (MouseDifferenceLength > 0 && !LMBDownLastFrame)// if mouse moved
            {
                bool foundButton = false;
                for (int i = 0; i < Game.GameData.CurrentMenu.rectanglesButton.Length; i++)
                    if (Game.GameData.CurrentMenu.rectanglesButton[i].Contains(MousePosition))
                    {
                        Game.GameData.CurrentMenu.SelectedButton = i;
                        ButtonMouseSelected = true;
                        foundButton = true;
                        break;
                    }
                if (!foundButton && ButtonMouseSelected)
                {
                    ButtonMouseSelected = false;
                    Game.GameData.CurrentMenu.SelectedButton = -1;
                }
            }
        }

        private static void MenuSelectButtons(Action ButtonAction)
        {
            bool modeChanged = false;

            bool mouseOverButtonAndLMBPressed = ButtonMouseSelected //|| 
                //(Data.CurrentMenu.SelectedButtonRectangle.HasValue &&
                // Data.CurrentMenu.SelectedButtonRectangle.Value.Contains(MousePosition))) 
                     &&
                     MouseInput.LeftButton == ButtonState.Pressed;
            if (KeyboardInput.IsKeyDown(Keybindings.MenuSelect) || //if keyselected or
                (!LMBDownLastFrame && mouseOverButtonAndLMBPressed)) //only if button chosen by mouse
            {
                Menu currentMenu = Game.GameData.CurrentMenu;
                DisplayMode preSelectDisplayMode = Game.GameData.GameDislayMode;

                ButtonAction();
                if (currentMenu.SelectedButton != -1)
                {
                    modeChanged = true;
                    LeaveButtonLastUp = false;
                    if (Game.GameData.GameDislayMode != preSelectDisplayMode)
                        currentMenu.SelectedButton = -1;
                }
            }

            if (KeyboardInput.IsKeyDown(Keybindings.WorldOpenMenu) && LeaveButtonLastUp)
            {
                Game.GameData.GameDislayMode = Game.GameData.LastDisplayMode;
            }
            if (!modeChanged)
                LeaveButtonLastUp = KeyboardInput.IsKeyUp(Keybindings.WorldOpenMenu);
        }
        #endregion

        #region PauseMenuInput

        private static void PauseMenuInput()
        {
            MenuInput(PauseMenuButtonActions);
        }

        private static void PauseMenuButtonActions()
        {
            if (Game.GameData.CurrentMenu.SelectedButton == 0)
            {
                if (Game.GameData.LastDisplayMode == DisplayMode.Inventory)
                    Game.GameData.GameDislayMode = DisplayMode.Inventory;
                else
                    Game.GameData.GameDislayMode = DisplayMode.World;
            }
            else if (Game.GameData.CurrentMenu.SelectedButton == 1)
                Game.GameData.GameDislayMode = DisplayMode.OptionMenu;

            else if (Game.GameData.CurrentMenu.SelectedButton == 2)
                Game1.ExitGame = true;

        }

        #endregion

        #region OptionsMenuInput
        private static void OptionsMenuInput()
        {
            MenuInput(OptionMenuButtonActions);
        }

        private static void OptionMenuButtonActions()
        {
            if (Game.GameData.CurrentMenu.SelectedButton == 0)
            {
                Game.GameData.GameDislayMode = DisplayMode.GraphicsMenu;
            }
            else if (Game.GameData.CurrentMenu.SelectedButton == 1)
            {
                Game.GameData.GameDislayMode = DisplayMode.OptionMenu;
            }
            else if (Game.GameData.CurrentMenu.SelectedButton == 2)
            {
                Game.GameData.GameDislayMode = DisplayMode.PauseMenu;
            }
        }

        #endregion

        #region GraphicsMenu
        private static void GraphicsMenuInput()
        {
            MenuInput(GraphicsMenuButtonActions);
        }

        private static void GraphicsMenuButtonActions()
        {
            if (Game.GameData.CurrentMenu.SelectedButton == 0)
            {
                int resolutionIndex = ((GraphicsMenu)Game.GameData.CurrentMenu).ResolutionIndex;
                resolutionIndex++;
                resolutionIndex = resolutionIndex % GraphicsMenu.SupportedResolutions.Length;

                Point newResolution;
                if (GraphicsMenu.SupportedResolutions[resolutionIndex] == Point.Zero)
                    newResolution = new Point(Game.GraphicsDevice.Adapter.CurrentDisplayMode.Width,
                        Game.GraphicsDevice.Adapter.CurrentDisplayMode.Height);
                else
                    newResolution = GraphicsMenu.SupportedResolutions[resolutionIndex];

                bool fullscreen = Game.GameData.GraphicsDManager.IsFullScreen;
                Game.SetGraphics(newResolution, fullscreen);

                Game.GameData.InitializeIngameHUD();
                Game.GameData.InitializeMenus(Game1.IODataContainer.GraphicsOptions);
                Game.LoadInputProcessor(Game1.IODataContainer);
                Game.GameData.GameDislayMode = DisplayMode.GraphicsMenu;
            }
            else if (Game.GameData.CurrentMenu.SelectedButton == 1)
            {
                ((Menus.GraphicsMenu)Game.GameData.CurrentMenu).Fullscreen ^= true;

                Point previousResolution = new Point(Game.GameData.GraphicsDManager.PreferredBackBufferWidth,
                                 Game.GameData.GraphicsDManager.PreferredBackBufferHeight);
                Game.SetGraphics(previousResolution, ((Menus.GraphicsMenu)Game.GameData.CurrentMenu).Fullscreen);

                Game.GameData.InitializeMenus(Game1.IODataContainer.GraphicsOptions);
                Game.GameData.GameDislayMode = DisplayMode.GraphicsMenu;
            }
            else if (Game.GameData.CurrentMenu.SelectedButton == 2)
            {
                Menus.GraphicsMenuOptions.ViewDistances viewDis =
                    ((Menus.GraphicsMenu)Game.GameData.CurrentMenu).ViewDistance;
                viewDis = (Menus.GraphicsMenuOptions.ViewDistances)
                    ((((int)viewDis) + 1) % ((int)Menus.GraphicsMenuOptions.ViewDistances.States));
                ((Menus.GraphicsMenu)Game.GameData.CurrentMenu).ViewDistance = viewDis;

                Game.GameData.InGame.BufferRange = Game.GameData.InGame.Bufferranges[viewDis];
                Game.GameData.GameDislayMode = DisplayMode.GraphicsMenu;
            }
            else if (Game.GameData.CurrentMenu.SelectedButton == 3)
            {
                Game.GameData.GameDislayMode = DisplayMode.OptionMenu;
            }
        }

        #endregion

    }
}
