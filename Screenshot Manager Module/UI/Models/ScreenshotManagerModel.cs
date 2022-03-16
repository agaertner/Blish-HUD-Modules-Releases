using System;
using System.Collections.Generic;
using Blish_HUD.Modules.Managers;
using Microsoft.Xna.Framework.Graphics;

namespace Nekres.Screenshot_Manager.UI.Models
{
    public class ScreenshotManagerModel : IDisposable
    {

        #region Textures

        public Texture2D CompleteHeartIcon;
        public Texture2D IncompleteHeartIcon;

        public Texture2D DeleteSearchBoxContentIcon;

        public Texture2D InspectIcon;

        public Texture2D TrashcanClosedIcon64;
        public Texture2D TrashcanOpenIcon64;
        //public Texture2D TrashcanClosedIcon128;
        //public Texture2D TrashcanOpenIcon128;

        //public Texture2D PortaitModeIcon128;
        //public Texture2D PortaitModeIcon512;
        #endregion

        public const int MaxFileNameLength = 50;

        public IEnumerable<char> InvalidFileNameCharacters;

        private static ContentsManager ContentsManager => ScreenshotManagerModule.ModuleInstance.ContentsManager;
        public ScreenshotManagerModel()
        {
        }

        public void Dispose()
        {
            InspectIcon?.Dispose();
            TrashcanClosedIcon64?.Dispose();
            TrashcanOpenIcon64?.Dispose();
            IncompleteHeartIcon?.Dispose();
            CompleteHeartIcon?.Dispose();
            DeleteSearchBoxContentIcon?.Dispose();
        }
    }
}
