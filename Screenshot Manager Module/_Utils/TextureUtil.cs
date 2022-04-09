using Blish_HUD;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Security;
using System.Threading.Tasks;

namespace Nekres.Screenshot_Manager
{
    internal class TextureUtil
    {
        /*public static async Task<Texture2D> GetThumbnail(string filePath)
        {
            // Decode the image directly in the given DecodePixelHeight (or width), maintaining aspect ratio.
            var thumbnail = new BitmapImage();
            thumbnail.BeginInit();
            thumbnail.UriSource = new Uri(filePath, UriKind.Absolute);
            thumbnail.DecodePixelHeight = 144;
            thumbnail.EndInit();

            // Format the bitmap image into a known format.
            var formatted = new FormatConvertedBitmap();
            formatted.BeginInit();
            formatted.Source = thumbnail;
            formatted.DestinationFormat = System.Windows.Media.PixelFormats.Default;
            formatted.EndInit();

            using var stream = new MemoryStream();
            var bytesPerPixel = (formatted.DestinationFormat.BitsPerPixel + 7) / 8;
            var stride = 4 * ((formatted.PixelWidth * bytesPerPixel + 3) / 4);
            var buffer = new byte[formatted.PixelHeight * stride];
            formatted.CopyPixels(buffer, stride, 0);
            await stream.WriteAsync(buffer, 0, buffer.Length);

            return Texture2D.FromStream(GameService.Graphics.GraphicsDevice, stream);
        }*/
        //TODO: Consider making thumbnail independent of shell
        public static async Task<Texture2D> GetThumbnail(string filePath)
        {
            return await Task.Run(async () => {
                var timeout = DateTime.UtcNow.AddMilliseconds(ScreenshotManagerModule.FileTimeOutMilliseconds);
                while (DateTime.UtcNow < timeout)
                {
                    try
                    {
                        using var shellFile = ShellFile.FromFilePath(filePath);
                        if (shellFile.Thumbnail == null) throw new ShellException();
                        shellFile.Thumbnail.FormatOption = ShellThumbnailFormatOption.ThumbnailOnly;
                        using var shellThumb = shellFile.Thumbnail.ExtraLargeBitmap;
                        using var textureStream = new MemoryStream();
                        shellThumb.Save(textureStream, ImageFormat.Jpeg);
                        var buffer = new byte[textureStream.Length];
                        textureStream.Position = 0;
                        // ReSharper disable once MustUseReturnValue
                        await textureStream.ReadAsync(buffer, 0, buffer.Length);
                        return Texture2D.FromStream(GameService.Graphics.GraphicsDevice, textureStream);
                    }
                    catch (Exception e) when (e is ShellException or IOException or UnauthorizedAccessException or SecurityException or InvalidOperationException)
                    {
                        if (DateTime.UtcNow < timeout) continue;
                        ScreenshotManagerModule.Logger.Error(e, e.Message);
                        return ContentService.Textures.Pixel;
                    }
                }
                return ContentService.Textures.Pixel;
            });
        }

        public static async Task<Texture2D> GetScreenShot(string filePath)
        {
            return await Task.Run(async () =>
            {
                var timeout = DateTime.UtcNow.AddMilliseconds(ScreenshotManagerModule.FileTimeOutMilliseconds);
                while (DateTime.UtcNow < timeout)
                {
                    try
                    {
                        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                        using var source = Image.FromStream(fs);

                        using var target = new Bitmap(source);
                        using var graphic = Graphics.FromImage(target);
                        graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphic.SmoothingMode = SmoothingMode.HighSpeed;
                        graphic.PixelOffsetMode = PixelOffsetMode.Default;
                        graphic.CompositingQuality = CompositingQuality.Default;
                        graphic.DrawImage(target, 0, 0);

                        using var textureStream = new MemoryStream();
                        target.Save(textureStream, ImageFormat.Jpeg);
                        var buffer = new byte[textureStream.Length];
                        textureStream.Position = 0;
                        // ReSharper disable once MustUseReturnValue
                        await textureStream.ReadAsync(buffer, 0, buffer.Length);
                        return Texture2D.FromStream(GameService.Graphics.GraphicsDevice, textureStream);
                    }
                    catch (Exception e) when (e is IOException or UnauthorizedAccessException or SecurityException or InvalidOperationException)
                    {
                        if (DateTime.UtcNow < timeout) continue;
                        ScreenshotManagerModule.Logger.Error(e, e.Message);
                        return ContentService.Textures.Pixel;
                    }
                }
                return ContentService.Textures.Pixel;
            });
        }
    }
}
