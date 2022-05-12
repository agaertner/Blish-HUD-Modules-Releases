using Blish_HUD.Graphics.UI;
using Nekres.Chat_Shorts.UI.Models;
using Nekres.Chat_Shorts.UI.Views;
using System;
using System.Threading.Tasks;

namespace Nekres.Chat_Shorts.UI.Presenters
{
    internal class MacroEditPresenter : Presenter<MacroEditView, MacroModel>
    {
        public MacroEditPresenter(MacroEditView view, MacroModel model) : base(view, model)
        {
            model.Changed += View_OnModelChanged;
        }

        public async Task Delete()
        {
            await ChatShorts.Instance.DataService.DeleteById(this.Model.Id);
        }

        private async void View_OnModelChanged(object o, EventArgs e)
        {
            await ChatShorts.Instance.DataService.UpsertMacro(this.Model);
            ChatShorts.Instance.ChatService.ToggleMacro(this.Model);
        }
    }
}
