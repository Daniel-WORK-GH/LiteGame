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
    public static class RandomHelper
    {
        private static Random random = new Random();

        static RandomHelper()
        {
            random = new Random(Guid.NewGuid().GetHashCode());
        }

        public static int RandomInteger(int min, int max)
        {
            return random.Next(min, max);
        }

        public static float RandomSingle(float min, float max)
        {
            if (min > max) throw new ArgumentException("Min can't be bigger then Max");
            return random.NextSingle() * (max - min) + min;
        }

        public static bool RandomBoolean()
        {
            int value = random.Next(0, 2);
            return value == 1;
        }

        public static Color RandomColor()
        {
            return new Color(
                RandomInteger(0, 256),
                RandomInteger(0, 256),
                RandomInteger(0, 256)
                );
        }
    }
}
