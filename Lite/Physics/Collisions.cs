using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lite.Physics
{
    public static class Collisions
    {
        public static bool IntersectCircles(
            LiteVector centerA, float radiusA, LiteVector centerB, float radiusB,
            out LiteVector normal, out float depth)
        {
            normal = LiteVector.Zero;
            depth = 0;

            float distance = LiteMath.Distance(centerA, centerB);
            float radii = radiusA + radiusB;

            if(distance >= radii)
            {
                return false;
            }

            normal = LiteMath.Normalize(centerB - centerA);
            depth = radii - distance;

            return true;
        }
    }
}
