using Blish_HUD;
using Blish_HUD.Input;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.ObjectModel;
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

        private ObservableCollection<int> _mapIds;
        public ObservableCollection<int> MapIds
        {
            get => _mapIds;
            set
            {
                if (_mapIds != null && _mapIds.Equals(value)) return;
                _mapIds = value;
                _mapIds.CollectionChanged += (_, _) => Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        private ObservableCollection<int> _excludedMapIds;
        public ObservableCollection<int> ExcludedMapIds
        {
            get => _excludedMapIds;
            set
            {
                if (_excludedMapIds != null && _excludedMapIds.Equals(value)) return;
                _excludedMapIds = value;
                _excludedMapIds.CollectionChanged += (_, _) => Changed?.Invoke(this, EventArgs.Empty);
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

        private KeyBinding _keyBinding;

        public KeyBinding KeyBinding
        {
            get => _keyBinding;
            set
            {
                if (_keyBinding != null && _keyBinding.Equals(value)) return;
                _keyBinding = value;
            }
        }

        public MacroModel(Guid id, KeyBinding binding)
        {
            this.Id = id;
            this.MapIds = new ObservableCollection<int>();
            this.ExcludedMapIds = new ObservableCollection<int>();
            this.KeyBinding = binding;
            this.KeyBinding.Enabled = false;
            this.Title = "Empty Macro";
            this.Text = string.Empty;
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
                MapIds = this.MapIds.ToList(),
                ExcludedMapIds = this.ExcludedMapIds.ToList(),
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
                MapIds = new ObservableCollection<int>(entity.MapIds),
                ExcludedMapIds = new ObservableCollection<int>(entity.ExcludedMapIds)
            };
        }

        // Awful, hacky workaround for KeyBinding missing a KeysChanged event
        // TODO: Remove someday
        public void NewKeysAssigned()
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }
}
