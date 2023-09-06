using Lite.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lite
{
    public static class LiteMath
    {
        public const float HalfAMillimeter = 0.0005f;

        public static float Length(this LiteVector v)
        {
            return MathF.Sqrt(v.LengthSquared());
        }

        public static float LengthSquared(this LiteVector v)
        {
            return v.X * v.X + v.Y * v.Y;
        }

        public static float DistanceSquared(LiteVector a, LiteVector b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            return dx * dx + dy * dy;
        }

        public static float Distance(LiteVector a, LiteVector b)
        {
            return MathF.Sqrt(DistanceSquared(a, b));
        }

        public static LiteVector Normalize(this LiteVector v)
        {
            float len = v.Length();
            if (LiteMath.NearlyEqual(len, 0))
            {
                return v;
            }
            return v / v.Length();
        }

        public static float Dot(LiteVector a, LiteVector b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        public static float Cross(LiteVector a, LiteVector b)
        {
            return a.X * b.Y - a.Y * b.X;
        }

        public static bool NearlyEqual(float a, float b)
        {
            return Math.Abs(a - b) < LiteMath.HalfAMillimeter;
        }

        public static bool NearlyEqual(LiteVector a, LiteVector b)
        {
            return LiteMath.DistanceSquared(a, b) < LiteMath.HalfAMillimeter * LiteMath.HalfAMillimeter;
        }
    }
}
