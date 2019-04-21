using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using BlockGame3D.Input;
using BlockGame3D.IO;
using BlockGame3D.Menus.GraphicsMenuOptions;

namespace BlockGame3D
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        static readonly string SavePath = "config.cfg";
        public static IODataContainer IODataContainer;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Effect effect;
        
        public static Texture2D EmptyTexture;
        public static bool ExitGame = false;

        internal DataContainer GameData { get; private set; }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this)
            {
                GraphicsProfile = GraphicsProfile.HiDef
            };

            Content.RootDirectory = "Content";

            this.IsFixedTimeStep = false;
            graphics.SynchronizeWithVerticalRetrace = false;

            this.Exiting += new EventHandler<EventArgs>(Game1_Exiting);
        }

        void Game1_Exiting(object sender, EventArgs e)
        {
            IODataContainer.Save(SavePath);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            IODataContainer = IODataContainer.Load(SavePath);
            SetGraphics(IODataContainer.GraphicsOptions);

            EmptyTexture = new Texture2D(GraphicsDevice, 1, 1);
            EmptyTexture.SetData<Color>(new Color[1] { Color.White });
            StaticHUD.EmptyTex = EmptyTexture;
            Menus.InventoryMenu.InventoryBackgroundTexture = EmptyTexture;
            Menus.PauseMenu.BackgroundTexture = EmptyTexture;

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            DataContainer.IngameFont = Content.Load<SpriteFont>("Fonts/SpriteFont1");
            DataContainer.MenuFont = Content.Load<SpriteFont>("Fonts/menuFont");

            effect = Content.Load<Effect>("Shader/OldEffect");

            World.BlockEffect = Content.Load<Effect>("Shader/BlockShader");
            World.PointSpriteEffect = Content.Load<Effect>("Shader/PointSpriteShader");
            World.CharacterEffect = Content.Load<Effect>("Shader/CharacterShader");
            World.SunsTexture = Content.Load<Texture2D>("Textures/Suns/sunTexture");

            Enemy.EnemyTex = Content.Load<Texture2D>("Textures/Enemies/enemyTextures");

            Block.IconTex = Content.Load<Texture2D>("Textures/HUD/blockIcons");
            Block.BlockTex = Content.Load<Texture2D>("Textures/Environment/HRBlocks2k");
            Block.BlockBumpMap = Content.Load<Texture2D>("Textures/Environment/HRBlocks2kNM");
            //Block.BlockTex = Content.Load<Texture2D>("Textures/Environment/shaderColorTest");
            //Block.BlockBumpMap = Content.Load<Texture2D>("Textures/Environment/shaderColorTest");
            Menus.PauseMenu.ButtonBackgroundTextures = Content.Load<Texture2D>("Textures/Menus/ButtonBackground");
          
            GameData = new DataContainer(spriteBatch, graphics);

            LoadInputProcessor(IODataContainer);
            // TODO: use this.Content to load your game content here
            
            //Auskommentieren, falls mehr Daten gespeichert werden sollen
            //IODataContainer.Save(SavePath);

        }

        private void SetGraphics(GraphicsOptions go)
        {
            graphics.PreferredBackBufferWidth = go.Resolution.X;
            graphics.PreferredBackBufferHeight = go.Resolution.Y;
            graphics.IsFullScreen = go.Fullscreen;
            graphics.ApplyChanges();
        }

        public void LoadInputProcessor(IODataContainer iodc)
        {
            KeyboardState ks = Keyboard.GetState();
            MouseState ms = Mouse.GetState();
            InputProcessor.SetData(this, ks, ms, new Point(GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDevice.PresentationParameters.BackBufferHeight), iodc.Keybindings);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            KeyboardState ks = Keyboard.GetState();
            MouseState ms = Mouse.GetState();
            // Allows the game to exit
            if (ExitGame)
                this.Exit();

            this.IsMouseVisible = GameData.MouseVisible[GameData.GameDislayMode];
            // if (this.IsActive)
            GameData.Update(ks, ms, gameTime);

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        static readonly RasterizerState rs = new RasterizerState()
        {
            CullMode = CullMode.CullClockwiseFace,
            FillMode = FillMode.Solid
        };
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.DeepSkyBlue, 1.0f, 0);
            graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            

            graphics.GraphicsDevice.RasterizerState = rs;

            GameData.Draw(spriteBatch);
            // TODO: Add your drawing code here
            base.Draw(gameTime);
        }

        public void SetGraphics(Point resolution, bool isFullscreen)
        {
            IODataContainer.GraphicsOptions.Resolution = resolution;
            IODataContainer.GraphicsOptions.Fullscreen = isFullscreen;

            graphics.PreferredBackBufferWidth = resolution.X;
            graphics.PreferredBackBufferHeight = resolution.Y;
            graphics.IsFullScreen = isFullscreen;
            graphics.ApplyChanges();
        }
    }
}
