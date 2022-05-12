using System;
using System.IO;
using System.Threading.Tasks;
using Blish_HUD;
namespace Nekres.Stream_Out
{
    internal static class FileUtil
    {
        private static readonly Logger Logger = Logger.GetLogger(typeof(FileUtil));

        public static async Task WriteAllTextAsync(string filePath, string data, bool overwrite = true)
        {
            if (string.IsNullOrEmpty(filePath))
                return;

            if (!overwrite && File.Exists(filePath))
                return;

            data ??= string.Empty;

            try
            {
                using var sw = new StreamWriter(filePath);
                await sw.WriteAsync(data);
            } catch (ArgumentException aEx) {
                Logger.Error(aEx.Message);
            } catch (UnauthorizedAccessException uaEx) {
                Logger.Error(uaEx.Message);
            } catch (IOException ioEx) {
                Logger.Error(ioEx.Message);
            }
        }

        public static async Task<bool> DeleteAsync(string filePath)
        {
            return await Task.Run(() => {
                var timeout = DateTime.UtcNow.AddMilliseconds(10000);
                while (DateTime.UtcNow < timeout)
                {
                    try
                    {
                        File.Delete(filePath);
                        return true;
                    }
                    catch (Exception e) when (e is IOException or UnauthorizedAccessException)
                    {
                        if (DateTime.UtcNow < timeout) continue;
                        StreamOutModule.Logger.Error(e, e.Message);
                        break;
                    }
                }
                return false;
            });
        }

        public static async Task<bool> DeleteDirectoryAsync(string dirPath)
        {
            return await Task.Run(() => {
                var timeout = DateTime.UtcNow.AddMilliseconds(10000);
                while (DateTime.UtcNow < timeout)
                {
                    try
                    {
                        Directory.Delete(dirPath, true);
                        return true;
                    }
                    catch (Exception e) when (e is IOException or UnauthorizedAccessException)
                    {
                        if (DateTime.UtcNow < timeout) continue;
                        StreamOutModule.Logger.Error(e, e.Message);
                        break;
                    }
                }
                return false;
            });
        }
    }
}
