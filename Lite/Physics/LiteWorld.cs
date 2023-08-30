using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lite.Physics
{
    internal class LiteWorld
    {
        public const float MinBodySize = 0.01f * 0.1f;
        public const float MaxBodySize = 64f * 64f;

        public const float MinDensity = 0.5f; // g/cm^3
        public const float MaxDensity = 22f;
    }
}
