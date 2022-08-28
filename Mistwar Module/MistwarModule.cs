using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
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
using System.Threading.Tasks;
using Blish_HUD.Extended;
using Blish_HUD.Extended.Core.Views;

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
        internal SettingEntry<KeyBinding> ChatMessageKeySetting;

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
        internal SettingEntry<bool> HideAlliedMarkersSetting;
        internal SettingEntry<bool> DrawRuinMarkersSetting;
        internal SettingEntry<bool> DrawEmergencyWayPointsSetting;
        internal SettingEntry<bool> DrawDistanceSetting;
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
            ChatMessageKeySetting = hotKeySettings.DefineSetting("ChatMessageKey", new KeyBinding(Keys.Enter), () => "Chat Message", () => "Give focus to the chat edit box.");

            var mapSettings = settings.AddSubCollection("Map", true, false);
            DrawSectorsSetting = mapSettings.DefineSetting("DrawSectors", true, () => "Show Sector Boundaries", () => "Indicates if the sector boundaries should be drawn.");
            DrawObjectiveNamesSetting = mapSettings.DefineSetting("DrawObjectiveNames", true, () => "Show Objective Names", () => "Indicates if the names of the objectives should be drawn.");
            DrawRuinMapSetting = mapSettings.DefineSetting("ShowRuins", true, () => "Show Ruins", () => "Indicates if the ruins should be shown.");
            DrawEmergencyWayPointsSetting = mapSettings.DefineSetting("ShowEmergencyWayPoints", false, () => "Show Emergency Waypoints", () => "Shows your team's Emergency Waypoints.");
            OpacitySetting = mapSettings.DefineSetting("Opacity", 80f, () => "Opacity", () => "Changes the opacity of the tactical map interface.");
            ColorIntensitySetting = mapSettings.DefineSetting("ColorIntensity", 80f, () => "Color Intensity", () => "Intensity of the background color.");
            ScaleRatioSetting = mapSettings.DefineSetting("ScaleRatio", 80f, () => "Scale Ratio", () => "Changes the size of the tactical map interface");

            var markerSettings = settings.AddSubCollection("Markers", true, false);
            EnableMarkersSetting = markerSettings.DefineSetting("EnableMarkers", true, () => "Enable Markers", () => "Enables the markers overlay which shows objectives at their world position.");
            HideAlliedMarkersSetting = markerSettings.DefineSetting("HideAlliedMarkers", false, () => "Hide Allied Objectives", () => "Only hostile objectives will be shown.");
            HideInCombatSetting = markerSettings.DefineSetting("HideInCombat", true, () => "Hide in Combat", () => "Only the closest objective will be shown when in combat.");
            DrawRuinMarkersSetting = markerSettings.DefineSetting("ShowRuinMarkers", true, () => "Show Ruins", () => "Show markers for the ruins.");
            DrawDistanceSetting = markerSettings.DefineSetting("ShowDistance", true, () => "Show Distance", () => "Show flight distance to objectives.");
            MaxViewDistanceSetting = markerSettings.DefineSetting("MaxViewDistance", 50f, () => "Max View Distance", () => "The max view distance at which an objective marker can be seen.");
            MarkerScaleSetting = markerSettings.DefineSetting("ScaleRatio", 70f, () => "Scale Ratio", () => "Changes the size of the markers.");
        }

        private AsyncTexture2D _cornerTex;
        private CornerIcon _moduleIcon;
        internal WvwService WvwService;
        private MapService _mapService;
        internal MarkerService MarkerService;

        protected override void Initialize()
        {
            ChatMessageKeySetting.Value.Enabled = false;
            _cornerTex = new AsyncTexture2D(ContentsManager.GetTexture("corner_icon.png"));
            _moduleIcon = new CornerIcon(_cornerTex, this.Name);
            WvwService = new WvwService(Gw2ApiManager);
            if (EnableMarkersSetting.Value)
            {
                MarkerService = new MarkerService();
            }
            _mapService = new MapService(DirectoriesManager, WvwService, GetModuleProgressHandler());
        }

        public override IView GetSettingsView()
        {
            return new SocialsSettingsView(new SocialsSettingsModel(this.SettingsManager.ModuleSettings, "https://pastebin.com/raw/Kk9DgVmL"));
        }

        protected override async Task LoadAsync()
        {
            await this.WvwService.LoadAsync();
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
            GameService.Gw2Mumble.CurrentMap.MapChanged += OnMapChanged;
            GameService.Gw2Mumble.UI.IsMapOpenChanged += OnIsMapOpenChanged;
            GameService.GameIntegration.Gw2Instance.IsInGameChanged += OnIsInGameChanged;
            ToggleMapKeySetting.Value.Enabled = true;
            ToggleMarkersKeySetting.Value.Enabled = true;

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
            if (loadingMessage == null && !GameService.Gw2Mumble.CurrentMap.Type.IsWvWMatch()) {
                _moduleIcon?.Dispose(); // Show during initialization, but hide on completion if we are not in WvW.
            }
        }

        public IProgress<string> GetModuleProgressHandler()
        {
            return new Progress<string>(UpdateModuleLoading);
        }

        private async void OnSubtokenUpdated(object o, ValueEventArgs<IEnumerable<TokenPermission>> e)
        {
            _mapService.DownloadMaps(await WvwService.GetWvWMapIds(await WvwService.GetWorldId()));
        }

        private void OnOpacitySettingChanged(object o, ValueChangedEventArgs<float> e)
        {
            _mapService.Opacity = MathHelper.Clamp(e.NewValue / 100f, 0, 1);
        }

        private void OnToggleKeyActivated(object o, EventArgs e)
        {
            _mapService.Toggle();
            MarkerService?.Toggle(_mapService.IsVisible);
        }

        private void OnToggleMarkersKeyActivated(object o, EventArgs e)
        {
            EnableMarkersSetting.Value = !EnableMarkersSetting.Value;
        }

        protected override async void Update(GameTime gameTime)
        {
            if (!Gw2ApiManager.HasPermission(TokenPermission.Account)) return;
            await WvwService.Update();
        }

        private void OnColorIntensitySettingChanged(object o, ValueChangedEventArgs<float> e)
        {
            _mapService.ColorIntensity = (100 - e.NewValue) / 100f;
        }

        private void OnIsMapOpenChanged(object o, ValueEventArgs<bool> e)
        {
            ToggleMapKeySetting.Value.Enabled = GameService.Gw2Mumble.CurrentMap.Type.IsWvWMatch();
        }

        private void OnMapChanged(object o, ValueEventArgs<int> e)
        {
            if (GameService.Gw2Mumble.CurrentMap.Type.IsWvWMatch())
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

        private void OnIsInGameChanged(object o, ValueEventArgs<bool> e)
        {
            if (e.Value && GameService.Gw2Mumble.CurrentMap.Type.IsWvWMatch())
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

        private async void OnEnableMarkersSettingChanged(object o, ValueChangedEventArgs<bool> e)
        {
            if (e.NewValue)
            {
                MarkerService ??= new MarkerService();
                if (!GameService.Gw2Mumble.CurrentMap.Type.IsWvWMatch()) return;
                var obj = await WvwService.GetObjectives(GameService.Gw2Mumble.CurrentMap.Id);
                MarkerService?.ReloadMarkers(obj);
                MarkerService?.Toggle(_mapService.IsVisible);
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
            GameService.GameIntegration.Gw2Instance.IsInGameChanged -= OnIsInGameChanged;
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
