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
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont Font;
        Effect effect;

        DataContainer data;


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            this.TargetElapsedTime = TimeSpan.FromMilliseconds(16);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            graphics.PreferredBackBufferHeight = graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Height;
            graphics.PreferredBackBufferWidth = graphics.GraphicsDevice.Adapter.CurrentDisplayMode.Width;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Font = Content.Load<SpriteFont>("SpriteFont1");
            effect = Content.Load<Effect>("Effect1");

            Block.BlockTex = Content.Load<Texture2D>("Blocks");


            Mouse.SetPosition(
                GraphicsDevice.Viewport.Width / 2,
                GraphicsDevice.Viewport.Width / 2);
            //Window.ClientBounds.X + (int)(Window.ClientBounds.Width / 2f),
            //Window.ClientBounds.Y + (int)(Window.ClientBounds.Height / 2f));
            data = new DataContainer(spriteBatch, Font);
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
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
            if (ks.IsKeyDown(Keys.Escape))
                this.Exit();

            data.Update(ks, ms, gameTime);

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.DeepSkyBlue, 1.0f, 0);
            graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            RasterizerState rs = new RasterizerState();
            rs.CullMode = CullMode.CullClockwiseFace;
            rs.FillMode = FillMode.Solid;
            graphics.GraphicsDevice.RasterizerState = rs;

            data.Draw(effect, spriteBatch, Font);
            // TODO: Add your drawing code here
            base.Draw(gameTime);
        }
    }
}
