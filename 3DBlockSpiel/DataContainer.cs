using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using _1st3DGame.Input;
using _1st3DGame.Menus;
using _1st3DGame.Menus.InventoryStates;
using _1st3DGame.Menus.GraphicsMenuOptions;

namespace _1st3DGame
{
    class DataContainer
    {
        public static SpriteFont IngameFont;
        public static SpriteFont MenuFont;
        public SpriteBatch SB { get; private set; }
        public GraphicsDeviceManager GraphicsDManager { get; private set; }

        public Dictionary<DisplayMode, Menu> Menus { get; private set; }
        public Dictionary<DisplayMode, bool> MouseVisible { get; private set; }

        public Menu CurrentMenu { get; private set; }

        public World InGame { get; private set; }
        private StaticHUD IngameHud;

        public DisplayMode LastDisplayMode { get; private set; }
        private DisplayMode _GameDisplayMode;
        public DisplayMode GameDislayMode
        {
            get { return _GameDisplayMode; }
            set
            {
                Menu newMenu = Menus[value];

                //management of inventoryrelated stuff 
                if (value == DisplayMode.Inventory)
                {
                    InGame.Player1.Digging = false;
                    InGame.Player1.Building = false;
                    ((InventoryMenu)newMenu).State = InventoryShowStates.ShowAll;
                }
                else if (_GameDisplayMode == DisplayMode.Inventory && value == DisplayMode.World)
                    ((InventoryMenu)CurrentMenu).State = InventoryShowStates.ShowItembar;
                //management of Pause / Unpause
                if (value == DisplayMode.World || value == DisplayMode.Inventory)
                    InGame.Paused = false;
                else
                    InGame.Paused = true;



                //setting final values
                this.LastDisplayMode = _GameDisplayMode;
                this._GameDisplayMode = value;
                this.CurrentMenu = newMenu;
            }
        }

        public DataContainer(SpriteBatch sb, GraphicsDeviceManager graphics)
        {
            this.SB = sb;
            this.GraphicsDManager = graphics;

            InGame = new World(sb.GraphicsDevice);
            InitializeIngameHUD();
            InitializeMenus(Game1.IODataContainer.GraphicsOptions);
        }

        public void InitializeIngameHUD()
        {
            IngameHud = new StaticHUD(SB, IngameFont, InGame);
        }

        public void InitializeMenus(GraphicsOptions go)
        {
            Menu.ScreenResolution = new Point(GraphicsDManager.GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDManager.GraphicsDevice.PresentationParameters.BackBufferHeight); 
            Menu inventory = new Menus.InventoryMenu(SB, IngameFont, InGame.Player1.inventory);
            Menu pause = new Menus.PauseMenu(SB, MenuFont);
            Menu options = new Menus.OptionMenu(SB, MenuFont);
            Menu graphics = new GraphicsMenu(SB, MenuFont, GraphicsDManager, go);

            this.Menus = new Dictionary<DisplayMode, Menu>();
            Menus.Add(DisplayMode.World, inventory);
            Menus.Add(DisplayMode.Inventory, inventory);
            Menus.Add(DisplayMode.PauseMenu, pause);
            Menus.Add(DisplayMode.OptionMenu, options);
            Menus.Add(DisplayMode.GraphicsMenu, graphics);

            this.MouseVisible = new Dictionary<DisplayMode, bool>();
            this.MouseVisible.Add(DisplayMode.World, false);
            this.MouseVisible.Add(DisplayMode.Inventory, true);
            this.MouseVisible.Add(DisplayMode.PauseMenu, true);
            this.MouseVisible.Add(DisplayMode.OptionMenu, true);
            this.MouseVisible.Add(DisplayMode.GraphicsMenu, true);

            this.CurrentMenu = Menus[GameDislayMode];
        }

        public void Update(KeyboardState ks, MouseState ms, GameTime time)
        {
            InputProcessor.UpdateInput(ks, ms);
            InGame.Update(time.ElapsedGameTime);
            if (CurrentMenu != null)
                CurrentMenu.Update();

            updateCounter++;
            UpdateFPS();
        }

