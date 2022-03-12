using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Nekres.Notes.UI.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nekres.Notes.UI.Controls
{
    internal class BookFactory : IDisposable
    {
        public event EventHandler<ValueEventArgs<Guid>> OnIndexChanged;

        private IList<BookBase> _displayedBooks;

        private IList<BookModel> _fetchedBooks;

        private readonly int AutoSaveInternalSeconds = 30;
        private DateTime _prevAutoSaveTime = DateTime.UtcNow;

        private Dictionary<Guid, string> _index;

        public Dictionary<Guid, string> Index => new (_index);

        public string CacheDir { get; private set; }

        private readonly string _indexFileName;
        public BookFactory(string cacheDir)
        {
            _indexFileName = "index.json";
            _displayedBooks = new List<BookBase>();
            _fetchedBooks = new List<BookModel>();
            _index = new Dictionary<Guid, string>();
            this.CacheDir = cacheDir;
            this.LoadIndex();
        }

        private async void LoadIndex()
        {
            var filePath = Path.Combine(this.CacheDir, _indexFileName);
            if (!File.Exists(filePath))
            {
                this.SaveIndex();
                return;
            }

            try
            {
                using var str = new StreamReader(Path.Combine(this.CacheDir, _indexFileName));
                var content = await str.ReadToEndAsync();
                _index = JsonConvert.DeserializeObject<Dictionary<Guid, string>>(content);

                if (_index == null)
                    throw new JsonException("No data after deserialization. Possibly corrupted Json.");

                OnIndexChanged?.Invoke(this, new ValueEventArgs<Guid>(Guid.Empty));
            }
            catch (Exception ex) when (ex is IOException or InvalidOperationException or JsonException)
            {
                ScreenNotification.ShowNotification("There was an error loading your library.", ScreenNotification.NotificationType.Error);
                NotesModule.Logger.Error(ex, ex.Message);
            }
        }

        private void AddOrUpdateIndex(Guid key, string newTitle)
        {
            if (_index.ContainsKey(key)) 
                _index[key] = newTitle;
            else
                _index.Add(key, newTitle);

            OnIndexChanged?.Invoke(this, new ValueEventArgs<Guid>(key));
            this.SaveIndex();
        }

        private void RemoveFromIndex(Guid key)
        {
            if (_index.ContainsKey(key)) 
                _index.Remove(key);

            OnIndexChanged?.Invoke(this, new ValueEventArgs<Guid>(key));
            this.SaveIndex();
        }

        public void Dispose()
        {
            foreach (var displayedBook in _displayedBooks) 
                displayedBook?.Dispose();
        }

        public void Create(string title)
        {
            var book = new EditableBook(Guid.NewGuid(), title)
            {
                Parent = GameService.Graphics.SpriteScreen,
                Location = Point.Zero
            };
            book.Disposed += OnBookDisposed;
            book.OnDelete += OnBookDelete;
            book.OnChanged += OnBookChanged;
            book.Location = new Point((GameService.Graphics.SpriteScreen.Width - book.Width) / 2, (GameService.Graphics.SpriteScreen.Height - book.Height) / 2);
            book.Show();

            _displayedBooks.Add(book);
        }

        private void Create(Guid id, string title, IList<(string, string)> pages)
        {
            var book = new EditableBook(id, title, pages)
            {
                Parent = GameService.Graphics.SpriteScreen,
                Location = Point.Zero
            };
            book.Disposed += OnBookDisposed;
            book.OnDelete += OnBookDelete;
            book.OnChanged += OnBookChanged;
            book.Location = new Point((GameService.Graphics.SpriteScreen.Width - book.Width) / 2, (GameService.Graphics.SpriteScreen.Height - book.Height) / 2);
            book.Show();

            _displayedBooks.Add(book);
        }

        public void FromStream(Stream stream)
        {
            var book = new OnlineBookModel();
            throw new NotImplementedException();
        }

        public async void FromCache(Guid id)
        {
            var filePath = Path.Combine(this.CacheDir, $"{id}.json");
            BookModel bookModel;
            try
            {
                using var str = new StreamReader(filePath);
                var content = await str.ReadToEndAsync();
                bookModel = JsonConvert.DeserializeObject<BookModel>(content);
                if (bookModel == null)
                    throw new JsonException("No data after deserialization. Possibly corrupted Json.");
            }
            catch (Exception ex) when (ex is IOException or InvalidOperationException or JsonException)
            {
                ScreenNotification.ShowNotification("There was an error loading your book.", ScreenNotification.NotificationType.Error);
                NotesModule.Logger.Error(ex, ex.Message);
                return;
            }

            BookBase prevBook;
            if ((prevBook = _displayedBooks.FirstOrDefault(b => b.Guid.Equals(bookModel.Id))) != null)
            {
                prevBook.Location = new Point((GameService.Graphics.SpriteScreen.Width - prevBook.Width) / 2, (GameService.Graphics.SpriteScreen.Height - prevBook.Height) / 2);
                return;
            }

            this.Create(bookModel.Id, bookModel.Title, bookModel.Pages.Select(p => (p.Title, p.Content)).ToList());
        }

        private void OnBookDisposed(object o, EventArgs e)
        {
            _displayedBooks.Remove((BookBase)o);
        }

        private void OnBookDelete(object o, EventArgs e)
        {
            var book = (BookBase)o;
            ConfirmationPrompt.ShowPrompt(confirmed =>
            {
                if (!confirmed) return;
                
                this.Delete(book.Guid);
                book.Dispose();
            }, $"Are you sure you want to delete \"{book.Title}\"?\nConfirm this action by typing the title:", "Yes", "Cancel", book.Title);
        }

        private void OnBookChanged(object o, EventArgs e)
        {
            this.Save(BookModel.FromControl((BookBase)o));
        }

        internal void Update(Action onSaveCallback)
        {
            if (DateTime.UtcNow.Subtract(_prevAutoSaveTime).TotalSeconds <= this.AutoSaveInternalSeconds) return;

            _prevAutoSaveTime = DateTime.UtcNow;
            foreach (var activeBook in _displayedBooks)
            {
                this.Save(BookModel.FromControl(activeBook));
            }
            onSaveCallback();
        }

        public async void Save(BookModel book)
        {
            var fileContents = Encoding.Default.GetBytes(JsonConvert.SerializeObject(book, Formatting.Indented));

            var fileName = GetFilePath(book.Id);

            try
            {
                using var sourceStream = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite, bufferSize: 4096, useAsync: true);
                await sourceStream.WriteAsync(fileContents, 0, fileContents.Length);
            }
            catch (UnauthorizedAccessException)
            {
                ScreenNotification.ShowNotification($"Autosave of \"{book.Title}\" failed. Access denied.", ScreenNotification.NotificationType.Error);
                return;
            }
            catch (IOException ex)
            {
                NotesModule.Logger.Error(ex, ex.Message);
            }

            AddOrUpdateIndex(book.Id, book.Title);
        }

        private async void SaveIndex()
        {
            var fileContents = Encoding.Default.GetBytes(JsonConvert.SerializeObject(_index, Formatting.Indented));

            var fileName = Path.Combine(this.CacheDir, _indexFileName);

            try
            {
                using var sourceStream = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite, bufferSize: 4096, useAsync: true);
                await sourceStream.WriteAsync(fileContents, 0, fileContents.Length);
            }
            catch (UnauthorizedAccessException)
            {
                ScreenNotification.ShowNotification($"Saving index file failed. Access denied.", ScreenNotification.NotificationType.Error);
            }
            catch (IOException ex)
            {
                NotesModule.Logger.Error(ex, ex.Message);
            }
        }

        public void Delete(Guid id)
        {
            if (!FileUtil.TryDelete(this.GetFilePath(id))) return;

            this.RemoveFromIndex(id);
        }

        private string GetFilePath(Guid id)
        {
            return Path.Combine(this.CacheDir, $"{id}.json");
        }
    }
}
