﻿using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
namespace Nekres.Mistwar
{
    internal static class BitmapExtensions
    {
        public static void MakeGrayscale(this Bitmap original)
        {
            //get a graphics object from the new image
            Graphics g = Graphics.FromImage(original);
            g.CompositingMode = CompositingMode.SourceCopy;

            //create the grayscale ColorMatrix
            ColorMatrix colorMatrix = new ColorMatrix(
                new float[][]
                {
                    new float[] {.3f, .3f, .3f, 0, 0},
                    new float[] {.59f, .59f, .59f, 0, 0},
                    new float[] {.11f, .11f, .11f, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {0, 0, 0, 0, 1}
                });

            //create some image attributes
            ImageAttributes attributes = new ImageAttributes();

            //set the color matrix attribute
            attributes.SetColorMatrix(colorMatrix);

            //draw the original image on the new image
            //using the grayscale color matrix
            g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
                0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);

            //dispose the Graphics object
            g.Dispose();
        }
    }
}
