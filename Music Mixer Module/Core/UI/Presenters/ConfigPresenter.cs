using Blish_HUD.Graphics.UI;
using Nekres.Music_Mixer.Core.UI.Models;
using Nekres.Music_Mixer.Core.UI.Views;
using System;

namespace Nekres.Music_Mixer.Core.UI.Presenters
{
    internal class ConfigPresenter : Presenter<ConfigView, ConfigModel>
    {
        public ConfigPresenter(ConfigView view, ConfigModel model) : base(view, model)
        {
            model.MusicContextModel.Changed += View_OnModelChanged;
            model.MusicContextModel.Deleted += View_OnModelDeleted;
        }

        private void View_OnModelChanged(object o, EventArgs e)
        {
            MusicMixer.Instance.DataService.Upsert(this.Model.MusicContextModel);
        }

        private void View_OnModelDeleted(object o, EventArgs e)
        {
            this.View.Parent.Dispose();
        }

        protected override void Unload()
        {
            this.Model.MusicContextModel.Changed -= View_OnModelChanged;
            this.Model.MusicContextModel.Deleted -= View_OnModelDeleted;
            base.Unload();
        }
    }
}