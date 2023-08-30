using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lite
{
    public static class LiteMath
    {
        public static float Length(this LiteVector v)
        {
            return MathF.Sqrt(v.LengthSquared());
        }

        public static float LengthSquared(this LiteVector v)
        {
            return v.X * v.X + v.Y * v.Y;
        }

        public static float Distance(LiteVector a, LiteVector b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            return MathF.Sqrt(dx * dx + dy * dy);
        }

        public static LiteVector Normalize(this LiteVector v)
        {
            return new LiteVector(v.X, v.Y) / v.Length();
        }

        public static float Dot(LiteVector a, LiteVector b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        public static float Cross(LiteVector a, LiteVector b)
        {
            return a.X * b.Y - a.Y * b.X;
        }
    }
}
