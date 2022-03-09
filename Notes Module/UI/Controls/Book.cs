using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Blish_HUD.Modules.Managers;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.BitmapFonts;
using Nekres.Notes.UI.Models;
using Newtonsoft.Json;
using Color = Microsoft.Xna.Framework.Color;
using File = Gw2Sharp.WebApi.V2.Models.File;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
namespace Nekres.Notes.UI.Controls
{
    public sealed class Book : WindowBase2
    {
        private const int MaxCharacterCountTitle = 35;
        private const int MaxCharacterCountContent = 400;

        private const int PADDING_LEFT = 20;
        private const int PADDING_TOP = 75;
        private const int TITLE_MARGIN_Y = 20;
        private const int SHEET_PADDING_X = 38;
        private const int SHEET_PADDING_Y = 46;
        private const int SHEET_MARGIN = 15;

        // Book
        private static readonly BitmapFont TitleFont = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size24, ContentService.FontStyle.Regular);
        private static readonly Texture2D TurnPageSprite = GameService.Content.GetTexture("1909317");
        private static readonly Texture2D TitleBorderTexture = NotesModule.ModuleInstance.ContentsManager.GetTexture("1002163.png");
        private static readonly Texture2D TitleHighlightTexture = NotesModule.ModuleInstance.ContentsManager.GetTexture("716658.png");

