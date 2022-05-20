using System;
using System.Threading.Tasks;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Nekres.Music_Mixer.UI.Models;
using Nekres.Music_Mixer.UI.Presenters;

namespace Nekres.Music_Mixer.UI.Views
{
    internal class LibraryView : View<LibraryPresenter>
    {
        public LibraryView()
        {
            this.WithPresenter(new LibraryPresenter(this, new LibraryModel()));
        }

        protected override Task<bool> Load(IProgress<string> progress)
        {
            return base.Load(progress);
        }

        protected override void Build(Container buildPanel)
        {
            base.Build(buildPanel);
        }
    }
}
