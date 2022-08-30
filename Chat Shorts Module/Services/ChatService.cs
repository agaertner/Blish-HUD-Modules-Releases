using Blish_HUD;
using Blish_HUD.Extended;
using Nekres.Chat_Shorts.Core;
using Nekres.Chat_Shorts.UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nekres.Chat_Shorts.Services
{
    internal class ChatService : IDisposable
    {

        private DataService _dataService;

        private List<Macro> _activeMacros;

        public ChatService(DataService dataService)
        {
            _dataService = dataService;
            _activeMacros = new List<Macro>();
            GameService.Gw2Mumble.CurrentMap.MapChanged += OnMapChanged;
            GameService.Gw2Mumble.PlayerCharacter.IsCommanderChanged += OnIsCommanderChanged;
        }

        private void OnMapChanged(object o, ValueEventArgs<int> e)
        {
            if (!ChatShorts.Instance.Loaded) return;
            LoadMacros();
        }

        private void OnIsCommanderChanged(object o, ValueEventArgs<bool> e)
        {
            if (!ChatShorts.Instance.Loaded) return;
            LoadMacros();
        }

        public void LoadMacros()
        {
            foreach (var macro in _activeMacros)
            {
                macro.Activated -= OnMacroActivated;
                macro.Dispose();
            }
            _activeMacros.Clear();
            foreach (var entity in _dataService.GetAllActives())
            {
                this.ToggleMacro(MacroModel.FromEntity(entity));
            }
        }

        public void ToggleMacro(MacroModel model)
        {
            var macro = _activeMacros.FirstOrDefault(x => x.Model.Id.Equals(model.Id));
            _activeMacros.RemoveAll(x => x.Model.Id.Equals(model.Id));
            if (macro != null) macro.Activated -= OnMacroActivated;
            macro?.Dispose();
            macro = Macro.FromModel(model);
            if (!macro.CanActivate()) return;
            macro.Activated += OnMacroActivated;
            _activeMacros.Add(macro);
        }

        private async void OnMacroActivated(object o, EventArgs e)
        {
            var macro = (Macro)o;
            await this.Send(macro.Model.Text, macro.Model.SquadBroadcast);
        }

        public async Task Send(string text, bool squadBroadcast = false)
        {
            if (IsBusy() || !IsTextValid(text)) return;
            if (squadBroadcast && !GameService.Gw2Mumble.PlayerCharacter.IsCommander) return;
            await ChatUtil.Send(text, squadBroadcast ? ChatShorts.Instance.SquadBroadcast.Value : ChatShorts.Instance.ChatMessage.Value);
        }

        private bool IsTextValid(string text)
        {
            return !string.IsNullOrEmpty(text) && text.Length < 200;
            // More checks? (Symbols: https://wiki.guildwars2.com/wiki/User:MithranArkanere/Charset)
        }

        private bool IsBusy()
        {
            return !GameService.GameIntegration.Gw2Instance.Gw2IsRunning 
                   || !GameService.GameIntegration.Gw2Instance.Gw2HasFocus 
                   || !GameService.GameIntegration.Gw2Instance.IsInGame
                   || GameService.Gw2Mumble.UI.IsTextInputFocused;
        }

        public void Dispose()
        {
            foreach (var macro in _activeMacros)
            {
                macro.Activated -= OnMacroActivated;
                macro.Dispose();
            }
            GameService.Gw2Mumble.CurrentMap.MapChanged -= OnMapChanged;
            GameService.Gw2Mumble.PlayerCharacter.IsCommanderChanged -= OnIsCommanderChanged;
        }
    }
}