        public void Draw(SpriteBatch sb)
        {
            drawCounter++;

            this.InGame.Draw();

            sb.Begin();
            // DEBUG INFO
            if (InGame.DrawDebugInfo)
            {
                DrawDebug(sb);
                DrawTimestamps(sb);
            }
            sb.End();
            IngameHud.Draw();
            if (CurrentMenu != null)
                CurrentMenu.Draw();
        }

        int updateCounter = 0;
        int updates = 0;
        int drawCounter = 0;
        int draws = 0;
        DateTime lastSec = DateTime.Now;
        private void UpdateFPS()
        {
            if ((DateTime.Now - lastSec) > TimeSpan.FromSeconds(1))
            {
                updates = updateCounter;
                updateCounter = 0;
                draws = drawCounter;
                drawCounter = 0;
                lastSec = DateTime.Now;
            }
        }

        private void DrawDebug(SpriteBatch sb)
        {
            DrawOrientation(sb);
            DrawPeformance(sb);

        }

        private void DrawOrientation(SpriteBatch sb)
        {
            sb.DrawString(IngameFont, InGame.WorldTime + "", Vector2.Zero, Color.Yellow);
            sb.DrawString(IngameFont, "PlayerPos: " + InGame.Player1.EyePosition.ToShortString(), new Vector2(0, 20), Color.Yellow);
            sb.DrawString(IngameFont, "PlayerChunk: " + Chunk.GetGridPosition(InGame.Player1.EyePosition), new Vector2(0, 40), Color.Yellow);
            sb.DrawString(IngameFont, "PlayerLevel: " + Chunk.GetSurfaceLevel(Chunk.GetGridPosition(InGame.Player1.EyePosition)), new Vector2(0, 60), Color.Yellow);
            sb.DrawString(IngameFont, "Bodies: " + InGame.Characters.Count, new Vector2(0, 80), Color.Yellow);

        }

        private void DrawPeformance(SpriteBatch sb)
        {
            sb.DrawString(IngameFont, "BufferedChunkCount: " + InGame.BufferedChunks.Count, new Vector2(0, 120), Color.Yellow);
            sb.DrawString(IngameFont, "LoadedChunkCount: " + InGame.LoadedChunks.Count, new Vector2(0, 140), Color.Yellow);
            sb.DrawString(IngameFont, "Update/sec.: " + updates, new Vector2(0, 160), Color.Yellow);
            sb.DrawString(IngameFont, "Draw/sec.: " + draws, new Vector2(0, 180), Color.Yellow);
            sb.DrawString(IngameFont, "Vertices: " + InGame.WorldVertices, new Vector2(0, 200), Color.Yellow);
            sb.DrawString(IngameFont, "Drawn Vetices: " + InGame.WorldVerticesDrawn, new Vector2(0, 220), Color.Yellow);
            sb.DrawString(IngameFont, "Mode: " + GameDislayMode, new Vector2(0, 240), Color.Yellow);
        }

        private void DrawTimestamps(SpriteBatch sb)
        {
            sb.DrawString(IngameFont, "Timestamps", new Vector2(0, 300), Color.Yellow);
            sb.DrawString(IngameFont, "generation: " + InGame.generationUpdate, new Vector2(0, 320), Color.Yellow);
            sb.DrawString(IngameFont, "collsion: " + InGame.collisionUpdate, new Vector2(0, 340), Color.Yellow);
            sb.DrawString(IngameFont, "player: " + InGame.playerUpdate, new Vector2(0, 360), Color.Yellow);
            sb.DrawString(IngameFont, "interaction: " + InGame.interactionUpdate, new Vector2(0, 380), Color.Yellow);
            sb.DrawString(IngameFont, "digging: " + InGame.Player1.Digging, new Vector2(0, 400), Color.Yellow);

            sb.DrawString(IngameFont, "PointFinderTime:  " + InGame.pointRecursion1Time, new Vector2(0, 460), Color.Yellow);
            sb.DrawString(IngameFont, "PointFinderLongestTime:  " + InGame.longest1, new Vector2(0, 480), Color.Yellow);
            sb.DrawString(IngameFont, "drawtime:  " + InGame.drawTime, new Vector2(0, 500), Color.Yellow);
        }
    }
}
