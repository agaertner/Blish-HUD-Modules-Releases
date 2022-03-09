using System;
using System.Drawing;
using System.Drawing.Drawing2D;
namespace Nekres.Stream_Out
{
    internal static class BitmapExtensions
    {
        public static Bitmap FitToHeight(this Bitmap source, int destHeight)
        {
            if (destHeight < 1)
                throw new ArgumentOutOfRangeException();

            if (destHeight == source.Height)
                return source;

            double ratio = destHeight / (double)source.Height;
            int newHeight = Convert.ToInt32(source.Width * ratio);
            int newWidth = Convert.ToInt32(source.Height * ratio);
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

        public static Bitmap FitTo(this Bitmap source, Bitmap other)
        {
            if (other == null)
                throw new ArgumentNullException();

            if (source.Size.Equals(other.Size))
                return source;

            float scale = Math.Min(other.Width / source.Width, other.Height / source.Height);

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

        public static void Colorize(this Bitmap source, Color replacement)
        {
            for (int x = 0; x < source.Width; ++x)
            {
                for (int y = 0; y < source.Height; ++y)
                {
                    var color = source.GetPixel(x, y);
                    source.SetPixel(x, y, Color.FromArgb(color.A,replacement.R, replacement.G, replacement.B));
                }
            }
        }

        public static Bitmap Merge(this Bitmap bmp1, Bitmap bmp2)
        {
            Bitmap result = new Bitmap(Math.Max(bmp1.Width, bmp2.Width), Math.Max(bmp1.Height, bmp2.Height));
            using (Graphics g = Graphics.FromImage(result))
            {
                g.DrawImage(bmp1, Point.Empty);
                g.DrawImage(bmp2, Point.Empty);
            }
            return result;
        }

        public static void Flip(this Bitmap source, bool xFlip, bool yFlip)
        {
            switch (xFlip)
            {
                case true when yFlip:
                    source.RotateFlip(RotateFlipType.RotateNoneFlipXY);
                    return;
                case true:
                    source.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    break;
            }

            if (yFlip)
                source.RotateFlip(RotateFlipType.RotateNoneFlipY);
        }
    }
}
