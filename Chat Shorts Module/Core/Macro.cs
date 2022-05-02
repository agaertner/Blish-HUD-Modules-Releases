using Blish_HUD.Input;
using Nekres.Chat_Shorts.UI.Models;
using System;
using System.Collections.Generic;

namespace Nekres.Chat_Shorts.Core
{
    internal class Macro : IDisposable
    {
        public event EventHandler<EventArgs> Activated;

        public Guid Id { get; set; }

        public string Text { get; set; }
        
        public GameMode Mode { get; set; }

        public IList<int> MapIds { get; set; }

        public KeyBinding KeyBinding { get; }

        public Macro(Guid id, KeyBinding binding)
        {
            this.Id = id;
            this.MapIds = new List<int>();
            this.KeyBinding = binding;
            this.KeyBinding.Enabled = true;
            this.KeyBinding.Activated += OnKeyBindingActivated;

        }

        private void OnKeyBindingActivated(object o, EventArgs e)
        {
            Activated?.Invoke(this, EventArgs.Empty);
        }

        public static Macro FromEntity(MacroEntity entity)
        {
            return new Macro(entity.Id, new KeyBinding(entity.ModifierKey, entity.PrimaryKey))
            {
                Text = entity.Text,
                Mode = entity.GameMode,
                MapIds = entity.MapIds
            };
        }

        public static Macro FromModel(MacroModel model)
        {
            return new Macro(model.Id, new KeyBinding(model.KeyBinding.ModifierKeys, model.KeyBinding.PrimaryKey))
            {
                Id = model.Id, 
                Text = model.Text,
                Mode = model.Mode,
                MapIds = model.MapIds
            };
        }

        public void Dispose()
        {
            this.KeyBinding.Enabled = false;
            this.KeyBinding.Activated -= OnKeyBindingActivated;
        }
    }
}
