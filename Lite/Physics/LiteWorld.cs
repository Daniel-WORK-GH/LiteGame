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

        public const int MinIterations = 1;
        public const int MaxIterations = 128;

        private List<LiteBody> bodylist;
        private LiteVector gravity;
        private List<LiteManifold> contactList;

        public List<LiteVector> ContactPointsList;

        public int BodyCount
        {
            get { return this.bodylist.Count; }
        }

        public LiteWorld()
        {
            this.gravity = new LiteVector(0f, 9.81f);
            this.bodylist = new List<LiteBody>();
            this.contactList = new List<LiteManifold>();
            this.ContactPointsList = new List<LiteVector>();
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

        public void Step(float time, int iterations)
        {
            iterations = Math.Clamp(iterations, LiteWorld.MinIterations, LiteWorld.MaxIterations);

            this.ContactPointsList.Clear();

            for (int it = 0; it < iterations; it++)
            {
                // Movement step
                for (int i = 0; i < this.bodylist.Count; i++)
                {
                    this.bodylist[i].Step(time, gravity, iterations);
                }

                this.contactList.Clear();

                // Collision step
                for (int i = 0; i < this.bodylist.Count; i++)
                {
                    LiteBody bodyA = this.bodylist[i];
                    LiteAABB bodyA_aabb = bodyA.GetAABB();

                    for (int j = i + 1; j < this.bodylist.Count; j++)
                    {
                        LiteBody bodyB = this.bodylist[j];
                        LiteAABB bodyB_aabb = bodyB.GetAABB();

                        if (bodyA.IsStatic && bodyB.IsStatic)
                        {
                            continue;
                        }

                        if(!Collisions.IntersectAABB(bodyA_aabb, bodyB_aabb))
                        {
                            continue;
                        }

                        if (Collisions.Collide(bodyA, bodyB, out LiteVector normal, out float depth))
                        {
                            if (bodyA.IsStatic)
                            {
                                bodyB.Move(normal * depth);
                            }
                            else if (bodyB.IsStatic)
                            {
                                bodyA.Move(-normal * depth);
                            }
                            else
                            {
                                bodyA.Move(-normal * depth / 2);
                                bodyB.Move(normal * depth / 2);
                            }

                            Collisions.FindContactPoints(bodyA, bodyB, out LiteVector contact1,
                                out LiteVector contact2, out int contactCount);

                            LiteManifold contact = new LiteManifold(bodyA, bodyB, normal, depth,
                                contact1, contact2, contactCount);
                            this.contactList.Add(contact);
                        }
                    }
                }

                for(int i = 0; i < this.contactList.Count; i++)
                {
                    LiteManifold contact =  this.contactList[i];
                    this.ResolveCollision(contact);

                    if (contact.ContactCount > 0)
                    {
                        this.ContactPointsList.Add(contact.Contact1);

                        if (contact.ContactCount > 1)
                        {     
                            this.ContactPointsList.Add(contact.Contact2);                       
                        }
                    }
                }
            }
        }

        private void ResolveCollision(in LiteManifold contact)
        {
            LiteBody bodyA = contact.BodyA;
            LiteBody bodyB = contact.BodyB;
            LiteVector normal = contact.Normal;
            float depth = contact.Depth;

            LiteVector relativeVelocity = bodyB.LinearVelocity - bodyA.LinearVelocity;

            if(LiteMath.Dot(relativeVelocity, normal) > 0f)
            {
                return;
            }

            float e = MathF.Min(bodyA.Restitution, bodyB.Restitution);

            float j = -(1f + e) * LiteMath.Dot(relativeVelocity, normal);
            j /= bodyA.InvMass + bodyB.InvMass;

            LiteVector impulse = j * normal;

            bodyA.LinearVelocity -= impulse * bodyA.InvMass;
            bodyB.LinearVelocity += impulse * bodyB.InvMass;
        }  
    }
}
