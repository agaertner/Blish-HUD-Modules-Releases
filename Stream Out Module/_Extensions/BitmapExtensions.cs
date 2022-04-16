using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

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
                gfx.SetHighestQuality();
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

            float scale = Math.Min(other.Width / (float)source.Width, other.Height / (float)source.Height);

            int newHeight = Convert.ToInt32(source.Width * scale);
            int newWidth = Convert.ToInt32(source.Height * scale);
            var newBitmap = new Bitmap(newWidth, newHeight);
            using (var gfx = Graphics.FromImage(newBitmap))
            {
                gfx.SetHighestQuality();
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
                default: break;
            }

            if (yFlip)
                source.RotateFlip(RotateFlipType.RotateNoneFlipY);
        }

        public static async Task SaveOnNetworkShare(this Image image, string fileName, ImageFormat imageFormat)
        {
            try {
                using var lMemoryStream = new MemoryStream();
                image.Save(lMemoryStream, imageFormat);

                using var lFileStream = new FileStream(fileName, FileMode.Create);
                lMemoryStream.Position = 0;

                await lMemoryStream.CopyToAsync(lFileStream);
            }
            catch (Exception ex) when (ex is ExternalException or UnauthorizedAccessException or IOException)
            {
                StreamOutModule.Logger.Warn(ex, ex.Message);
            }
        }
    }
}
