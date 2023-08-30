using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Lite.Graphics
{
    public class Camera
    {
        private const float MinScale = 1f;
        private const float MaxScale = 64f;

        private const float MinZ = 1f;
        private const float MaxZ = 4096f;

        private Screen screen;

        private float z;
        private float baseZ;

        private Vector2 position;
        private float rotation;
        private float scale;

        private float fieldOfView;
        private float aspectRatio;

        public Matrix View { get; private set; }
        public Matrix Projection { get; private set; }

        internal Vector2 Position
        {
            get { return position; }
            private set { position = value; }
        }

        internal float Scale
        {
            get { return this.scale; }
            private set { this.scale = Math.Clamp(value, Camera.MinScale, Camera.MaxScale); }
        }

        internal float Rotation
        {
            get { return this.rotation; }
            private set { this.rotation = value % MathHelper.TwoPi; }
        }

        public Camera(Screen screen)
        {
            this.screen = screen;

            this.position = Vector2.Zero;
            this.rotation = 0f;
            this.scale = 1f;

            this.fieldOfView = MathHelper.PiOver2;
            this.aspectRatio = (float)screen.Width / screen.Height;

            this.baseZ = this.GetZFromHeight(screen.Height);
            this.z = this.baseZ;

            this.View = Matrix.Identity;
            this.Projection = Matrix.Identity;
        }

        public void UpdateMatrices()
        {
            this.View = Matrix.CreateLookAt(new Vector3(position, (float)-this.z), new Vector3(position, 0), Vector3.Down);
            this.Projection = Matrix.CreatePerspectiveFieldOfView(this.fieldOfView, this.aspectRatio, Camera.MinZ, Camera.MaxZ);
        }

        public float GetZFromHeight(float height)
        {
            return (height * 0.5f) / (float)Math.Tan(this.fieldOfView * 0.5d);
        }

        private float GetHeightFromZ(float z)
        {
            return z * (float)Math.Tan(this.fieldOfView * 0.5f) * 2f;
        }

        private float GetVisibleHeight()
        {
            return this.z * (float)Math.Tan(this.fieldOfView * 0.5d) * 2f;
        }

        public void SetZ(float value)
        {
            double new_z = value;

            if (new_z < Camera.MinZ ||
                new_z > Camera.MaxZ)
            {
                return;
            }

            this.z = value;
        }

        public void Move(Vector2 amount)
        {
            this.Position += amount;
        }

        public void MoveTo(Vector2 position)
        {
            this.Position = position;
        }

        public void SetRotation(float value)
        {
            this.Rotation = value;
        }

        public void AddRotation(float value)
        {
            this.Rotation += value;
        }

        public void SetScale(float value)
        {
            this.Scale = value;
            this.z = this.baseZ * (1f / this.Scale);
        }

        public void IncScale()
        {
            this.Scale++;
            this.z = this.baseZ * (1f / this.Scale);
        }

        public void DecScale()
        {
            this.Scale--;
            this.z = this.baseZ * (1f / this.Scale);
        }

        public void GetExtents(out float width, out float height)
        {
            height = (float)this.GetVisibleHeight();
            width = height * this.aspectRatio;
        }

        public void GetExtents(out float top, out float left, out float right, out float bottom)
        {
            this.GetExtents(out float width, out float height);

            left = this.position.X - width * 0.5f;
            right = left + width;
            top = this.position.Y - height * 0.5f;
            bottom = top + height;
        }

        public void GetExtents(out Vector2 min, out Vector2 max)
        {
            this.GetExtents(out float top, out float left, out float right, out float bottom);

            min = new Vector2(left, bottom);
            max = new Vector2(right, top);
        }
    }
}
