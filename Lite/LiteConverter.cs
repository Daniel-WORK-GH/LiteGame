using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lite.Physics;
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

        public static void ToVector2Array(this LiteVector[] src, ref Vector2[] dest)
        {
            if(dest is null || src.Length != dest.Length)
            {
                dest = new Vector2[src.Length];
            }

            for(int i = 0; i < src.Length; i++)
            {
                LiteVector v = src[i];
                dest[i] = v.ToVector2();
            }
        }
    }
}
