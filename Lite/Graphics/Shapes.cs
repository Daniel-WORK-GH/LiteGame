using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Lite.Graphics
{
    public sealed class Shapes
    {
        private Game game;

        private BasicEffect effect;

        private Camera camera;

        private VertexPositionColor[] vertices;
        private int[] indices;

        private int shapeCount;
        private int verticesCount;
        private int indicesCount;

        private bool started;

        const int NumberOfVertices = 1024;

        public Shapes(Game game)
        {
            this.game = game;

            this.vertices = new VertexPositionColor[NumberOfVertices];
            this.indices = new int[NumberOfVertices * 3];

            this.effect = new BasicEffect(game.GraphicsDevice);
            this.effect.FogEnabled = false;
            this.effect.TextureEnabled = false;
            this.effect.LightingEnabled = false;
            this.effect.VertexColorEnabled = true;
            this.effect.View = Matrix.Identity;
            this.effect.World = Matrix.Identity;
            this.effect.Projection = Matrix.Identity;

            this.shapeCount = 0;
            this.verticesCount = 0;
            this.indicesCount = 0;

            this.started = false;
        }

        private void EnsureStarted()
        {
            if (!this.started)
            {
                throw new Exception("Begin() wasn't called.");
            }
        }

        private void EnsureSpace(int numberOfVertices, int numberOfIndices)
        {
            if (numberOfVertices > this.vertices.Length)
            {
                throw new Exception($"Too many vetices ({numberOfVertices}), maximum {this.vertices.Length}");
            }

            if (numberOfIndices > this.indices.Length)
            {
                throw new Exception($"Too many indices ({numberOfIndices}), maximum {this.indices.Length}");
            }

            if (this.verticesCount + numberOfVertices >= this.vertices.Length ||
                this.indicesCount + numberOfIndices >= this.indices.Length)
            {
                this.Flush();
            }
        }

        private void EnsureAddShape(int numberOfVertices, int numberOfIndices)
        {
            this.EnsureStarted();
            this.EnsureSpace(numberOfVertices, numberOfIndices);
            this.shapeCount++;
        }

        public void Begin(Camera camera = null)
        {
            if (this.started)
            {
                throw new Exception("Begin() called before End().");
            }

            if(camera != null)
            {
                this.effect.View = camera.View;
                this.effect.Projection = camera.Projection;
            }
            else 
            { 
                this.effect.Projection = Matrix.CreateOrthographicOffCenter(
                    left: 0f,
                    this.game.GraphicsDevice.Viewport.Width,
                    this.game.GraphicsDevice.Viewport.Height,
                    top: 0f,
                    zNearPlane: 0f,
                    zFarPlane: 1f);
            }

            this.camera = camera;
            this.started = true;
        }

        private void Flush()
        {
            if (this.shapeCount == 0) return;

            foreach (var pass in this.effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                this.game.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(
                    PrimitiveType.TriangleList,
                    this.vertices, vertexOffset: 0,
                    this.verticesCount,
                    this.indices,
                    indexOffset: 0,
                    this.indicesCount / 3);
            }

            this.verticesCount = 0;
            this.indicesCount = 0;
            this.shapeCount = 0;
        }

        public void End()
        {
            this.EnsureStarted();

            this.Flush();
            this.started = false;
        }

        public void FillRectangle(float x, float y, float width, float height, Color color)
        {
            const int RectVertices = 4;
            const int RectIndices = 6;

            this.EnsureAddShape(RectVertices, RectIndices);

            float left = x;
            float top = y;
            float right = x + width;
            float bottom = y + height;

            this.indices[this.indicesCount++] = 0 + this.verticesCount;
            this.indices[this.indicesCount++] = 1 + this.verticesCount;
            this.indices[this.indicesCount++] = 2 + this.verticesCount;
            this.indices[this.indicesCount++] = 0 + this.verticesCount;
            this.indices[this.indicesCount++] = 2 + this.verticesCount;
            this.indices[this.indicesCount++] = 3 + this.verticesCount;

            this.vertices[this.verticesCount++] = new VertexPositionColor(new Vector3(left, top, 0), color);
            this.vertices[this.verticesCount++] = new VertexPositionColor(new Vector3(right, top, 0), color);
            this.vertices[this.verticesCount++] = new VertexPositionColor(new Vector3(right, bottom, 0), color);
            this.vertices[this.verticesCount++] = new VertexPositionColor(new Vector3(left, bottom, 0), color);
        }

        public void FillRectangle(Rectangle rect, Color color)
        {
            this.FillRectangle(rect.X, rect.Y, rect.Width, rect.Height, color);
        }

        public void DrawRectangle(float x, float y, float width, float height, float thickness, Color color)
        {
            const int RectVertices = 4;
            const int RectIndices = 6;
            this.EnsureAddShape(RectVertices, RectIndices);

            float left = x;
            float top = y;
            float right = x + width;
            float bottom = y + height;

            this.DrawLine(left, top, right, top, thickness, color);
            this.DrawLine(right, top, right, bottom, thickness, color);
            this.DrawLine(right, bottom, left, bottom, thickness, color);
            this.DrawLine(left, bottom, left, top, thickness, color);
        }

        public void DrawRectangle(Rectangle rect, float thickness, Color color)
        {
            this.DrawRectangle(rect.X, rect.Y, rect.Width, rect.Height, thickness, color);
        }

        public void FillCircle(float centerx, float centery, float radius, int points, Color color)
        {
            const int MinPoints = 3;
            const int MaxPoints = 256;

            int shapeVertexCount = Math.Clamp(points, MinPoints, MaxPoints);
            int shapeTriangleCount = shapeVertexCount - 2;
            int shapeIndexCount = shapeTriangleCount * 3;

            EnsureAddShape(shapeVertexCount, shapeIndexCount);

            int index = 1;

            for (int i = 0; i < shapeTriangleCount; i++)
            {
                this.indices[this.indicesCount++] = 0 + this.verticesCount;
                this.indices[this.indicesCount++] = index + this.verticesCount;
                this.indices[this.indicesCount++] = index + 1 + this.verticesCount;
                index++;
            }


            float rotation = MathHelper.TwoPi / (float)shapeVertexCount;

            float sin = MathF.Sin(rotation);
            float cos = MathF.Cos(rotation);

            float ax = radius;
            float ay = 0;

            for (int i = 0; i < shapeVertexCount; i++)
            {
                float x1 = ax;
                float y1 = ay;

                this.vertices[this.verticesCount++] = new VertexPositionColor(new Vector3(x1 + centerx, y1 + centery, 0f), color);

                ax = cos * x1 - sin * y1;
                ay = sin * x1 + cos * y1;
            }
        }

        public void FillCircle(Vector2 position, float radius, int points, Color color)
        {
            FillCircle(position.X, position.Y, radius, points, color);
        }

        public void DrawCircle(float centerx, float centery, float radius, int points, float thickness, Color color)
        {
            const int MinPoints = 3;
            const int MaxPoints = 256;

            points = Math.Clamp(points, MinPoints, MaxPoints);

            float rotation = MathHelper.TwoPi / (float)points;

            float sin = MathF.Sin(rotation);
            float cos = MathF.Cos(rotation);

            float ax = radius;
            float ay = 0;
            float bx = 0;
            float by = 0;

            for (int i = 0; i < points; i++)
            {
                bx = cos * ax - sin * ay;
                by = sin * ax + cos * ay;

                this.DrawLine(ax + centerx, ay + centery, bx + centerx, by + centery, thickness, color);

                ax = bx;
                ay = by;
            }
        }

        public void DrawCircle(Vector2 center, float radius, int points, float thickness, Color color)
        {
            this.DrawCircle(center.X, center.Y, radius, points, thickness, color);
        }

        public void FillPolygon(Vector2[] vertex, int[] index, Color color)
        {
            EnsureAddShape(vertex.Length, index.Length);

            for(int i = 0; i < index.Length; i++)
            {
                this.indices[this.indicesCount++] = index[i] + this.verticesCount;
            }

            for(int i = 0; i < vertex.Length; i++)
            {
                this.vertices[this.verticesCount++] = new VertexPositionColor(new Vector3(vertex[i], 0), color);
            }
        }

        public void FillPolygon(Vector2[] vertex, Color color)
        {
            int[] index = LiteUtil.Triangulate(vertex);

            this.FillPolygon(vertex, index, color);
        }

        public void DrawPolygon(Vector2[] points, float thickness, Color color)
        {
            for (int i = 0; i < points.Length; i++)
            {
                Vector2 p1 = points[i];
                Vector2 p2 = points[(i + 1) % points.Length];

                this.DrawLine(p1, p2, thickness, color);
            }
        }

        public void DrawLine(float ax, float ay, float bx, float by, float thickness, Color color)
        {
            const int LineVertices = 4;
            const int LineIndices = 6;
            this.EnsureAddShape(LineVertices, LineIndices);

            if(camera != null)
            {
                thickness *= 1 / camera.Scale;
            }

            float halfthickness = thickness / 2f;

            float e1x = bx - ax;
            float e1y = by - ay;
            LiteUtil.Normalize(ref e1x, ref e1y);
            e1x *= halfthickness;
            e1y *= halfthickness;

            float e2x = -e1x;
            float e2y = -e1y;
            float n1x = e1y;
            float n1y = -e1x;
            float n2x = -n1x;
            float n2y = -n1y;

            float q1x = ax + n1x + e2x;
            float q1y = ay + n1y + e2y;
            float q2x = bx + n1x + e1x;
            float q2y = by + n1y + e1y;
            float q3x = bx + n2x + e1x;
            float q3y = by + n2y + e1y;
            float q4x = ax + n2x + e2x;
            float q4y = ay + n2y + e2y;

            this.indices[this.indicesCount++] = 0 + this.verticesCount;
            this.indices[this.indicesCount++] = 1 + this.verticesCount;
            this.indices[this.indicesCount++] = 2 + this.verticesCount;
            this.indices[this.indicesCount++] = 0 + this.verticesCount;
            this.indices[this.indicesCount++] = 2 + this.verticesCount;
            this.indices[this.indicesCount++] = 3 + this.verticesCount;

            this.vertices[this.verticesCount++] = new VertexPositionColor(new Vector3(q1x, q1y, 0), color);
            this.vertices[this.verticesCount++] = new VertexPositionColor(new Vector3(q2x, q2y, 0), color);
            this.vertices[this.verticesCount++] = new VertexPositionColor(new Vector3(q3x, q3y, 0), color);
            this.vertices[this.verticesCount++] = new VertexPositionColor(new Vector3(q4x, q4y, 0), color);
        }

        public void DrawLine(Vector2 p1, Vector2 p2, float thickness, Color color)
        {
            this.DrawLine(p1.X, p1.Y, p2.X, p2.Y, thickness, color);
        }
    }
}
