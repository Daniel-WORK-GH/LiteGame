using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Lite
{
    public static class LiteConverter
    {
        public static Vector2 ToVector2(this LiteVector v)
        {
            return new Vector2(v.X, v.Y);
        }

        public static LiteVector ToLiteVector(this Vector2 v)
        {
            return new LiteVector(v.X, v.Y);
        }
    }
}
