using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nekres.Notes.UI.Controls;
using Nekres.Notes.UI.Models;
using Nekres.Notes.UI.Views;
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
namespace Nekres.Notes
{

    [Export(typeof(Module))]
    public class NotesModule : Module
    {

        internal static Logger Logger = Logger.GetLogger(typeof(NotesModule));

        internal static NotesModule ModuleInstance;

        #region Service Managers
        internal SettingsManager SettingsManager => this.ModuleParameters.SettingsManager;
        internal ContentsManager ContentsManager => this.ModuleParameters.ContentsManager;
        internal DirectoriesManager DirectoriesManager => this.ModuleParameters.DirectoriesManager;
        internal Gw2ApiManager Gw2ApiManager => this.ModuleParameters.Gw2ApiManager;
        #endregion

        [ImportingConstructor]
        public NotesModule([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters) { ModuleInstance = this; }

        protected override void DefineSettings(SettingCollection settings)
        {
            
        }

        private Texture2D _icon64;
        //private Texture2D _icon128;

        private CornerIcon moduleCornerIcon;
        private ContextMenuStrip moduleContextMenu;

        private BookFactory _bookFactory;
        protected override void Initialize()
        {
            LoadTextures();
            moduleCornerIcon = new CornerIcon
            {
                IconName = Name,
                Icon = _icon64,
                Priority = Name.GetHashCode()
            };
            moduleCornerIcon.Click += ModuleCornerIconClicked;

            _bookFactory = new BookFactory(DirectoriesManager.GetFullDirectoryPath("notes"));
            _bookFactory.OnIndexChanged += OnBookIndexChanged;

            BuildContextMenu();
        }

        private void OnBookIndexChanged(object o, ValueEventArgs<Guid> e)
        {
            this.BuildContextMenu();
        }

        private void ModuleCornerIconClicked(object o, MouseEventArgs e)
        {
            moduleContextMenu?.Show(moduleCornerIcon);
        }

        internal void BuildContextMenu()
        {
            var prevVisible = moduleContextMenu?.Visible;
            var prevLocation = moduleContextMenu?.Location;
            moduleContextMenu?.Dispose();
            moduleContextMenu = new ContextMenuStrip();

            var newItem = new ContextMenuStripItem
            {
                Text = "New",
                Parent = moduleContextMenu,
            };
            newItem.Click += (o, e) => _bookFactory.Create("Empty Book");

            var separatorItem = new ContextMenuStripItemSeparator
            {
                CanCheck = false,
                Enabled = false,
                Parent = moduleContextMenu,
            };

            foreach (var fileRef in _bookFactory.Index)
            {
                var noteEntry = new ContextMenuStripItemWithId(fileRef.Key)
                {
                    Text = fileRef.Value,
                    Parent = moduleContextMenu
                };
                noteEntry.Click += (o, _) => _bookFactory.FromCache(((ContextMenuStripItemWithId)o).Id);
            }

            if (!prevVisible.GetValueOrDefault()) return;
            moduleContextMenu.Location = prevLocation.GetValueOrDefault();
            moduleContextMenu.Show();
        }

        private void LoadTextures()
        {
            _icon64 = ContentsManager.GetTexture("notes_icon_64x64.png");
            //_icon128 = ContentsManager.GetTexture("notes_icon_128x128.png");
        }

        protected override async Task LoadAsync() {
        }

        protected override void OnModuleLoaded(EventArgs e) {

            // Base handler must be called
            base.OnModuleLoaded(e);
        }

        protected override void Update(GameTime gameTime) {
            _bookFactory.Update(BuildContextMenu);
        }

        /// <inheritdoc />
        protected override void Unload() {
            _bookFactory.OnIndexChanged -= OnBookIndexChanged;
            _bookFactory.Dispose();
            moduleCornerIcon.Click -= ModuleCornerIconClicked;
            moduleCornerIcon.Dispose();
            // All static members must be manually unset
            ModuleInstance = null;
        }

        public override IView GetSettingsView()
        {
            return new CustomSettingsView(new CustomSettingsModel(SettingsManager.ModuleSettings));
        }
    }

}
