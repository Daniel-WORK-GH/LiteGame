using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
