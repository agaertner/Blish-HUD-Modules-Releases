using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Controls.Extern;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nekres.Inquest_Module.Core.Controllers;
using Nekres.Inquest_Module.UI.Models;
using Nekres.Inquest_Module.UI.Views;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Gw2Sharp.Models;

namespace Nekres.Inquest_Module
{
    [Export(typeof(Module))]
    public class InquestModule : Module
    {
        internal static readonly Logger Logger = Logger.GetLogger(typeof(InquestModule));

        internal static InquestModule ModuleInstance;

        #region Service Managers

        internal SettingsManager SettingsManager => ModuleParameters.SettingsManager;
        internal ContentsManager ContentsManager => ModuleParameters.ContentsManager;
        internal DirectoriesManager DirectoriesManager => ModuleParameters.DirectoriesManager;
        internal Gw2ApiManager Gw2ApiManager => ModuleParameters.Gw2ApiManager;

        #endregion

        internal SettingEntry<KeyBinding> AutoClickHoldKeySetting;
        internal SettingEntry<bool> HoldKeyWithLeftClickEnabledSetting;
        internal SettingEntry<KeyBinding> AutoClickToggleKeySetting;
        internal SettingEntry<bool> AutoClickSoundDisabledSetting;
        internal SettingEntry<float> AutoClickSoundVolume;
        internal SettingEntry<KeyBinding> JumpKeyBindingSetting;
        internal SettingEntry<KeyBinding> DodgeKeyBindingSetting;
        internal SettingEntry<KeyBinding> DodgeJumpKeyBindingSetting;

        // Hidden Settings (Cache)
        internal SettingEntry<double> AutoClickToggleInterval;

        private CornerIcon _moduleIcon;

        private AutoClickController _autoClickController;
        private DateTime _nextDodgeJump = DateTime.UtcNow;

