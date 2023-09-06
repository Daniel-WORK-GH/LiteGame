using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lite
{
    public readonly struct LiteVector
    {
        public readonly float X;
        public readonly float Y;

        public static readonly LiteVector Zero = new LiteVector(0f, 0f);
        public static readonly LiteVector One = new LiteVector(1f, 1f);

        public LiteVector(float x, float y)
        {
            this.X = x;
            this.Y = y; 
        }

        internal static LiteVector Transform(LiteVector v, LiteTransform transform)
        {
            return new LiteVector(
                transform.Cos * v.X - transform.Sin * v.Y + transform.PositionX,
                transform.Sin * v.X + transform.Cos * v.Y + transform.PositionY);
        }

        public static LiteVector operator +(LiteVector a, LiteVector b) => new LiteVector(a.X + b.X, a.Y + b.Y);
        public static LiteVector operator -(LiteVector a, LiteVector b) => new LiteVector(a.X - b.X, a.Y - b.Y);
        public static LiteVector operator -(LiteVector v) => new LiteVector(-v.X, -v.Y);
        public static LiteVector operator *(LiteVector v, float s) => new LiteVector(v.X * s, v.Y * s);
        public static LiteVector operator *(float s, LiteVector v) => new LiteVector(v.X * s, v.Y * s);
        public static LiteVector operator /(LiteVector v, float s) => new LiteVector(v.X / s, v.Y / s);
    }
}
