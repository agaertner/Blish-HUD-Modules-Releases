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
            foreach (var macro in _activeMacros) macro.Activated -= SendToChat;

            _activeMacros = await _dataService.GetAllForMap(e.Value, GetCurrentGameMode()).ContinueWith(t =>
            {
                var result = t.Result.Select(Macro.FromEntity).ToList();
                foreach (var entity in result) entity.Activated += SendToChat;
                return result;
            });
        }

        public void UpdateMacro(MacroModel model)
        {
            var macro = _activeMacros.FirstOrDefault(x => x.Id.Equals(model.Id)) ?? Macro.FromModel(model);
            macro.Activated -= SendToChat;
            macro.KeyBinding.ModifierKeys = model.KeyBinding.ModifierKeys;
            macro.KeyBinding.PrimaryKey = model.KeyBinding.PrimaryKey;
            macro.Text = model.Text;
            macro.MapIds = model.MapIds;
            macro.Mode = model.Mode;
            var mode = GetCurrentGameMode();
            if ((model.Mode == mode || model.Mode == GameMode.All)
                && (model.MapIds.Any(id => id == GameService.Gw2Mumble.CurrentMap.Id) || !model.MapIds.Any()))
            {
                macro.Activated += SendToChat;
                return;
            }
            _activeMacros.Remove(macro);
        }

        private void SendToChat(object o, EventArgs e)
        {
            GameService.GameIntegration.Chat.Send(((Macro)o).Text);
        }

        public void Dispose()
        {
            GameService.Gw2Mumble.CurrentMap.MapChanged -= OnMapChanged;
        }

        private GameMode GetCurrentGameMode()
        {
            switch (GameService.Gw2Mumble.CurrentMap.Type)
            {
                case MapType.Pvp:
                case MapType.Gvg:
                case MapType.Tournament:
                case MapType.UserTournament:
                case MapType.EdgeOfTheMists:
                    return GameMode.PvP;
                case MapType.Instance:
                case MapType.Public:
                case MapType.Tutorial:
                case MapType.PublicMini:
                    return GameService.Gw2Mumble.CurrentMap.Id == 350 ? GameMode.PvP : GameMode.PvE; // Heart of the Mists
                case MapType.Center:
                case MapType.BlueHome:
                case MapType.GreenHome:
                case MapType.RedHome:
                case MapType.WvwLounge:
                case MapType.JumpPuzzle:
                    return GameMode.WvW;
                default: return GameMode.All;
            }
        }
    }
}