        // Sheet
        private static readonly Texture2D SheetTexture = Content.GetTexture("1909316");
        private static readonly BitmapFont PageNumberFont = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size18, ContentService.FontStyle.Regular);
        private static readonly BitmapFont TextFont = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size14, ContentService.FontStyle.Regular);

        private static readonly Texture2D AddPageButtonTexture = GameService.Content.GetTexture("plus");
        private static readonly Texture2D RemovePageButtonTexture = GameService.Content.GetTexture("minus");

        // Edit Button
        private static readonly Texture2D EditButtonTextureDisabled = NotesModule.ModuleInstance.ContentsManager.GetTexture("155939.png");
        private static readonly Texture2D EditButtonTextureHover = NotesModule.ModuleInstance.ContentsManager.GetTexture("155940.png");
        private static readonly Texture2D EditButtonTextureNormal = NotesModule.ModuleInstance.ContentsManager.GetTexture("155941.png");
        private static readonly Texture2D EditButtonTextureClick = NotesModule.ModuleInstance.ContentsManager.GetTexture("155942.png");

        private static readonly Texture2D DeleteButtonTextureNormal = NotesModule.ModuleInstance.ContentsManager.GetTexture("trashcanClosed_icon_64x64.png");
        private static readonly Texture2D DeleteButtonTextureHover = NotesModule.ModuleInstance.ContentsManager.GetTexture("trashcanOpen_icon_64x64.png");

        private readonly Guid _guid;

        public Guid Guid
        {
            get => _guid;
            private init => SetProperty(ref _guid, value);
        }

        private bool _allowEdit;
        /// <summary>
        /// <see langword="True"/> if editing of the book is permitted. Otherwise <see langword="false"/>.
        /// </summary>
        public bool AllowEdit
        {
            get => _allowEdit;
            set
            {
                SetProperty(ref _allowEdit, value);
                if (_editTitleTextBox != null)
                    _editTitleTextBox.Enabled = _allowEdit;
                if (_editContentTextBox != null)
                    _editContentTextBox.Enabled = _allowEdit;
            }
        }

        private bool _useChapters;
        /// <summary>
        /// <see langword="True"/> if pages can have their own unique titles. Otherwise <see langword="false"/>.
        /// </summary>
        public bool UseChapters
        {
            get => _useChapters;
            set
            {
                SetProperty(ref _useChapters, value);
                Title = _useChapters ? BookTitle : string.Empty;
            }
        }

        private string _bookTitle;
        /// <summary>
        /// Title used for the entire book when <see cref="UseChapters"/> is <see langword="false"/>.
        /// </summary>
        public string BookTitle
        {
            get => _bookTitle;
            set
            {
                SetProperty(ref _bookTitle, value);
                Title = UseChapters ? BookTitle : string.Empty;
            }
        }

        private bool _editingBookTitle;
        private TextBox _bookTitlePromptInputBox;

        private readonly List<(string, string)> _pages;
        public List<(string, string)> Pages => new (_pages);

        private int _pagesTotal;
        private int _currentPageIndex;

        // Turn Page Buttons
        private Rectangle _rightTurnButtonBounds;
        private Rectangle _leftTurnButtonBounds;
        private bool _mouseOverTurnPageRight;
        private bool _mouseOverTurnPageLeft;

        // Add/Remove Page Buttons
        private Rectangle _rightAddPageButtonBounds;
        private Rectangle _leftAddPageButtonBounds;
        private Rectangle _removePageButtonBounds;
        private bool _mouseOverRightAddPageButton;
        private bool _mouseOverLeftAddPageButton;
        private bool _mouseOverRemovePageButton;


        // Edit BookTitle Button
        private Rectangle _editTitleButtonBounds;
        private bool _mouseOverDeleteBook;

        // Delete Button
        private Rectangle _deleteBookButtonBounds;
        private bool _mouseOverEditBookTitle;

        // Title Wrapper
        private Rectangle _titleRegion;
        private TextBox _editTitleTextBox;

        // Content Wrapper
        private Rectangle _sheetContentRegion;
        private MultilineTextBox _editContentTextBox;

        /// <summary>
        /// Creates a book window similar to that seen when interacting with books in Guild Wars 2.
        /// </summary>
        /// <param name="title">Title used for the entire book when <see cref="UseChapters"/> is <see langword="false"/>.</param>
        /// <param name="pages">Array of tuples in which each element is a pair of title and content.</param>
        /// <param name="useChapters">If each page can have its own unique title.</param>
        /// <param name="guid">Identifier that is used to differentiate this book from others.</param>
        /// <param name="allowEdit">Indicates if editing of the book (adding, removing, changing pages etc) is permitted.</param>
        public Book(string title, (string, string)[] pages, bool useChapters = true, bool allowEdit = false, string guid = null)
        {
            if (Guid.TryParse(guid, out var parsedGuid))
                Guid = parsedGuid;

            if (Guid.Empty.Equals(Guid))
                Guid = Guid.NewGuid();

            _bookTitle = string.IsNullOrEmpty(title) ? "Empty Book" : title;
            _pages = pages?.ToList() ?? new List<(string, string)>{(string.Empty, string.Empty)};
            _useChapters = useChapters;
            _allowEdit = allowEdit;
            _pagesTotal = _pages.Count;

            ConstructWindow(NotesModule.ModuleInstance.ContentsManager.GetTexture("1909321.png").Duplicate().GetRegion(30, 9, 625, 785), new Rectangle(0, 20, 625, 760), new Rectangle(0, 0, 625, 800));
            Title = useChapters ? title : string.Empty;
        }

        /// <summary>
        /// Creates a book window similar to that seen when interacting with books in Guild Wars 2.
        /// </summary>
        /// <param name="title">Title used for the entire book when <see cref="UseChapters"/> is <see langword="false"/>.</param>
        /// <param name="contentPages">Array of string in which each element represents an additional page of text.</param>
        /// <param name="useChapters">If each page can have its own unique title.</param>
        /// <param name="allowEdit">Indicates if editing of the book (adding, removing, changing pages etc) is permitted.</param>
        public Book(string title, string[] contentPages, bool useChapters = false, bool allowEdit = false, string guid = null) : this(title, contentPages.Select(p => (string.Empty, p)).ToArray(), useChapters, allowEdit, guid){}

        /// <summary>
        /// Creates a book window similar to that seen when interacting with books in Guild Wars 2.
        /// </summary>
        /// <param name="title">Title used for the entire book when <see cref="UseChapters"/> is <see langword="false"/>.</param>
        /// <param name="content">Text of the first page.</param>
        /// <param name="useChapters">If each page can have its own unique title.</param>
        /// <param name="allowEdit">Indicates if editing of the book (adding, removing, changing pages etc) is permitted.</param>
        public Book(string title, string content, bool useChapters = true, bool allowEdit = false, string guid = null) : this(title, new []{(string.Empty, content)}, useChapters, allowEdit, guid){}

        /// <summary>
        /// Creates an empty book window similar to that seen when interacting with books in Guild Wars 2.
        /// </summary>
        /// <param name="title">Title used for the entire book when <see cref="UseChapters"/> is <see langword="false"/>.</param>
        /// <param name="useChapters">If each page can have its own unique title.</param>
        public Book(string title = "Empty Book", bool useChapters = true) : this(title, string.Empty, useChapters, true){}

        private bool _deletedLock;
        public async void Save()
        {
            if (_deletedLock) return;
            var bookModel = new BookModel(this);

            var fileContents = Encoding.Default.GetBytes(JsonConvert.SerializeObject(bookModel, Formatting.Indented));

            var fileName = $"{NotesModule.ModuleInstance.DirectoriesManager.GetFullDirectoryPath("notes")}/{BookTitle}.json";

            try
            {
                using var sourceStream = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite, bufferSize: 4096, useAsync: true);
                await sourceStream.WriteAsync(fileContents, 0, fileContents.Length);
            }
            catch (UnauthorizedAccessException)
            {
                ScreenNotification.ShowNotification($"Autosave of \"{BookTitle}\" failed. Access denied.", ScreenNotification.NotificationType.Error);
                return;
            }
            catch (IOException ex)
            {
                NotesModule.Logger.Error(ex, ex.Message);
            }

            NotesModule.ModuleInstance.BuildContextMenu();
        }

        public void Delete()
        {
            if (!FileUtil.TryDelete(GetFilePath())) return;
            _deletedLock = true;
            NotesModule.ModuleInstance.BuildContextMenu();
            Dispose();
        }

        public string GetFilePath()
        {
            return $"{NotesModule.ModuleInstance.DirectoriesManager.GetFullDirectoryPath("notes")}/{BookTitle}.json";
        }

        public override void Hide()
        {
            Save();
            base.Hide();
            this.Dispose();
        }

        public void AddPage(int index, string content = "", string title = "")
        {
            if (!AllowEdit) return;
            if (index < 0)
                index = 0;
            else if (index > _pagesTotal)
                index = _pagesTotal;
            content ??= string.Empty;
            title ??= string.Empty;
            _pages.Insert(index, (title, content));
            _pagesTotal++;
            TurnPage(index);
        }

        public void RemovePage(int index)
        {
            if (!AllowEdit || _pagesTotal == 1) return;
            if (index < 0)
                index = 0;
            else if (index >= _pagesTotal)
                index = _pagesTotal - 1;
            _pages.RemoveAt(index);
            _pagesTotal--;
            TurnPage(index >= _pagesTotal ? _pagesTotal - 1 : index);
        }

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            var relPos = RelativeMousePosition;
            _mouseOverTurnPageLeft = _leftTurnButtonBounds.Contains(relPos);
            _mouseOverTurnPageRight = _rightTurnButtonBounds.Contains(relPos);
            _mouseOverRightAddPageButton = _rightAddPageButtonBounds.Contains(relPos);
            _mouseOverLeftAddPageButton = _leftAddPageButtonBounds.Contains(relPos);
            _mouseOverRemovePageButton = _removePageButtonBounds.Contains(relPos);
            _mouseOverEditBookTitle = _editTitleButtonBounds.Contains(relPos);
            _mouseOverDeleteBook = _deleteBookButtonBounds.Contains(relPos);
            base.OnMouseMoved(e);
        }

        protected override void OnLeftMouseButtonReleased(MouseEventArgs e)
        {
            if (_mouseOverDeleteBook)
                Delete();

            if (!_editingBookTitle)
            {
                if (_mouseOverTurnPageLeft)
                    TurnPage(_currentPageIndex - 1);
                else if (_mouseOverTurnPageRight)
                    TurnPage(_currentPageIndex + 1);
                else if (_mouseOverLeftAddPageButton)
                    AddPage(_currentPageIndex);
                else if (_mouseOverRightAddPageButton)
                    AddPage(_currentPageIndex + 1);
                else if (_mouseOverRemovePageButton)
                    RemovePage(_currentPageIndex);

                if (AllowEdit && _mouseOverEditBookTitle)
                {
                    // Show file renaming overlay. Disable other inputs.
                    _editingBookTitle = true;
                    _editTitleTextBox.Hide();
                    _editContentTextBox.Enabled = false;
                    _bookTitlePromptInputBox = new TextBox
                    {
                        Parent = this,
                        Text = BookTitle,
                        MaxLength = MaxCharacterCountTitle,
                        Size = new Point(400, _titleRegion.Height),
                        Location = new Point((ContentRegion.Width - 400) / 2, (Height - _titleRegion.Height) / 2),
                        Font = TitleFont,
                        ForeColor = Color.White,
                        Visible = true,
                        Enabled = true,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        PlaceholderText = BookTitle
                    };
                    _bookTitlePromptInputBox.InputFocusChanged += (o, e) =>
                    {
                        if (e.Value) return;
                        if (string.IsNullOrEmpty(_bookTitlePromptInputBox.Text)) return;
                        FileUtil.TryDelete(GetFilePath());
                        BookTitle = _bookTitlePromptInputBox.Text;
                        _editTitleTextBox.Text = UseChapters ? _pages[_currentPageIndex].Item1 : _bookTitlePromptInputBox.Text;
                        _editTitleTextBox.Show();
                        _editContentTextBox.Enabled = AllowEdit;
                        _bookTitlePromptInputBox.Dispose();
                        _editingBookTitle = false;
                        Save();
                    };
                }
            } 
            else if (_mouseOverEditBookTitle)
            {
                // Hide file renaming overlay. Enable other inputs.
                _editContentTextBox.Enabled = AllowEdit;
                _editTitleTextBox.Show();
                _bookTitlePromptInputBox.Dispose();
                _editingBookTitle = false;
            }
            base.OnLeftMouseButtonReleased(e);
        }

        private void TurnPage(int index)
        {
            if (index < 0 || index >= _pagesTotal) return;
            GameService.Content.PlaySoundEffectByName("page-open-" + RandomUtil.GetRandom(1, 3));
            _currentPageIndex = index;
            if (_editContentTextBox != null)
                _editContentTextBox.Text = _pages[index].Item2;
            if (_editTitleTextBox != null)
                _editTitleTextBox.Text = UseChapters ? _pages[index].Item1 : BookTitle;
        }

        private void EditTitle(int index, string newTitle)
        {
            if (!AllowEdit || index < 0 || index > _pagesTotal) return;
            newTitle ??= string.Empty;
            if (UseChapters)
                _pages[index] = (newTitle, _pages[index].Item2);
            else
                BookTitle = newTitle;
        }

        private void EditContent(int index, string newContent)
        {
            if (!AllowEdit || index < 0 || index > _pagesTotal) return;
            newContent ??= string.Empty;
            _pages[index] = (_pages[index].Item1, newContent);
        }

        private void AddOrUpdateEditTextBox()
        {
            if (_editTitleTextBox == null)
            {
                _editTitleTextBox = new TextBox
                {
                    Parent = this,
                    Text = _pages[_currentPageIndex].Item1,
                    MaxLength = MaxCharacterCountTitle,
                    Size = new Point(400, _titleRegion.Height),
                    Location = new Point((ContentRegion.Width - 400) / 2, _titleRegion.Y - _titleRegion.Height),
                    Font = TitleFont,
                    ForeColor = Color.White,
                    Visible = true,
                    Enabled = AllowEdit,
                    HideBackground = true,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    PlaceholderText = "No Title"
                };
                _editTitleTextBox.InputFocusChanged += (o, e) =>
                {
                    if (e.Value) return;
                    EditTitle(_currentPageIndex, _editTitleTextBox.Text);
                    Save();
                };
            }
            _editTitleTextBox.Location = new Point((ContentRegion.Width - 400) / 2, _titleRegion.Y - _titleRegion.Height);
            _editTitleTextBox.Size = new Point(400, _titleRegion.Height);

            if (_editContentTextBox == null)
            {
                _editContentTextBox = new MultilineTextBox
                {
                    Parent = this,
                    Text = _pages[_currentPageIndex].Item2,
                    MaxLength = MaxCharacterCountContent,
                    Size = _sheetContentRegion.Size,
                    Location = new Point(_sheetContentRegion.X, _sheetContentRegion.Y - 30),
                    Font = TextFont,
                    ForeColor = Color.Black,
                    Visible = true,
                    Enabled = AllowEdit,
                    HideBackground = true
                };
                _editContentTextBox.InputFocusChanged += (o, e) =>
                {
                    if (e.Value) return;
                    EditContent(_currentPageIndex, _editContentTextBox.Text);
                    Save();
                };
            }
            _editContentTextBox.Location = new Point(_sheetContentRegion.X, _sheetContentRegion.Y - 30);
            _editContentTextBox.Size = _sheetContentRegion.Size;
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            /* Draw title */
            _titleRegion = new Rectangle(PADDING_LEFT, PADDING_TOP, bounds.Width - PADDING_LEFT * 2, 29);
#if DEBUG
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, _titleRegion, new Color(0, 50, 0, 20));
#endif
            spriteBatch.DrawOnCtrl(this, TitleHighlightTexture, new Rectangle(_titleRegion.Center.X - TitleHighlightTexture.Width, _titleRegion.Y - 17, TitleHighlightTexture.Width, _titleRegion.Height * 2), TitleBorderTexture.Bounds, new Color(25,25,25,10));
            spriteBatch.DrawOnCtrl(this, TitleHighlightTexture, new Rectangle(_titleRegion.Center.X, _titleRegion.Y - 17, TitleHighlightTexture.Width, _titleRegion.Height * 2), TitleBorderTexture.Bounds, new Color(25, 25, 25, 10), 0, Vector2.Zero, SpriteEffects.FlipHorizontally);
            spriteBatch.DrawOnCtrl(this, TitleBorderTexture, new Rectangle(_titleRegion.Center.X - TitleBorderTexture.Width / 2, _titleRegion.Y - 20, TitleBorderTexture.Width, TitleBorderTexture.Height), TitleBorderTexture.Bounds, Color.White, 0, Vector2.Zero, SpriteEffects.FlipVertically);
            spriteBatch.DrawOnCtrl(this, TitleBorderTexture, new Rectangle(_titleRegion.Center.X - TitleBorderTexture.Width / 2, _titleRegion.Bottom + 5, TitleBorderTexture.Width, TitleBorderTexture.Height));
            
            /* Draw sheet */
            var sheetSize = PointExtensions.ResizeKeepAspect(SheetTexture.Bounds.Size, ContentRegion.Width - 150, ContentRegion.Height - 150, true);
            var sheetLocation = new Point((ContentRegion.Width - sheetSize.X) / 2, _titleRegion.Bottom + TITLE_MARGIN_Y);
            var sheetDest = new Rectangle(sheetLocation, sheetSize);
            spriteBatch.DrawOnCtrl(this, SheetTexture, sheetDest, SheetTexture.Bounds, Color.White, 0f, Vector2.Zero);

            _sheetContentRegion = new Rectangle(sheetDest.X + SHEET_PADDING_X, sheetDest.Y + SHEET_PADDING_Y, sheetDest.Width - SHEET_PADDING_X * 2, sheetDest.Height - SHEET_PADDING_Y * 2);
