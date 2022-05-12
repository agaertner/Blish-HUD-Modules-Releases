using Blish_HUD;
using Blish_HUD.Input;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nekres.Chat_Shorts.UI.Models
{
    internal class MacroModel
    {
        public event EventHandler<EventArgs> Changed;

        public Guid Id { get; private init; }

        private string _title;
        public string Title
        {
            get => _title;
            set
            {
                if (!string.IsNullOrEmpty(_title) && _title.Equals(value)) return;
                _title = value;
                Changed?.Invoke(this, new ValueEventArgs<string>(value));
            }
        }

        private GameMode _gameMode;
        public GameMode Mode
        {
            get => _gameMode;
            set
            {
                if (_gameMode == value) return;
                _gameMode = value;
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        private string _text;
        public string Text { 
            get => _text;
            set
            {
                if (!string.IsNullOrEmpty(_text) && _text.Equals(value)) return;
                _text = value;
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        private IList<int> _mapIds;
        public IList<int> MapIds
        {
            get => _mapIds;
            set
            {
                if (_mapIds != null && value != null && _mapIds.SequenceEqual(value)) return;
                _mapIds = value;
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        private IList<int> _excludedMapIds;
        public IList<int> ExcludedMapIds
        {
            get => _excludedMapIds;
            set
            {
                if (_excludedMapIds != null && value != null && _excludedMapIds.SequenceEqual(value)) return;
                _excludedMapIds = value;
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        private bool _squadBroadcast;
        public bool SquadBroadcast
        {
            get => _squadBroadcast;
            set
            {
                if (value == _squadBroadcast) return;
                _squadBroadcast = value;
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        public KeyBinding KeyBinding { get; }

        public MacroModel(Guid id, KeyBinding binding)
        {
            this.Id = id;
            this.MapIds = new List<int>();
            this.ExcludedMapIds = new List<int>();
            this.KeyBinding = binding;
            this.KeyBinding.Enabled = false;
            this.Title = "Empty Macro";
            this.Text = string.Empty;
        }

        internal void InvokeChanged()
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }

        public MacroModel() : this(Guid.NewGuid(), new KeyBinding(Keys.None))
        {
        }

        public MacroEntity ToEntity()
        {
            return new MacroEntity(this.Id)
            {
                Title = this.Title,
                GameMode = this.Mode,
                Text = this.Text,
                MapIds = this.MapIds,
                ExcludedMapIds = this.ExcludedMapIds,
                SquadBroadcast = this.SquadBroadcast,
                ModifierKey = this.KeyBinding.ModifierKeys,
                PrimaryKey = this.KeyBinding.PrimaryKey,
            };
        }

        public static MacroModel FromEntity(MacroEntity entity)
        {
            return new MacroModel(entity.Id, new KeyBinding(entity.ModifierKey, entity.PrimaryKey))
            {
                Title = entity.Title,
                Mode = entity.GameMode,
                SquadBroadcast = entity.SquadBroadcast,
                Text = entity.Text,
                MapIds = entity.MapIds,
                ExcludedMapIds = entity.ExcludedMapIds
            };
        } 
    }
}
