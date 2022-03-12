﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using Newtonsoft.Json;
using Book = Nekres.Notes.UI.Controls.Book;
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
            BuildContextMenu();

            _bookFactory = new BookFactory();
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
            var moduleDirectory = DirectoriesManager.GetFullDirectoryPath("notes");
            var allNotes = Directory.GetFiles(moduleDirectory, "*.json");

            foreach (var filePath in allNotes)
            {
                var noteEntry = new ContextMenuStripItem
                {
                    Text = Path.GetFileNameWithoutExtension(filePath),
                    BasicTooltipText = filePath,
                    Parent = moduleContextMenu
                };
                noteEntry.Click += (o, _) => _bookFactory.LoadNote(((Control)o).BasicTooltipText);
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
