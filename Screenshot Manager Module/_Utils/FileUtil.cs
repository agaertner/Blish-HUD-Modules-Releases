using Microsoft.VisualBasic.FileIO;
using System;
using System.IO;
using System.Security;
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
                    catch (Exception e) when (e is IOException or UnauthorizedAccessException or SecurityException)
                    {
                        if (DateTime.UtcNow < timeout) continue;
                        ScreenshotManagerModule.Logger.Error(e, e.Message);
                        break;
                    }
                }
                return false;
            });
        }

        public static async Task<bool> DeleteAsync(string filePath, bool sendToRecycleBin = true)
        {
            return await Task.Run(() => {
                var timeout = DateTime.UtcNow.AddMilliseconds(ScreenshotManagerModule.FileTimeOutMilliseconds);
                while (DateTime.UtcNow < timeout)
                {
                    try
                    {
                        if (sendToRecycleBin)
                            FileSystem.DeleteFile(filePath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin, UICancelOption.DoNothing);
                        else
                            File.Delete(filePath);
                        return true;
                    }
                    catch (Exception e) when (e is IOException or UnauthorizedAccessException or SecurityException)
                    {
                        if (DateTime.UtcNow < timeout) continue;
                        ScreenshotManagerModule.Logger.Error(e, e.Message);
                        break;
                    }
                }
                return false;
            });
        }
    }
}
