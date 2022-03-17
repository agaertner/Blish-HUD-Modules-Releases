using System;
using System.Collections.Generic;
using Blish_HUD.Modules.Managers;
using Microsoft.Xna.Framework.Graphics;
using Nekres.Screenshot_Manager.Core;

namespace Nekres.Screenshot_Manager.UI.Models
{
    public class ScreenshotManagerModel : IDisposable
    {

        #region Textures

        public Texture2D CompleteHeartIcon;
        public Texture2D IncompleteHeartIcon;

        public Texture2D DeleteSearchBoxContentIcon;

        //public Texture2D PortaitModeIcon128;
        //public Texture2D PortaitModeIcon512;
        #endregion

        public FileWatcherFactory FileWatcherFactory;
        private static ContentsManager ContentsManager => ScreenshotManagerModule.ModuleInstance.ContentsManager;
        public ScreenshotManagerModel(FileWatcherFactory fileWatcherFactory)
        {
            FileWatcherFactory = fileWatcherFactory;
        }

        public void Dispose()
        {
            IncompleteHeartIcon?.Dispose();
            CompleteHeartIcon?.Dispose();
            DeleteSearchBoxContentIcon?.Dispose();
        }
    }
}
