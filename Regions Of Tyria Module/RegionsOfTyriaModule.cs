using Blish_HUD;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using Nekres.Regions_Of_Tyria.Geometry;
using RBush;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Blish_HUD.Extended.Core.Views;
using Blish_HUD.Graphics.UI;
using Nekres.Regions_Of_Tyria.UI.Controls;
using static Blish_HUD.GameService;
namespace Nekres.Regions_Of_Tyria
{
    [Export(typeof(Module))]
    public class RegionsOfTyriaModule : Module
    {
        internal static readonly Logger Logger = Logger.GetLogger(typeof(RegionsOfTyriaModule));

        internal static RegionsOfTyriaModule ModuleInstance;

        #region Service Managers
        internal SettingsManager SettingsManager => ModuleParameters.SettingsManager;
        internal ContentsManager ContentsManager => ModuleParameters.ContentsManager;
        internal DirectoriesManager DirectoriesManager => ModuleParameters.DirectoriesManager;
        internal Gw2ApiManager Gw2ApiManager => ModuleParameters.Gw2ApiManager;
        #endregion

        /// <summary>
        /// Fires when the in-game sector changes.
        /// </summary>
        public static event EventHandler<ValueEventArgs<int>> SectorChanged;

        private SettingEntry<float> _showDurationSetting;
        private SettingEntry<float> _fadeInDurationSetting;
        private SettingEntry<float> _fadeOutDurationSetting;
        private SettingEntry<bool> _toggleMapNotificationSetting;
        private SettingEntry<bool> _toggleSectorNotificationSetting;
        private SettingEntry<bool> _includeRegionInMapNotificationSetting;
        private SettingEntry<bool> _includeMapInSectorNotification;

        private float _showDuration;
        private float _fadeInDuration;
        private float _fadeOutDuration;

        private AsyncCache<int, Map> _mapRepository;
        private AsyncCache<int, RBush<Sector>> _sectorRepository;

        private static int _prevSectorId;
        public static int CurrentSector
        {
            get => _prevSectorId;
            private set
            {
                if (value == _prevSectorId) return;
                _prevSectorId = value;
                SectorChanged?.Invoke(ModuleInstance, new ValueEventArgs<int>(value));
            }
        }

        private int _prevMapId;
        private double _lastRun;
        private DateTime _lastUpdate;

