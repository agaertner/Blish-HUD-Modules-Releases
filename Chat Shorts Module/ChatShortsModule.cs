using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nekres.Chat_Shorts.Services;
using Nekres.Chat_Shorts.UI.Controls;
using Nekres.Chat_Shorts.UI.Models;
using Nekres.Chat_Shorts.UI.Views;
using System;
using System.ComponentModel.Composition;
using System.Linq;
using Blish_HUD.Extended.Core.Views;
using Blish_HUD.Graphics.UI;

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
        private ContextMenuStrip _moduleContextMenu;

        // Textures
        internal Texture2D EditTexture;
        private Texture2D _cornerTexture;
        private Texture2D _backgroundTexture;

        internal SettingEntry<KeyBinding> SquadBroadcast;
        internal SettingEntry<KeyBinding> ChatMessage;

        [ImportingConstructor]
        public ChatShorts([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters)
        {
            Instance = this;
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            var controlSettings = settings.AddSubCollection("Control Options (User Interface)", true, false);
            ChatMessage = controlSettings.DefineSetting("chatMessageKeyBinding", new KeyBinding(Keys.Enter),
                () => "Chat Message",
                () => "Give focus to the chat edit box.");
            SquadBroadcast = controlSettings.DefineSetting("squadBroadcastKeyBinding", new KeyBinding(ModifierKeys.Shift, Keys.Enter),
                () => "Squad Broadcast Message", 
                () => "Give focus to the chat edit box.");
        }

        protected override void Initialize()
        {
            DataService = new DataService(this.DirectoriesManager.GetFullDirectoryPath("chat_shorts"));
            _cornerTexture = ContentsManager.GetTexture("corner_icon.png");
            _backgroundTexture = ContentsManager.GetTexture("background.png");
            EditTexture = ContentsManager.GetTexture("edit_icon.png");
            ChatService = new ChatService(this.DataService);
            SquadBroadcast.Value.Enabled = false;
            ChatMessage.Value.Enabled = false;
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

            GameService.Gw2Mumble.CurrentMap.MapChanged += OnMapChanged;
            GameService.Gw2Mumble.PlayerCharacter.IsCommanderChanged += OnIsCommanderChanged;

            ChatService.LoadMacros();

            BuildContextMenu();

            // Base handler must be called
            base.OnModuleLoaded(e);
        }

        private void OnMapChanged(object o, ValueEventArgs<int> e)
        {
            if (!this.Loaded) return;
            BuildContextMenu();
        }

        public override IView GetSettingsView()
        {
            return new SocialsSettingsView(new SocialsSettingsModel(this.SettingsManager.ModuleSettings, "https://pastebin.com/raw/Kk9DgVmL"));
        }

        private void OnIsCommanderChanged(object o, ValueEventArgs<bool> e)
        {
            if (!this.Loaded) return;
            BuildContextMenu();
        }

        public void OnModuleIconClick(object o, MouseEventArgs e)
        {
            _moduleContextMenu?.Show(_cornerIcon);
        }

        internal void BuildContextMenu()
        {
            var prevVisible = _moduleContextMenu?.Visible;
            var prevLocation = _moduleContextMenu?.Location;
            _moduleContextMenu?.Dispose();
            _moduleContextMenu = new ContextMenuStrip();

            var openLibrary = new ContextMenuStripItem("Open Library")
            {
                Parent = _moduleContextMenu
            };
            openLibrary.Click += (_, _) => _moduleWindow.ToggleWindow(new LibraryView(new LibraryModel()));

            var separatorItem = new ContextMenuStripItemSeparator
            {
                Parent = _moduleContextMenu,
                CanCheck = false,
                Enabled = false
            };

            var macros = DataService.GetAllActives().Select(MacroModel.FromEntity);

            foreach (var model in macros)
            {
                var entry = new ContextMenuStripItemWithModel<MacroModel>(model)
                {
                    Text = model.Title,
                    Parent = _moduleContextMenu,
                    BasicTooltipText = model.Text
                };
                entry.Click += async (_, _) => await ChatService.Send(model.Text, model.SquadBroadcast);
            }

            if (!prevVisible.GetValueOrDefault()) return;
            _moduleContextMenu.Location = prevLocation.GetValueOrDefault();
            _moduleContextMenu.Show();
        }

        /// <inheritdoc />
        protected override void Unload()
        {
            // Unload here
            this.ChatService?.Dispose();
            this.DataService?.Dispose();
            GameService.Gw2Mumble.CurrentMap.MapChanged -= OnMapChanged;
            GameService.Gw2Mumble.PlayerCharacter.IsCommanderChanged -= OnIsCommanderChanged;
            _moduleContextMenu?.Dispose();
            if (_cornerIcon != null)
            {
                _cornerIcon.Click -= OnModuleIconClick;
                _cornerIcon.Dispose();
            }
            _moduleWindow?.Dispose();
            _backgroundTexture?.Dispose();
            _cornerTexture?.Dispose();
            EditTexture?.Dispose();
            // All static members must be manually unset
        }
    }
}
