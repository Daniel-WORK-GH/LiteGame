using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lite.Physics
{
    public readonly struct LiteManifold
    {
        public readonly LiteBody BodyA;
        public readonly LiteBody BodyB;
        public readonly LiteVector Normal;
        public readonly float Depth;
        public readonly LiteVector Contact1;
        public readonly LiteVector Contact2;
        public readonly int ContactCount;

        public LiteManifold(LiteBody bodyA, LiteBody bodyB, LiteVector normal, float depth, LiteVector contact1, LiteVector contact2, int contactCount)
        {
            this.BodyA = bodyA;
            this.BodyB = bodyB;
            this.Normal = normal;
            this.Depth = depth;
            this.Contact1 = contact1;
            this.Contact2 = contact2;
            this.ContactCount = contactCount;
        }
    }
}
