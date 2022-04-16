﻿using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

namespace Nekres.Stream_Out
{
    internal static class TextureUtil
    {
        public static void ClearImage(string path)
        {
            if (!File.Exists(path)) return;
            using var stream = new MemoryStream(File.ReadAllBytes(path));
            using var bitmap = (Bitmap)Image.FromStream(stream);
            using (var gfx = Graphics.FromImage(bitmap))
            {
                gfx.Clear(Color.Transparent);
                gfx.Flush();
            }
            bitmap.SaveOnNetworkShare(path, ImageFormat.Png);
        }

        public static async Task SaveToImage(string renderUri, string path)
        {
            await StreamOutModule.ModuleInstance.Gw2ApiManager.Gw2ApiClient.Render.DownloadToByteArrayAsync(renderUri).ContinueWith(textureDataResponse =>
            {
                if (textureDataResponse.IsFaulted)
                {
                    StreamOutModule.Logger.Warn($"Request to render service for {renderUri} failed.");
                    return;
                }
                using var textureStream = new MemoryStream(textureDataResponse.Result);
                using var bitmap = new Bitmap(textureStream);
                bitmap.SaveOnNetworkShare(path, ImageFormat.Png);
            });
        }
    }
}
