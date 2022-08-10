using Blish_HUD.Graphics.UI;
using Nekres.Music_Mixer.Core.UI.Models;
using Nekres.Music_Mixer.Core.UI.Views;
using System;

namespace Nekres.Music_Mixer.Core.UI.Presenters
{
    internal class MusicContextConfigPresenter : Presenter<ConfigView, MusicContextModel>
    {
        public MusicContextConfigPresenter(ConfigView view, MusicContextModel model) : base(view, model)
        {
            model.Changed += View_OnModelChanged;
            model.Deleted += View_OnModelDeleted;
        }

        private async void View_OnModelChanged(object o, EventArgs e)
        {
            await MusicMixer.Instance.DataService.Upsert(this.Model);
        }

        private void View_OnModelDeleted(object o, EventArgs e)
        {
            this.View.Parent.Dispose();
        }

        protected override void Unload()
        {
            this.Model.Changed -= View_OnModelChanged;
            this.Model.Deleted -= View_OnModelDeleted;
            base.Unload();
        }
    }
}