        [ImportingConstructor]
        public RegionsOfTyriaModule([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters) { ModuleInstance = this; }

        protected override void DefineSettings(SettingCollection settings) {
            _showDurationSetting = settings.DefineSetting("ShowDuration", 40.0f, () => "Show Duration", () => "The duration in which to stay in full opacity.");
            _fadeInDurationSetting = settings.DefineSetting("FadeInDuration", 20.0f, () => "Fade-In Duration", () => "The duration of the fade-in.");
            _fadeOutDurationSetting = settings.DefineSetting("FadeOutDuration", 20.0f, () => "Fade-Out Duration", () => "The duration of the fade-out.");
            _toggleMapNotificationSetting = settings.DefineSetting("EnableMapChangedNotification", true, () => "Notify Map Change", () => "Whether a map's name should be shown when entering a map.");
            _includeRegionInMapNotificationSetting = settings.DefineSetting("IncludeRegionInMapNotification", true, () => "Include Region Name in Map Notification", () => "Whether the corresponding region name of a map should be shown above a map's name.");
            _toggleSectorNotificationSetting = settings.DefineSetting("EnableSectorChangedNotification", true, () => "Notify Sector Change", () => "Whether a sector's name should be shown when entering a sector.");
            _includeMapInSectorNotification = settings.DefineSetting("IncludeMapInSectorNotification", true, () => "Include Map Name in Sector Notification", () => "Whether the corresponding map name of a sector should be shown above a sector's name.");
        }

        protected override void Initialize()
        {
            _mapRepository = new AsyncCache<int, Map>(RequestMap);
            _sectorRepository = new AsyncCache<int, RBush<Sector>>(RequestSectors);
        }

        public override IView GetSettingsView()
        {
            return new SocialsSettingsView(new SocialsSettingsModel(SettingsManager.ModuleSettings, "https://pastebin.com/raw/Kk9DgVmL"));
        }

        protected override async void Update(GameTime gameTime) {
            
            if (gameTime.TotalGameTime.TotalMilliseconds - _lastRun < 10 || DateTime.UtcNow.Subtract(_lastUpdate).TotalMilliseconds < 1000 || !_toggleSectorNotificationSetting.Value || !Gw2Mumble.IsAvailable || !GameIntegration.Gw2Instance.IsInGame)
                return;

            _lastRun = gameTime.ElapsedGameTime.TotalMilliseconds;
            _lastUpdate = DateTime.UtcNow;

            var currentMap = await _mapRepository.GetItem(Gw2Mumble.CurrentMap.Id);
            var currentSector = await GetSector(currentMap);

            if (currentSector != null)
                MapNotification.ShowNotification(_includeMapInSectorNotification.Value ? currentMap.Name : null, currentSector.Name, null, _showDuration, _fadeInDuration, _fadeOutDuration);
        }

        protected override void OnModuleLoaded(EventArgs e)
        {
            Gw2Mumble.CurrentMap.MapChanged += OnMapChanged;
            Overlay.UserLocaleChanged += OnUserLocaleChanged;

            OnShowDurationSettingChanged(_showDurationSetting, new ValueChangedEventArgs<float>(0,_showDurationSetting.Value));
            OnFadeInDurationSettingChanged(_fadeInDurationSetting, new ValueChangedEventArgs<float>(0,_fadeInDurationSetting.Value));
            OnFadeOutDurationSettingChanged(_fadeOutDurationSetting, new ValueChangedEventArgs<float>(0,_fadeOutDurationSetting.Value));

            _showDurationSetting.SettingChanged += OnShowDurationSettingChanged;
            _fadeInDurationSetting.SettingChanged += OnFadeInDurationSettingChanged;
            _fadeOutDurationSetting.SettingChanged += OnFadeOutDurationSettingChanged;

            // Base handler must be called
            base.OnModuleLoaded(e);
        }

        private void OnShowDurationSettingChanged(object o, ValueChangedEventArgs<float> e) => _showDuration = MathHelper.Clamp(e.NewValue, 0, 100) / 10;
        private void OnFadeInDurationSettingChanged(object o, ValueChangedEventArgs<float> e) => _fadeInDuration = MathHelper.Clamp(e.NewValue, 0, 100) / 10;
        private void OnFadeOutDurationSettingChanged(object o, ValueChangedEventArgs<float> e) => _fadeOutDuration = MathHelper.Clamp(e.NewValue, 0, 100) / 10;

        /// <inheritdoc />
        protected override void Unload() {
            _showDurationSetting.SettingChanged -= OnShowDurationSettingChanged;
            _fadeInDurationSetting.SettingChanged -= OnFadeInDurationSettingChanged;
            _fadeOutDurationSetting.SettingChanged -= OnFadeOutDurationSettingChanged;
            Gw2Mumble.CurrentMap.MapChanged -= OnMapChanged;
            Overlay.UserLocaleChanged -= OnUserLocaleChanged;

            // All static members must be manually unset
            ModuleInstance = null;
        }

        private void OnUserLocaleChanged(object o, ValueEventArgs<System.Globalization.CultureInfo> e)
        {
            _mapRepository = new AsyncCache<int, Map>(RequestMap);
            _sectorRepository = new AsyncCache<int, RBush<Sector>>(RequestSectors);
        }

        private async void OnMapChanged(object o, ValueEventArgs<int> e)
        {
            if (!_toggleMapNotificationSetting.Value) 
                return;

            var currentMap = await _mapRepository.GetItem(e.Value);
            if (currentMap == null || currentMap.Id == _prevMapId)
                return;

            _prevMapId = currentMap.Id;

            var header = currentMap.RegionName;
            var text = currentMap.Name;

            //Some maps consist of just a single sector and hide their actual name in it.
            if (text.Equals(header, StringComparison.InvariantCultureIgnoreCase))
            {
                var currentSector = await GetSector(currentMap);
                if (currentSector != null && !string.IsNullOrEmpty(currentSector.Name))
                    text = currentSector.Name;
            }
            MapNotification.ShowNotification(_includeRegionInMapNotificationSetting.Value ? header : null, text, null, _showDuration, _fadeInDuration, _fadeOutDuration);
        }

        private async Task<Sector> GetSector(Map currentMap)
        {
            if (currentMap == null) 
                return null;
            var playerPos = Gw2Mumble.RawClient.IsCompetitiveMode ? Gw2Mumble.RawClient.CameraPosition : Gw2Mumble.RawClient.AvatarPosition;
            var playerLocation = playerPos.ToContinentCoords(CoordsUnit.Mumble, currentMap.MapRect, currentMap.ContinentRect).SwapYZ().ToPlane();
            var rtree = await _sectorRepository.GetItem(Gw2Mumble.CurrentMap.Id);
            var foundPoints = rtree.Search(new Envelope(playerLocation.X, playerLocation.Y, playerLocation.X, playerLocation.Y));
            if (foundPoints == null || foundPoints.Count == 0 || _prevSectorId.Equals(foundPoints[0].Id))
                return null;

            CurrentSector = foundPoints[0].Id;
            return foundPoints[0];
        }

        private async Task<RBush<Sector>> RequestSectors(int mapId)
        {
            return await await _mapRepository.GetItem(mapId).ContinueWith(async result =>
            {
                if (result.IsFaulted) 
                    return null;

                var map = result.Result;
                IEnumerable<Sector> sectors = new HashSet<Sector>();
                var comparer = ProjectionEqualityComparer<Sector>.Create(x => x.Id);
                foreach (var floor in map.Floors)
                    sectors = sectors.Union(await RequestSectorsForFloor(map.ContinentId, floor, map.RegionId, map.Id), comparer);

                var rtree = new RBush<Sector>();
                foreach (var sector in sectors) 
                    rtree.Insert(sector);

                return rtree;
            });
        }

        private async Task<IEnumerable<Sector>> RequestSectorsForFloor(int continentId, int floor, int regionId, int mapId) {
            try {
                return await Gw2ApiManager.Gw2ApiClient.V2.Continents[continentId].Floors[floor].Regions[regionId].Maps[mapId].Sectors.AllAsync().ContinueWith(task =>
                    {
                        var result = new HashSet<Sector>();
                        if (task.IsFaulted)
                            return result;
                        foreach (var sector in task.Result)
                            result.Add(new Sector(sector));
                        return result;
                    });
            } catch (Gw2Sharp.WebApi.Exceptions.BadRequestException bre) {
                Logger.Debug("{0} | The map id {1} does not exist on floor {2}.", bre.GetType().FullName, mapId, floor);
                return Enumerable.Empty<Sector>();
            } catch (Gw2Sharp.WebApi.Exceptions.UnexpectedStatusException use) {
                Logger.Debug(use.Message);
                return Enumerable.Empty<Sector>();
            }
        }

        private async Task<Map> RequestMap(int id)
        {
            try {
                return await Gw2ApiManager.Gw2ApiClient.V2.Maps.GetAsync(id).ContinueWith(task => task.IsFaulted || !task.IsCompleted ? null : task.Result);
            } catch (Gw2Sharp.WebApi.Exceptions.BadRequestException bre) {
                Logger.Debug(bre.Message);
                return null;
            } catch (Gw2Sharp.WebApi.Exceptions.UnexpectedStatusException use)  {
                Logger.Debug(use.Message);
                return null;
            }
        }
    }
}
