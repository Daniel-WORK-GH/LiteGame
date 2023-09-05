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

        private Screen screen;
        private Camera camera;
        private Shapes shapes;

        private SpriteFont fontConsolas18;

        private LiteWorld world;

        private List<LiteEntity> entities;

        private Texture2D texture;

        private Stopwatch watch;

        private double totalWorldStepTime = 0;
        private int totalBodyCount = 0;
        private int totalSampleCount = 0;
        private Stopwatch sampletimer = new Stopwatch();
        private string worldStepTimeString = string.Empty;
        private string bodyCountString = string.Empty;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            this.Window.Position = new Point(10, 40);

            shapes = new Shapes(this);
            screen = new Screen(this, 1280, 720);
            camera = new Camera(screen);

            camera.SetScale(20);
            camera.UpdateMatrices();

            this.texture = new Texture2D(GraphicsDevice, 1, 1);
            this.texture.SetData<Color>(new Color[] { Color.White });

            this.entities = new();

            this.world = new LiteWorld();

            this.camera.GetExtents(out float top, out float left, out float right, out float bottom);
            float padding = Math.Abs(right - left) * 0.1f;

            if (!LiteBody.CreateBoxBody(right - left - padding * 2, 3f, 1f, 0.5f, true,
                out LiteBody groudbody, out string err))
            {
                throw new Exception(err);
            }
            groudbody.MoveTo(new LiteVector(0, 10f));
            this.world.AddBody(groudbody);
            this.entities.Add(new LiteEntity(groudbody, Color.DarkGreen));

            if (!LiteBody.CreateBoxBody(20f, 2f, 1f, 0.5f, true,
                out LiteBody ledgebody1, out err))
            {
                throw new Exception(err);
            }
            ledgebody1.MoveTo(new LiteVector(-10f, -3f));
            ledgebody1.Rotate(MathHelper.TwoPi / 20f);
            this.world.AddBody(ledgebody1);
            this.entities.Add(new LiteEntity(ledgebody1, Color.DarkGray));

            if (!LiteBody.CreateBoxBody(15f, 2f, 1f, 0.5f, true,
                out LiteBody ledgebody2, out err))
            {
                throw new Exception(err);
            }
            ledgebody2.MoveTo(new LiteVector(10f, -10f));
            ledgebody2.Rotate(-MathHelper.TwoPi / 20f);
            this.world.AddBody(ledgebody2);
            this.entities.Add(new LiteEntity(ledgebody2, Color.DarkRed));

            this.watch = new Stopwatch();
            this.sampletimer.Start();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            this.fontConsolas18 = this.Content.Load<SpriteFont>("Consolas18");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            KeyboardHelper.Update();
            MouseHelper.Update();

            if (KeyboardHelper.IsClicked(Keys.Up)) camera.IncScale();
            if (KeyboardHelper.IsClicked(Keys.Down)) camera.DecScale();

            if (MouseHelper.IsRightClick())
            {
                float w = RandomHelper.RandomSingle(2f, 3f);
                float h = RandomHelper.RandomSingle(2f, 3f);

                LiteVector pos = MouseHelper.GetMouseWorldPosition(this, screen, camera).ToLiteVector();

                this.entities.Add(new LiteEntity(this.world, w, h, false, pos));
            }
            if (MouseHelper.IsLeftClick())
            {
                float r = RandomHelper.RandomSingle(1.25f, 1.5f);
     
                LiteVector pos = MouseHelper.GetMouseWorldPosition(this, screen, camera).ToLiteVector();

                this.entities.Add(new LiteEntity(this.world, r, false, pos));
            }

            if (KeyboardHelper.IsClicked(Keys.OemTilde))
            {
                Console.WriteLine($"body count : {this.world.BodyCount}");
                Console.WriteLine($"stop time : {Math.Round(this.watch.Elapsed.TotalMilliseconds, 4)}");
            }

            if(this.sampletimer.Elapsed.TotalSeconds > 1d)
            {
                this.bodyCountString = "BodyCount : " + Math.Round(this.totalBodyCount / (double)this.totalSampleCount, 4).ToString();
                this.worldStepTimeString = "StepTime : " + Math.Round(this.totalWorldStepTime / (double)this.totalSampleCount, 4).ToString();

                this.totalBodyCount = 0;
                this.totalWorldStepTime = 0;
                this.totalSampleCount = 0;
                this.sampletimer.Restart();
            }

            watch.Restart();
            this.world.Step(LiteUtil.GetElapsedTimeSeconds(gameTime), 20);
            watch.Stop();

            this.totalWorldStepTime += this.watch.Elapsed.TotalMilliseconds;
            this.totalBodyCount += this.world.BodyCount;
            this.totalSampleCount++;

            this.camera.GetExtents(out _, out _, out _, out float bottom);

            for (int i = 0; i < this.entities.Count; i++)
            {
                LiteBody body = this.entities[i].Body;

                if (body.IsStatic) continue;

                LiteAABB box = body.GetAABB();

                if(box.Min.Y > bottom)
                {
                    this.world.RemoveBody(body);
                    this.entities.Remove(this.entities[i]);
                    i--;
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(50,60,70));

            shapes.Begin(camera);

            for(int i = 0; i < this.entities.Count; i++)
            {
                this.entities[i].Draw(this.shapes);
            }

            shapes.End();

            Vector2 stringSize = this.fontConsolas18.MeasureString(this.bodyCountString);

            this._spriteBatch.Begin();
            this._spriteBatch.DrawString(this.fontConsolas18, this.bodyCountString, Vector2.Zero, Color.White);
            this._spriteBatch.DrawString(this.fontConsolas18, this.worldStepTimeString, new Vector2(0, stringSize.Y), Color.White);
            this._spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}