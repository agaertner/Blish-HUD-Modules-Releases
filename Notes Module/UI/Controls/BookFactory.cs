using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
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
        private IList<BookBase> _displayedBooks;

        private IList<BookModel> _fetchedBooks;

        private readonly int AutoSaveInternalSeconds = 30;
        private DateTime _prevAutoSaveTime = DateTime.UtcNow;

        public void Dispose()
        {
            foreach (var displayedBook in _displayedBooks) 
                displayedBook?.Dispose();
        }

        public BookBase FromStream(Stream stream)
        {
            throw new NotImplementedException();
        }

        public void Create(string title)
        {
            var book = new EditableBook(Guid.NewGuid(), title)
            {
                Parent = GameService.Graphics.SpriteScreen,
                Location = new Point(GameService.Graphics.SpriteScreen.Width / 2, GameService.Graphics.SpriteScreen.Height / 2),
            };
            book.Disposed += OnBookDisposed;
            book.Location = new Point((GameService.Graphics.SpriteScreen.Width - book.Width) / 2, (GameService.Graphics.SpriteScreen.Height - book.Height) / 2);
            book.Show();

            _displayedBooks.Add(book);
        }

        public void Create(Guid id, string title, IEnumerable<(string, string)> pages)
        {
            var book = new EditableBook(id, title, pages)
            {
                Parent = GameService.Graphics.SpriteScreen,
                Location = new Point(GameService.Graphics.SpriteScreen.Width / 2, GameService.Graphics.SpriteScreen.Height / 2),
            };
            book.Disposed += OnBookDisposed;
            book.Location = new Point((GameService.Graphics.SpriteScreen.Width - book.Width) / 2, (GameService.Graphics.SpriteScreen.Height - book.Height) / 2);
            book.Show();

            _displayedBooks.Add(book);
        }

        public async void LoadNote(string filePath)
        {
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
            if ((prevBook = _displayedBooks.FirstOrDefault(b => b.Guid.Equals(bookModel.Guid))) != null)
            {
                prevBook.Location = new Point((GameService.Graphics.SpriteScreen.Width - prevBook.Width) / 2, (GameService.Graphics.SpriteScreen.Height - prevBook.Height) / 2);
                return;
            }

            this.Create(bookModel.Guid, bookModel.Title, bookModel.Pages.Select(p => (p.Title, p.Content)));
        }

        private void OnBookDisposed(object o, EventArgs e)
        {
            _displayedBooks.Remove((BookBase)o);
        }

        internal void Update(Action callback)
        {
            if (DateTime.UtcNow.Subtract(_prevAutoSaveTime).TotalSeconds > this.AutoSaveInternalSeconds)
            {
                _prevAutoSaveTime = DateTime.UtcNow;
                foreach (var activeBook in _displayedBooks)
                {
                    this.Save(activeBook);
                }
                callback();
            }
        }

        public async void Save(BookBase book)
        {
            var bookModel = new BookModel(book);

            var fileContents = Encoding.Default.GetBytes(JsonConvert.SerializeObject(bookModel, Formatting.Indented));

            var fileName = $"{NotesModule.ModuleInstance.DirectoriesManager.GetFullDirectoryPath("notes")}/{book.Title}.json";

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

            NotesModule.ModuleInstance.BuildContextMenu();
        }

        public void Delete(BookBase book)
        {
            if (!FileUtil.TryDelete(this.GetFilePath(book))) return;
            NotesModule.ModuleInstance.BuildContextMenu();
            Dispose();
        }

        private string GetFilePath(BookBase book)
        {
            return $"{NotesModule.ModuleInstance.DirectoriesManager.GetFullDirectoryPath("notes")}/{book.Title}.json";
        }
    }
}
