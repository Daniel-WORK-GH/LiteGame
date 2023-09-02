using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using Lite;
using Lite.Graphics;
using Lite.Physics;
using System.Collections.Generic;

namespace LiteGame
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Camera camera;
        private Shapes shapes;

        private LiteWorld world;

        private Color[] colors;
        private Color[] outlinecolors;

        private Vector2[] vertexBuffer;

        private Texture2D texture;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            shapes = new Shapes(this);
            camera = new Camera(new Screen(this, 1280, 720));

            camera.SetScale(24);
            camera.UpdateMatrices();

            this.texture = new Texture2D(GraphicsDevice, 1, 1);
            this.texture.SetData<Color>(new Color[] { Color.White });

            const int count = 10;
            this.world = new LiteWorld();

            this.camera.GetExtents(out float top, out float left, out float right, out float bottom);

            float padding = Math.Abs(right - left) * 0.05f;

            this.colors = new Color[count];
            this.outlinecolors = new Color[count];

            for (int i = 0; i < count; i++)
            {
                int type = (int)RandomHelper.RandomInteger(0, 2);

                LiteBody body = null;

                float x = RandomHelper.RandomSingle(left + padding, right - padding);
                float y = RandomHelper.RandomSingle(top + padding, bottom - padding);

                if (type == (int)ShapeType.Circle)
                {
                    if(!LiteBody.CreateCircleBody(1f, new LiteVector(x, y), 2, 0.5f, false, out body, out string err))
                    {
                        throw new Exception(err);
                    }
                }
                else if(type == (int)ShapeType.Box)
                {
                    if (!LiteBody.CreateBoxBody(2f, 2f, new LiteVector(x, y), 2, 0.5f, false, out body, out string err))
                    {
                        throw new Exception(err);
                    }
                }
                else
                {
                    throw new Exception();
                }

                colors[i] = RandomHelper.RandomColor();
                outlinecolors[i] = Color.White;
                this.world.AddBody(body);
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            KeyboardHelper.Update();
            MouseHelper.Update();

            if (KeyboardHelper.IsClicked(Keys.Up)) camera.IncScale();
            if (KeyboardHelper.IsClicked(Keys.Down)) camera.DecScale();

            float dx = 0f;
            float dy = 0f;
            float speed = 8f;

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
                LiteVector direction = LiteMath.Normalize(new LiteVector(dx, dy));
                LiteVector velocity = direction * speed * LiteUtil.GetElapsedTimeSeconds(gameTime);

                body.Move(velocity);
            }

            for(int i = 0; i < outlinecolors.Length; i++)
            {
                outlinecolors[i] = Color.White;
            }

            this.world.Step(LiteUtil.GetElapsedTimeSeconds(gameTime));

            base.Update(gameTime);
        }

        List<Vector2> vecs = new List<Vector2>();

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
                    shapes.FillCircle(position, body.Raidus, 26, colors[i]);
                    shapes.DrawCircle(position, body.Raidus, 26, 2f, this.outlinecolors[i]);
                }
                else if(body.ShapeType == ShapeType.Box)
                {
                    //shapes.FillRectangle(position.X, position.Y, body.Width, body.Height, Color.White);
                    LiteConverter.ToVector2Array(body.GetTransformedVertices(), ref vertexBuffer);

                    shapes.FillPolygon(vertexBuffer, body.Triangles, colors[i]);
                    shapes.DrawPolygon(vertexBuffer, 2f, this.outlinecolors[i]);
                }
            }

            if (vecs.Count >= 3)
            {
                shapes.FillPolygon(vecs.ToArray(), Color.Blue);
                Console.WriteLine("drawn");
            }

            shapes.End();

            base.Draw(gameTime);
        }
    }
}