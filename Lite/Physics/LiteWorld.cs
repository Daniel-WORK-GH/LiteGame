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
        private List<(int, int)> contactPairs;


        private LiteVector[] contactList;
        private LiteVector[] impulseList;
        private LiteVector[] raList;
        private LiteVector[] rbList;

        public int BodyCount
        {
            get { return this.bodylist.Count; }
        }

        public LiteWorld()
        {
            this.gravity = new LiteVector(0f, 9.81f);
            this.bodylist = new List<LiteBody>();
            this.contactPairs = new List<(int, int)>();

            this.contactList = new LiteVector[2];
            this.impulseList = new LiteVector[2];
            this.raList = new LiteVector[2];
            this.rbList = new LiteVector[2];
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
            if (index < 0 || index >= this.bodylist.Count)
            {
                body = null;
                return false;
            }

            body = this.bodylist[index];
            return true;
        }

        public void Step(float time, int totalIterations)
        {
            totalIterations = Math.Clamp(totalIterations, LiteWorld.MinIterations, LiteWorld.MaxIterations);

            for (int currentIteration = 0; currentIteration < totalIterations; currentIteration++)
            {
                this.contactPairs.Clear();
                this.StepBodies(time, totalIterations);         
                this.BroadPhase();
                this.NarrowPhase();
            }
        }

        private void BroadPhase()
        {
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

                    if (!Collisions.IntersectAABB(bodyA_aabb, bodyB_aabb))
                    {
                        continue;
                    }

                    this.contactPairs.Add((i, j));
                }
            }
        }

        private void NarrowPhase()
        {
            for (int i = 0; i < this.contactPairs.Count; i++)
            {
                (int, int) pair = this.contactPairs[i];
                LiteBody bodyA = this.bodylist[pair.Item1];
                LiteBody bodyB = this.bodylist[pair.Item2];

                if (Collisions.Collide(bodyA, bodyB, out LiteVector normal, out float depth))
                {
                    this.SeparateBodies(bodyA, bodyB, normal * depth);

                    Collisions.FindContactPoints(bodyA, bodyB, out LiteVector contact1,
                        out LiteVector contact2, out int contactCount);

                    LiteManifold contact = new LiteManifold(bodyA, bodyB, normal, depth,
                        contact1, contact2, contactCount);
                    this.ResolveCollisionWithRotation(in contact);
                }

            }
        }

        private void SeparateBodies(LiteBody bodyA, LiteBody bodyB, LiteVector mtv)
        {
            if (bodyA.IsStatic)
            {
                bodyB.Move(mtv);
            }
            else if (bodyB.IsStatic)
            {
                bodyA.Move(-mtv);
            }
            else
            {
                bodyA.Move(-mtv / 2);
                bodyB.Move(mtv / 2);
            }
        }

        private void StepBodies(float time, int totalIterations)
        {
            for (int i = 0; i < this.bodylist.Count; i++)
            {
                this.bodylist[i].Step(time, gravity, totalIterations);
            }
        }

        private void ResolveCollisionBasic(in LiteManifold contact)
        {
            LiteBody bodyA = contact.BodyA;
            LiteBody bodyB = contact.BodyB;
            LiteVector normal = contact.Normal;
            float depth = contact.Depth;

            LiteVector relativeVelocity = bodyB.LinearVelocity - bodyA.LinearVelocity;

            if (LiteMath.Dot(relativeVelocity, normal) > 0f)
            {
                return;
            }

            float e = MathF.Min(bodyA.Restitution, bodyB.Restitution);

            float j = -(1f + e) * LiteMath.Dot(relativeVelocity, normal);
            j /= bodyA.InvMass + bodyB.InvMass;

            LiteVector impulse = j * normal;

            bodyA.LinearVelocity += -impulse * bodyA.InvMass;
            bodyB.LinearVelocity += impulse * bodyB.InvMass;
        }

        private void ResolveCollisionWithRotation(in LiteManifold contact)
        {
            LiteBody bodyA = contact.BodyA;
            LiteBody bodyB = contact.BodyB;
            LiteVector normal = contact.Normal;
            LiteVector contact1 = contact.Contact1;
            LiteVector contact2 = contact.Contact2;
            int contactCount = contact.ContactCount;

            float e = MathF.Min(bodyA.Restitution, bodyB.Restitution);

            this.contactList[0] = contact1;
            this.contactList[1] = contact2;

            for(int i = 0; i < contactCount; i++)
            {
                this.impulseList[i] = LiteVector.Zero;
                this.raList[i] = LiteVector.Zero;
                this.rbList[i] = LiteVector.Zero;
            }

            for (int i = 0; i < contactCount; i++)
            {
                LiteVector ra = contactList[i] - bodyA.Position;
                LiteVector rb = contactList[i] - bodyB.Position;

                raList[i] = ra;
                rbList[i] = rb;

                LiteVector raPerp = new LiteVector(-ra.Y, ra.X);
                LiteVector rbPerp = new LiteVector(-rb.Y, rb.X);

                LiteVector angularLinerVelocityA = raPerp * bodyA.AngularVelocity;
                LiteVector angularLinerVelocityB = rbPerp * bodyB.AngularVelocity;

                LiteVector relativeVelocity =
                    (bodyB.LinearVelocity + angularLinerVelocityB) -
                    (bodyA.LinearVelocity + angularLinerVelocityA);

                float contactVelocityMag = LiteMath.Dot(relativeVelocity, normal);

                if (contactVelocityMag > 0f)
                {
                    continue;
                }

                float raPrepDotN = LiteMath.Dot(raPerp, normal);
                float rbPrepDotN = LiteMath.Dot(rbPerp, normal);

                float denom = (bodyA.InvMass + bodyB.InvMass) + 
                    (raPrepDotN * raPrepDotN) * bodyA.InvInertia + 
                    (rbPrepDotN * rbPrepDotN) * bodyB.InvInertia;

                float j = -(1f + e) * contactVelocityMag;
                j /= denom;
                j /= (float)contactCount;

                LiteVector impulse = j * normal;
                impulseList[i] = impulse;
            }

            for(int i = 0; i < contactCount; i++)
            {
                LiteVector impulse = impulseList[i];
                LiteVector ra = raList[i];
                LiteVector rb = rbList[i];

                bodyA.LinearVelocity += -impulse * bodyA.InvMass;
                bodyA.AngularVelocity += -LiteMath.Cross(ra, impulse) * bodyA.InvInertia;
                bodyB.LinearVelocity += impulse * bodyB.InvMass;
                bodyB.AngularVelocity += LiteMath.Cross(rb, impulse) * bodyB.InvInertia;
            }
        }
    }
}
