using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

namespace Nekres.Stream_Out
{
    internal static class TextureUtil
    {
        public static async Task ClearImage(string path)
        {
            if (!File.Exists(path)) return;
            using var stream = new MemoryStream(File.ReadAllBytes(path));
            using var bitmap = (Bitmap)Image.FromStream(stream);
            using (var gfx = Graphics.FromImage(bitmap))
            {
                gfx.Clear(Color.Transparent);
                gfx.Flush();
            }
            await bitmap.SaveOnNetworkShare(path, ImageFormat.Png);
        }

        public static async Task SaveToImage(string renderUri, string path)
        {
            await StreamOutModule.Instance.Gw2ApiManager.Gw2ApiClient.Render.DownloadToByteArrayAsync(renderUri).ContinueWith(async textureDataResponse =>
            {
                if (textureDataResponse.IsFaulted)
                {
                    StreamOutModule.Logger.Warn($"Request to render service for {renderUri} failed.");
                    return;
                }
                using var textureStream = new MemoryStream(textureDataResponse.Result);
                using var bitmap = new Bitmap(textureStream);
                using var resized = new Bitmap(bitmap, new Size(64, 64));
                await resized.SaveOnNetworkShare(path, ImageFormat.Png);
            });
        }
    }
}
