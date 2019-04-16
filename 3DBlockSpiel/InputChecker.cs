using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using _1st3DGame.enums;

namespace _1st3DGame
{
    class InputChecker
    {
        private World inGame;

        public Point Resolution = new Point(800, 600);

        private MouseState ms;
        private Point LastMPos;
        private Point MouseDifference = Point.Zero;
        private const float Sensitivity = 0.16f;

        private KeyboardState ks;
        private TimeSpan elapsedTime;
        private float ActionSpeed { get { return elapsedTime.Milliseconds / 1000.0f; } }
        #region KeyBind and MouseBind
        Keys kMoveForward = Keys.W;
        Keys kMoveLeft = Keys.A;
        Keys kMoveRight = Keys.D;
        Keys kMoveBackwards = Keys.S;
        Keys kJump = Keys.Space;

        #endregion

        public InputChecker(World ingame)
        {
            this.LastMPos = new Point(-1, -1);
            this.inGame = ingame;
        }

        private void PrepareData(KeyboardState ks, MouseState ms, GameTime gameTime)
        {
            this.ms = ms;
            this.ks = ks;
            this.elapsedTime = gameTime.ElapsedGameTime;
            if (!ks.IsKeyDown(Keys.LeftAlt))
            {
                Point mousePos = new Point(ms.X, ms.Y);
                if (LastMPos == new Point(-1, -1))
                    LastMPos = mousePos;
                this.MouseDifference = LastMPos.Subtract(mousePos);
                Mouse.SetPosition(LastMPos.X, LastMPos.Y);
            }
        }

        public void ControlInput(KeyboardState ks, MouseState ms, GameTime time)
        {
            PrepareData(ks, ms, time);

            PlayerInput();

            FKeys();
        }

        #region Player Input
        private void PlayerInput()
        {
            PlayerMovement();
            PlayerLookAround();
            PlayerTakePlaceBlocks();
        }

        private void PlayerMovement()
        {
            if (ks.IsKeyDown(kMoveForward))
                inGame.Player1.MoveInput(MoveDirection.Forward);
            if (ks.IsKeyDown(kMoveBackwards))
                inGame.Player1.MoveInput(MoveDirection.Backwards);
            if (ks.IsKeyDown(kMoveLeft))
                inGame.Player1.MoveInput(MoveDirection.Left);
            if (ks.IsKeyDown(kMoveRight))
                inGame.Player1.MoveInput(MoveDirection.Right);
            if (ks.IsKeyDown(kJump))
                inGame.Player1.MoveInput(MoveDirection.Jump);
        }

        private void PlayerLookAround()
        {

            float rotLeft =  this.MouseDifference.X * ActionSpeed * Sensitivity;
            float rotUp = this.MouseDifference.Y * ActionSpeed * Sensitivity;


            inGame.Player1.LookAround(rotUp, rotLeft);
        }


        private void PlayerTakePlaceBlocks()
        {
            inGame.Player1.Digging = ms.LeftButton == ButtonState.Pressed;
            inGame.Player1.Building = ms.RightButton == ButtonState.Pressed;
        }
        #endregion

        private void FKeys()
        {
            F2_Debug();
        }

        private DateTime LastF2 = DateTime.Now;
        private void F2_Debug()
        {
            if (ks.IsKeyDown(Keys.F2) && (DateTime.Now - LastF2).Milliseconds > 150)
            {
                inGame.DrawDebugInfo = !inGame.DrawDebugInfo;
                LastF2 = DateTime.Now;
            }
        }
    }
}
