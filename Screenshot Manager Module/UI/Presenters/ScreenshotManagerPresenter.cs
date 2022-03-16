using Blish_HUD.Graphics.UI;
using Nekres.Screenshot_Manager.UI.Models;
using Nekres.Screenshot_Manager.UI.Views;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Nekres.Screenshot_Manager.UI.Presenters
{
    public class ScreenshotManagerPresenter : Presenter<ScreenshotManagerView, ScreenshotManagerModel>
    {

        public ScreenshotManagerPresenter(ScreenshotManagerView view, ScreenshotManagerModel model) : base(view, model)
        {
            this.Model.InvalidFileNameCharacters = Path.GetInvalidFileNameChars().Union(Path.GetInvalidPathChars());
        }

        private void LoadTextures()
        {
            this.Model.InspectIcon = ScreenshotManagerModule.ModuleInstance.ContentsManager.GetTexture("inspect.png");
            //this.Model.PortaitModeIcon128 = ScreenshotManagerModule.ModuleInstance.ContentsManager.GetTexture("portaitMode_icon_128x128.png");
            //this.Model.PortaitModeIcon512 = ScreenshotManagerModule.ModuleInstance.ContentsManager.GetTexture("portaitMode_icon_128x128.png");
            this.Model.TrashcanClosedIcon64 = ScreenshotManagerModule.ModuleInstance.ContentsManager.GetTexture("trashcanClosed_icon_64x64.png");
            this.Model.TrashcanOpenIcon64 = ScreenshotManagerModule.ModuleInstance.ContentsManager.GetTexture("trashcanOpen_icon_64x64.png");
            //this.Model.TrashcanClosedIcon128 = ScreenshotManagerModule.ModuleInstance.ContentsManager.GetTexture("trashcanClosed_icon_128x128.png");
            //this.Model.TrashcanOpenIcon128 = ScreenshotManagerModule.ModuleInstance.ContentsManager.GetTexture("trashcanOpen_icon_128x128.png");
            this.Model.IncompleteHeartIcon = ScreenshotManagerModule.ModuleInstance.ContentsManager.GetTexture("incomplete_heart.png");
            this.Model.CompleteHeartIcon = ScreenshotManagerModule.ModuleInstance.ContentsManager.GetTexture("complete_heart.png");
            this.Model.DeleteSearchBoxContentIcon = ScreenshotManagerModule.ModuleInstance.ContentsManager.GetTexture("784262.png");
        }

        protected override Task<bool> Load(IProgress<string> progress)
        {
            

            return base.Load(progress);
        }

        protected override void Unload()
        {

        }
    }
}