#if DEBUG
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, _sheetContentRegion, new Color(0, 0, 255, 20));
#endif
            /* Draw page number */
            var pageNumer = (_currentPageIndex + 1).ToString();
            Point pageNumberSize = (Point)PageNumberFont.MeasureString(pageNumer);
            var pageNumberRegion = new Rectangle(sheetDest.Center.X - pageNumberSize.X / 2, sheetDest.Bottom - pageNumberSize.Y * 2, pageNumberSize.X, pageNumberSize.Y);
#if DEBUG
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, pageNumberRegion, new Color(0, 0, 255, 20));
#endif
            spriteBatch.DrawStringOnCtrl(this, pageNumer, PageNumberFont, pageNumberRegion, Color.Black, false, HorizontalAlignment.Center);

            /* Draw turn buttons */
            _leftTurnButtonBounds = new Rectangle(sheetDest.Left - SHEET_MARGIN - TurnPageSprite.Width, sheetDest.Y + sheetDest.Height / 2 - SHEET_MARGIN, (int)(1.2f * TurnPageSprite.Width), (int)(1.2f * TurnPageSprite.Height));
            _rightTurnButtonBounds = new Rectangle(sheetDest.Right + SHEET_MARGIN, sheetDest.Y + sheetDest.Height / 2 - SHEET_MARGIN, (int)(1.2f * TurnPageSprite.Width), (int)(1.2f * TurnPageSprite.Height));
            var leftTurnButtonColor = _currentPageIndex > 0 ? _mouseOverTurnPageLeft ? Color.White : new Color(200, 200, 200) : new Color(100, 100, 100);
            var rightTurnButtonColor = _currentPageIndex < _pagesTotal - 1 ? _mouseOverTurnPageRight ? Color.White : new Color(200, 200, 200) : new Color(100, 100, 100);
            spriteBatch.DrawOnCtrl(this, TurnPageSprite, _leftTurnButtonBounds, TurnPageSprite.Bounds, leftTurnButtonColor, 0, Vector2.Zero, SpriteEffects.FlipHorizontally);
            spriteBatch.DrawOnCtrl(this, TurnPageSprite, _rightTurnButtonBounds, TurnPageSprite.Bounds, rightTurnButtonColor);

            /* Draw add/remove buttons */
            if (AllowEdit)
            {
                _rightAddPageButtonBounds = new Rectangle(_rightTurnButtonBounds.Center.X - AddPageButtonTexture.Width / 2,_rightTurnButtonBounds.Bottom + AddPageButtonTexture.Height, AddPageButtonTexture.Width, AddPageButtonTexture.Height);
                spriteBatch.DrawOnCtrl(this, AddPageButtonTexture, _rightAddPageButtonBounds, AddPageButtonTexture.Bounds, _mouseOverRightAddPageButton ? Color.White : new Color(200, 200, 200));
                _leftAddPageButtonBounds = new Rectangle(_leftTurnButtonBounds.Center.X, _leftTurnButtonBounds.Bottom + AddPageButtonTexture.Height, AddPageButtonTexture.Width, AddPageButtonTexture.Height);
                spriteBatch.DrawOnCtrl(this, AddPageButtonTexture, _leftAddPageButtonBounds, AddPageButtonTexture.Bounds, _mouseOverLeftAddPageButton ? Color.White : new Color(200, 200, 200));
                _removePageButtonBounds = new Rectangle(sheetDest.Center.X - RemovePageButtonTexture.Width / 2, sheetDest.Bottom - RemovePageButtonTexture.Height / 2, RemovePageButtonTexture.Width, RemovePageButtonTexture.Height);
                spriteBatch.DrawOnCtrl(this, RemovePageButtonTexture, _removePageButtonBounds, RemovePageButtonTexture.Bounds, _mouseOverRemovePageButton ? Color.White : new Color(200, 200, 200));

                if (_editingBookTitle)
                    spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, bounds, Color.Black * 0.8f);

                var editBookTitleButtonTexture = !_editingBookTitle ? _mouseOverEditBookTitle ? Mouse.GetState().LeftButton == ButtonState.Pressed ? EditButtonTextureClick : EditButtonTextureHover : EditButtonTextureNormal : EditButtonTextureDisabled;
                _editTitleButtonBounds = new Rectangle(bounds.X - 5, bounds.Y - 10, editBookTitleButtonTexture.Width, editBookTitleButtonTexture.Height);
                spriteBatch.DrawOnCtrl(this, editBookTitleButtonTexture, _editTitleButtonBounds);
            }

            var deleteButtonTexture = _mouseOverDeleteBook ? DeleteButtonTextureHover : DeleteButtonTextureNormal;
            _deleteBookButtonBounds = new Rectangle(_editTitleButtonBounds.X + 12, bounds.Height - 32, 32, 32);
            spriteBatch.DrawOnCtrl(this, deleteButtonTexture, _deleteBookButtonBounds);

            AddOrUpdateEditTextBox();
        }
    }
}
