using System;
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

        internal static readonly Logger Logger = Logger.GetLogger(typeof(NotesModule));

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

        private const int AutoSaveInternalSeconds = 30;

        private Texture2D _icon64;
        //private Texture2D _icon128;

        private CornerIcon moduleCornerIcon;
        private ContextMenuStrip moduleContextMenu;

        private List<Book> _activeBooks;
        protected override void Initialize()
        {
            _activeBooks = new List<Book>();
            LoadTextures();
            moduleCornerIcon = new CornerIcon
            {
                IconName = Name,
                Icon = _icon64,
                Priority = Name.GetHashCode()
            };
            moduleCornerIcon.Click += ModuleCornerIconClicked;
            BuildContextMenu();
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
            newItem.Click += (o, e) => CreateNewBook();

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
                noteEntry.Click += LoadNote;
            }

            if (!prevVisible.GetValueOrDefault()) return;
            moduleContextMenu.Location = prevLocation.GetValueOrDefault();
            moduleContextMenu.Show();
        }

        private async void LoadNote(object o, MouseEventArgs e)
        {
            var path = ((Control)o).BasicTooltipText;

            BookModel bookEntity;
            try
            {
                using var str = new StreamReader(path);
                var content = await str.ReadToEndAsync();
                bookEntity = JsonConvert.DeserializeObject<BookModel>(content);
                if (bookEntity == null)
                    throw new JsonException("No data after deserialization. Possibly corrupted Json.");
            }
            catch (Exception ex) when (ex is IOException or InvalidOperationException or JsonException)
            {
                ScreenNotification.ShowNotification("There was an error loading your book.", ScreenNotification.NotificationType.Error);
                Logger.Error(ex, ex.Message);
                return;
            }

            Book prevBook;
            if ((prevBook = _activeBooks.FirstOrDefault(b => b.Guid.Equals(bookEntity.Guid))) != null)
            {
                prevBook.Location = new Point((GameService.Graphics.SpriteScreen.Width - prevBook.Width) / 2, (GameService.Graphics.SpriteScreen.Height - prevBook.Height) / 2);
                return;
            }

            var book = new Book(bookEntity.Title, bookEntity.Pages.Select(p => (p.Title, p.Content)).ToArray(), bookEntity.UseChapters, bookEntity.AllowEdit, bookEntity.Guid.ToString())
            {
                Parent = GameService.Graphics.SpriteScreen,
                Location = new Point(GameService.Graphics.SpriteScreen.Width / 2, GameService.Graphics.SpriteScreen.Height / 2),
            };
            book.Disposed += OnBookDisposed;
            book.Location = new Point((GameService.Graphics.SpriteScreen.Width - book.Width) / 2, (GameService.Graphics.SpriteScreen.Height - book.Height) / 2);
            book.Show();

            _activeBooks.Add(book);
        }

        private void CreateNewBook()
        {
            var book = new Book
            {
                Parent = GameService.Graphics.SpriteScreen,
                Location = new Point(GameService.Graphics.SpriteScreen.Width / 2, GameService.Graphics.SpriteScreen.Height / 2),
            };
            book.Disposed += OnBookDisposed;
            book.Location = new Point((GameService.Graphics.SpriteScreen.Width - book.Width) / 2, (GameService.Graphics.SpriteScreen.Height - book.Height) / 2);
            book.Show();
            _activeBooks.Add(book);
        }

        private void OnBookDisposed(object o, EventArgs e)
        {
            _activeBooks.Remove((Book)o);
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

        private DateTime _prevAutoSaveTime = DateTime.UtcNow;
        protected override void Update(GameTime gameTime) {
            if (DateTime.UtcNow.Subtract(_prevAutoSaveTime).TotalSeconds > AutoSaveInternalSeconds)
            {
                _prevAutoSaveTime = DateTime.UtcNow;
                foreach (var activeBook in _activeBooks)
                {
                    activeBook.Save();
                }
                BuildContextMenu();
            }
        }

        /// <inheritdoc />
        protected override void Unload() {
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
