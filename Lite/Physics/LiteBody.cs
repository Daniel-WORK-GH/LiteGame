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

        private readonly LiteVector[] vertices;
        public int[] Triangles;
        private LiteVector[] transformedVertices;
        private bool transformUpdateRequired;

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

            if (this.ShapeType == ShapeType.Box)
            {
                this.vertices = LiteBody.CreateBoxVertices(this.Width, this.Height);
                this.Triangles = CreateBoxTriangles();
                this.transformedVertices = new LiteVector[this.vertices.Length];
            }
            else
            {
                this.vertices = null;
                this.Triangles = null;
                this.transformedVertices = null;
            }

            this.transformUpdateRequired = true;
        }

        public LiteVector[] GetTransformedVertices()
        {
            if (this.transformUpdateRequired)
            {
                LiteTransform transfrom = new LiteTransform(this.position, this.rotation);

                for(int i=  0; i<this.vertices.Length; i++)
                {
                    LiteVector v = this.vertices[i];
                    this.transformedVertices[i] = LiteVector.Transform(v, transfrom);
                }

                this.transformUpdateRequired = false;
            }

            return this.transformedVertices;
        }

        public void Move(LiteVector amount)
        {
            this.position += amount;
            this.transformUpdateRequired = true;
        }

        public void Rotate(float amount)
        {
            this.rotation += amount;
            this.transformUpdateRequired = true;
        }

        private static LiteVector[] CreateBoxVertices(float width, float height)
        {
            float left = -width / 2;
            float right = left + width;
            float top = -height / 2;
            float bottom = top + height;

            LiteVector[] vertices = new LiteVector[4];
            vertices[0] = new LiteVector(left, top);
            vertices[1] = new LiteVector(right, top);
            vertices[2] = new LiteVector(right, bottom);
            vertices[3] = new LiteVector(left, bottom);

            return vertices;
        }

        private static int[] CreateBoxTriangles()
        {
            int[] triangles = new int[6];
            triangles[0] = 0;
            triangles[1] = 1;
            triangles[2] = 2;
            triangles[3] = 0;
            triangles[4] = 2;
            triangles[5] = 3;
            return triangles;
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
