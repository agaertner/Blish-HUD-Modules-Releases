using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using Nekres.Chat_Shorts.UI.Controls;
using Nekres.Chat_Shorts.UI.Models;
using Nekres.Chat_Shorts.UI.Views;
using System;

namespace Nekres.Chat_Shorts.UI.Presenters
{
    internal class LibraryPresenter : Presenter<LibraryView, LibraryModel>
    {
        public LibraryPresenter(LibraryView view, LibraryModel model) : base(view, model)
        {
            this.View.AddNewClick += OnAddNewClicked;
        }

        private async void OnAddNewClicked(object o, EventArgs e)
        {
            var model = new MacroModel();
            this.AddMacro(model);
            await ChatShorts.Instance.DataService.UpsertMacro(model);
        }

        internal void AddMacro(MacroModel model)
        {
            var macroPanel = new MacroContainer(model)
            {
                Parent = this.View.MacroPanel,
                Size = new Point(345, 80)
            };
            macroPanel.Show(new MacroView(model));
        }
    }
}
