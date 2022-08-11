using Blish_HUD;
using Blish_HUD.Content;
using Gw2Sharp.Models;
using Nekres.Music_Mixer.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nekres.Music_Mixer.Core.UI.Models
{
    internal class MusicContextModel
    {
        public event EventHandler<EventArgs> Changed;
        public event EventHandler<ValueEventArgs<Guid>> Deleted;

        private Guid _id;
        public Guid Id {
            get => _id;
            set
            {
                if (_id.Equals(value)) return;
                _id = value;
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        private string _title;
        public string Title
        {
            get => _title;
            set
            {
                if (!string.IsNullOrEmpty(_title) && _title.Equals(value)) return;
                _title = value;
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        private string _artist;
        public string Artist
        {
            get => _artist;
            set
            {
                if (!string.IsNullOrEmpty(_artist) && _artist.Equals(value)) return;
                _artist = value;
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        private string _uri;
        public string Uri
        {
            get => _uri;
            set
            {
                if (!string.IsNullOrEmpty(_uri) && _uri.Equals(value)) return;
                _uri = value;
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        private string _audioUrl;
        public string AudioUrl
        {
            get => _audioUrl;
            set
            {
                if (!string.IsNullOrEmpty(_audioUrl) && _audioUrl.Equals(value)) return;
                _audioUrl = value;
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        private TimeSpan _duration;
        public TimeSpan Duration
        {
            get => _duration;
            set
            {
                if (_duration.Equals(value)) return;
                _duration = value;
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        private ObservableCollection<int> _mapIds;
        public ObservableCollection<int> MapIds
        {
            get => _mapIds;
            set
            {
                if (value == null) return;
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
                if (value == null) return;
                _excludedMapIds = value;
                _excludedMapIds.CollectionChanged += (_, _) => Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        private ObservableCollection<TyrianTime> _dayTimes;
        public ObservableCollection<TyrianTime> DayTimes
        {
            get => _dayTimes;
            set
            {
                if (value == null) return;
                _dayTimes = value;
                _dayTimes.CollectionChanged += (_, _) => Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        private ObservableCollection<MountType> _mountTypes;
        public ObservableCollection<MountType> MountTypes
        {
            get => _mountTypes;
            set
            {
                if (value == null) return;
                _mountTypes = value;
                _mountTypes.CollectionChanged += (_, _) => Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        private Gw2StateService.State _state;
        public Gw2StateService.State State
        {
            get => _state;
            set
            {
                if (value == _state) return;
                _state = value;
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        private AsyncTexture2D _thumbnail;
        public AsyncTexture2D Thumbnail
        {
            get => _thumbnail;
            set
            {
                if (value == null) return;
                _thumbnail = value;
                _thumbnail.TextureSwapped += (_,_) => Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        private float _volume;
        public float Volume
        {
            get => _volume;
            set
            {
                if (Math.Abs(value - _volume) < 0.005) return;
                _volume = value;
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        public MusicContextModel(Gw2StateService.State state, string title, string artist, string url, TimeSpan duration,
            IEnumerable<int> mapIds = null,
            IEnumerable<int> excludedMapIds = null, 
            IEnumerable<TyrianTime> dayTimes = null,
            IEnumerable<MountType> mountTypes = null)
        {
            this.Id = Guid.NewGuid();
            this.Title = title;
            this.Artist = artist;
            this.Uri = url;
            this.Duration = duration;
            this.State = state;
            this.MapIds = new ObservableCollection<int>(mapIds ?? Enumerable.Empty<int>());
            this.ExcludedMapIds = new ObservableCollection<int>(excludedMapIds ?? Enumerable.Empty<int>());
            this.DayTimes = new ObservableCollection<TyrianTime>(dayTimes ?? Enumerable.Empty<TyrianTime>());
            this.MountTypes = new ObservableCollection<MountType>(mountTypes ?? Enumerable.Empty<MountType>());
            this.Thumbnail = new AsyncTexture2D();
            this.Volume = 1f;
        }

        public void Delete()
        {
            Deleted?.Invoke(this, new ValueEventArgs<Guid>(this.Id));
        }

        public static bool CanPlay(MusicContextModel model)
        {
            return model.State == MusicMixer.Instance.Gw2State.CurrentState 
                   && model.DayTimes.Contains(MusicMixer.Instance.ToggleFourDayCycleSetting.Value ? TyrianTimeUtil.GetCurrentDayCycle() : TyrianTimeUtil.GetCurrentDayCycle().Resolve())
                   && model.MapIds.Contains(GameService.Gw2Mumble.CurrentMap.Id)
                   && (!model.ExcludedMapIds.Any() || !model.ExcludedMapIds.Contains(GameService.Gw2Mumble.CurrentMap.Id))
                   && (!model.MountTypes.Any() || model.MountTypes.Contains(GameService.Gw2Mumble.PlayerCharacter.CurrentMount));
        }
    }
}
