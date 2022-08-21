using Blish_HUD;
using Blish_HUD.Controls.Extern;
using Blish_HUD.Controls.Intern;
using Microsoft.Xna.Framework.Input;
using Nekres.Chat_Shorts.Core;
using Nekres.Chat_Shorts.UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Keyboard = Blish_HUD.Controls.Intern.Keyboard;
using Mouse = Blish_HUD.Controls.Intern.Mouse;

namespace Nekres.Chat_Shorts.Services
{
    internal class ChatService : IDisposable
    {

        private DataService _dataService;

        private List<Macro> _activeMacros;

        private Dictionary<ModifierKeys, VirtualKeyShort> _modifierLookUp;

        public ChatService(DataService dataService)
        {
            _dataService = dataService;
            _activeMacros = new List<Macro>();
            _modifierLookUp = new Dictionary<ModifierKeys, VirtualKeyShort>()
            {
                {ModifierKeys.Alt, VirtualKeyShort.MENU},
                {ModifierKeys.Ctrl, VirtualKeyShort.CONTROL},
                {ModifierKeys.Shift, VirtualKeyShort.SHIFT}
            };
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
            await this.PastText(text, squadBroadcast);
        }

        private async Task PastText(string text, bool squadBroadcast = false)
        {
            byte[] prevClipboardContent = await ClipboardUtil.WindowsClipboardService.GetAsUnicodeBytesAsync();
            await ClipboardUtil.WindowsClipboardService.SetTextAsync(text)
                .ContinueWith(async clipboardResult => {
                    if (clipboardResult.IsFaulted) {
                        ChatShorts.Logger.Warn(clipboardResult.Exception, $"Failed to set clipboard text to {text}!");
                    } else {
                        await Task.Run(() =>
                        {
                            Focus(squadBroadcast);
                            Keyboard.Press(VirtualKeyShort.LCONTROL, true);
                            Keyboard.Stroke(VirtualKeyShort.KEY_V, true);
                            Thread.Sleep(50);
                            Keyboard.Release(VirtualKeyShort.LCONTROL, true);
                            Keyboard.Stroke(VirtualKeyShort.RETURN);
                            UnFocus();
                        }).ContinueWith(async _ =>
                        {
                            if (prevClipboardContent == null) return;
                            await ClipboardUtil.WindowsClipboardService.SetUnicodeBytesAsync(prevClipboardContent);
                        });
                    }
                });
        }

        private void Focus(bool squadBroadcast = false)
        {
            UnFocus();

            if (squadBroadcast)
            {
                if (ChatShorts.Instance.SquadBroadcast.Value.ModifierKeys != ModifierKeys.None) 
                {
                    Keyboard.Press(_modifierLookUp[ChatShorts.Instance.SquadBroadcast.Value.ModifierKeys]);
                }
                if (ChatShorts.Instance.SquadBroadcast.Value.PrimaryKey != Keys.None)
                {
                    Keyboard.Press((VirtualKeyShort)ChatShorts.Instance.SquadBroadcast.Value.PrimaryKey);
                    Keyboard.Release((VirtualKeyShort)ChatShorts.Instance.SquadBroadcast.Value.PrimaryKey);
                }
                if (ChatShorts.Instance.SquadBroadcast.Value.ModifierKeys != ModifierKeys.None) 
                {
                    Keyboard.Release(_modifierLookUp[ChatShorts.Instance.SquadBroadcast.Value.ModifierKeys]);
                }
                return;
            }

            if (ChatShorts.Instance.ChatMessage.Value.ModifierKeys != ModifierKeys.None)
            {
                Keyboard.Press(_modifierLookUp[ChatShorts.Instance.ChatMessage.Value.ModifierKeys]);
            }
            if (ChatShorts.Instance.ChatMessage.Value.PrimaryKey != Keys.None)
            {
                Keyboard.Press((VirtualKeyShort)ChatShorts.Instance.ChatMessage.Value.PrimaryKey);
                Keyboard.Release((VirtualKeyShort)ChatShorts.Instance.ChatMessage.Value.PrimaryKey);
            }
            if (ChatShorts.Instance.ChatMessage.Value.ModifierKeys != ModifierKeys.None) 
            {
                Keyboard.Release(_modifierLookUp[ChatShorts.Instance.ChatMessage.Value.ModifierKeys]);
            }
        }

        private void UnFocus()
        {
            Mouse.Click(MouseButton.LEFT, GameService.Graphics.WindowWidth - 1);
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
