using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nekres.Screenshot_Manager.Properties;
using Nekres.Screenshot_Manager.UI.Models;
using Nekres.Screenshot_Manager.UI.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Nekres.Screenshot_Manager.Core;
using Nekres.Screenshot_Manager_Module.Controls;

namespace Nekres.Screenshot_Manager
{
    [Export(typeof(Module))]
    public class ScreenshotManagerModule : Module
    {
        internal static readonly Logger Logger = Logger.GetLogger(typeof(ScreenshotManagerModule));

        internal static ScreenshotManagerModule ModuleInstance;

        #region Service Managers

        internal SettingsManager SettingsManager => ModuleParameters.SettingsManager;
        internal ContentsManager ContentsManager => ModuleParameters.ContentsManager;
        internal DirectoriesManager DirectoriesManager => ModuleParameters.DirectoriesManager;
        internal Gw2ApiManager Gw2ApiManager => ModuleParameters.Gw2ApiManager;

        #endregion

        [ImportingConstructor]
        public ScreenshotManagerModule([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(
            moduleParameters)
        {
            ModuleInstance = this;
        }

        #region Settings

        private SettingEntry<KeyBinding> ScreenshotNormalBinding;
        private SettingEntry<KeyBinding> ScreenshotStereoscopicBinding;
        private SettingEntry<List<string>> _favorites;

        #endregion

        private Texture2D _icon64;
        //private Texture2D _icon128;

        private CornerIcon moduleCornerIcon;
        private WindowTab moduleTab;
        private FileWatcherFactory _fileWatcherFactory;

        public const int FileTimeOutMilliseconds = 10000;
        protected override void DefineSettings(SettingCollection settings)
        {
            var keyBindingCol = settings.AddSubCollection("Screenshot", true, false);
            ScreenshotNormalBinding = keyBindingCol.DefineSetting("NormalKey", new KeyBinding(Keys.PrintScreen),
                () => Resources.Normal, () => Resources.Take_a_normal_screenshot_);
            ScreenshotStereoscopicBinding = keyBindingCol.DefineSetting("StereoscopicKey", new KeyBinding(Keys.None),
                () => Resources.Stereoscopic, () => Resources.Take_a_stereoscopic_screenshot_);

            var selfManagedSettings = settings.AddSubCollection("ManagedSettings", false, false);
            _favorites = selfManagedSettings.DefineSetting("favorites", new List<string>());
        }
        protected override void Initialize()
        {
            _fileWatcherFactory = new FileWatcherFactory();
            LoadTextures();

            moduleTab = GameService.Overlay.BlishHudWindow.AddTab(Name, _icon64, () => new ScreenshotManagerView(new ScreenshotManagerModel()));
            moduleCornerIcon = new CornerIcon
            {
                IconName = Name,
                Icon = _icon64,
                Priority = Name.GetHashCode()
            };
            moduleCornerIcon.Click += ModuleCornerIconClicked;
        }

        public override IView GetSettingsView()
        {
            return new CustomSettingsView(new CustomSettingsModel(SettingsManager.ModuleSettings));
        }

        private void LoadTextures()
        {
            _icon64 = ContentsManager.GetTexture("screenshots_icon_64x64.png");
            //_icon128 = ContentsManager.GetTexture("screenshots_icon_128x128.png");
        }

        protected override void OnModuleLoaded(EventArgs e)
        {
            // Base handler must be called
            base.OnModuleLoaded(e);
        }
        protected override void Update(GameTime gameTime)
        {
        }
        /// <inheritdoc />
        protected override void Unload()
        {
            _fileWatcherFactory.Dispose();
            moduleCornerIcon.Click -= ModuleCornerIconClicked;
            moduleCornerIcon.Dispose();
            GameService.Overlay.BlishHudWindow.RemoveTab(moduleTab);
            // All static members must be manually unset
            ModuleInstance = null;
        }

        private void ModuleCornerIconClicked(object o, MouseEventArgs e)
        {
            GameService.Overlay.BlishHudWindow.Show();
            GameService.Overlay.BlishHudWindow.Navigate(new ScreenshotManagerView(new ScreenshotManagerModel()));
        }
    }
}