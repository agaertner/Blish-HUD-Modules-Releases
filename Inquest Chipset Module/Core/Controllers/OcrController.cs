using System;
using IronOcr;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Nekres.Inquest_Module.Core.Controllers
{
    public class OcrController
    {
        private Color _itemAmountUpperColor = Color.FromArgb(255, 243, 170);
        private Color _itemAmountLowerColor = Color.FromArgb(224, 202, 127);
        private readonly uint ARGB_BLACK = 0xFF000000; //Text color
        private readonly uint ARGB_WHITE = 0xFFFFFFFF; //Other color

        private readonly uint
            ARGB_RED = 0xFFFF0000; //Color used when marking pixels in clusters we want to keep (letters)

        private readonly uint
            ARGB_GREEN = 0xFF00FF00; //Color used when marking pixels in oversized clusters that are likely to be erroneously picked up clouds

        private readonly IronTesseract _tesseract;

        private IEnumerable<string> _targetList { get; set; }

        private readonly int maxOCRSubpixelFFDistance = 0;
        private readonly int maximumLetterPixelCount = 8500;

        public OcrController()
        {
            _targetList = Enumerable.Range(1, 250).Select(i => i.ToString());
            _tesseract = new IronTesseract();
            _tesseract.UseCustomTesseractLanguageFile(Path.Combine(InquestModule.ModuleInstance.DirectoriesManager.GetFullDirectoryPath("inquest_chipset"), "tessdata/digits_comma.traineddata"));
        }

        public string ReadText(Bitmap ocrViewport)
        {
            RemoveBackground(ocrViewport);
            ocrViewport.Save(Path.Combine(
                InquestModule.ModuleInstance.DirectoriesManager.GetFullDirectoryPath("inquest_chipset"), "test.bmp"));
            using var input = new OcrInput(ocrViewport);
            input.Sharpen();
            input.ToGrayScale();
            input.Invert();

            input.Pages.First().SaveAsImage(Path.Combine(
                    InquestModule.ModuleInstance.DirectoriesManager.GetFullDirectoryPath("inquest_chipset"),
                    "test2.bmp"));

            var read = _tesseract.Read(input);
            var text = read.Text;

            return text.FindClosestMatch(_targetList);
        }

        private void RemoveBackground(Bitmap image)
        {
            Color c1 = _itemAmountLowerColor; //lower color
            Color c2 = _itemAmountUpperColor; //upper color
            for (var x = 0; x < image.Width; x++)
            for (var y = 0; y < image.Height; y++)
            {
                var c = image.GetPixel(x, y);
                if (c.R >= c1.R && c.R <= c2.R && c.G >= c1.G && c.G <= c2.G && c.B >= c1.B && c.B <= c2.B)
                    continue;
                image.SetPixel(x, y, Color.Black);
            }
        }

        private void ProcessImage(Bitmap image)
        {
            int imageHeight = image.Height;
            int imageWidth = image.Width;

            uint[,] whiteMask = new uint[imageWidth, imageHeight];

            for (int i = 0; i < imageWidth; i++)
            {
                for (int j = 0; j < imageHeight; j++)
                {
                    int pixel = image.GetPixel(i, j).ToArgb();
                    if (TestWhite(pixel))
                    {
                        whiteMask[i, j] = ARGB_BLACK;
                    }
                    else
                    {
                        whiteMask[i, j] = ARGB_WHITE;
                    }
                }
            }

            //Scan along the middle of the image
            int scanningLine = imageHeight / 2;

            //We're going to identify letters using a DB-scan like method.
            //Initial points will be black pixels along the middle of the image
            //Pixels will join them in a cluster if they're black too.
            //Anything not part of this cluster is noise and gets removed

            int pixelID = 0;
            Dictionary<int, int> pixelMap = new Dictionary<int, int>();

            //Populate the pixelMap with pixelIDs and pixel locations along the scanning line
            for (int i = 0; i < imageWidth; i++)
            {
                if (whiteMask[i, scanningLine] == ARGB_BLACK)
                {
                    pixelMap[pixelID++] = i;
                }
            }

            //For each pixel in the IDs we'll set the whiteMask to RED and keep doing that for neighbouring pixels
            foreach (int PixelID in pixelMap.Keys)
            {
                int pixelI = pixelMap[PixelID];

                if (whiteMask[pixelI, scanningLine] == ARGB_RED || whiteMask[pixelI, scanningLine] == ARGB_GREEN)
                {
                    //We've already marked this pixel as a part of a letter (red pixels) OR part of an oversized cluster that isn't a letter (green pixels)
                    continue;
                }

                //Run the check on this pixel
                PixelCheck(pixelI, scanningLine, whiteMask, imageHeight, imageWidth);
            }

            //Set red pixels to black and rest to white
            for (int i = 0; i < imageWidth; i++)
            {
                for (int j = 0; j < imageHeight; j++)
                {
                    if (whiteMask[i, j] == ARGB_RED)
                    {
                        whiteMask[i, j] = ARGB_BLACK;
                    }
                    else
                    {
                        whiteMask[i, j] = ARGB_WHITE;
                    }
                }
            }

            //Set the image pixels to the pixel values in the white mask
            for (int i = 0; i < imageWidth; i++)
            {
                for (int j = 0; j < imageHeight; j++)
                {
                    image.SetPixel(i, j, Color.FromArgb(unchecked((int) whiteMask[i, j])));
                }
            }

        }

        private bool TestWhite(int pixel)
        {

            // Test the pixel isn't transparent
            if ((pixel & ARGB_BLACK) != ARGB_BLACK)
            {
                return false;
            }

            // Test the maximum difference from 255 of a color
            if ((0xFF - (pixel >> 16 & 0xFF)) > maxOCRSubpixelFFDistance)
            {
                return false;
            }
            else if ((0xFF - (pixel >> 8 & 0xFF)) > maxOCRSubpixelFFDistance)
            {
                return false;
            }
            else if ((0xFF - (pixel & 0xFF)) > maxOCRSubpixelFFDistance)
            {
                return false;
            }

            return true;
        }

        private void PixelCheck(int i, int j, uint[,] whiteMask, int imageHeight, int imageWidth)
        {
            int objectPixelCount = 1;
            //List of relative coordinates of pixels around a target pixel used 
            List<Tuple<int, int>> pixelOffsets = new List<Tuple<int, int>>
            {
                Tuple.Create(-1, -1),
                Tuple.Create(0, -1),
                Tuple.Create(1, -1),
                Tuple.Create(-1, 0),
                Tuple.Create(1, 0),
                Tuple.Create(-1, 1),
                Tuple.Create(0, 1),
                Tuple.Create(1, 1)
            };
            Queue<Tuple<int, int>> pixelStack = new Queue<Tuple<int, int>>();
            //Add the initial pixel coordinates
            pixelStack.Enqueue(Tuple.Create(i, j));
            //Proceess the queue
            while (pixelStack.Count != 0 && objectPixelCount <= maximumLetterPixelCount)
            {
                //Get pixel coordinates from the stack
                Tuple<int, int> coordinate = pixelStack.Dequeue();
                //Set that pixel red
                whiteMask[coordinate.Item1, coordinate.Item2] = ARGB_RED;
                //Check neighboring pixels
                foreach (Tuple<int, int> offset in pixelOffsets)
                {
                    if (coordinate.Item1 + offset.Item1 >= imageWidth || coordinate.Item2 + offset.Item2 >= imageHeight)
                    {
                        //We shouldn't reach the edge with our letters, so we can also say we've exceeded the count here too
                        objectPixelCount = maximumLetterPixelCount + 1;
                        continue;
                    }

                    if (coordinate.Item1 + offset.Item1 < 0 || coordinate.Item2 + offset.Item2 < 0)
                    {
                        //We shouldn't reach the edge with our letters, so we can also say we've exceeded the count here too
                        objectPixelCount = maximumLetterPixelCount + 1;
                        continue;
                    }

                    if (whiteMask[coordinate.Item1 + offset.Item1, coordinate.Item2 + offset.Item2] == ARGB_GREEN)
                    {
                        //If we've found a green pixel in our neighbours we're part of a body of green pixels
                        //We want to stop processing in this case
                        objectPixelCount = maximumLetterPixelCount + 1;
                    }

                    if (whiteMask[coordinate.Item1 + offset.Item1, coordinate.Item2 + offset.Item2] == ARGB_BLACK)
                    {
                        Tuple<int, int> newTuple = Tuple.Create(coordinate.Item1 + offset.Item1,
                            coordinate.Item2 + offset.Item2);
                        if (!pixelStack.Contains(newTuple))
                        {
                            pixelStack.Enqueue(newTuple);
                            //Increase the count of pixels that are part of this object
                            objectPixelCount++;
                        }
                    }
                }
            }

            if (objectPixelCount > maximumLetterPixelCount)
            {
                //We've reached a size that is too large to be a letter
                //Continue the work of the stack, but now we're marking everything green
                while (pixelStack.Count != 0)
                {
                    //Get pixel coordinates from the stack
                    Tuple<int, int> coordinate = pixelStack.Dequeue();
                    //Set that pixel green
                    whiteMask[coordinate.Item1, coordinate.Item2] = ARGB_GREEN;
                    //Check neighboring pixels
                    foreach (Tuple<int, int> offset in pixelOffsets)
                    {
                        if (coordinate.Item1 + offset.Item1 >= imageWidth ||
                            coordinate.Item2 + offset.Item2 >= imageHeight)
                        {
                            continue;
                        }

                        if (coordinate.Item1 + offset.Item1 < 0 || coordinate.Item2 + offset.Item2 < 0)
                        {
                            continue;
                        }

                        if (whiteMask[coordinate.Item1 + offset.Item1, coordinate.Item2 + offset.Item2] == ARGB_RED)
                        {
                            Tuple<int, int> newTuple = Tuple.Create(coordinate.Item1 + offset.Item1,
                                coordinate.Item2 + offset.Item2);
                            if (!pixelStack.Contains(newTuple))
                            {
                                pixelStack.Enqueue(newTuple);
                            }
                        }
                    }
                }
            }
        }
    }
}