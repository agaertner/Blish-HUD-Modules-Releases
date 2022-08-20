using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nekres.Mistwar.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace Nekres.Mistwar
{
    [Export(typeof(Module))]
    public class MistwarModule : Module
    {

        internal static readonly Logger Logger = Logger.GetLogger<MistwarModule>();

        internal static MistwarModule ModuleInstance;

        #region Service Managers
        internal SettingsManager SettingsManager => this.ModuleParameters.SettingsManager;
        internal ContentsManager ContentsManager => this.ModuleParameters.ContentsManager;
        internal DirectoriesManager DirectoriesManager => this.ModuleParameters.DirectoriesManager;
        internal Gw2ApiManager Gw2ApiManager => this.ModuleParameters.Gw2ApiManager;
        #endregion

        [ImportingConstructor]
        public MistwarModule([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters) { ModuleInstance = this; }

        // General settings
        internal SettingEntry<ColorType> ColorTypeSetting;
        internal SettingEntry<bool> TeamShapesSetting;
        // Hotkeys
        internal SettingEntry<KeyBinding> ToggleMapKeySetting;
        internal SettingEntry<KeyBinding> ToggleMarkersKeySetting;

        // Map settings
        internal SettingEntry<float> ColorIntensitySetting;
        internal SettingEntry<bool> DrawSectorsSetting;
        internal SettingEntry<float> ScaleRatioSetting;
        internal SettingEntry<bool> DrawObjectiveNamesSetting;
        internal SettingEntry<float> OpacitySetting;
        internal SettingEntry<bool> DrawRuinMapSetting;

        // Marker settings
        internal SettingEntry<bool> EnableMarkersSetting;
        internal SettingEntry<bool> HideInCombatSetting;
        internal SettingEntry<bool> DrawRuinMarkersSetting;
        internal SettingEntry<float> MaxViewDistanceSetting;
        internal SettingEntry<float> MarkerScaleSetting;

        protected override void DefineSettings(SettingCollection settings)
        {
            var generalSettings = settings.AddSubCollection("General", true, false);
            ColorTypeSetting = generalSettings.DefineSetting("ColorType", ColorType.Normal, () => "Color Type", () => "Select a different color type if you have a color deficiency.");
            TeamShapesSetting = generalSettings.DefineSetting("TeamShapes", true, () => "Team Shapes", () => "Enables uniquely shaped objective markers per team.");

            var hotKeySettings = settings.AddSubCollection("Control Options", true, false);
            ToggleMapKeySetting = hotKeySettings.DefineSetting("ToggleKey", new KeyBinding(Keys.N), () => "Toggle Map", () => "Key used to show and hide the strategic map.");
            ToggleMarkersKeySetting = hotKeySettings.DefineSetting("ToggleMarkersKey", new KeyBinding(Keys.OemOpenBrackets), () => "Toggle Markers", () => "Key used to show and hide the objective markers.");

            var mapSettings = settings.AddSubCollection("Map", true, false);
            DrawSectorsSetting = mapSettings.DefineSetting("DrawSectors", true, () => "Show Sector Boundaries", () => "Indicates if the sector boundaries should be drawn.");
            DrawObjectiveNamesSetting = mapSettings.DefineSetting("DrawObjectiveNames", true, () => "Show Objective Names", () => "Indicates if the names of the objectives should be drawn.");
            DrawRuinMapSetting = mapSettings.DefineSetting("ShowRuins", true, () => "Show Ruins", () => "Indicates if the ruins should be shown.");
            OpacitySetting = mapSettings.DefineSetting("Opacity", 80f, () => "Opacity", () => "Changes the opacity of the tactical map interface.");
            ColorIntensitySetting = mapSettings.DefineSetting("ColorIntensity", 80f, () => "Color Intensity", () => "Intensity of the background color.");
            ScaleRatioSetting = mapSettings.DefineSetting("ScaleRatio", 80f, () => "Scale Ratio", () => "Changes the size of the tactical map interface");

            var markerSettings = settings.AddSubCollection("Markers", true, false);
            EnableMarkersSetting = markerSettings.DefineSetting("EnableMarkers", true, () => "Enable Markers", () => "Enables the markers overlay which shows objectives at their world position.");
            HideInCombatSetting = markerSettings.DefineSetting("HideInCombat", true, () => "Hide in Combat", () => "Shows only the closest objective in combat and hides all others.");
            DrawRuinMarkersSetting = markerSettings.DefineSetting("ShowRuinMarkers", true, () => "Show Ruins", () => "Shows markers for the ruins.");
            MaxViewDistanceSetting = markerSettings.DefineSetting("MaxViewDistance", 50f, () => "Max View Distance", () => "The max view distance at which an objective marker can be seen.");
            MarkerScaleSetting = markerSettings.DefineSetting("ScaleRatio", 70f, () => "Scale Ratio", () => "Changes the size of the markers.");
        }

        private CornerIcon _moduleIcon;
        internal WvwService WvwService;
        private MapService _mapService;
        internal MarkerService MarkerService;

        private AsyncTexture2D _cornerTex;
        protected override void Initialize()
        {
            _cornerTex = new AsyncTexture2D(ContentsManager.GetTexture("corner_icon.png"));
            _moduleIcon = new CornerIcon(_cornerTex, this.Name);
            WvwService = new WvwService(Gw2ApiManager);
            if (EnableMarkersSetting.Value)
            {
                MarkerService = new MarkerService();
            }
            _mapService = new MapService(DirectoriesManager, WvwService, GetModuleProgressHandler());

        }

        protected override async Task LoadAsync()
        {
            if (!this.Gw2ApiManager.HasPermission(TokenPermission.Account)) return;
            _mapService.DownloadMaps(await WvwService.GetWvWMapIds(await WvwService.GetWorldId()));
        }

        protected override void OnModuleLoaded(EventArgs e)
        {
            Gw2ApiManager.SubtokenUpdated += OnSubtokenUpdated;
            ColorIntensitySetting.SettingChanged += OnColorIntensitySettingChanged;
            ToggleMapKeySetting.Value.Activated += OnToggleKeyActivated;
            ToggleMarkersKeySetting.Value.Activated += OnToggleMarkersKeyActivated;
            OpacitySetting.SettingChanged += OnOpacitySettingChanged;
            EnableMarkersSetting.SettingChanged += OnEnableMarkersSettingChanged;
            ToggleMapKeySetting.Value.Enabled = true;
            ToggleMarkersKeySetting.Value.Enabled = true;
            GameService.Gw2Mumble.CurrentMap.MapChanged += OnMapChanged;
            GameService.Gw2Mumble.UI.IsMapOpenChanged += OnIsMapOpenChanged;

            OnColorIntensitySettingChanged(null, new ValueChangedEventArgs<float>(0, ColorIntensitySetting.Value));
            OnOpacitySettingChanged(null, new ValueChangedEventArgs<float>(0, OpacitySetting.Value));

            _moduleIcon.Click += OnModuleIconClick;
            // Base handler must be called

            base.OnModuleLoaded(e);
        }

        private void OnModuleIconClick(object o, MouseEventArgs e)
        {
            _mapService.Toggle();
        }

        private void UpdateModuleLoading(string loadingMessage)
        {
            if (_moduleIcon == null) return;
            _moduleIcon.LoadingMessage = loadingMessage;
            if (loadingMessage == null && !GameService.Gw2Mumble.CurrentMap.Type.IsWorldVsWorld()) {
                _moduleIcon?.Dispose(); // Show during initialization, but hide on completion if we are not in WvW.
            }
        }

        public IProgress<string> GetModuleProgressHandler()
        {
            return new Progress<string>(UpdateModuleLoading);
        }

        private async void OnSubtokenUpdated(object o, ValueEventArgs<IEnumerable<TokenPermission>> e)
        {
            if (!e.Value.Contains(TokenPermission.Account)) return;
            _mapService.DownloadMaps(await WvwService.GetWvWMapIds(await WvwService.GetWorldId()));
        }

        private void OnOpacitySettingChanged(object o, ValueChangedEventArgs<float> e)
        {
            _mapService.Opacity = MathHelper.Clamp(e.NewValue / 100f, 0, 1);
        }

        private void OnToggleKeyActivated(object o, EventArgs e)
        {
            if (!GameUtil.IsUiAvailable() || !GameService.Gw2Mumble.CurrentMap.Type.IsWorldVsWorld()) return;
            _mapService.Toggle();
        }

        private void OnToggleMarkersKeyActivated(object o, EventArgs e)
        {
            EnableMarkersSetting.Value = !EnableMarkersSetting.Value;
            if (EnableMarkersSetting.Value)
            {
                MarkerService?.Dispose();
            }
            else
            {
                MarkerService = new MarkerService(WvwService.CurrentObjectives);
            }
        }

        protected override async void Update(GameTime gameTime)
        {
            await WvwService.Update();
        }

        private void OnColorIntensitySettingChanged(object o, ValueChangedEventArgs<float> e)
        {
            _mapService.ColorIntensity = (100 - e.NewValue) / 100f;
        }

        private void OnIsMapOpenChanged(object o, ValueEventArgs<bool> e)
        {
            ToggleMapKeySetting.Value.Enabled = GameService.Gw2Mumble.CurrentMap.Type.IsWorldVsWorld();
        }

        private void OnMapChanged(object o, ValueEventArgs<int> e)
        {
            if (GameService.Gw2Mumble.CurrentMap.Type.IsWorldVsWorld())
            {
                _moduleIcon?.Dispose();
                _moduleIcon = new CornerIcon(_cornerTex, this.Name);
                _moduleIcon.Click += OnModuleIconClick;
                ToggleMapKeySetting.Value.Enabled = true;
                return;
            }
            _moduleIcon?.Dispose();
            ToggleMapKeySetting.Value.Enabled = false;
        }

        private void OnEnableMarkersSettingChanged(object o, ValueChangedEventArgs<bool> e)
        {
            if (e.NewValue)
            {
                MarkerService ??= new MarkerService(WvwService.CurrentObjectives);
                return;
            }
            MarkerService?.Dispose();
            MarkerService = null;
        }

        /// <inheritdoc />
        protected override void Unload()
        {
            Gw2ApiManager.SubtokenUpdated -= OnSubtokenUpdated;
            ColorIntensitySetting.SettingChanged -= OnColorIntensitySettingChanged;
            ToggleMapKeySetting.Value.Activated -= OnToggleKeyActivated;
            ToggleMarkersKeySetting.Value.Activated -= OnToggleMarkersKeyActivated;
            OpacitySetting.SettingChanged -= OnOpacitySettingChanged;
            EnableMarkersSetting.SettingChanged -= OnEnableMarkersSettingChanged;
            ToggleMapKeySetting.Value.Enabled = false;
            ToggleMarkersKeySetting.Value.Enabled = false;
            GameService.Gw2Mumble.CurrentMap.MapChanged -= OnMapChanged;
            GameService.Gw2Mumble.UI.IsMapOpenChanged -= OnIsMapOpenChanged;

            MarkerService?.Dispose();
            _mapService?.Dispose();
            _moduleIcon?.Dispose();
            _cornerTex?.Dispose();
            // All static members must be manually unset
            ModuleInstance = null;
        }

    }

}
