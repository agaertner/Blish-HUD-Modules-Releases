using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nekres.Chat_Shorts.Services;
using Nekres.Chat_Shorts.UI.Models;
using Nekres.Chat_Shorts.UI.Views;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace Nekres.Chat_Shorts
{
    [Export(typeof(Module))]
    public class ChatShorts : Module
    {

        internal static readonly Logger Logger = Logger.GetLogger<ChatShorts>();

        internal static ChatShorts Instance;

        #region Service Managers
        internal SettingsManager SettingsManager => this.ModuleParameters.SettingsManager;
        internal ContentsManager ContentsManager => this.ModuleParameters.ContentsManager;
        internal DirectoriesManager DirectoriesManager => this.ModuleParameters.DirectoriesManager;
        internal Gw2ApiManager Gw2ApiManager => this.ModuleParameters.Gw2ApiManager;
        #endregion

        internal DataService DataService { get; set; }
        internal ChatService ChatService { get; set; }

        private StandardWindow _moduleWindow;
        private CornerIcon _cornerIcon;

        // Textures
        private Texture2D _cornerTexture;
        private Texture2D _backgroundTexture;

        [ImportingConstructor]
        public ChatShorts([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters)
        {
            Instance = this;
        }

        protected override void DefineSettings(SettingCollection settings)
        {

        }

        protected override void Initialize()
        {
            DataService = new DataService(this.DirectoriesManager.GetFullDirectoryPath("chat_shorts"));
            _cornerTexture = ContentsManager.GetTexture("corner_icon.png");
            _backgroundTexture = ContentsManager.GetTexture("background.png");
            ChatService = new ChatService(this.DataService);
        }

        protected override async Task LoadAsync()
        {
        }

        protected override void OnModuleLoaded(EventArgs e)
        {
            var windowRegion = new Rectangle(40, 26, 423, 780 - 56);
            var contentRegion = new Rectangle(70, 41, 380, 780 - 42);
            _moduleWindow = new StandardWindow(_backgroundTexture, windowRegion, contentRegion)
            {
                Parent = GameService.Graphics.SpriteScreen,
                Emblem = _cornerTexture,
                Location = new Point((GameService.Graphics.SpriteScreen.Width - windowRegion.Width) / 2, (GameService.Graphics.SpriteScreen.Height) / 2),
                SavesPosition = true,
                Title = this.Name,
                Id = $"ChatShorts_{nameof(LibraryView)}_42d3a11e-ffa7-4c82-8fd9-ee9d9a118914"
            };
            _cornerIcon = new CornerIcon
            {
                Icon = ContentsManager.GetTexture("corner_icon.png")
            };
            _cornerIcon.Click += OnModuleIconClick;
            // Base handler must be called
            base.OnModuleLoaded(e);
        }

        public void OnModuleIconClick(object o, MouseEventArgs e)
        {
            _moduleWindow.ToggleWindow(new LibraryView(new LibraryModel()));
        }

        protected override void Update(GameTime gameTime)
        {

        }

        public override IView GetSettingsView()
        {
            return new CustomSettingsView(new CustomSettingsModel(this.SettingsManager.ModuleSettings, this.ContentsManager));
        }

        /// <inheritdoc />
        protected override void Unload()
        {
            // Unload here
            this.ChatService.Dispose();
            _cornerIcon.Click -= OnModuleIconClick;
            _cornerIcon?.Dispose();
            _moduleWindow?.Dispose();
            _backgroundTexture?.Dispose();
            _cornerTexture?.Dispose();
            // All static members must be manually unset
        }

    }

}
