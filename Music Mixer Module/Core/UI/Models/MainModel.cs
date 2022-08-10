using Nekres.Music_Mixer.Core.Services;
using System;
using Gw2Sharp.Models;

namespace Nekres.Music_Mixer.Core.UI.Models
{
    internal class MainModel
    {
        public event EventHandler<EventArgs> Changed;

        public readonly Gw2StateService.State State;

        private TyrianTime _dayCycle;
        public TyrianTime DayCycle
        { 
            get => _dayCycle;
            set
            {
                if (_dayCycle == value) return;
                _dayCycle = value;
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        private int _continentId;
        public int ContinentId
        {
            get => _continentId;
            set
            {
                if (_continentId == value) return;
                _continentId = value;
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        private int _regionId;
        public int RegionId
        {
            get => _regionId;
            set
            {
                if (_regionId == value) return;
                _regionId = value;
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        private int _mapId;
        public int MapId
        {
            get => _mapId;
            set
            {
                if (_mapId == value) return;
                _mapId = value;
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        private MountType _mountType;
        public MountType MountType
        {
            get => _mountType;
            set
            {
                if (_mountType == value) return;
                _mountType = value;
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        public MainModel(Gw2StateService.State state)
        {
            this.State = state;
            this.DayCycle = TyrianTime.Day;
            this.ContinentId = 1;
            this.RegionId = 4;
            this.MapId = 15;
            this.MountType = MountType.None;
        }
    }
}
