using System;
using Blish_HUD.Graphics.UI;
using Nekres.Music_Mixer.Core.UI.Models;
using Nekres.Music_Mixer.Core.UI.Views;

namespace Nekres.Music_Mixer.Core.UI.Presenters
{
    internal class ContextPresenter : Presenter<ContextView, MainModel>
    {
        public ContextPresenter(ContextView view, MainModel model) : base(view, model)
        {
            model.Changed += OnModelChanged;
        }

        private void OnModelChanged(object o, EventArgs e)
        {
            this.View.Library.Show(new LibraryView(this.Model));
        }

        protected override void Unload()
        {
            this.Model.Changed -= OnModelChanged;
            base.Unload();
        }
    }
}
