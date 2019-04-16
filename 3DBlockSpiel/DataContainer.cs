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
    class DataContainer
    {
        private World inGame;
        private InputChecker inputChecker;
        private StaticHUD staticHud;
        private DynamicHUD dynamicHUD;

        public DataContainer(SpriteBatch sb, SpriteFont font)
        {
            inGame = new World(sb.GraphicsDevice);
            inputChecker = new InputChecker(inGame);
            staticHud = new StaticHUD(sb, font);
            dynamicHUD = new DynamicHUD(sb, font);
        }

        public void Update(KeyboardState ks, MouseState ms, GameTime time)
        {
            inputChecker.ControlInput(ks, ms, time);
            inGame.Update(time.ElapsedGameTime);


            updateCounter++;
            UpdateFPS();
        }

        public void Draw(Effect effect, SpriteBatch sb, SpriteFont font)
        {
            drawCounter++;

            this.inGame.Draw(effect);

            sb.Begin();
            // DEBUG INFO
            if (inGame.DrawDebugInfo)
            {
                DrawDebug(sb, font);
            }
            sb.End();
            staticHud.Draw();

            dynamicHUD.Update();
            dynamicHUD.Draw();
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

        private void DrawDebug(SpriteBatch sb, SpriteFont font)
        {
            sb.DrawString(font, "Items: " + inGame.Player1.inventory.itemSlotCount[0], Vector2.Zero, Color.Yellow);
            sb.DrawString(font, "PlayerPos: " + inGame.debugPlayerIndex, new Vector2(0, 20), Color.Yellow);
            sb.DrawString(font, "PlayerChunk: " + inGame.PlayerChunkIndex, new Vector2(0, 40), Color.Yellow);
            sb.DrawString(font, "ChunkCount: " + inGame.Chunks.Count, new Vector2(0, 60), Color.Yellow);
            sb.DrawString(font, "Update/sec.: " + updates, new Vector2(0, 80), Color.Yellow);
            sb.DrawString(font, "Draw/sec.: " + draws, new Vector2(0, 100), Color.Yellow);
        }
    }
}
