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
        private float angle;
        private float angularVelocity;
        private LiteVector force;

        public readonly ShapeType ShapeType;
        public readonly float Density;
        public readonly float Mass;
        public readonly float InvMass;
        public readonly float Restitution;
        public readonly float Area;
        public readonly float Inertia;
        public readonly float InvInertia;
        public readonly bool IsStatic;
        public readonly float Radius;
        public readonly float Width;
        public readonly float Height;

        private readonly LiteVector[] vertices;
        private LiteVector[] transformedVertices;
        private LiteAABB aabb;

        private bool transformUpdateRequired;
        private bool aabbmUpdateRequired;

        public LiteVector Position
        {
            get { return this.position; }
        }

        public LiteVector LinearVelocity
        {
            get { return this.linearVelocity; }
            internal set { this.linearVelocity = value; }
        }

        public float Angle
        {
            get { return this.angle; }
        }

        public float AngularVelocity
        {
            get { return this.angularVelocity; }
            internal set { this.angularVelocity = value; }
        }

        private LiteBody(float density, float mass, float inertia,
            float restitution, float area, bool isStatic, float radius,
            float width, float height, LiteVector[] vertices, ShapeType shapeType)
        {
            this.position = LiteVector.Zero;
            this.linearVelocity = LiteVector.Zero;
            this.angle = 0f;
            this.angularVelocity = 0f;

            this.force = LiteVector.Zero;

            this.ShapeType = shapeType;
            this.Density = density;
            this.Mass = mass;
            this.InvMass = mass > 0f ? 1 / mass : 0f;
            this.Inertia = inertia;
            this.InvInertia = inertia > 0f ? 1f / inertia : 0f  ;
            this.Restitution = restitution;
            this.Area = area;
            this.IsStatic = isStatic;
            this.Radius = radius;
            this.Width = width;
            this.Height = height;

            if (!this.IsStatic)
            {
                this.InvMass = 1f / this.Mass;
                this.InvInertia = 1f / this.Inertia;
            }
            else
            {
                this.InvMass = 0f;
                this.InvInertia = 0f;
            }

            if (this.ShapeType == ShapeType.Box)
            {
                this.vertices = vertices;
                this.transformedVertices = new LiteVector[this.vertices.Length];
            }
            else
            {
                this.vertices = null;
                this.transformedVertices = null;
            }

            this.transformUpdateRequired = true;
            this.aabbmUpdateRequired = true;
        }

        public LiteVector[] GetTransformedVertices()
        {
            if (this.transformUpdateRequired)
            {
                LiteTransform transfrom = new LiteTransform(this.position, this.angle);

                for(int i=  0; i<this.vertices.Length; i++)
                {
                    LiteVector v = this.vertices[i];
                    this.transformedVertices[i] = LiteVector.Transform(v, transfrom);
                }

                this.transformUpdateRequired = false;
            }

            return this.transformedVertices;
        }

        public LiteAABB GetAABB()
        {
            if (this.aabbmUpdateRequired)
            {
                float minX = float.MaxValue;
                float minY = float.MaxValue;
                float maxX = float.MinValue;
                float maxY = float.MinValue;

                if (this.ShapeType is ShapeType.Box)
                {
                    LiteVector[] vertices = this.GetTransformedVertices();

                    for (int i = 0; i < vertices.Length; i++)
                    {
                        LiteVector v = vertices[i];

                        if (v.X < minX) minX = v.X;
                        if (v.Y < minY) minY = v.Y;
                        if (v.X > maxX) maxX = v.X;
                        if (v.Y > maxY) maxY = v.Y;
                    }

                }
                else if (this.ShapeType is ShapeType.Circle)
                {
                    minX = this.position.X - this.Radius;
                    minY = this.position.Y - this.Radius;
                    maxX = this.position.X + this.Radius;
                    maxY = this.position.Y + this.Radius;
                }
                else
                {
                    throw new Exception("Unknown ShapeType.");
                }

                this.aabb = new LiteAABB(minX, minY, maxX, maxY);            
            }

            this.aabbmUpdateRequired = false;

            return this.aabb;
        }

        internal void Step(float time, LiteVector gravity, int iterations)
        {            
            if (this.IsStatic) return;

            //LiteVector acceleration = this.force / this.Mass;
            //this.linearVelocity += acceleration * time;

            time /= (float)iterations;

            this.linearVelocity += gravity * time;

            this.position += this.linearVelocity * time;
            this.angle += this.angularVelocity * time;

            this.force = LiteVector.Zero;
            this.transformUpdateRequired = true;
            this.aabbmUpdateRequired = true;
        }

        public void Move(LiteVector amount)
        {
            this.position += amount;
            this.transformUpdateRequired = true;
            this.aabbmUpdateRequired = true;
        }

        public void MoveTo(LiteVector position)
        {
            this.position = position;
            this.transformUpdateRequired = true;
            this.aabbmUpdateRequired = true;
        }

        public void Rotate(float amount)
        {
            this.angle += amount;
            this.transformUpdateRequired = true;
            this.aabbmUpdateRequired = true;
        }

        public void RotateTo(float angle)
        {
            this.angle = angle;
            this.transformUpdateRequired = true;
            this.aabbmUpdateRequired = true;
        }

        public void AddForce(LiteVector amount)
        {
            this.force = amount;
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

        public static bool CreateCircleBody(float radius, float density, float restitution, bool isStatic, out LiteBody body, out string error)
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

            if (density < LiteWorld.MinDensity)
            {
                error = $"Density is too small, Min density is {LiteWorld.MinDensity}";
                return false;
            }

            if (density > LiteWorld.MaxDensity)
            {
                error = $"Density is too big, Max density is {LiteWorld.MaxDensity}";
                return false;
            }

            restitution = Math.Clamp(restitution, 0f, 1f);

            float mass = 0;
            float inertia = 0;

            if (!isStatic)
            {
                mass = area * density;
                inertia = (1f / 2) * mass * radius * radius;
            }

            body = new LiteBody(density, mass, inertia, restitution, area, isStatic, radius, 0f, 0f, null, ShapeType.Circle);

            return true;
        }

        public static bool CreateBoxBody(float width, float height, float density, float restitution, bool isStatic, out LiteBody body, out string error)
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

            if (density < LiteWorld.MinDensity)
            {
                error = $"Density is too small, Min density is {LiteWorld.MinDensity}";
                return false;
            }

            if (density > LiteWorld.MaxDensity)
            {
                error = $"Density is too big, Max density is {LiteWorld.MaxDensity}";
                return false;
            }

            restitution = Math.Clamp(restitution, 0f, 1f);

            float mass = 0;
            float inertia = 0;

            if (!isStatic)
            {
                mass = area * density;
                inertia = (1f / 12) * mass * (width * width + height * height);
            }

            LiteVector[] vertices = LiteBody.CreateBoxVertices(width, height);

            body = new LiteBody(density, mass, inertia, restitution, area, isStatic, 0f, width, height, vertices, ShapeType.Box);

            return true;
        }
    }
}
