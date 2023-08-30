using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lite.Physics
{
    public enum ShapeType
    {
        Circle = 0,
        Box = 1,
        Polygon = 2,
    }

    public class LiteBody
    {
        private LiteVector position;
        private LiteVector linearVelocity;
        private float rotation;
        private float rotationalVelocity;

        public readonly float Density;
        public readonly float Mass;
        public readonly float Restitution;
        public readonly float Area;

        public readonly bool IsStatic;

        public readonly float Raidus;
        public readonly float Width;
        public readonly float Height;

        public readonly ShapeType ShapeType;

        public LiteVector Position
        {
            get { return position; }
        }

        private LiteBody(LiteVector position, float density, float mass,
            float restitution, float area, bool isStatic, float radius,
            float width, float height, ShapeType shapeType)
        {
            this.position = position;
            this.linearVelocity = LiteVector.Zero;
            this.rotation = 0;
            this.rotationalVelocity = 0;

            this.Density = density;
            this.Mass = mass;
            this.Restitution = restitution;
            this.Area = area;

            this.IsStatic = isStatic;

            this.Raidus = radius;
            this.Width = width;
            this.Height = height;
            this.ShapeType = shapeType;
        }

        public static bool CreateCircleBody(float radius, LiteVector position, float density, float restitution, bool isStatic, out LiteBody body, out string error)
        {
            body = null;
            error = string.Empty;

            float area = radius * MathF.PI;

            if(area < LiteWorld.MinBodySize)
            {
                error = $"Area is too small, Min area is {LiteWorld.MinBodySize}";
                return false;
            }

            if (area > LiteWorld.MaxBodySize)
            {
                error = $"Area is too big, Max area is {LiteWorld.MaxBodySize}";
                return false;
            }

            if (area < LiteWorld.MinDensity)
            {
                error = $"Density is too small, Min density is {LiteWorld.MinDensity}";
                return false;
            }

            if (area > LiteWorld.MaxDensity)
            {
                error = $"Density is too big, Max density is {LiteWorld.MaxDensity}";
                return false;
            }

            restitution = Math.Clamp(restitution, 0f, 1f);

            // M = area * depth * density
            float mass = area * density;

            body = new LiteBody(position, density, mass, restitution, area, isStatic, radius, 0f, 0f, ShapeType.Circle);

            return true;
        }

        public static bool CreateBoxBody(float width, float height, LiteVector position, float density, float restitution, bool isStatic, out LiteBody body, out string error)
        {
            body = null;
            error = string.Empty;

            float area = width * height;

            if (area < LiteWorld.MinBodySize)
            {
                error = $"Area is too small, Min area is {LiteWorld.MinBodySize}";
                return false;
            }

            if (area > LiteWorld.MaxBodySize)
            {
                error = $"Area is too big, Max area is {LiteWorld.MaxBodySize}";
                return false;
            }

            if (area < LiteWorld.MinDensity)
            {
                error = $"Density is too small, Min density is {LiteWorld.MinDensity}";
                return false;
            }

            if (area > LiteWorld.MaxDensity)
            {
                error = $"Density is too big, Max density is {LiteWorld.MaxDensity}";
                return false;
            }

            restitution = Math.Clamp(restitution, 0f, 1f);

            // M = area * depth * density
            float mass = area * density;

            body = new LiteBody(position, density, mass, restitution, area, isStatic, 0f, width, height, ShapeType.Box);

            return true;
        }
    }
}
