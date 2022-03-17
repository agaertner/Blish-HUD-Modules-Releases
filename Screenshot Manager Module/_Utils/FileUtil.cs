using System;
using System.IO;
using System.Threading.Tasks;

namespace Nekres.Screenshot_Manager
{
    internal static class FileUtil
    {
        public static async Task<bool> MoveAsync(string oldFilePath, string newFilePath)
        {
            return await Task.Run(() => { 
                var timeout = DateTime.UtcNow.AddMilliseconds(ScreenshotManagerModule.FileTimeOutMilliseconds);
                while (DateTime.UtcNow < timeout) {
                    try
                    {
                        File.Move(oldFilePath, newFilePath);
                        return true;
                    }
                    catch (IOException e)
                    {
                        if (DateTime.UtcNow < timeout) continue;
                        ScreenshotManagerModule.Logger.Error(e.Message + e.StackTrace);
                    }
                }
                return false;
            });
        }

        public static async Task<bool> DeleteAsync(string filePath)
        {
            return await Task.Run(() => {
                var timeout = DateTime.UtcNow.AddMilliseconds(ScreenshotManagerModule.FileTimeOutMilliseconds);
                while (DateTime.UtcNow < timeout)
                {
                    try
                    {
                        File.Delete(filePath);
                        return true;
                    }
                    catch (IOException e)
                    {
                        if (DateTime.UtcNow < timeout) continue;
                        ScreenshotManagerModule.Logger.Error(e.Message + e.StackTrace);
                    }
                }
                return false;
            });
        }
    }
}
