using Blish_HUD;
using Gw2Sharp.Models;
using Nekres.Chat_Shorts.UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Nekres.Chat_Shorts.Core;

namespace Nekres.Chat_Shorts.Services
{
    internal class ChatService : IDisposable
    {

        private DataService _dataService;

        private IList<Macro> _activeMacros;

        public ChatService(DataService dataService)
        {
            _dataService = dataService;
            _activeMacros = new List<Macro>();
            GameService.Gw2Mumble.CurrentMap.MapChanged += OnMapChanged;
        }

        private async void OnMapChanged(object o, ValueEventArgs<int> e)
        {
            foreach (var macro in _activeMacros) macro.Activated -= OnMacroActivated;

            _activeMacros = await _dataService.GetAllActives().ContinueWith(t =>
            {
                var result = t.Result.Select(Macro.FromEntity).ToList();
                foreach (var entity in result) entity.Activated += OnMacroActivated;
                return result;
            });
        }

        public void UpdateMacro(MacroModel model)
        {
            var macro = _activeMacros.FirstOrDefault(x => x.Id.Equals(model.Id)) ?? Macro.FromModel(model);
            _activeMacros.Remove(macro);
            macro.Activated -= OnMacroActivated;
            macro.KeyBinding.ModifierKeys = model.KeyBinding.ModifierKeys;
            macro.KeyBinding.PrimaryKey = model.KeyBinding.PrimaryKey;
            macro.Text = model.Text;
            macro.MapIds = model.MapIds;
            macro.Mode = model.Mode;
            if (!macro.CanActivate()) return;
            macro.Activated += OnMacroActivated;
            _activeMacros.Add(macro);
        }

        private void OnMacroActivated(object o, EventArgs e) => SendToChat(((Macro)o).Text);

        public void SendToChat(string text)
        {
            if (!GameService.GameIntegration.Gw2Instance.IsInGame || GameService.Gw2Mumble.UI.IsTextInputFocused) return;
            GameService.GameIntegration.Chat.Send(text);
        }

        public void Dispose()
        {
            GameService.Gw2Mumble.CurrentMap.MapChanged -= OnMapChanged;
        }
    }
}
