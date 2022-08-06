using Blish_HUD.Graphics.UI;
using Nekres.Music_Mixer.Core.UI.Models;
using Nekres.Music_Mixer.Core.UI.Views.StateViews;
using System;

namespace Nekres.Music_Mixer.Core.UI.Presenters
{
    internal class MainPresenter : Presenter<MainView, MainModel>
    {
        public MainPresenter(MainView view, MainModel model) : base(view, model)
        {
            model.Changed += OnModelChanged;
        }

        private void OnModelChanged(object o, EventArgs e)
        {

        }

        protected override void Unload()
        {
            this.Model.Changed -= OnModelChanged;
            base.Unload();
        }
    }
}
