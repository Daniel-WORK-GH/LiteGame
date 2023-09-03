using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lite.Physics
{
    public class LiteWorld
    {
        public const float MinBodySize = 0.01f * 0.1f;
        public const float MaxBodySize = 64f * 64f;

        public const float MinDensity = 0.5f; // g/cm^3
        public const float MaxDensity = 22f;

        private List<LiteBody> bodylist;
        private LiteVector gravity;

        public int BodyCount
        {
            get { return this.bodylist.Count; }
        }

        public LiteWorld()
        {
            this.gravity = new LiteVector(0f, 9.81f);
            this.bodylist = new List<LiteBody>();
        }

        public void AddBody(LiteBody body)
        {
            this.bodylist.Add(body);
        }

        public bool RemoveBody(LiteBody body)
        {
            return this.bodylist.Remove(body);
        }

        public bool GetBody(int index, out LiteBody body)
        {
            if(index < 0 || index >= this.bodylist.Count)
            {
                body = null;
                return false;
            }

            body = this.bodylist[index];
            return true;
        }

        public void Step(float time)
        {
            // Movement step
            for (int i = 0; i < this.bodylist.Count; i++)
            {
                this.bodylist[i].Step(time);
            }

            // Collision step
            for (int i = 0; i < this.bodylist.Count; i++)
            {
                LiteBody bodyA = this.bodylist[i];

                for (int j = i + 1; j < this.bodylist.Count; j++)
                {
                    LiteBody bodyB = this.bodylist[j];

                    if(Collide(bodyA, bodyB, out LiteVector normal, out float depth))
                    {
                        bodyA.Move(-normal * depth / 2);   
                        bodyB.Move(normal * depth / 2);

                        this.ResolveCollision(bodyA, bodyB, normal, depth);
                    }
                }
            }
        }

        private void ResolveCollision(LiteBody bodyA, LiteBody bodyB, LiteVector normal, float depth)
        {
            LiteVector relativeVelocity = bodyB.LinearVelocity - bodyA.LinearVelocity;

            float e = MathF.Min(bodyA.Restitution, bodyB.Restitution);

            float j = -(1f + e) * LiteMath.Dot(relativeVelocity, normal);
            j /= (1 / bodyA.Mass) + (1 / bodyB.Mass);

            bodyA.LinearVelocity -= j / bodyA.Mass * normal;
            bodyB.LinearVelocity += j / bodyB.Mass * normal;
        }

        private bool Collide(LiteBody bodyA, LiteBody bodyB, out LiteVector normal, out float depth)
        {
            normal = LiteVector.Zero;
            depth = 0;

            ShapeType shapeTypeA = bodyA.ShapeType;
            ShapeType shapeTypeB = bodyB.ShapeType;

            if(shapeTypeA is ShapeType.Box)
            {
                if(shapeTypeB is ShapeType.Box)
                {
                    return Collisions.IntersectPolygons(bodyA.GetTransformedVertices(), bodyB.GetTransformedVertices(), 
                        out normal, out depth);
                }
                else if(shapeTypeB == ShapeType.Circle)
                {
                    bool result = Collisions.IntersectCirclePolygon(bodyB.Position, bodyB.Raidus, bodyA.GetTransformedVertices(),
                        out normal, out depth);

                    normal = -normal;
                    return result;
                }
            }
            if (shapeTypeA is ShapeType.Circle)
            {
                if (shapeTypeB is ShapeType.Box)
                {
                    return Collisions.IntersectCirclePolygon(bodyA.Position, bodyA.Raidus, bodyB.GetTransformedVertices(),
                        out normal, out depth);
                }
                else if (shapeTypeB == ShapeType.Circle)
                {
                    return Collisions.IntersectCircles(bodyA.Position, bodyA.Raidus, bodyB.Position, bodyB.Raidus,
                        out normal, out depth);
                }
            }

            return false;
        }
    }
}
