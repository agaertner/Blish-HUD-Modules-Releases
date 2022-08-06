using Nekres.Music_Mixer.Core.Services;
using System;
using Gw2Sharp.Models;

namespace Nekres.Music_Mixer.Core.UI.Models
{
    internal class MainModel
    {
        public event EventHandler<EventArgs> Changed;
        
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

        private Gw2StateService.State _state;
        public Gw2StateService.State State
        {
            get => _state;
            set
            {
                if (_state == value) return;
                _state = value;
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
    }
}
