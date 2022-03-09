using Microsoft.Xna.Framework;
namespace Nekres.Mistwar
{
    internal static class PointExtensions
    {
        public static Point ToBounds(this Point point, Rectangle bounds)
        {
            return point + bounds.Location;
        }
    }
}