        [ImportingConstructor]
        public InquestModule([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters)
        {
            ModuleInstance = this;
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            var hotkeys = settings.AddSubCollection("Control Options", true, false);

            AutoClickHoldKeySetting = hotkeys.DefineSetting("autoClickHoldKeyBinding", new KeyBinding(Keys.OemComma), 
                () => "Hold Double Clicking", 
                () => "Perform Double Clicks at the current cursor position while the key is being pressed.");
            AutoClickToggleKeySetting = hotkeys.DefineSetting("autoClickToggleKeyBinding", new KeyBinding(Keys.OemOpenBrackets),
                () => "Toggle Double Clicking",
                () => "Perform Double Clicks in an interval at the position of the cursor at the time of pressing the key.");
            DodgeJumpKeyBindingSetting = hotkeys.DefineSetting("dodgeJumpKeyBinding", new KeyBinding(ModifierKeys.Ctrl, Keys.Space),
                () => "Dodge-Jump",
                () => "Perform a dodge roll and a jump simultaneously.");

            var controlOptions = hotkeys.AddSubCollection("Movement Keys to Trigger on Dodge-Jump", true, false);
            DodgeKeyBindingSetting = controlOptions.DefineSetting("dodgeKeyBinding", new KeyBinding(Keys.V), () => "Dodge", () => "Do an evasive dodge roll, negating damage, in the direction your character is moving (backward if stationary).");
            JumpKeyBindingSetting = controlOptions.DefineSetting("jumpKeyBinding", new KeyBinding(Keys.Space), () => "Jump", () => "Press to jump over obstacles.");

            HoldKeyWithLeftClickEnabledSetting = hotkeys.DefineSetting("holdKeyWithLeftClick", false,
                () => "Hold Double Clicking + Left Mouse Button",
                () => "Perform Double clicks at the current cursor position while Hold Double Clicking and Left Mouse Button is being pressed.");

            var audio = settings.AddSubCollection("Sound Options", true, false);
            AutoClickSoundDisabledSetting = audio.DefineSetting("autoClickSoundsDisabled", false, 
                () => "Disable Clicking Sounds", 
                () => "Disables the sound alert when an auto click is performed.");

            AutoClickSoundVolume = audio.DefineSetting("autoClickSoundsVolume", 80f,
                () => "Clicking Sounds Volume", 
                () => "Sets the audio volume of the clicking alerts.");

            var hiddenSettingsCache = settings.AddSubCollection("hiddenSettingsCache", false, false);
            AutoClickToggleInterval = hiddenSettingsCache.DefineSetting("autoClickToggleInterval", 0.0);
        }

        protected override void Initialize()
        {
            _autoClickController = new AutoClickController();
            _autoClickController.SoundVolume = AutoClickSoundVolume.Value / 1000;

            /*_moduleIcon = new CornerIcon
            {
                IconName = "Inquest Chipset",
                Icon = ContentsManager.GetTexture("assault_cube_icon.png"),
                Priority = "Inquest Chipset".GetHashCode()
            };*/
            JumpKeyBindingSetting.Value.Enabled = false;
            DodgeKeyBindingSetting.Value.Enabled = false;
            DodgeJumpKeyBindingSetting.Value.Enabled = true;
            ToggleDodgeJump(GameService.Gw2Mumble.PlayerCharacter.CurrentMount == MountType.None);
            AutoClickToggleKeySetting.Value.Enabled = true;
            AutoClickHoldKeySetting.Value.Enabled = true;
            AutoClickSoundVolume.SettingChanged += OnAutoClickSoundVolumeSettingChanged;
            GameService.Gw2Mumble.PlayerCharacter.CurrentMountChanged += OnCurrentMountChanged;
            GameService.Gw2Mumble.UI.IsTextInputFocusedChanged += OnIsTextInputFocusChanged;
            GameService.Gw2Mumble.UI.IsMapOpenChanged += OnIsMapOpenChanged;
            GameService.GameIntegration.Gw2Instance.IsInGameChanged += OnIsInGameChanged;
        }
        public override IView GetSettingsView()
        {
            return new CustomSettingsView(new CustomSettingsModel(SettingsManager.ModuleSettings));
        }

        protected override async Task LoadAsync()
        {

        }

        private void OnAutoClickSoundVolumeSettingChanged(object o, ValueChangedEventArgs<float> e)
        {
            _autoClickController.SoundVolume = e.NewValue / 1000;
        }

        private void OnDodgeJumpKeyActivated(object o, EventArgs e)
        {
            if (DateTime.UtcNow < _nextDodgeJump) return;

            if (DodgeKeyBindingSetting.Value.PrimaryKey == DodgeJumpKeyBindingSetting.Value.PrimaryKey 
                && DodgeKeyBindingSetting.Value.ModifierKeys == DodgeJumpKeyBindingSetting.Value.ModifierKeys)
            {
                ScreenNotification.ShowNotification("Endless Loop Error. Dodge-Jump key cannot be the same as Dodge.", ScreenNotification.NotificationType.Error);
                return;
            }

            if (JumpKeyBindingSetting.Value.PrimaryKey == DodgeJumpKeyBindingSetting.Value.PrimaryKey
                && JumpKeyBindingSetting.Value.ModifierKeys == DodgeJumpKeyBindingSetting.Value.ModifierKeys)
            {
                ScreenNotification.ShowNotification("Endless Loop Error. Dodge-Jump key cannot be the same as Jump.", ScreenNotification.NotificationType.Error);
                return;
            }
            _nextDodgeJump = DateTime.UtcNow.AddMilliseconds(80);
            
            if (DodgeKeyBindingSetting.Value.PrimaryKey != Keys.None)
                Blish_HUD.Controls.Intern.Keyboard.Stroke((VirtualKeyShort)DodgeKeyBindingSetting.Value.PrimaryKey);
            if (JumpKeyBindingSetting.Value.PrimaryKey != Keys.None)
                Blish_HUD.Controls.Intern.Keyboard.Stroke((VirtualKeyShort)JumpKeyBindingSetting.Value.PrimaryKey);
        }

        private void OnCurrentMountChanged(object o, ValueEventArgs<MountType> e) => ToggleDodgeJump(e.Value == MountType.None);
        private void OnIsInGameChanged(object o, ValueEventArgs<bool> e) => ToggleDodgeJump(e.Value);
        private void OnIsTextInputFocusChanged(object o, ValueEventArgs<bool> e) => ToggleDodgeJump(!e.Value);
        private void OnIsMapOpenChanged(object o, ValueEventArgs<bool> e) => ToggleDodgeJump(!e.Value);
        private void ToggleDodgeJump(bool enabled)
        {
            DodgeJumpKeyBindingSetting.Value.BlockSequenceFromGw2 = enabled;
            if (enabled)
                DodgeJumpKeyBindingSetting.Value.Activated += OnDodgeJumpKeyActivated;
            else
                DodgeJumpKeyBindingSetting.Value.Activated -= OnDodgeJumpKeyActivated;
        }

        protected override void OnModuleLoaded(EventArgs e)
        {
            // Base handler must be called
            base.OnModuleLoaded(e);
        }

        protected override void Update(GameTime gameTime)
        {
            _autoClickController?.Update();
        }

        /// <inheritdoc />
        protected override void Unload()
        {
            AutoClickToggleKeySetting.Value.Enabled = false;
            AutoClickHoldKeySetting.Value.Enabled = false;
            GameService.Gw2Mumble.PlayerCharacter.CurrentMountChanged -= OnCurrentMountChanged;
            GameService.GameIntegration.Gw2Instance.IsInGameChanged -= OnIsInGameChanged;
            GameService.Gw2Mumble.UI.IsTextInputFocusedChanged -= OnIsTextInputFocusChanged;
            GameService.Gw2Mumble.UI.IsMapOpenChanged -= OnIsMapOpenChanged;
            DodgeJumpKeyBindingSetting.Value.Enabled = false;
            DodgeJumpKeyBindingSetting.Value.BlockSequenceFromGw2 = false;
            _autoClickController?.Dispose();
            DodgeJumpKeyBindingSetting.Value.Activated -= OnDodgeJumpKeyActivated;
            AutoClickSoundVolume.SettingChanged -= OnAutoClickSoundVolumeSettingChanged;
            _moduleIcon?.Dispose();
            // All static members must be manually unset
            ModuleInstance = null;
        }
    }
}
