using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Nekres.Mistwar
{
    public static class PolygonUtil
    {
        private static double Cross(Vector2 O, Vector2 A, Vector2 B)
        {
            return (A.X - O.X) * (B.Y - O.Y) - (A.Y - O.Y) * (B.X - O.X);
        }

        public static List<Vector2> Get(List<Vector2> points)
        {
            if (points == null)
                return null;

            if (points.Count() <= 1)
                return points;

            int n = points.Count(), k = 0;
            List<Vector2> H = new List<Vector2>(new Vector2[2 * n]);

            points.Sort((a, b) =>
                 a.X == b.X ? a.Y.CompareTo(b.Y) : a.X.CompareTo(b.X));

            // Build lower hull
            for (int i = 0; i < n; ++i)
            {
                while (k >= 2 && Cross(H[k - 2], H[k - 1], points[i]) <= 0)
                    k--;
                H[k++] = points[i];
            }

            // Build upper hull
            for (int i = n - 2, t = k + 1; i >= 0; i--)
            {
                while (k >= t && Cross(H[k - 2], H[k - 1], points[i]) <= 0)
                    k--;
                H[k++] = points[i];
            }

            return H.Take(k - 1).ToList();
        }
        /// <summary>
        /// Determines if the given point is inside the polygon
        /// </summary>
        /// <param name="hull">the vertices of polygon</param>
        /// <param name="point">the given point</param>
        /// <returns>true if the point is inside the polygon; otherwise, false</returns>
        public static bool InBounds(Vector2 point, IReadOnlyList<Vector2> hull)
        {
            bool result = false;
            int j = hull.Count() - 1;
            for (int i = 0; i < hull.Count(); i++)
            {
                if (hull[i].Y < point.Y && hull[j].Y >= point.Y || hull[j].Y < point.Y && hull[i].Y >= point.Y)
                {
                    if (hull[i].X + (point.Y - hull[i].Y) / (hull[j].Y - hull[i].Y) * (hull[j].X - hull[i].X) < point.X)
                    {
                        result = !result;
                    }
                }
                j = i;
            }
            return result;
        }

        public static bool IsPointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            Vector2 ab = b - a;
            Vector2 bc = c - b;
            Vector2 ca = a - c;

            Vector2 ap = p - a;
            Vector2 bp = p - b;
            Vector2 cp = p - c;

            float c1 = MathUtil.Cross(ab, ap);
            float c2 = MathUtil.Cross(bc, bp);
            float c3 = MathUtil.Cross(ca, cp);

            if (c1 < 0f && c2 < 0f && c3 < 0f)
            {
                return true;
            }

            return false;
        }

        public static bool Triangulate(Vector2[] vertices, out int[] triangles)
        {
            triangles = null;
            if (vertices == null || vertices.Length < 3 || vertices.Length > 1024) return false;

            var indexList = new List<int>();
            for (int i = 0; i < vertices.Length; i++)
            {
                indexList.Add(i);
            }

            int totalTriangleCount = vertices.Length - 2;
            int totalTriangleIndexCount = totalTriangleCount * 3;

            triangles = new int[totalTriangleIndexCount];
            int triangleIndexCount = 0;

            while (indexList.Count > 3)
            {
                for (int i = 0; i < indexList.Count; i++)
                {
                    var a = indexList[i];
                    int b = indexList.GetItem(i - 1);
                    int c = indexList.GetItem(i + 1);

                    Vector2 va = vertices[a];
                    Vector2 vb = vertices[b];
                    Vector2 vc = vertices[c];

                    Vector2 va_to_vb = vb - va;
                    Vector2 va_to_vc = vc - va;

                    // Is ear test vertex convex?
                    if (MathUtil.Cross(va_to_vb, va_to_vc) < 0f)
                    {
                        continue;
                    }

                    bool isEar = true;

                    // Does test ear contain any polygon vertices?
                    for (int j = 0; j < vertices.Length; j++)
                    {
                        if (j == a || j == b || j == c)
                        {
                            continue;
                        }

                        Vector2 p = vertices[j];

                        if (PolygonUtil.IsPointInTriangle(p, vb, va, vc))
                        {
                            isEar = false;
                            break;
                        }
                    }

                    if (isEar)
                    {
                        triangles[triangleIndexCount++] = b;
                        triangles[triangleIndexCount++] = a;
                        triangles[triangleIndexCount++] = c;

                        indexList.RemoveAt(i);
                        break;
                    }
                }
            }

            triangles[triangleIndexCount++] = indexList[0];
            triangles[triangleIndexCount++] = indexList[1];
            triangles[triangleIndexCount++] = indexList[2];
            return true;
        }
    }
}