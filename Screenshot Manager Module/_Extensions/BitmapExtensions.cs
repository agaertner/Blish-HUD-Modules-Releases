using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Nekres.Screenshot_Manager
{
    internal static class BitmapExtensions
    {
        public static Bitmap Fit(this Bitmap source, Size size)
        {
            if (source.Size.Equals(size))
                return source;

            float scale = Math.Min(size.Width / source.Width, size.Height / source.Height);

            int newHeight = Convert.ToInt32(source.Width * scale);
            int newWidth = Convert.ToInt32(source.Height * scale);
            var newBitmap = new Bitmap(newWidth, newHeight);
            using (var gfx = Graphics.FromImage(newBitmap))
            {
                gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gfx.SmoothingMode = SmoothingMode.HighQuality;
                gfx.PixelOffsetMode = PixelOffsetMode.HighQuality;
                gfx.CompositingQuality = CompositingQuality.HighQuality;
                gfx.Clear(Color.Transparent);
                gfx.DrawImage(source, 0, 0, newWidth, newHeight);
                gfx.Flush();
                gfx.Save();
            }
            source.Dispose();
            return newBitmap;
        }
    }
}
