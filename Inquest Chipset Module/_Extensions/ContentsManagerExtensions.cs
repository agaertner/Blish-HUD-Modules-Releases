using Blish_HUD.Modules.Managers;
using System.IO;
using System.Threading.Tasks;

namespace Nekres.Inquest_Module
{
    internal static class ContentsManagerExtensions
    {
        public static async Task ExtractFile(this ContentsManager contentsManager, string outDir, string refFilePath)
        {
            var fullPath = Path.Combine(outDir, refFilePath);
            if (File.Exists(fullPath)) return;
            using (var fs = contentsManager.GetFileStream(refFilePath))
            {
                fs.Position = 0;
                byte[] buffer = new byte[fs.Length];
                var content = await fs.ReadAsync(buffer, 0, (int)fs.Length);
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                File.WriteAllBytes(fullPath, buffer);
            }
        }
    }
}
