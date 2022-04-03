using System;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using Nekres.Stopwatch.Core.Controls;
using Nekres.Stopwatch.UI.Models;
using Nekres.Stopwatch.UI.Views;
using Stopwatch;

namespace Nekres.Stopwatch.UI.Presenters
{
    public class CustomSettingsPresenter : Presenter<CustomSettingsView, CustomSettingsModel>
    {
        public CustomSettingsPresenter(CustomSettingsView view, CustomSettingsModel model) : base(view, model) {}

        protected override Task<bool> Load(IProgress<string> progress)
        {
            this.View.BrowserButtonClick += View_BrowserButtonClicked;
            this.View.PositionButtonClick += View_PositionButtonClicked;
            return base.Load(progress);
        }

        protected override void Unload()
        {
            this.View.BrowserButtonClick -= View_BrowserButtonClicked;
        }

        private void View_BrowserButtonClicked(object o, EventArgs e)
        {
            GameService.Overlay.BlishHudWindow.Hide();
            BrowserUtil.OpenInDefaultBrowser(((Control)o).BasicTooltipText);
        }

        private void View_PositionButtonClicked(object o, EventArgs e)
        {
            var tempSizeSetting = new SettingEntry<Point>
            {
                Value = new Point(400, 100)
            };

            var choseLocation = new SpriteScreenMover(new ScreenRegion("Stopwatch", StopwatchModule.ModuleInstance.Position, tempSizeSetting));
            choseLocation.Parent = GameService.Graphics.SpriteScreen;
            choseLocation.Size = GameService.Graphics.SpriteScreen.ContentRegion.Size;
        }
    }
}
