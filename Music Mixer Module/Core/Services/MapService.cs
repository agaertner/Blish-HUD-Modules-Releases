using System;
using Blish_HUD;
using Gw2Sharp.WebApi.Exceptions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blish_HUD.Content;
using Blish_HUD.Modules.Managers;

namespace Nekres.Music_Mixer.Core.Services
{
    internal class MapService : IDisposable
    {
        public event EventHandler<ValueEventArgs<int>> RegionChanged;

        public IReadOnlyDictionary<int, string> RegionNames { get; private set; }

        public IReadOnlyDictionary<int, AsyncTexture2D> RegionThumbs { get; private set; }

        private int _currentRegion;
        public int CurrentRegion
        {
            get => _currentRegion;
            set
            {
                if (value == _currentRegion) return;
                _currentRegion = value;
                RegionChanged?.Invoke(this, new ValueEventArgs<int>(_currentRegion));
            }
        }

        private ContentsManager _ctnMgr;

        public MapService(ContentsManager ctnMgr)
        {
            _ctnMgr = ctnMgr;
            GameService.Gw2Mumble.CurrentMap.MapChanged += OnMapChanged;
        }

        public async Task RequestRegions()
        {
            try
            {
                RegionNames = await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Continents[1].Floors[1].Regions.AllAsync()
                .ContinueWith(t => 
                    t.Result.Select(x => new KeyValuePair<int, string>(x.Id, x.Name))
                            .ToDictionary(x => x.Key, x => x.Value));

                RegionThumbs = RegionNames.ToDictionary(x => x.Key, _ => new AsyncTexture2D());

                GetRegionThumbs();
            }
            catch (RequestException e)
            {
                MusicMixer.Logger.Error(e, e.Message);
            }
        }

        public void GetRegionThumbs()
        {
            foreach (var id in RegionThumbs.Keys)
            {
                this.RegionThumbs[id].SwapTexture(_ctnMgr.GetTexture($"regions/region_{id}.png"));
            }
        }

        private async void OnMapChanged(object o, ValueEventArgs<int> e)
        {
            this.CurrentRegion = await MapUtil.GetRegion(e.Value);
        }

        //TODO: Create UI for playlist per region.
        //TODO: Battle, mounted etc per region.
        //TODO: Change models accordingly, remove map inclusion/exclusion list.
        public void Dispose()
        {
            GameService.Gw2Mumble.CurrentMap.MapChanged -= OnMapChanged;
            foreach (var tex in RegionThumbs.Values)
            {
                tex?.Dispose();
            }
        }
    }
}
