using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Controls.Extern;
using Blish_HUD.Input;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nekres.Inquest_Module.Core.Controllers;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

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
        internal SettingEntry<KeyBinding> AutoClickToggleKeySetting;
        internal SettingEntry<bool> AutoClickSoundDisabledSetting;
        internal SettingEntry<KeyBinding> JumpKeyBindingSetting;
        internal SettingEntry<KeyBinding> DodgeKeyBindingSetting;
        internal SettingEntry<KeyBinding> DodgeJumpKeyBindingSetting;

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
            AutoClickHoldKeySetting = settings.DefineSetting("autoClickHoldKeyBinding", new KeyBinding(Keys.OemComma), 
                () => "Hold Double Clicking", 
                () => "Perform Double Clicks at the current cursor position while the key is being held down.");

            AutoClickToggleKeySetting = settings.DefineSetting("autoClickToggleKeyBinding", new KeyBinding(Keys.OemOpenBrackets), 
                () => "Toggle Double Clicking", 
                () => "Perform Double Clicks in an interval at the position of the cursor at the time of pressing the key.");

            AutoClickSoundDisabledSetting = settings.DefineSetting("autoClickSoundsDisabled", false, 
                () => "Disable Clicking Sounds", 
                () => "Disables the sound alert when an auto click is performed.");

            DodgeJumpKeyBindingSetting = settings.DefineSetting("dodgeJumpKeyBinding", new KeyBinding(ModifierKeys.Ctrl, Keys.Space), 
                () => "Dodge-Jump", 
                () => "Perform a dodge roll and a jump simultaneously.");

            var controlOptions = settings.AddSubCollection("Movement Keys to Trigger on Dodge-Jump", true, false);
            DodgeKeyBindingSetting = controlOptions.DefineSetting("dodgeKeyBinding", new KeyBinding(Keys.V), () => "Dodge", () => "Do an evasive dodge roll, negating damage, in the direction your character is moving (backward if stationary).");
            JumpKeyBindingSetting = controlOptions.DefineSetting("jumpKeyBinding", new KeyBinding(Keys.Space), () => "Jump", () => "Press to jump over obstacles.");
        }

        protected override void Initialize()
        {
            _autoClickController = new AutoClickController();
            /*_moduleIcon = new CornerIcon
            {
                IconName = "Inquest Chipset",
                Icon = ContentsManager.GetTexture("assault_cube_icon.png"),
                Priority = "Inquest Chipset".GetHashCode()
            };*/
            JumpKeyBindingSetting.Value.Enabled = false;
            DodgeKeyBindingSetting.Value.Enabled = false;
            DodgeJumpKeyBindingSetting.Value.Enabled = true;
            DodgeJumpKeyBindingSetting.Value.Activated += OnDodgeJumpKeyActivated;
        }

        protected override async Task LoadAsync()
        {

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
                Blish_HUD.Controls.Intern.Keyboard.Stroke((VirtualKeyShort)DodgeKeyBindingSetting.Value.PrimaryKey, true);
            if (JumpKeyBindingSetting.Value.PrimaryKey != Keys.None)
                Blish_HUD.Controls.Intern.Keyboard.Stroke((VirtualKeyShort)JumpKeyBindingSetting.Value.PrimaryKey, true);
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
            _autoClickController?.Dispose();
            DodgeJumpKeyBindingSetting.Value.Activated -= OnDodgeJumpKeyActivated;
            _moduleIcon?.Dispose();
            // All static members must be manually unset
            ModuleInstance = null;
        }
    }
}