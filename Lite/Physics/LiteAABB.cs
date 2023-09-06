using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lite.Physics
{
    public readonly struct LiteAABB
    {
        public readonly LiteVector Min;
        public readonly LiteVector Max;

        public LiteAABB(LiteVector min, LiteVector max)
        {
            this.Min = min;
            this.Max = max;
        }

        public LiteAABB(float minX, float minY, float maxX, float maxY)
        {
            this.Min = new LiteVector(minX, minY);
            this.Max = new LiteVector(maxX, maxY);
        }
    }
}
