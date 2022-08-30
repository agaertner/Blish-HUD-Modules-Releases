using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Nekres.Chat_Shorts.UI.Controls;
using Nekres.Chat_Shorts.UI.Models;
using Nekres.Chat_Shorts.UI.Views;
using System;
using System.Linq;

namespace Nekres.Chat_Shorts.UI.Presenters
{
    internal class LibraryPresenter : Presenter<LibraryView, LibraryModel>
    {
        public LibraryPresenter(LibraryView view, LibraryModel model) : base(view, model)
        {
            model.MacroModels = ChatShorts.Instance.DataService.GetAll().Select(MacroModel.FromEntity).ToList();
            this.View.AddNewClick += OnAddNewClicked;
        }

        private void OnAddNewClicked(object o, EventArgs e)
        {
            var model = new MacroModel();
            this.AddMacro(model);
            ChatShorts.Instance.DataService.UpsertMacro(model);
        }

        internal void AddMacro(MacroModel model)
        {
            var macroEntry = new MacroDetails(model)
            {
                Parent = this.View.MacroPanel,
                Size = new Point(345, 100)
            };
            macroEntry.EditClick += OnEditMacroClicked;
        }

        private void OnEditMacroClicked(object o, MouseEventArgs e)
        {
            var ctrl = (MacroDetails)o;
            ctrl.Active = true;
            var bgTex = GameService.Content.GetTexture("controls/window/502049");
            var windowRegion = new Rectangle(40, 26, 895 + 38, 780 - 56);
            var contentRegion = new Rectangle(70, 41, 895 - 43, 780 - 142);
            var editWindow = new StandardWindow(bgTex, windowRegion, contentRegion)
            {
                Emblem = ChatShorts.Instance.EditTexture,
                Parent = GameService.Graphics.SpriteScreen,
                Location = new Point((GameService.Graphics.SpriteScreen.Width - windowRegion.Width) / 2, (GameService.Graphics.SpriteScreen.Height - windowRegion.Height) / 2),
                Title = $"Edit Macro - {ctrl.Title}",
                Id = $"ChatShorts_{nameof(MacroEditView)}_b273ada2-95ad-4d54-a071-44ca63c65120",
                SavesPosition = true
            };
            editWindow.Show(new MacroEditView(ctrl.Model));
            editWindow.Hidden += (_, _) => editWindow.Dispose();
            editWindow.Disposed += (_, _) => ctrl.Active = false;
        }
    }
}
