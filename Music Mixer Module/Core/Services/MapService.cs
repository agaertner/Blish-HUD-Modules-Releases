using Blish_HUD;
using Blish_HUD.Modules.Managers;
using Gw2Sharp.WebApi.Exceptions;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekres.Music_Mixer.Core.Services
{
    internal class MapService : IDisposable
    {
        public event EventHandler<ValueEventArgs<int>> RegionChanged;
        public event EventHandler<ValueEventArgs<int>> ContinentChanged;

        public IReadOnlyDictionary<int, string> PvERegions { get; private set; }

        public IReadOnlyDictionary<int, string> PvPRegions { get; private set; }


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

        private int _currentContinent;
        public int CurrentContinent
        {
            get => _currentContinent;
            set
            {
                if (value == _currentContinent) return;
                _currentContinent = value;
                ContinentChanged?.Invoke(this, new ValueEventArgs<int>(_currentContinent));
            }
        }

        private Dictionary<string, int> _mapsLookUp;

        private ContentsManager _ctnMgr;

        private AsyncCache<(int, int), List<ContinentFloorRegionMap>> _mapsRepository;

        private AsyncCache<int, int> _regionsRepository;

        public MapService(ContentsManager ctnMgr)
        {
            _ctnMgr = ctnMgr;
            _mapsRepository = new AsyncCache<(int, int), List<ContinentFloorRegionMap>>(x => RequestMapsForRegion(x.Item1,x.Item2));
            _regionsRepository = new AsyncCache<int, int>(MapUtil.GetRegion);

            GameService.Gw2Mumble.CurrentMap.MapChanged += OnMapChanged;
        }

        public async Task Initialize()
        {
            await RequestRegions();
            await LoadMapLookUp();
        }

        public Texture2D GetMapThumb(int mapId)
        {
            return _ctnMgr.GetTexture($"regions/maps/map_{mapId}.jpg");
        }

        public bool GetMapIdFromHash(string mapSHA1, out int mapId)
        {
            mapId = 0;
            if (!_mapsLookUp.ContainsKey(mapSHA1)) return false;
            mapId = _mapsLookUp[mapSHA1];
            return true;
        }

        public async Task<Dictionary<ContinentFloorRegionMap, Texture2D>> GetMapsForRegion(int contentId, int regionId)
        {
            var maps = await _mapsRepository.GetItem((contentId, regionId));
            return maps?.ToDictionary(map => map, map => GetMapThumb(map.Id));
        }

        private async Task<List<ContinentFloorRegionMap>> RequestMapsForRegion(int contentId, int regionId)
        {
            try
            {
                var floorIds = await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Continents[contentId].Floors.IdsAsync();
                var resolved = new List<string>();
                var thumbs = new List<ContinentFloorRegionMap>();
                foreach (var floorId in floorIds)
                {
                    try
                    {
                        var regions = await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Continents[contentId].Floors[floorId].Regions.GetAsync(regionId); 
                        foreach (var map in regions.Maps)
                        {
                            var hash = MapUtil.GetSHA1(contentId, map.Value.ContinentRect);
                            if (resolved.Contains(hash)) continue;
                            resolved.Add(hash);

                            // Resolve any mission instance map id to the actual public map id.
                            if (!GetMapIdFromHash(hash, out var mapId)) continue;

                            var publicMap = regions.Maps.Values.FirstOrDefault(x => x.Id == mapId);
                            if (publicMap == null) continue;

                            thumbs.Add(publicMap);
                        }
                    }
                    catch (NotFoundException)
                    {
                        // Ignore. Floor id does not exist in region or continent.
                    }
                }
                return thumbs;
            }
            catch (RequestException e)
            {
                MusicMixer.Logger.Error(e, e.Message);
            }
            return null;
        }

        private async Task RequestRegions()
        {
            try
            {
                PvERegions = await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Continents[1].Floors[1].Regions.AllAsync()
                    .ContinueWith(t =>
                        t.Result.Select(x => new KeyValuePair<int, string>(x.Id, x.Name))
                            .ToDictionary(x => x.Key, x => x.Value));

                PvPRegions = await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Continents[2].Floors[1].Regions.AllAsync()
                    .ContinueWith(t =>
                        t.Result.Select(x => new KeyValuePair<int, string>(x.Id, x.Name))
                            .ToDictionary(x => x.Key, x => x.Value));
            }
            catch (RequestException e)
            {
                MusicMixer.Logger.Error(e, e.Message);
            }
        }

        private async Task LoadMapLookUp()
        {
            using var stream = _ctnMgr.GetFileStream("regions/maps/maps.jsonc");
            stream.Position = 0;
            var buffer = new byte[stream.Length];
            await stream.ReadAsync(buffer, 0, buffer.Length);
            _mapsLookUp = JsonConvert.DeserializeObject<Dictionary<string, int>>(Encoding.UTF8.GetString(buffer));
        }

        private async void OnMapChanged(object o, ValueEventArgs<int> e)
        {
            this.CurrentContinent = GameService.Gw2Mumble.CurrentMap.Type.IsPvP() 
                                    || GameService.Gw2Mumble.CurrentMap.Type.IsWvW() ? 2 : 1;
            this.CurrentRegion = await _regionsRepository.GetItem(e.Value);
        }

        public void Dispose()
        {
            GameService.Gw2Mumble.CurrentMap.MapChanged -= OnMapChanged;
        }
    }
}
