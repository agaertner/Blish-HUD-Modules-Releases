using System;
using System.Drawing;

namespace Nekres.Screenshot_Manager
{
    internal static class SizeExtensions
    {
        public static Size Fit(this Size source, Size size)
        {
            if (source.Equals(size))
                return source;

            float scale = Math.Min(size.Width / (float)source.Width, size.Height / (float)source.Height);

            return new Size(Convert.ToInt32(source.Width * scale), Convert.ToInt32(source.Height * scale));
        }
    }
}
