using System;
using System.IO;
using Blish_HUD;
using Blish_HUD.Controls;
namespace Nekres.Notes
{
    internal static class FileUtil
    {
        public static bool TryDelete(string filePath)
        {
            try
            {
                File.Delete(filePath);
            }
            catch (UnauthorizedAccessException)
            {
                ScreenNotification.ShowNotification($"Deletion of \"{Path.GetFileNameWithoutExtension(filePath)}\" failed. Access denied.", ScreenNotification.NotificationType.Error);
                return false;
            }
            catch (IOException ex)
            {
                NotesModule.Logger.Error(ex, ex.Message);
            }
            return true;
        }
    }
}