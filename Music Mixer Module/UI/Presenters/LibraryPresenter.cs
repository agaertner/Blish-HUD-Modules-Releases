using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Graphics.UI;
using Nekres.Music_Mixer.UI.Models;
using Nekres.Music_Mixer.UI.Views;

namespace Nekres.Music_Mixer.UI.Presenters
{
    internal class LibraryPresenter : Presenter<LibraryView, LibraryModel>
    {
        public LibraryPresenter(LibraryView view, LibraryModel model) : base(view, model)
        {
        }
    }
}
