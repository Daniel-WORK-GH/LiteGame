using Lite;
using Lite.Graphics;
using Lite.Physics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lite.Physics
{
    public class LiteEntity
    {
        public readonly LiteBody Body;
        public readonly Color Color;

        public LiteEntity(LiteBody body)
        {
            this.Body = body;
            this.Color = RandomHelper.RandomColor();
        }

        public LiteEntity(LiteBody body, Color color)
        {
            this.Body = body;
            this.Color = color;
        }

        public LiteEntity(LiteWorld world, float radius, bool isStatic, LiteVector position)
        {
            if (!LiteBody.CreateCircleBody(radius, 1f, 0.5f, isStatic, out LiteBody body, out string err))
            {
                throw new Exception(err);
            }

            body.MoveTo(position);
            this.Body = body;
            world.AddBody(body);

            this.Color = RandomHelper.RandomColor();
        }

        public LiteEntity(LiteWorld world, float width, float height, bool isStatic, LiteVector position)
        {
            if (!LiteBody.CreateBoxBody(width, height, 1f, 0.5f, isStatic, out LiteBody body, out string err))
            {
                throw new Exception(err);
            }

            body.MoveTo(position);
            this.Body = body;
            world.AddBody(body);

            this.Color = RandomHelper.RandomColor();
        }

        public void Draw(Shapes shapes)
        {
            Vector2 position = this.Body.Position.ToVector2();

            if (this.Body.ShapeType == ShapeType.Circle)
            {
                Vector2 vb = new Vector2(Body.Radius, 0);
                vb = Vector2.Transform(vb, Matrix.CreateRotationZ(Body.Angle));
                vb += position;

                shapes.FillCircle(position, this.Body.Radius, 26, this.Color);
                shapes.DrawCircle(position, this.Body.Radius, 26, 2f, Color.White);

                shapes.DrawLine(position, vb, 2f, Color.White);
            }
            else if (this.Body.ShapeType == ShapeType.Box)
            {
                shapes.FillBox(position, this.Body.Width, this.Body.Height, this.Body.Angle, this.Color);
                shapes.DrawBox(position, this.Body.Width, this.Body.Height, 2f, this.Body.Angle, Color.White);

            }
        }
    }
}
