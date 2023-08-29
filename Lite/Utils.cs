using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Lite
{
    internal class Utils
    {
        public static void Normalize(ref float x, ref float y)
        {
            float len = MathF.Sqrt(x * x + y * y);

            if (len == 0) throw new DivideByZeroException("Cant devide by 0 (vector of length 0).");

            float leninv = 1 / len;
            x *= leninv;
            y *= leninv;
        }

        public static int[] Triangulate(Vector2[] vertices)
        {
            if (vertices.Length < 3)
                throw new ArgumentException("Polygon must have at least 3 vertices.");

            int[] indices = new int[(vertices.Length - 2) * 3];
            int index = 0;

            Vector2[] remainingVertices = new Vector2[vertices.Length];
            Array.Copy(vertices, remainingVertices, vertices.Length);

            while (remainingVertices.Length >= 3)
            {
                for (int i = 0; i < remainingVertices.Length; i++)
                {
                    Vector2 prevVertex = remainingVertices[PrevIndex(i, remainingVertices.Length)];
                    Vector2 currentVertex = remainingVertices[i];
                    Vector2 nextVertex = remainingVertices[NextIndex(i, remainingVertices.Length)];

                    if (IsEar(prevVertex, currentVertex, nextVertex, remainingVertices))
                    {
                        indices[index++] = Array.IndexOf(vertices, prevVertex);
                        indices[index++] = Array.IndexOf(vertices, currentVertex);
                        indices[index++] = Array.IndexOf(vertices, nextVertex);

                        remainingVertices = RemoveAtIndex(remainingVertices, i);
                        break;
                    }
                }
            }

            return indices;
        }

        private static bool IsEar(Vector2 prev, Vector2 current, Vector2 next, Vector2[] vertices)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector2 vertex = vertices[i];
                if (vertex != prev && vertex != current && vertex != next &&
                    IsPointInTriangle(vertex, prev, current, next))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsPointInTriangle(Vector2 point, Vector2 v1, Vector2 v2, Vector2 v3)
        {
            float d1 = Sign(point, v1, v2);
            float d2 = Sign(point, v2, v3);
            float d3 = Sign(point, v3, v1);

            bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);

            return !(hasNeg && hasPos);
        }

        private static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y);
        }

        private static int PrevIndex(int i, int count)
        {
            return (i + count - 1) % count;
        }

        private static int NextIndex(int i, int count)
        {
            return (i + 1) % count;
        }

        private static T[] RemoveAtIndex<T>(T[] array, int index)
        {
            T[] result = new T[array.Length - 1];
            Array.Copy(array, 0, result, 0, index);
            Array.Copy(array, index + 1, result, index, array.Length - index - 1);
            return result;
        }
    }
}
