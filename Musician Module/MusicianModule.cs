using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nekres.Musician.Core.Player;
using Nekres.Musician.UI;
using Nekres.Musician.UI.Models;
using Nekres.Musician.UI.Views;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using static Blish_HUD.GameService;

namespace Nekres.Musician
{

    [Export(typeof(Module))]
    public class MusicianModule : Module
    {
        internal static readonly Logger Logger = Logger.GetLogger(typeof(MusicianModule));

        internal static MusicianModule ModuleInstance;

        #region Service Managers
        internal SettingsManager SettingsManager => this.ModuleParameters.SettingsManager;
        internal ContentsManager ContentsManager => this.ModuleParameters.ContentsManager;
        internal DirectoriesManager DirectoriesManager => this.ModuleParameters.DirectoriesManager;
        internal Gw2ApiManager Gw2ApiManager => this.ModuleParameters.Gw2ApiManager;
        #endregion


        #region Settings

        internal SettingEntry<float> audioVolume;
        internal SettingEntry<bool> stopWhenMoving;
        internal SettingEntry<KeyBinding> keySwapWeapons;
        internal SettingEntry<KeyBinding> keyWeaponSkill1;
        internal SettingEntry<KeyBinding> keyWeaponSkill2;
        internal SettingEntry<KeyBinding> keyWeaponSkill3;
        internal SettingEntry<KeyBinding> keyWeaponSkill4;
        internal SettingEntry<KeyBinding> keyWeaponSkill5;
        internal SettingEntry<KeyBinding> keyHealingSkill;
        internal SettingEntry<KeyBinding> keyUtilitySkill1;
        internal SettingEntry<KeyBinding> keyUtilitySkill2;
        internal SettingEntry<KeyBinding> keyUtilitySkill3;
        internal SettingEntry<KeyBinding> keyEliteSkill;

        internal SettingEntry<string> SheetFilter;
        #endregion

        private CornerIcon _moduleIcon;

        private StandardWindow _moduleWindow;

        internal MusicPlayer MusicPlayer { get; private set; }

        internal MusicSheetService MusicSheetService { get; private set; }

        internal MusicSheetImporter MusicSheetImporter { get; private set; }

        [ImportingConstructor]
        public MusicianModule([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters) { ModuleInstance = this; }

        protected override void DefineSettings(SettingCollection settingsManager)
        {
            audioVolume = settingsManager.DefineSetting("audioVolume", 80f, () => "Audio Volume");
            stopWhenMoving = settingsManager.DefineSetting("stopWhenMoving", true, () => "Stop When Moving", () => "Stops any playback when you start moving.");
            var skillKeyBindingsCollection = settingsManager.AddSubCollection("Skills", true, false);
            keySwapWeapons = skillKeyBindingsCollection.DefineSetting("keySwapWeapons", new KeyBinding(Keys.OemPipe), () => "Swap Weapons");
            keyWeaponSkill1 = skillKeyBindingsCollection.DefineSetting("keyWeaponSkill1", new KeyBinding(Keys.D1), () => "Weapon Skill 1");
            keyWeaponSkill2 = skillKeyBindingsCollection.DefineSetting("keyWeaponSkill2", new KeyBinding(Keys.D2), () => "Weapon Skill 2");
            keyWeaponSkill3 = skillKeyBindingsCollection.DefineSetting("keyWeaponSkill3", new KeyBinding(Keys.D3), () => "Weapon Skill 3");
            keyWeaponSkill4 = skillKeyBindingsCollection.DefineSetting("keyWeaponSkill4", new KeyBinding(Keys.D4), () => "Weapon Skill 4");
            keyWeaponSkill5 = skillKeyBindingsCollection.DefineSetting("keyWeaponSkill5", new KeyBinding(Keys.D5), () => "Weapon Skill 5");
            keyHealingSkill = skillKeyBindingsCollection.DefineSetting("keyHealingSkill", new KeyBinding(Keys.D6), () => "Healing Skill");
            keyUtilitySkill1 = skillKeyBindingsCollection.DefineSetting("keyUtilitySkill1", new KeyBinding(Keys.D7), () => "Utility Skill 1");
            keyUtilitySkill2 = skillKeyBindingsCollection.DefineSetting("keyUtilitySkill2", new KeyBinding(Keys.D8), () => "Utility Skill 2");
            keyUtilitySkill3 = skillKeyBindingsCollection.DefineSetting("keyUtilitySkill3", new KeyBinding(Keys.D9), () => "Utility Skill 3");
            keyEliteSkill = skillKeyBindingsCollection.DefineSetting("keyEliteSkill", new KeyBinding(Keys.D0), () => "Elite Skill");

            var selfManagedSettings = settingsManager.AddSubCollection("selfManaged", false, false);
            SheetFilter = selfManagedSettings.DefineSetting("sheetFilter", "Title");
            GameIntegration.Gw2Instance.IsInGameChanged += OnIsInGameChanged;
        }

        private void OnIsInGameChanged(object o, ValueEventArgs<bool> e)
        {
            _moduleIcon.Visible = e.Value;
            if (!e.Value) _moduleWindow.Hide();
        }

        protected override void Initialize()
        {
            _moduleIcon = new CornerIcon(ContentsManager.GetTexture("corner_icon.png"), this.Name);
            MusicSheetService = new MusicSheetService(DirectoriesManager.GetFullDirectoryPath("musician"));
            MusicPlayer = new MusicPlayer();
        }

        public override IView GetSettingsView() => new CustomSettingsView(new CustomSettingsModel(this.SettingsManager.ModuleSettings));

        protected override async Task LoadAsync()
        {
            await MusicSheetService.LoadAsync();
            this.MusicSheetImporter = new MusicSheetImporter(this.MusicSheetService, GetModuleProgressHandler());
        }

        private void UpdateModuleLoading(string loadingMessage)
        {
            _moduleIcon.LoadingMessage = loadingMessage;
        }

        public IProgress<string> GetModuleProgressHandler()
        {
            return new Progress<string>(UpdateModuleLoading);
        }

        protected override void OnModuleLoaded(EventArgs e)
        {
            var windowRegion = new Rectangle(40, 26, 423, 780 - 56);
            var contentRegion = new Rectangle(70, 41, 380, 780 - 42);
            _moduleWindow = new StandardWindow(ContentsManager.GetTexture("background.png"), windowRegion, contentRegion)
            {
                Parent = Graphics.SpriteScreen,
                Emblem = ContentsManager.GetTexture("musician_icon.png"),
                Location = new Point((Graphics.SpriteScreen.Width - windowRegion.Width) / 2, (Graphics.SpriteScreen.Height - windowRegion.Height) / 2),
                SavesPosition = true,
                Id = Guid.NewGuid().ToString(),
                Title = this.Name
            };

            _moduleIcon.Click += OnModuleIconClick;

            MusicSheetImporter.Init();
            base.OnModuleLoaded(e);
        }

        private void OnModuleIconClick(object o, MouseEventArgs e)
        {
            _moduleWindow.ToggleWindow(new LibraryView(new LibraryModel(MusicSheetService)));
        }

        protected override void Unload()
        {
            GameIntegration.Gw2Instance.IsInGameChanged -= OnIsInGameChanged;
            _moduleIcon.Click -= OnModuleIconClick;
            _moduleIcon?.Dispose();
            _moduleWindow?.Dispose();
            MusicPlayer?.Dispose();
            MusicSheetService?.Dispose();
            MusicSheetImporter?.Dispose();
            ModuleInstance = null;
        }
    }
}
