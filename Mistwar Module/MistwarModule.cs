using Blish_HUD;
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
using Blish_HUD.Controls;

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

        internal SettingEntry<KeyBinding> ToggleKeySetting;
        internal SettingEntry<ColorType> ColorTypeSetting;
        internal SettingEntry<float> ColorIntensitySetting;
        internal SettingEntry<bool> DrawSectorsSetting;
        internal SettingEntry<float> ScaleRatioSetting;
        internal SettingEntry<bool> DrawObjectiveNamesSetting;
        internal SettingEntry<float> OpacitySetting;
        protected override void DefineSettings(SettingCollection settings)
        {
            ToggleKeySetting = settings.DefineSetting("ToggleKey", new KeyBinding(Keys.N), () => "Toggle Key", () => "Key used to show and hide the tactical map overlay.");
            ColorTypeSetting = settings.DefineSetting("ColorType", ColorType.Normal, () => "Color Type", () => "Select a different color type if you have a color deficiency.");
            ColorIntensitySetting = settings.DefineSetting("ColorIntensity", 80f, () => "Color Intensity", () => "Intensity of the background color.");
            OpacitySetting = settings.DefineSetting("Opacity", 80f, () => "Opacity", () => "Changes the opacity of the tactical map interface.");
            ScaleRatioSetting = settings.DefineSetting("ScaleRatio", 80f, () => "Scale Ratio", () => "Changes the size of the tactical map interface");
            DrawSectorsSetting = settings.DefineSetting("DrawSectors", true, () => "Draw Sector Boundaries", () => "Indicates if the sector boundaries should be drawn.");
            DrawObjectiveNamesSetting = settings.DefineSetting("DrawObjectiveNames", true, () => "Draw Objective Names", () => "Indicates if the names of the objectives should be drawn.");
        }

        private CornerIcon _moduleIcon;
        private WvwService _wvwService;
        private MapService _mapService;
        protected override void Initialize()
        {
            _moduleIcon = new CornerIcon(ContentsManager.GetTexture("corner_icon.png"), this.Name);
            _wvwService = new WvwService(Gw2ApiManager);
            _mapService = new MapService(Gw2ApiManager, DirectoriesManager, _wvwService, GetModuleProgressHandler());
        }

        protected override async Task LoadAsync()
        {

        }

        protected override void OnModuleLoaded(EventArgs e)
        {
            Gw2ApiManager.SubtokenUpdated += OnSubtokenUpdated;
            ColorIntensitySetting.SettingChanged += OnColorIntensitySettingChanged;
            ToggleKeySetting.Value.Activated += OnToggleKeyActivated;
            OpacitySetting.SettingChanged += OnOpacitySettingChanged;
            ToggleKeySetting.Value.Enabled = true;
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
                _moduleIcon.Hide(); // Show during initialization, but hide on completion if we are not in WvW.
            }
        }

        public IProgress<string> GetModuleProgressHandler()
        {
            return new Progress<string>(UpdateModuleLoading);
        }

        private async void OnSubtokenUpdated(object o, ValueEventArgs<IEnumerable<TokenPermission>> e)
        {
            if (!e.Value.Contains(TokenPermission.Account)) return;
            _mapService.DownloadMaps(await _wvwService.GetWvWMapIds(await _wvwService.GetWorldId()));
        }

        private bool IsUiAvailable() => GameService.Gw2Mumble.IsAvailable && GameService.GameIntegration.Gw2Instance.IsInGame && !GameService.Gw2Mumble.UI.IsMapOpen;

        private void OnOpacitySettingChanged(object o, ValueChangedEventArgs<float> e)
        {
            _mapService.Opacity = MathHelper.Clamp(e.NewValue / 100f, 0, 1);
        }

        private void OnToggleKeyActivated(object o, EventArgs e)
        {
            if (!IsUiAvailable() || !GameService.Gw2Mumble.CurrentMap.Type.IsWorldVsWorld()) return;
            _mapService.Toggle();
        }

        protected override async void Update(GameTime gameTime)
        {
            await _wvwService.Update();
        }

        private void OnColorIntensitySettingChanged(object o, ValueChangedEventArgs<float> e)
        {
            _mapService.ColorIntensity = (100 - e.NewValue) / 100f;
        }

        private void OnIsMapOpenChanged(object o, ValueEventArgs<bool> e)
        {
            ToggleKeySetting.Value.Enabled = GameService.Gw2Mumble.CurrentMap.Type.IsWorldVsWorld();
        }

        private void OnMapChanged(object o, ValueEventArgs<int> e)
        {
            if (GameService.Gw2Mumble.CurrentMap.Type.IsWorldVsWorld())
            {
                _moduleIcon.Hide();
                ToggleKeySetting.Value.Enabled = false;
                return;
            }
            _moduleIcon.Show();
            ToggleKeySetting.Value.Enabled = true;
        }

        /// <inheritdoc />
        protected override void Unload()
        {
            _mapService?.Dispose();
            _moduleIcon?.Dispose();
            GameService.Gw2Mumble.CurrentMap.MapChanged -= OnMapChanged;
            GameService.Gw2Mumble.UI.IsMapOpenChanged -= OnIsMapOpenChanged;
            ToggleKeySetting.Value.Enabled = false;
            Gw2ApiManager.SubtokenUpdated -= OnSubtokenUpdated;
            ColorIntensitySetting.SettingChanged -= OnColorIntensitySettingChanged;
            OpacitySetting.SettingChanged -= OnOpacitySettingChanged;
            // All static members must be manually unset
            ModuleInstance = null;
        }

    }

}
