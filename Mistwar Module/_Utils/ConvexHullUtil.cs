using System.Collections.Generic;
using System.Drawing;
using System.Linq;
namespace Nekres.Mistwar
{
    public static class ConvexHullUtil
    {
        private static double Cross(PointF O, PointF A, PointF B)
        {
            return (A.X - O.X) * (B.Y - O.Y) - (A.Y - O.Y) * (B.X - O.X);
        }

        public static List<PointF> Get(List<PointF> points)
        {
            if (points == null)
                return null;

            if (points.Count() <= 1)
                return points;

            int n = points.Count(), k = 0;
            List<PointF> H = new List<PointF>(new PointF[2 * n]);

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
        public static bool InBounds(PointF point, IReadOnlyList<PointF> hull)
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
    }
}