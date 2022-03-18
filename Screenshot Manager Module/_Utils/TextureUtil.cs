using Blish_HUD;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAPICodePack.Shell;

namespace Nekres.Screenshot_Manager
{
    internal class TextureUtil
    {
        public static async Task<Texture2D> GetThumbnail(string filePath)
        {
            return await Task.Run(() => {
                using var shellFile = ShellFile.FromFilePath(filePath);
                using var shellThumb = shellFile.Thumbnail.ExtraLargeBitmap;
                using var textureStream = new MemoryStream();
                shellThumb.Save(textureStream, ImageFormat.Jpeg);
                var buffer = new byte[textureStream.Length];
                textureStream.Position = 0;
                textureStream.Read(buffer, 0, buffer.Length);
                return Texture2D.FromStream(GameService.Graphics.GraphicsDevice, textureStream);
            });
        }

        public static async Task<Texture2D> GetScreenShot(string filePath)
        {
            return await Task.Run(() =>
            {
                var timeout = DateTime.UtcNow.AddMilliseconds(ScreenshotManagerModule.FileTimeOutMilliseconds);
                while (DateTime.UtcNow < timeout)
                {
                    if (!File.Exists(filePath)) return ContentService.Textures.Pixel;
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
                        textureStream.Read(buffer, 0, buffer.Length);

                        return Texture2D.FromStream(GameService.Graphics.GraphicsDevice, textureStream);
                    }
                    catch (IOException e)
                    {
                        if (DateTime.UtcNow < timeout) continue;
                        ScreenshotManagerModule.Logger.Error(e.Message);
                        return ContentService.Textures.Pixel;
                    }
                }

                return ContentService.Textures.Pixel;
            });
        }
    }
}
