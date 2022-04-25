using Blish_HUD.Modules.Managers;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
namespace Nekres.Stream_Out
{
    internal static class ContentsManagerExtensions
    {
        public static void ExtractIcons(this ContentsManager contentsManager, string archiveFilePath, string outFilePath)
        {
            if (File.Exists(outFilePath)) return;

            Directory.CreateDirectory(Path.GetDirectoryName(outFilePath) ?? string.Empty);

            using var texStr = contentsManager.GetFileStream(archiveFilePath);
            using var icon = new Bitmap(texStr);
            icon.Save(outFilePath, ImageFormat.Png);
        }
    }
}
