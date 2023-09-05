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

        private List<Color> colors;
        private List<Color> outlinecolors;

        private Vector2[] vertexBuffer;

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

            shapes = new Shapes(this);
            screen = new Screen(this, 1280, 720);
            camera = new Camera(screen);

            camera.SetScale(24);
            camera.UpdateMatrices();

            this.texture = new Texture2D(GraphicsDevice, 1, 1);
            this.texture.SetData<Color>(new Color[] { Color.White });

            this.colors = new();
            this.outlinecolors = new();

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
            this.colors.Add(Color.DarkGreen);
            this.outlinecolors.Add(Color.White);

            if (!LiteBody.CreateBoxBody(20f, 2f, 1f, 0.5f, true,
                out LiteBody ledgebody1, out err))
            {
                throw new Exception(err);
            }
            ledgebody1.MoveTo(new LiteVector(-10f, -3f));
            ledgebody1.Rotate(MathHelper.TwoPi / 20f);
            this.world.AddBody(ledgebody1);
            this.colors.Add(Color.DarkGray);
            this.outlinecolors.Add(Color.White);

            if (!LiteBody.CreateBoxBody(15f, 2f, 1f, 0.5f, true,
                out LiteBody ledgebody2, out err))
            {
                throw new Exception(err);
            }
            ledgebody2.MoveTo(new LiteVector(10f, -10f));
            ledgebody2.Rotate(-MathHelper.TwoPi / 20f);
            this.world.AddBody(ledgebody2);
            this.colors.Add(Color.DarkGray);
            this.outlinecolors.Add(Color.White);

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
                float w = RandomHelper.RandomSingle(1f, 2f);
                float h = RandomHelper.RandomSingle(1f, 2f);

                LiteVector pos = MouseHelper.GetMouseWorldPosition(this, screen, camera).ToLiteVector();

                if (!LiteBody.CreateBoxBody(w, h, 2f, 0.6f, false, out LiteBody body, out string err))
                {
                    throw new Exception(err);
                }
                body.MoveTo(pos);
                this.world.AddBody(body);
                this.colors.Add(RandomHelper.RandomColor());
                this.outlinecolors.Add(Color.White);
            }
            if (MouseHelper.IsLeftClick())
            {
                float r = RandomHelper.RandomSingle(0.75f, 1.5f);
     
                LiteVector pos = MouseHelper.GetMouseWorldPosition(this, screen, camera).ToLiteVector();

                if (!LiteBody.CreateCircleBody(r, 2f, 0.6f, false, out LiteBody body, out string err))
                {
                    throw new Exception(err);
                }
                body.MoveTo(pos);
                this.world.AddBody(body);
                this.colors.Add(RandomHelper.RandomColor());
                this.outlinecolors.Add(Color.White);
            }

            if (KeyboardHelper.IsClicked(Keys.OemTilde))
            {
                Console.WriteLine($"body count : {this.world.BodyCount}");
                Console.WriteLine($"stop time : {Math.Round(this.watch.Elapsed.TotalMilliseconds, 4)}");
            }

#if false
            float dx = 0f;
            float dy = 0f;
            float speedMagnitute = 96f;

            if (KeyboardHelper.IsPressed(Keys.W)) dy -= 1;
            if (KeyboardHelper.IsPressed(Keys.A)) dx -= 1;
            if (KeyboardHelper.IsPressed(Keys.D)) dx += 1;
            if (KeyboardHelper.IsPressed(Keys.S)) dy += 1;

            if (!this.world.GetBody(0, out LiteBody body))
            {
                throw new Exception("");
            }

            if (KeyboardHelper.IsPressed(Keys.E)) body.Rotate(MathF.PI / 2f * LiteUtil.GetElapsedTimeSeconds(gameTime));
            if (KeyboardHelper.IsPressed(Keys.Q)) body.Rotate(-MathF.PI / 2f * LiteUtil.GetElapsedTimeSeconds(gameTime));

            if (dx != 0 || dy != 0)
            {
                LiteVector forceDirc = LiteMath.Normalize(new LiteVector(dx, dy));
                LiteVector force = forceDirc * speedMagnitute;

                body.AddForce(force);
            }
            WrapScreen();
#endif
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

            for (int i = 0; i < this.world.BodyCount; i++)
            {
                if (!this.world.GetBody(i, out LiteBody body))
                {
                    throw new Exception("");
                }

                LiteAABB box = body.GetAABB();

                if(box.Min.Y > bottom)
                {
                    this.world.RemoveBody(body);
                    this.colors.RemoveAt(i);
                    this.outlinecolors.RemoveAt(i);
                    i--;
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(50,60,70));

            shapes.Begin(camera);

            for(int i = 0; i < this.world.BodyCount; i++)
            {
                if(!this.world.GetBody(i, out LiteBody body))
                {
                    throw new Exception("");
                }

                Vector2 position = body.Position.ToVector2();

                if(body.ShapeType == ShapeType.Circle)
                {
                    shapes.FillCircle(position, body.Radius, 26, colors[i]);
                    shapes.DrawCircle(position, body.Radius, 26, 2f, this.outlinecolors[i]);
                }
                else if(body.ShapeType == ShapeType.Box)
                {
                    //shapes.FillRectangle(position.X, position.Y, body.Width, body.Height, Color.White);
                    LiteConverter.ToVector2Array(body.GetTransformedVertices(), ref vertexBuffer);

                    shapes.FillBox(body.Position.ToVector2(), body.Width, body.Height, body.Angle, colors[i]);
                    shapes.DrawPolygon(vertexBuffer, 2f, this.outlinecolors[i]);
                }
            }

            List<LiteVector> contactPoints = this.world.ContactPointsList;
            for(int i = 0; i < contactPoints.Count; i++)
            {
                shapes.FillBox(contactPoints[i].ToVector2(), 0.5f, 0.5f, Color.Orange);
            }

            shapes.End();

            Vector2 stringSize = this.fontConsolas18.MeasureString(this.bodyCountString);

            this._spriteBatch.Begin();
            this._spriteBatch.DrawString(this.fontConsolas18, this.bodyCountString, Vector2.Zero, Color.White);
            this._spriteBatch.DrawString(this.fontConsolas18, this.worldStepTimeString, new Vector2(0, stringSize.Y), Color.White);
            this._spriteBatch.End();

            base.Draw(gameTime);
        }

        private void WrapScreen()
        {
            this.camera.GetExtents(out Vector2 camMin, out Vector2 camMax);

            float viewWidth = camMax.X - camMin.X;
            float viewHeight = camMax.Y - camMin.Y;

            for (int i = 0; i < this.world.BodyCount; i++)
            {
                if(!this.world.GetBody(i, out LiteBody body))
                {
                    throw new Exception("");
                }

                if (body.Position.X < camMin.X) body.Move(new LiteVector(viewWidth, 0f));
                if (body.Position.Y < camMin.Y) body.Move(new LiteVector(0f, viewHeight));
                if (body.Position.X > camMax.X) body.Move(new LiteVector(-viewWidth, 0f));
                if (body.Position.Y > camMax.Y) body.Move(new LiteVector(0f, -viewHeight));
            }
        }
    }
}