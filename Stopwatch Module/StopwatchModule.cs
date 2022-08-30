using Blish_HUD;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nekres.Stopwatch.Core.Controllers;
using Nekres.Stopwatch.UI.Models;
using Nekres.Stopwatch.UI.Views;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace Stopwatch
{
    [Export(typeof(Module))]
    public class StopwatchModule : Module
    {
        internal static readonly Logger Logger = Logger.GetLogger<StopwatchModule>();

        internal static StopwatchModule ModuleInstance { get; private set; }

        #region Service Managers
        internal SettingsManager SettingsManager => this.ModuleParameters.SettingsManager;
        internal ContentsManager ContentsManager => this.ModuleParameters.ContentsManager;
        internal DirectoriesManager DirectoriesManager => this.ModuleParameters.DirectoriesManager;
        internal Gw2ApiManager Gw2ApiManager => this.ModuleParameters.Gw2ApiManager;
        #endregion

        [ImportingConstructor]
        public StopwatchModule([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters) => ModuleInstance = this;

        internal SettingEntry<KeyBinding> Toggle;
        internal SettingEntry<bool> StartOnMovementEnabled;
        internal SettingEntry<KeyBinding> Reset;
        internal SettingEntry<KeyBinding> SetStartTime;
        internal SettingEntry<ContentService.FontSize> FontSize;
        internal SettingEntry<float> BackgroundOpacity;
        internal SettingEntry<float> SoundVolume;
        internal SettingEntry<bool> TickingSoundDisabledSetting;
        internal SettingEntry<bool> BeepSoundDisabledSetting;

        // Hidden Internal Settings (Cache)
        internal SettingEntry<Color> FontColor;
        internal SettingEntry<Point> Position;
        internal SettingEntry<TimeSpan> StartTime;

        private StopwatchController _stopwatchController;

        protected override void DefineSettings(SettingCollection settings)
        {
            var hotkeys = settings.AddSubCollection("Control Options", true, false);
            Toggle = hotkeys.DefineSetting("toggleKey", new KeyBinding(Keys.LeftAlt), 
                () => "Toggle", 
                () => "Starts or pauses the stopwatch.");

            Reset = hotkeys.DefineSetting("resetKey", new KeyBinding(ModifierKeys.Alt, Keys.R),
                () => "Reset",
                () => "Rewinds and stops the stopwatch.");

            SetStartTime = hotkeys.DefineSetting("setStartTimeKey", new KeyBinding(ModifierKeys.Alt, Keys.C),
                () => "Set Goal Time",
                () => "Set a goal time and make the stopwatch count down into the negative.");


            var general = settings.AddSubCollection("General", true, false);
            StartOnMovementEnabled = general.DefineSetting("startOnMovement", false,
                () => "Wait for Character Movement",
                () => "When you activate the stopwatch it will delay its start until the moment you move from where you toggled it.\nIn competitive modes it will wait for camera movement instead.");
            FontSize = general.DefineSetting("fontSize", ContentService.FontSize.Size36, 
                () => "Font Size",
                () => "Sets the font size of the timer.");

            FontColor = general.DefineSetting("fontColor", Color.White,
                () => "Font Color",
                () => "Sets the font color of the timer.");

            BackgroundOpacity = general.DefineSetting("backgroundOpacity", 30f, 
                () => "Background Opacity",
                () => "Sets the transparency of the background.");

            var audio = settings.AddSubCollection("Sound Options", true, false);
            TickingSoundDisabledSetting = audio.DefineSetting("tickingSfxDisabled", false,
                () => "Disable Ticking Sound", 
                () => "Disables the ticking sounds");

            BeepSoundDisabledSetting = audio.DefineSetting("beepSfxDisabled", false,
                () => "Disable Beep Alerts",
                () => "Disables the beeping alerts during a count from three to zero.");

            SoundVolume = audio.DefineSetting("soundVolume", 80f,
                () => "Audio Volume",
                () => "Sets the volume of the audio effects");

            var hiddenSettingsCache = settings.AddSubCollection("hiddenSettingsCache", false, false);
            Position = hiddenSettingsCache.DefineSetting("position", new Point(180, 60));
            StartTime = hiddenSettingsCache.DefineSetting("startTime", TimeSpan.Zero);
        }

        protected override void Initialize()
        {
            _stopwatchController = new StopwatchController
            {
                FontColor = FontColor.Value,
                FontSize = FontSize.Value,
                BackgroundOpacity = BackgroundOpacity.Value / 100,
                AudioVolume = Math.Min(1, SoundVolume.Value / 1000),
                Position = Position.Value
            };
        }

        protected override async Task LoadAsync()
        {

        }

        public override IView GetSettingsView()
        {
            return new CustomSettingsView(new CustomSettingsModel(SettingsManager.ModuleSettings));
        }

        protected override void OnModuleLoaded(EventArgs e)
        {
            Toggle.Value.Enabled = true;
            SetStartTime.Value.Enabled = true;
            Reset.Value.Enabled = true;

            Toggle.Value.Activated += OnToggleActivated;
            SetStartTime.Value.Activated += SetStartTimeActivated;
            Reset.Value.Activated += OnResetActivated;
            SoundVolume.SettingChanged += OnSoundVolumeSettingChanged;
            FontSize.SettingChanged += OnFontSizeSettingChanged;
            FontColor.SettingChanged += OnFontColorSettingChanged;
            BackgroundOpacity.SettingChanged += OnBackgroundOpacitySettingChanged;
            Position.SettingChanged += OnPositionSettingChanged;
            // Base handler must be called
            base.OnModuleLoaded(e);
        }

        private void OnToggleActivated(object o, EventArgs e)
        {
            if (!GameService.GameIntegration.Gw2Instance.Gw2HasFocus || GameService.Gw2Mumble.UI.IsTextInputFocused) return;
            _stopwatchController.Toggle();
        }

        private void SetStartTimeActivated(object o, EventArgs e)
        {
            if (!GameService.GameIntegration.Gw2Instance.Gw2HasFocus || GameService.Gw2Mumble.UI.IsTextInputFocused) return;
            _stopwatchController.StartAt();
        }

        private void OnResetActivated(object o, EventArgs e)
        {
            _stopwatchController.Reset();
        }

        private void OnSoundVolumeSettingChanged(object o, ValueChangedEventArgs<float> e)
        {
            _stopwatchController.AudioVolume = e.NewValue / 1000;
        }

        private void OnFontSizeSettingChanged(object o, ValueChangedEventArgs<ContentService.FontSize> e)
        {
            _stopwatchController.FontSize = e.NewValue;
        }

        private void OnFontColorSettingChanged(object o, ValueChangedEventArgs<Color> e)
        {
            _stopwatchController.FontColor = e.NewValue;
        }

        private void OnBackgroundOpacitySettingChanged(object o, ValueChangedEventArgs<float> e)
        {
            _stopwatchController.BackgroundOpacity = e.NewValue / 100;
        }

        private void OnPositionSettingChanged(object o, ValueChangedEventArgs<Point> e)
        {
            _stopwatchController.Position = e.NewValue;
        }

        protected override void Update(GameTime gameTime)
        {
            _stopwatchController?.Update();
        }

        /// <inheritdoc />
        protected override void Unload()
        {
            Toggle.Value.Activated -= OnToggleActivated;
            SetStartTime.Value.Activated -= SetStartTimeActivated;
            Reset.Value.Activated -= OnResetActivated;
            SoundVolume.SettingChanged -= OnSoundVolumeSettingChanged;
            FontSize.SettingChanged -= OnFontSizeSettingChanged;
            FontColor.SettingChanged -= OnFontColorSettingChanged;
            BackgroundOpacity.SettingChanged -= OnBackgroundOpacitySettingChanged;
            Position.SettingChanged -= OnPositionSettingChanged;
            // Unload here
            _stopwatchController?.Dispose();
            // All static members must be manually unset
            ModuleInstance = null;
        }
    }
}
