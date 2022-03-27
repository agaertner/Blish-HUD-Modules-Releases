using System;
using System.IO;
using System.Security;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;

namespace Nekres.Inquest_Module
{
    internal static class FileUtil
    {
        public static async Task<bool> MoveAsync(string oldFilePath, string newFilePath)
        {
            return await Task.Run(() => { 
                var timeout = DateTime.UtcNow.AddMilliseconds(10000);
                while (DateTime.UtcNow < timeout) {
                    try
                    {
                        File.Move(oldFilePath, newFilePath);
                        return true;
                    }
                    catch (Exception e) when (e is IOException or UnauthorizedAccessException or SecurityException)
                    {
                        if (DateTime.UtcNow < timeout) continue;
                    }
                }
                return false;
            });
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
                    catch (Exception e) when (e is IOException or UnauthorizedAccessException or SecurityException)
                    {
                        if (DateTime.UtcNow < timeout) continue;
                    }
                }
                return false;
            });
        }

        public static async Task<bool> SendToRecycleBinAsync(string filePath)
        {
            return await Task.Run(() => {
                var timeout = DateTime.UtcNow.AddMilliseconds(10000);
                while (DateTime.UtcNow < timeout)
                {
                    try
                    {
                        FileSystem.DeleteFile(filePath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin, UICancelOption.DoNothing);
                        return true;
                    }
                    catch (Exception e) when (e is IOException or UnauthorizedAccessException or SecurityException)
                    {
                        if (DateTime.UtcNow < timeout) continue;
                    }
                }
                return false;
            });
        }
    }
}
