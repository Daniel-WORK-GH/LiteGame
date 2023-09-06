using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lite.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Lite.Physics
{
    public class LitePhysicsDemo
    {
        private Game game;

        private Screen screen;
        private Camera camera;
        private Shapes shapes;

        private SpriteFont font;

        private LiteWorld world;

        private List<LiteEntity> entities;

        private Stopwatch watch;

        private double totalWorldStepTime = 0;
        private int totalBodyCount = 0;
        private int totalSampleCount = 0;
        private Stopwatch sampletimer = new Stopwatch();
        private string worldStepTimeString = string.Empty;
        private string bodyCountString = string.Empty;

        public LitePhysicsDemo(Game game, SpriteFont font)
        {
            this.game = game;

            this.shapes = new Shapes(game);
            this.screen = new Screen(game, 1280, 720);
            this.camera = new Camera(screen);
            this.font = font;

            this.camera.SetScale(20);
            this.camera.UpdateMatrices();

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
        }

        public void Update(GameTime gameTime)
        {
            KeyboardHelper.Update();
            MouseHelper.Update();

            if (KeyboardHelper.IsClicked(Keys.Up)) camera.IncScale();
            if (KeyboardHelper.IsClicked(Keys.Down)) camera.DecScale();

            if (MouseHelper.IsRightClick())
            {
                float w = RandomHelper.RandomSingle(2f, 3f);
                float h = RandomHelper.RandomSingle(2f, 3f);

                LiteVector pos = MouseHelper.GetMouseWorldPosition(screen, camera).ToLiteVector();

                this.entities.Add(new LiteEntity(this.world, w, h, false, pos));
            }
            if (MouseHelper.IsLeftClick())
            {
                float r = RandomHelper.RandomSingle(1.25f, 1.5f);

                LiteVector pos = MouseHelper.GetMouseWorldPosition(screen, camera).ToLiteVector();

                this.entities.Add(new LiteEntity(this.world, r, false, pos));
            }

            if (KeyboardHelper.IsClicked(Keys.OemTilde))
            {
                Console.WriteLine($"body count : {this.world.BodyCount}");
                Console.WriteLine($"stop time : {Math.Round(this.watch.Elapsed.TotalMilliseconds, 4)}");
            }

            if (this.sampletimer.Elapsed.TotalSeconds > 1d)
            {
                this.bodyCountString = "BodyCount : " + Math.Round(this.totalBodyCount / (double)this.totalSampleCount, 4).ToString();
                this.worldStepTimeString = "StepTime : " + Math.Round(this.totalWorldStepTime / (double)this.totalSampleCount, 4).ToString();

                this.totalBodyCount = 0;
                this.totalWorldStepTime = 0;
                this.totalSampleCount = 0;
                this.sampletimer.Restart();
            }

            watch.Restart();
            this.world.Step(LiteUtil.GetElapsedTimeSeconds(gameTime), 20, CollisionResolveMode.RotationFriction);
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

                if (box.Min.Y > bottom)
                {
                    this.world.RemoveBody(body);
                    this.entities.Remove(this.entities[i]);
                    i--;
                }
            }
        }

        public void Draw(SpriteBatch _spriteBatch)
        {
            shapes.Begin(camera);

            for (int i = 0; i < this.entities.Count; i++)
            {
                this.entities[i].Draw(this.shapes);
            }

            shapes.End();

            Vector2 stringSize = this.font.MeasureString(this.bodyCountString);

            _spriteBatch.Begin();
            _spriteBatch.DrawString(this.font, this.bodyCountString, Vector2.Zero, Color.White);
            _spriteBatch.DrawString(this.font, this.worldStepTimeString, new Vector2(0, stringSize.Y), Color.White);
            _spriteBatch.End();
        }
    }
}
