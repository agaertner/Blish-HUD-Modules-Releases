using System.Drawing;
namespace Nekres.Regions_Of_Tyria
{
    public static class ColorExtensions
    {
        public static Microsoft.Xna.Framework.Color ToXnaColor(this Color color)
        {
            return new Microsoft.Xna.Framework.Color(color.R, color.G, color.B, color.A);
        }
    }
}
