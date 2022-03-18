using Nekres.Screenshot_Manager.Core;
using System;

namespace Nekres.Screenshot_Manager.UI.Models
{
    public class ScreenshotManagerModel : IDisposable
    {

        #region Textures

        //public Texture2D DeleteSearchBoxContentIcon;

        //public Texture2D PortaitModeIcon128;
        //public Texture2D PortaitModeIcon512;

        #endregion

        public FileWatcherFactory FileWatcherFactory;
        public ScreenshotManagerModel(FileWatcherFactory fileWatcherFactory)
        {
            FileWatcherFactory = fileWatcherFactory;
        }

        public void Dispose()
        {
        }
    }
}
