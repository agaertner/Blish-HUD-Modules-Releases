using System;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using Nekres.Musician_Module.UI.Models;
using Nekres.Musician_Module.UI.Views;

namespace Nekres.Musician_Module.UI.Presenters
{
    public class CustomSettingsPresenter : Presenter<CustomSettingsView, CustomSettingsModel>
    {
        public CustomSettingsPresenter(CustomSettingsView view, CustomSettingsModel model) : base(view, model) {}

        protected override Task<bool> Load(IProgress<string> progress)
        {
            this.View.BrowserButtonClick += View_BrowserButtonClicked;
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
    }
}
