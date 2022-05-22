using System;
using System.Threading.Tasks;
using Blish_HUD.Graphics.UI;
using Nekres.Music_Mixer.Core.UI.Models;
using Nekres.Music_Mixer.Core.UI.Views;

namespace Nekres.Music_Mixer.Core.UI.Presenters
{
    internal class MusicContextConfigPresenter : Presenter<MusicContextConfigView, MusicContextModel>
    {
        public MusicContextConfigPresenter(MusicContextConfigView view, MusicContextModel model) : base(view, model)
        {
            model.Changed += View_OnModelChanged;
        }

        public async Task Delete()
        {
            await MusicMixer.Instance.DataService.Delete(this.Model);
        }

        private async void View_OnModelChanged(object o, EventArgs e)
        {
            await MusicMixer.Instance.DataService.Upsert(this.Model);
        }

        protected override void Unload()
        {
            this.Model.Changed -= View_OnModelChanged;
            base.Unload();
        }
    }
}
