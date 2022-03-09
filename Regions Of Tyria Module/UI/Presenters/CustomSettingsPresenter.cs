using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Nekres.Regions_Of_Tyria.UI.Models;
using Nekres.Regions_Of_Tyria.UI.Views;
using System;
using System.Threading.Tasks;
namespace Nekres.Regions_Of_Tyria.UI.Presenters
{
    public class CustomSettingsPresenter : Presenter<CustomSettingsView, CustomSettingsModel>
    {
        public CustomSettingsPresenter(CustomSettingsView view, CustomSettingsModel model) : base(view, model) {}

        protected override Task<bool> Load(IProgress<string> progress)
        {
            this.View.SocialButtonClicked += View_SocialButtonClicked;
            return base.Load(progress);
        }

        protected override void Unload()
        {
            this.View.SocialButtonClicked -= View_SocialButtonClicked;
        }

        private void View_SocialButtonClicked(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(((Control)sender).BasicTooltipText);
        }
    }
}
