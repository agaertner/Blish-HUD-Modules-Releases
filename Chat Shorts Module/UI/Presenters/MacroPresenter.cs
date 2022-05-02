using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Nekres.Chat_Shorts.UI.Models;
using Nekres.Chat_Shorts.UI.Views;

namespace Nekres.Chat_Shorts.UI.Presenters
{
    internal class MacroPresenter : Presenter<MacroView, MacroModel>
    {
        public MacroPresenter(MacroView view, MacroModel model) : base(view, model)
        {
            view.EditClick += OnEditButtonClicked;
        }

        private void OnEditButtonClicked(object o, MouseEventArgs e)
        {
            var bgTex = GameService.Content.GetTexture("controls/window/502049");
            var windowRegion = new Rectangle(40, 26, 895 + 38, 780 - 56);
            var contentRegion = new Rectangle(70, 41, 895 - 43, 780 - 42);
            var editWindow = new StandardWindow(bgTex, windowRegion, contentRegion)
            {
                Parent = GameService.Graphics.SpriteScreen,
                Location = new Point((GameService.Graphics.SpriteScreen.Width - windowRegion.Width) / 2, (GameService.Graphics.SpriteScreen.Height - windowRegion.Height) / 2),
                Title = $"Edit Macro - {this.Model.Title}",
                Id = $"ChatShorts_{nameof(MacroEditView)}_b273ada2-95ad-4d54-a071-44ca63c65120",
                SavesPosition = true
            };
            editWindow.Show(new MacroEditView(this.Model));
            editWindow.Hidden += (_, _) => editWindow.Dispose();
        }
    }
}
