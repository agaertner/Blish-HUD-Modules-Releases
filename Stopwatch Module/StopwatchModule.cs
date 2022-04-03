using Blish_HUD;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using Nekres.Stopwatch.UI.Models;
using Nekres.Stopwatch.UI.Views;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Blish_HUD.Input;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Xna.Framework.Input;
using Nekres.Stopwatch.Core.Controllers;

namespace Stopwatch
{
    [Export(typeof(Module))]
    public class StopwatchModule : Module
    {
        private static readonly Logger Logger = Logger.GetLogger<StopwatchModule>();

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
        internal SettingEntry<KeyBinding> Reset;
        internal SettingEntry<KeyBinding> ToggleCountdown;
        internal SettingEntry<ContentService.FontSize> FontSize;
        internal SettingEntry<Color> FontColor;
        internal SettingEntry<Point> Position;

        private StopwatchController _stopwatchController;

        protected override void DefineSettings(SettingCollection settings)
        {
            Toggle = settings.DefineSetting("toggleKey", new KeyBinding(ModifierKeys.Ctrl, Keys.S), 
                () => "Toggle", 
                () => "Toggles an active timer on or off");
            
            Reset = settings.DefineSetting("resetKey", new KeyBinding(ModifierKeys.Ctrl, Keys.R), 
                () => "Reset", 
                () => "Resets the timer.");
            
            ToggleCountdown = settings.DefineSetting("toggleCountdownKey", new KeyBinding(ModifierKeys.Ctrl, Keys.C), 
                () => "Toggle Countdown", 
                () => "Toggles a timer starting from a specified time counting down into the negative.");
            
            FontSize = settings.DefineSetting("fontSize", ContentService.FontSize.Size36, 
                () => "Font Size",
                () => "Sets the font size of the timer.");

            FontColor = settings.DefineSetting("fontColor", Color.White,
                () => "Font Color",
                () => "Sets the font color of the timer.");

            var hiddenSettingsCache = settings.AddSubCollection("hiddenSettingsCache", false, false);
            Position = hiddenSettingsCache.DefineSetting("position", new Point(180, 60));
        }

        protected override void Initialize()
        {
            _stopwatchController = new StopwatchController
            {
                FontColor = FontColor.Value,
                FontSize = FontSize.Value
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
            ToggleCountdown.Value.Enabled = true;
            Reset.Value.Enabled = true;

            Toggle.Value.Activated += OnToggleActivated;
            ToggleCountdown.Value.Activated += OnToggleCountdownActivated;
            Reset.Value.Activated += OnResetActivated;
            FontColor.SettingChanged += OnFontColorSettingChanged;
            // Base handler must be called
            base.OnModuleLoaded(e);
        }

        private void OnToggleActivated(object o, EventArgs e)
        {
            _stopwatchController.Toggle();
        }

        private void OnToggleCountdownActivated(object o, EventArgs e)
        {
            _stopwatchController.StartAt();
        }

        private void OnResetActivated(object o, EventArgs e)
        {
            _stopwatchController.Reset();
        }

        private void OnFontColorSettingChanged(object o, ValueChangedEventArgs<Color> e)
        {
            _stopwatchController.FontColor = e.NewValue;
        }

        protected override void Update(GameTime gameTime)
        {
            _stopwatchController?.Update();
        }

        /// <inheritdoc />
        protected override void Unload()
        {
            Toggle.Value.Activated -= OnToggleActivated;
            ToggleCountdown.Value.Activated -= OnToggleCountdownActivated;
            Reset.Value.Activated -= OnResetActivated;
            FontColor.SettingChanged -= OnFontColorSettingChanged;
            // Unload here
            _stopwatchController?.Dispose();
            // All static members must be manually unset
            ModuleInstance = null;
        }

    }

}
