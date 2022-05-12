using Blish_HUD;
using Blish_HUD.Input;
using Microsoft.Xna.Framework.Input;
using Nekres.Chat_Shorts.UI.Models;
using System;
using System.Linq;

namespace Nekres.Chat_Shorts.Core
{
    internal class Macro : IDisposable
    {
        public event EventHandler<EventArgs> Activated;

        public MacroModel Model { get; }

        public KeyBinding KeyBinding { get; }

        public Macro(MacroModel model, KeyBinding binding)
        {
            this.Model = model;
            this.KeyBinding = binding;
            this.KeyBinding.Enabled = true;
            this.KeyBinding.Activated += OnKeyBindingActivated;
            this.Model.Changed += OnModelChanged;
        }

        public Macro() : this(new MacroModel(), new KeyBinding(Keys.None))
        {
        }

        private void OnModelChanged(object o, EventArgs e)
        {
            this.KeyBinding.ModifierKeys = this.Model.KeyBinding.ModifierKeys;
            this.KeyBinding.PrimaryKey = this.Model.KeyBinding.PrimaryKey;
        }

        private void OnKeyBindingActivated(object o, EventArgs e)
        {
            Activated?.Invoke(this, EventArgs.Empty);
        }

        public static Macro FromEntity(MacroEntity entity)
        {
            return new Macro(MacroModel.FromEntity(entity), new KeyBinding(entity.ModifierKey, entity.PrimaryKey));
        }

        public static Macro FromModel(MacroModel model)
        {
            return new Macro(model, new KeyBinding(model.KeyBinding.ModifierKeys, model.KeyBinding.PrimaryKey));
        }

        public bool CanActivate()
        {
            return (this.Model.Mode == MapUtil.GetCurrentGameMode() || this.Model.Mode == GameMode.All) &&
                   (this.Model.MapIds.Any(id => id == GameService.Gw2Mumble.CurrentMap.Id) || !this.Model.MapIds.Any()) &&
                   this.Model.ExcludedMapIds.All(id => id != GameService.Gw2Mumble.CurrentMap.Id) &&
                   (!this.Model.SquadBroadcast || GameService.Gw2Mumble.PlayerCharacter.IsCommander);
        }

        public void Dispose()
        {
            this.KeyBinding.Enabled = false;
            this.KeyBinding.Activated -= OnKeyBindingActivated;
            this.Model.Changed -= OnModelChanged;
        }
    }
}
