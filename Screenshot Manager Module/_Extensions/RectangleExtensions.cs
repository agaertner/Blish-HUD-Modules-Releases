using System;
using Microsoft.Xna.Framework;

namespace Nekres.Screenshot_Manager
{
    internal static class RectangleExtensions
    {
        public static Rectangle Fit(this Rectangle source, Rectangle bounds)
        {
            if (source.Equals(bounds))
                return source;

            float scale = Math.Min(bounds.Width / (float)source.Width, bounds.Height / (float)source.Height);

            var newWidth = Convert.ToInt32(source.Width * scale);
            var newHeight = Convert.ToInt32(source.Height * scale);
            return new Rectangle((bounds.Width - newWidth) / 2, (bounds.Height - newHeight) / 2, newWidth, newHeight);
        }
    }
}
