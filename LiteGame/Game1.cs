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

        private List<LiteBody> bodylist;
        private Color[] colors;
        private Color[] outlinecolors;

        private Vector2[] vertexBuffer;

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

            const int count = 10;
            bodylist = new List<LiteBody>(count);

            this.camera.GetExtents(out float top, out float left, out float right, out float bottom);

            float padding = Math.Abs(right - left) * 0.05f;

            this.colors = new Color[count];
            this.outlinecolors = new Color[count];

            for (int i = 0; i < count; i++)
            {
                int type = (int)ShapeType.Box;

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
                bodylist.Add(body);
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            KeyboardHelper.Update();

            if (KeyboardHelper.IsClicked(Keys.Up)) camera.IncScale();
            if (KeyboardHelper.IsClicked(Keys.Down)) camera.DecScale();

            float dx = 0f;
            float dy = 0f;
            float speed = 8f;

            if (KeyboardHelper.IsPressed(Keys.W)) dy -= 1;
            if (KeyboardHelper.IsPressed(Keys.A)) dx -= 1;
            if (KeyboardHelper.IsPressed(Keys.D)) dx += 1;
            if (KeyboardHelper.IsPressed(Keys.S)) dy += 1;

            if (KeyboardHelper.IsPressed(Keys.E)) bodylist[0].Rotate(MathF.PI / 2f * LiteUtil.GetElapsedTimeSeconds(gameTime));
            if (KeyboardHelper.IsPressed(Keys.Q)) bodylist[0].Rotate(-MathF.PI / 2f * LiteUtil.GetElapsedTimeSeconds(gameTime));


            if (dx != 0 || dy != 0)
            {
                LiteVector direction = LiteMath.Normalize(new LiteVector(dx, dy));
                LiteVector velocity = direction * speed * LiteUtil.GetElapsedTimeSeconds(gameTime);

                this.bodylist[0].Move(velocity);
            }

            for (int i = 0; i < this.bodylist.Count; i++)
            {
                LiteBody body = this.bodylist[i];

                //body.Rotate(MathF.PI / 2f * LiteUtil.GetElapsedTimeSeconds(gameTime));
            }

            for(int i = 0; i < outlinecolors.Length; i++)
            {
                outlinecolors[i] = Color.White;
            }

            for(int i = 0; i < this.bodylist.Count; i++)
            {
                LiteBody bodyA = this.bodylist[i];

                for(int j = i + 1; j < this.bodylist.Count; j++)
                {
                    LiteBody bodyB = this.bodylist[j];

                    if (Collisions.IntersectPolygons(
                        bodyA.GetTransformedVertices(), bodyB.GetTransformedVertices(),
                        out LiteVector normal, out float depth))
                    {
                        outlinecolors[i] = outlinecolors[j] = Color.Red;

                        bodyA.Move(-normal * depth / 2);
                        bodyB.Move(normal * depth / 2);
                    }
                }
            }

            camera.UpdateMatrices();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(50,60,70));

            shapes.Begin(camera);

            for(int i = 0; i < bodylist.Count; i++)
            {
                LiteBody body = bodylist[i];

                Vector2 position = body.Position.ToVector2();

                if(body.ShapeType == ShapeType.Circle)
                {
                    shapes.FillCircle(position, body.Raidus, 26, colors[i]);
                    shapes.DrawCircle(position, body.Raidus, 26, 2f, Color.White);
                }
                else if(body.ShapeType == ShapeType.Box)
                {
                    //shapes.FillRectangle(position.X, position.Y, body.Width, body.Height, Color.White);
                    LiteConverter.ToVector2Array(body.GetTransformedVertices(), ref vertexBuffer);

                    shapes.FillPolygon(vertexBuffer, body.Triangles, colors[i]);
                    shapes.DrawPolygon(vertexBuffer, 3f, this.outlinecolors[i]);
                }
            }

            shapes.End();

            base.Draw(gameTime);
        }
    }
}