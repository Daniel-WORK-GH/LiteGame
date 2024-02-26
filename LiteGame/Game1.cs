using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using Lite;
using Lite.Graphics;
using Lite.Physics;
using System.Collections.Generic;
using System.Diagnostics;

namespace LiteGame
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private SpriteFont fontConsolas18;
        private LitePhysicsDemo demo;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            //this.Window.Position = new Point(10, 40);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            this.fontConsolas18 = this.Content.Load<SpriteFont>("Consolas18");

            this.demo = new LitePhysicsDemo(this, this.fontConsolas18);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            demo.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(50,60,70));

            demo.Draw(_spriteBatch);

            base.Draw(gameTime);
        }
    }
}