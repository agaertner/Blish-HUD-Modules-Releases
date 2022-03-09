using System.Drawing;
namespace Nekres.Stream_Out
{
    public static class StringExtensions
    {
        public static Size Measure(this string str, Font font)
        {
            using var tempBitmap = new Bitmap(500, 500);
            using var canvas = Graphics.FromImage(tempBitmap);
            return canvas.MeasureString(str, font).ToSize();
        }
    }
}
