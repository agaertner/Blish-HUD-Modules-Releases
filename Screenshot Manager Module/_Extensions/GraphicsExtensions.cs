using System.Drawing;
using System.Drawing.Drawing2D;

namespace Nekres.Screenshot_Manager
{
    public static class GraphicsExtensions
    {
        public static void SetHighestQuality(this Graphics graphics)
        {
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
        }
    }
}
