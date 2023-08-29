using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lite
{
    internal readonly struct LiteTransform
    {
        public readonly float PositionX;
        public readonly float PositionY;
        public readonly float Sin;
        public readonly float Cos;

        public readonly static LiteTransform Zero = new LiteTransform(0f, 0f, 0f);

        public LiteTransform(LiteVector position, float angle)
        {
            this.PositionX = position.X;
            this.PositionY = position.Y;
            this.Sin = MathF.Sin(angle);
            this.Cos = MathF.Cos(angle);
        }

        public LiteTransform(float x, float y, float angle)
        {
            this.PositionX = x;
            this.PositionY = y;
            this.Sin = MathF.Sin(angle);
            this.Cos = MathF.Cos(angle);
        }
    }
}
