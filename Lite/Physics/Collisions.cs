using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lite.Physics
{
    public static class Collisions
    {
        private static void PointSegmentDistance(LiteVector p, LiteVector a, LiteVector b,
            out float distanceSquared, out LiteVector cp)
        {
            LiteVector ab = b - a;
            LiteVector ap = p - a;

            float proj = LiteMath.Dot(ab, ap);
            float abLenSq = ab.LengthSquared();
            float d = proj / abLenSq;

            if (d < 0f)
            {
                cp = a;
            }
            else if (d >= 1f)
            {
                cp = b;
            }
            else
            {
                cp = a + ab * d;
            }

            distanceSquared = LiteMath.DistanceSquared(p, cp);
        }

        public static bool IntersectAABB(LiteAABB a, LiteAABB b)
        {
            if(a.Max.X <= b.Min.X || b.Max.X <= a.Min.X)
            {
                return false;
            }

            if (a.Max.Y <= b.Min.Y || b.Max.Y <= a.Min.Y)
            {
                return false;
            }

            return true;
        }

        public static void FindContactPoints(LiteBody bodyA, LiteBody bodyB, 
            out LiteVector contact1, out LiteVector contact2, out int contactCount)
        {
            contact1 = contact2 = LiteVector.Zero;
            contactCount = 0;

            ShapeType shapeTypeA = bodyA.ShapeType;
            ShapeType shapeTypeB = bodyB.ShapeType;

            if (shapeTypeA is ShapeType.Box)
            {
                if (shapeTypeB is ShapeType.Box)
                {
                    Collisions.FindPolygonsContactPoints(bodyA.GetTransformedVertices(), bodyB.GetTransformedVertices(),
                        out contact1, out contact2, out contactCount);
                }
                else if (shapeTypeB == ShapeType.Circle)
                {
                    Collisions.FindCirclePolygonContactPoint(bodyB.Position, bodyB.Radius, bodyA.Position, bodyA.GetTransformedVertices(), out contact1);
                    contactCount = 1; 
                }
            }
            if (shapeTypeA is ShapeType.Circle)
            {
                if (shapeTypeB is ShapeType.Box)
                {
                    Collisions.FindCirclePolygonContactPoint(bodyA.Position, bodyA.Radius, bodyB.Position, bodyB.GetTransformedVertices(), out contact1);
                    contactCount = 1;
                }
                else if (shapeTypeB == ShapeType.Circle)
                {
                    Collisions.FindCirclesContactPoint(bodyA.Position, bodyA.Radius, bodyB.Position, out contact1);
                    contactCount = 1;
                }
            }
        }
        
        private static void FindPolygonsContactPoints(LiteVector[] verteciesA, LiteVector[] verteciesB,
            out LiteVector contact1, out LiteVector contact2, out int contactCount)
        {
            contact1 = contact2 = LiteVector.Zero;
            contactCount = 0;

            float minDistSq = float.MaxValue;

            for(int i = 0; i < verteciesA.Length; i++)
            {
                LiteVector p = verteciesA[i];

                for (int j = 0; j < verteciesB.Length; j++)
                {
                    LiteVector va = verteciesB[j];
                    LiteVector vb = verteciesB[(j + 1) % verteciesB.Length];

                    Collisions.PointSegmentDistance(p, va, vb, out float distSq, out LiteVector cp);

                    if (LiteMath.NearlyEqual(distSq, minDistSq))
                    {
                        if (!LiteMath.NearlyEqual(cp, contact1))
                        {
                            contact2 = cp;
                            contactCount = 2;
                        }
                    }
                    else if(distSq < minDistSq)
                    {
                        minDistSq = distSq;
                        contact1 = cp;
                        contactCount = 1;
                    }
                }
            }

            for (int i = 0; i < verteciesB.Length; i++)
            {
                LiteVector p = verteciesB[i];

                for (int j = 0; j < verteciesA.Length; j++)
                {
                    LiteVector va = verteciesA[j];
                    LiteVector vb = verteciesA[(j + 1) % verteciesA.Length];

                    Collisions.PointSegmentDistance(p, va, vb, out float distSq, out LiteVector cp);

                    if (LiteMath.NearlyEqual(distSq, minDistSq))
                    {
                        if (!LiteMath.NearlyEqual(cp, contact1))
                        {
                            contact2 = cp;
                            contactCount = 2;
                        }
                    }
                    else if (distSq < minDistSq)
                    {
                        minDistSq = distSq;
                        contact1 = cp;
                        contactCount = 1;
                    }
                }
            }
        }

        private static void FindCirclePolygonContactPoint(LiteVector circleCenter, float circleRadius, LiteVector polygonCenter,
            LiteVector[] polygonVertices, out LiteVector contact)
        {
            contact = LiteVector.Zero;

            float minDistSq = float.MaxValue;

            for(int i = 0; i < polygonVertices.Length; i++)
            {
                LiteVector va = polygonVertices[i];
                LiteVector vb = polygonVertices[(i + 1) % polygonVertices.Length];

                Collisions.PointSegmentDistance(circleCenter, va, vb,
                    out float distSq, out LiteVector cp);

                if (distSq < minDistSq)
                {
                    minDistSq = distSq;
                    contact = cp;
                }
            }
        }

        private static void FindCirclesContactPoint(LiteVector centerA, float radiusA, LiteVector centerB, out LiteVector contact)
        {
            LiteVector ab = centerB - centerA;
            LiteVector dir = ab.Normalize();
            contact = centerA + dir * radiusA;
        }

        public static bool Collide(LiteBody bodyA, LiteBody bodyB, out LiteVector normal, out float depth)
        {
            normal = LiteVector.Zero;
            depth = 0;

            ShapeType shapeTypeA = bodyA.ShapeType;
            ShapeType shapeTypeB = bodyB.ShapeType;

            if (shapeTypeA is ShapeType.Box)
            {
                if (shapeTypeB is ShapeType.Box)
                {
                    return Collisions.IntersectPolygons(
                        bodyA.Position, bodyA.GetTransformedVertices(),
                        bodyB.Position, bodyB.GetTransformedVertices(),
                        out normal, out depth);
                }
                else if (shapeTypeB == ShapeType.Circle)
                {
                    bool result = Collisions.IntersectCirclePolygon(
                        bodyB.Position, bodyB.Radius,
                        bodyA.Position, bodyA.GetTransformedVertices(),
                        out normal, out depth);

                    normal = -normal;
                    return result;
                }
            }
            if (shapeTypeA is ShapeType.Circle)
            {
                if (shapeTypeB is ShapeType.Box)
                {
                    return Collisions.IntersectCirclePolygon(
                        bodyA.Position, bodyA.Radius,
                        bodyB.Position, bodyB.GetTransformedVertices(),
                        out normal, out depth);
                }
                else if (shapeTypeB == ShapeType.Circle)
                {
                    return Collisions.IntersectCircles(bodyA.Position, bodyA.Radius, bodyB.Position, bodyB.Radius,
                        out normal, out depth);
                }
            }

            return false;
        }

        public static bool IntersectCirclePolygon(LiteVector circleCenter, float circleRadius, LiteVector polygonCenter,
            LiteVector[] vertices, out LiteVector normal, out float depth)
        {
            normal = LiteVector.Zero;
            depth = float.MaxValue;

            LiteVector axis = LiteVector.Zero;
            float asixDepth = 0;
            float minA, minB, maxA, maxB;

            for (int i = 0; i < vertices.Length; i++)
            {
                LiteVector va = vertices[i];
                LiteVector vb = vertices[(i + 1) % vertices.Length];

                LiteVector edge = vb - va;
                axis = new LiteVector(edge.Y, -edge.X);
                axis = axis.Normalize();

                Collisions.ProjectVertices(vertices, axis, out minA, out maxA);
                Collisions.ProjectCircle(circleCenter, circleRadius, axis, out minB, out maxB);

                if (minA >= maxB || minB >= maxA)
                {
                    return false;
                }

                asixDepth = MathF.Min(maxB - minA, maxA - minB);

                if (asixDepth < depth)
                {
                    depth = asixDepth;
                    normal = axis;
                }
            }

            int cpIndex = Collisions.FindClosestPointOnPolygon(circleCenter, vertices);
            LiteVector cp = vertices[cpIndex];

            axis = cp - circleCenter;
            axis.Normalize();

            Collisions.ProjectVertices(vertices, axis, out minA, out maxA);
            Collisions.ProjectCircle(circleCenter, circleRadius, axis, out minB, out maxB);

            if (minA >= maxB || minB >= maxA)
            {
                return false;
            }

            asixDepth = MathF.Min(maxB - minA, maxA - minB);

            if (asixDepth < depth)
            {
                depth = asixDepth;
                normal = axis;
            }

            LiteVector direcetion = polygonCenter - circleCenter;

            if (LiteMath.Dot(direcetion, normal) < 0)
            {
                normal = -normal;
            }

            return true;
        }

        private static int FindClosestPointOnPolygon(LiteVector circleCenter, LiteVector[] vertices)
        {
            int result = -1;
            float minDistance = float.MaxValue;

            for(int i = 0; i < vertices.Length; i++)
            {
                LiteVector v = vertices[i];
                float distance = LiteMath.Distance(v, circleCenter);

                if(distance < minDistance)
                {
                    minDistance = distance;
                    result = i;
                }
            }

            return result;
        }

        private static void ProjectCircle(LiteVector center, float radius, LiteVector axis, out float min, out float max)
        {
            LiteVector direction = LiteMath.Normalize(axis);
            LiteVector directionRadius = direction * radius;

            LiteVector p1 = center + directionRadius;
            LiteVector p2 = center - directionRadius;

            min = LiteMath.Dot(p1, axis);
            max = LiteMath.Dot(p2, axis);

            if(min > max)
            {
                float temp = min;
                min = max;
                max = temp;
            }
        }

        public static bool IntersectPolygons(LiteVector centerA, LiteVector[] verticesA, LiteVector centerB, LiteVector[] verticesB,
            out LiteVector normal, out float depth)
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

            LiteVector direcetion = centerB - centerA;

            if(LiteMath.Dot(direcetion, normal) < 0)
            {
                normal = -normal;
            }

            return true;
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
