using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lite.Physics
{
    public static class Collisions
    {
        public static bool IntersectPolygons(LiteVector[] verticesA, LiteVector[] verticesB, out LiteVector normal, out float depth)
        {
            normal = LiteVector.Zero;
            depth = float.MaxValue;

            for(int i = 0; i < verticesA.Length; i++)
            {
                LiteVector va = verticesA[i];
                LiteVector vb = verticesA[(i + 1) % verticesA.Length];

                LiteVector edge = vb - va;
                LiteVector axis = new LiteVector(edge.Y, -edge.X);
                axis = axis.Normalize();

                Collisions.ProjectVertices(verticesA, axis, out float minA, out float maxA);
                Collisions.ProjectVertices(verticesB, axis, out float minB, out float maxB);
            
                if(minA >= maxB || minB >= maxA)
                {
                    return false;
                }

                float asixDepth = MathF.Min(maxB - minA, maxA - minB);

                if(asixDepth < depth)
                {
                    depth = asixDepth;
                    normal = axis;
                }
            }

            for (int i = 0; i < verticesB.Length; i++)
            {
                LiteVector va = verticesB[i];
                LiteVector vb = verticesB[(i + 1) % verticesB.Length];

                LiteVector edge = vb - va;
                LiteVector axis = new LiteVector(edge.Y, -edge.X);
                axis = axis.Normalize();

                Collisions.ProjectVertices(verticesA, axis, out float minA, out float maxA);
                Collisions.ProjectVertices(verticesB, axis, out float minB, out float maxB);

                if (minA >= maxB || minB >= maxA)
                {
                    return false;
                }

                float asixDepth = MathF.Min(maxB - minA, maxA - minB);

                if (asixDepth < depth)
                {
                    depth = asixDepth;
                    normal = axis;
                }
            }

            depth /= LiteMath.Length(normal);
            normal = LiteMath.Normalize(normal);

            LiteVector centerA = Collisions.FindArithmeticMean(verticesA);
            LiteVector centerB = Collisions.FindArithmeticMean(verticesB);

            LiteVector direcetion = centerB - centerA;

            if(LiteMath.Dot(direcetion, normal) < 0)
            {
                normal = -normal;
            }

            return true;
        }

        private static LiteVector FindArithmeticMean(LiteVector[] vertices)
        {
            float sumX = 0;
            float sumY = 0;

            for(int i = 0; i < vertices.Length; i++)
            {
                LiteVector v = vertices[i];
                sumX += v.X;
                sumY += v.Y;
            }

            return new LiteVector(sumX / vertices.Length, sumY / vertices.Length);
        }

        private static void ProjectVertices(LiteVector[] vertices, LiteVector axis, out float min, out float max)
        {
            min = float.MaxValue;
            max = float.MinValue;

            for(int i = 0; i < vertices.Length; i++)
            {
                LiteVector v = vertices[i];
                float proj = LiteMath.Dot(v, axis);

                if(proj < min) min = proj;
                if(proj > max) max = proj;
            }
        }

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
