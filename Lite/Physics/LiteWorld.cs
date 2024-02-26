using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lite.Physics
{ 
    public enum CollisionResolveMode
    {
        None = 0,
        CollisionOnly,
        Basic,
        Rotation,
        RotationFriction,
    }

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
        private LiteVector[] frictionImpulseList;
        private LiteVector[] raList;
        private LiteVector[] rbList;
        private float[] jlist;

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
            this.frictionImpulseList = new LiteVector[2];
            this.raList = new LiteVector[2];
            this.rbList = new LiteVector[2];
            this.jlist = new float[2];
        }

        public void SetGravity(LiteVector gravity)
        {
            this.gravity = gravity;
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

        public void Step(float time, int totalIterations, CollisionResolveMode mode = CollisionResolveMode.Basic)
        {
            totalIterations = Math.Clamp(totalIterations, LiteWorld.MinIterations, LiteWorld.MaxIterations);

            for (int currentIteration = 0; currentIteration < totalIterations; currentIteration++)
            {
                this.contactPairs.Clear();
                this.StepBodies(time, totalIterations);         
                this.BroadPhase(mode);
                this.NarrowPhase(mode);
            }
        }

        private void BroadPhase(CollisionResolveMode mode)
        {
            if (mode == CollisionResolveMode.None) return;

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

        private void NarrowPhase(CollisionResolveMode mode)
        {
            if (mode == CollisionResolveMode.None) return;

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

                    if(mode == CollisionResolveMode.Basic) 
                    {
                        this.ResolveCollisionBasic(in contact);
                    }
                    else if(mode == CollisionResolveMode.Rotation)
                    {
                        this.ResolveCollisionWithRotation(in contact);
                    }
                    else if (mode == CollisionResolveMode.RotationFriction)
                    {
                        this.ResolveCollisionWithRotationAndFriction(in contact);
                    }
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

                this.raList[i] = ra;
                this.rbList[i] = rb;

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
                this.impulseList[i] = impulse;
            }

            for(int i = 0; i < contactCount; i++)
            {
                LiteVector impulse = this.impulseList[i];
                LiteVector ra = this.raList[i];
                LiteVector rb = this.rbList[i];

                bodyA.LinearVelocity += -impulse * bodyA.InvMass;
                bodyA.AngularVelocity += -LiteMath.Cross(ra, impulse) * bodyA.InvInertia;
                bodyB.LinearVelocity += impulse * bodyB.InvMass;
                bodyB.AngularVelocity += LiteMath.Cross(rb, impulse) * bodyB.InvInertia;
            }
        }

        private void ResolveCollisionWithRotationAndFriction(in LiteManifold contact)
        {
            LiteBody bodyA = contact.BodyA;
            LiteBody bodyB = contact.BodyB;
            LiteVector normal = contact.Normal;
            LiteVector contact1 = contact.Contact1;
            LiteVector contact2 = contact.Contact2;
            int contactCount = contact.ContactCount;

            float e = MathF.Min(bodyA.Restitution, bodyB.Restitution);

            float sf = (bodyA.StaticFriction + bodyB.StaticFriction) / 2f;
            float df = (bodyA.DynamicFriction + bodyB.DynamicFriction) / 2f;

            this.contactList[0] = contact1;
            this.contactList[1] = contact2;

            for (int i = 0; i < contactCount; i++)
            {
                this.impulseList[i] = LiteVector.Zero;
                this.raList[i] = LiteVector.Zero;
                this.rbList[i] = LiteVector.Zero;
                this.frictionImpulseList[i] = LiteVector.Zero;
                this.jlist[i] = 0f;
            }

            for (int i = 0; i < contactCount; i++)
            {
                LiteVector ra = contactList[i] - bodyA.Position;
                LiteVector rb = contactList[i] - bodyB.Position;

                this.raList[i] = ra;
                this.rbList[i] = rb;

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

                jlist[i] = j;

                LiteVector impulse = j * normal;
                this.impulseList[i] = impulse;
            }

            for (int i = 0; i < contactCount; i++)
            {
                LiteVector impulse = this.impulseList[i];
                LiteVector ra = this.raList[i];
                LiteVector rb = this.rbList[i];

                bodyA.LinearVelocity += -impulse * bodyA.InvMass;
                bodyA.AngularVelocity += -LiteMath.Cross(ra, impulse) * bodyA.InvInertia;
                bodyB.LinearVelocity += impulse * bodyB.InvMass;
                bodyB.AngularVelocity += LiteMath.Cross(rb, impulse) * bodyB.InvInertia;
            }

            // friction impulses 
            for (int i = 0; i < contactCount; i++)
            {
                LiteVector ra = contactList[i] - bodyA.Position;
                LiteVector rb = contactList[i] - bodyB.Position;

                this.raList[i] = ra;
                this.rbList[i] = rb;

                LiteVector raPerp = new LiteVector(-ra.Y, ra.X);
                LiteVector rbPerp = new LiteVector(-rb.Y, rb.X);

                LiteVector angularLinerVelocityA = raPerp * bodyA.AngularVelocity;
                LiteVector angularLinerVelocityB = rbPerp * bodyB.AngularVelocity;

                LiteVector relativeVelocity =
                    (bodyB.LinearVelocity + angularLinerVelocityB) -
                    (bodyA.LinearVelocity + angularLinerVelocityA);

                LiteVector tangent = relativeVelocity - LiteMath.Dot(relativeVelocity, normal) * normal;

                if(LiteMath.NearlyEqual(relativeVelocity, LiteVector.Zero))
                {
                    continue;
                }

                tangent = tangent.Normalize();

                float raPrepDotT = LiteMath.Dot(raPerp, tangent);
                float rbPrepDotT = LiteMath.Dot(rbPerp, tangent);

                float denom = (bodyA.InvMass + bodyB.InvMass) +
                    (raPrepDotT * raPrepDotT) * bodyA.InvInertia +
                    (rbPrepDotT * rbPrepDotT) * bodyB.InvInertia;

                float jt = -LiteMath.Dot(relativeVelocity, tangent);
                jt /= denom;
                jt /= (float)contactCount;

                LiteVector frictionImpulse;

                float j = jlist[i];

                if (MathF.Abs(jt) <= j * sf)
                {
                    frictionImpulse = jt * tangent;
                }
                else
                {
                    frictionImpulse = -j * tangent * df;
                }

                this.frictionImpulseList[i] = frictionImpulse;
            }

            for (int i = 0; i < contactCount; i++)
            {
                LiteVector frictionImpulse = this.frictionImpulseList[i];
                LiteVector ra = this.raList[i];
                LiteVector rb = this.rbList[i];

                bodyA.LinearVelocity += -frictionImpulse * bodyA.InvMass;
                bodyA.AngularVelocity += -LiteMath.Cross(ra, frictionImpulse) * bodyA.InvInertia;
                bodyB.LinearVelocity += frictionImpulse * bodyB.InvMass;
                bodyB.AngularVelocity += LiteMath.Cross(rb, frictionImpulse) * bodyB.InvInertia;
            }
        }
    }
}
