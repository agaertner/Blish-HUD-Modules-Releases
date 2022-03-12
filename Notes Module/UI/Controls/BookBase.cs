using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;

namespace Nekres.Notes.UI.Controls
{
    internal abstract class BookBase : WindowBase2
    {
        public event EventHandler<EventArgs> OnDelete;

        private const int PADDING_LEFT = 20;
        private const int PADDING_TOP = 75;
        private const int TITLE_MARGIN_Y = 20;
        private const int SHEET_PADDING_X = 38;
        private const int SHEET_PADDING_Y = 46;
        private const int SHEET_MARGIN = 15;

        // Book
        protected static readonly BitmapFont TitleFont = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size24, ContentService.FontStyle.Regular);
        private static readonly Texture2D TurnPageSprite = GameService.Content.GetTexture("1909317");
        private static readonly Texture2D TitleBorderTexture = NotesModule.ModuleInstance.ContentsManager.GetTexture("1002163.png");
        private static readonly Texture2D TitleHighlightTexture = NotesModule.ModuleInstance.ContentsManager.GetTexture("716658.png");

        // Sheet
        protected static readonly BitmapFont TextFont = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size14, ContentService.FontStyle.Regular);
        private static readonly Texture2D SheetTexture = Content.GetTexture("1909316");
        private static readonly BitmapFont PageNumberFont = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size18, ContentService.FontStyle.Regular);

        // Delete Texture
        private static readonly Texture2D DeleteButtonTextureNormal = NotesModule.ModuleInstance.ContentsManager.GetTexture("trashcanClosed_icon_64x64.png");
        private static readonly Texture2D DeleteButtonTextureHover = NotesModule.ModuleInstance.ContentsManager.GetTexture("trashcanOpen_icon_64x64.png");

        private readonly Guid _guid;
        public Guid Guid
        {
            get => _guid;
            private init => SetProperty(ref _guid, value);
        }

        // Turn Page Buttons
        protected Rectangle RightTurnButtonBounds { get; private set; }
        protected Rectangle LeftTurnButtonBounds { get; private set; }

        private bool _mouseOverTurnPageRight;
        private bool _mouseOverTurnPageLeft;

        // Edit BookTitle Button
        private Rectangle _deleteBookButtonBounds;
        private bool _mouseOverDeleteBook;

        // Title Wrapper
        protected Rectangle TitleRegion { get; private set; }

        // Content Wrapper
        protected Rectangle SheetBounds { get; private set; }

        protected Rectangle SheetContentRegion { get; private set; }

        public ObservableCollection<(string, string)> Pages { get; private set; }

        protected int PagesTotal { get; private set; }

        protected int CurrentPageIndex { get; private set; }

        /// <summary>
        /// Creates a book window similar to that seen when interacting with books in Guild Wars 2.
        /// </summary>
        /// <param name="id">Identifier that is used to differentiate this book from others.</param>
        /// <param name="title">Title used for the window name.</param>
        /// <param name="pages">Array of tuples in which each element is a pair of title and content.</param>
        protected BookBase(Guid id, string title, IEnumerable<(string, string)> pages)
        {
            this.Guid = id;

            pages = pages.ToList();

            Pages = pages.IsNullOrEmpty() ? new ObservableCollection<(string, string)> { (this.Title, string.Empty) } : pages.ToObservableCollection();
            PagesTotal = Pages.Count;

            this.Title = string.IsNullOrEmpty(title) ? "Empty Book" : title;

            Pages.CollectionChanged += OnPagesChanged;

            this.Disposed += delegate
            {
                Pages.CollectionChanged -= OnPagesChanged;
            };

            ConstructWindow(NotesModule.ModuleInstance.ContentsManager.GetTexture("1909321.png").Duplicate().GetRegion(30, 9, 625, 785), new Rectangle(0, 20, 625, 760), new Rectangle(0, 0, 625, 800));
        }

        /// <summary>
        /// Creates a book window similar to that seen when interacting with books in Guild Wars 2.
        /// </summary>
        /// <param name="id">Identifier that is used to differentiate this book from others.</param>
        /// <param name="title">Title used for the window name.</param>
        /// <param name="contentPages">Array of string in which each element represents an additional page of text.</param>
        protected BookBase(Guid id, string title, IEnumerable<string> contentPages) : this(id, title, contentPages.Select((p, i) => i == 0 ? (title, p) : (string.Empty, p)).ToArray()) { }

        /// <summary>
        /// Creates a book window similar to that seen when interacting with books in Guild Wars 2.
        /// </summary>
        /// <param name="id">Identifier that is used to differentiate this book from others.</param>
        /// <param name="title">Title used for the window name.</param>
        /// <param name="content">Text of the first page.</param>
        protected BookBase(Guid id, string title, string content) : this(id, title, new[] { (title, content) }) { }

        /// <summary>
        /// Creates an empty book window similar to that seen when interacting with books in Guild Wars 2.
        /// </summary>
        /// <param name="id">Identifier that is used to differentiate this book from others.</param>
        /// <param name="title">Title used for the window name.</param>
        protected BookBase(Guid id, string title) : this(id, title, string.Empty) { }

        public override void Hide()
        {
            base.Hide();
            this.Dispose();
        }

        private void OnPagesChanged(object o, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    PagesTotal++;
                    break;
                case NotifyCollectionChangedAction.Remove:
                    PagesTotal--;
                    break;
            }
        }

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            var relPos = RelativeMousePosition;
            _mouseOverTurnPageLeft = LeftTurnButtonBounds.Contains(relPos);
            _mouseOverTurnPageRight = RightTurnButtonBounds.Contains(relPos);
            _mouseOverDeleteBook = _deleteBookButtonBounds.Contains(relPos);
            base.OnMouseMoved(e);
        }

        protected override void OnLeftMouseButtonReleased(MouseEventArgs e)
        {
            if (_mouseOverDeleteBook)
                OnDelete?.Invoke(this, EventArgs.Empty);

            if (_mouseOverTurnPageLeft)
                TurnPage(CurrentPageIndex - 1);
            else if (_mouseOverTurnPageRight)
                TurnPage(CurrentPageIndex + 1);

            base.OnLeftMouseButtonReleased(e);
        }

        protected void TurnPage(int index)
        {
            if (index < 0 || index >= PagesTotal) return;
            GameService.Content.PlaySoundEffectByName("page-open-" + RandomUtil.GetRandom(1, 3));
            CurrentPageIndex = index;
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            /* Draw title */
            TitleRegion = new Rectangle(PADDING_LEFT, PADDING_TOP, bounds.Width - PADDING_LEFT * 2, 29);
#if DEBUG
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, TitleRegion, new Color(0, 50, 0, 20));
#endif
            spriteBatch.DrawOnCtrl(this, TitleHighlightTexture, new Rectangle(TitleRegion.Center.X - TitleHighlightTexture.Width, TitleRegion.Y - 17, TitleHighlightTexture.Width, TitleRegion.Height * 2), TitleBorderTexture.Bounds, new Color(25, 25, 25, 10));
            spriteBatch.DrawOnCtrl(this, TitleHighlightTexture, new Rectangle(TitleRegion.Center.X, TitleRegion.Y - 17, TitleHighlightTexture.Width, TitleRegion.Height * 2), TitleBorderTexture.Bounds, new Color(25, 25, 25, 10), 0, Vector2.Zero, SpriteEffects.FlipHorizontally);
            spriteBatch.DrawOnCtrl(this, TitleBorderTexture, new Rectangle(TitleRegion.Center.X - TitleBorderTexture.Width / 2, TitleRegion.Y - 20, TitleBorderTexture.Width, TitleBorderTexture.Height), TitleBorderTexture.Bounds, Color.White, 0, Vector2.Zero, SpriteEffects.FlipVertically);
            spriteBatch.DrawOnCtrl(this, TitleBorderTexture, new Rectangle(TitleRegion.Center.X - TitleBorderTexture.Width / 2, TitleRegion.Bottom + 5, TitleBorderTexture.Width, TitleBorderTexture.Height));

            /* Draw sheet */
            var sheetSize = PointExtensions.ResizeKeepAspect(SheetTexture.Bounds.Size, ContentRegion.Width - 150, ContentRegion.Height - 150, true);
            var sheetLocation = new Point((ContentRegion.Width - sheetSize.X) / 2, TitleRegion.Bottom + TITLE_MARGIN_Y);
            SheetBounds = new Rectangle(sheetLocation, sheetSize);
            spriteBatch.DrawOnCtrl(this, SheetTexture, SheetBounds, SheetTexture.Bounds, Color.White, 0f, Vector2.Zero);

            SheetContentRegion = new Rectangle(SheetBounds.X + SHEET_PADDING_X, SheetBounds.Y + SHEET_PADDING_Y, SheetBounds.Width - SHEET_PADDING_X * 2, SheetBounds.Height - SHEET_PADDING_Y * 2);
#if DEBUG
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, SheetContentRegion, new Color(0, 0, 255, 20));
#endif
            /* Draw page number */
            var pageNumer = (CurrentPageIndex + 1).ToString();
            Point pageNumberSize = (Point)PageNumberFont.MeasureString(pageNumer);
            var pageNumberRegion = new Rectangle(SheetBounds.Center.X - pageNumberSize.X / 2, SheetBounds.Bottom - pageNumberSize.Y * 2, pageNumberSize.X, pageNumberSize.Y);
#if DEBUG
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, pageNumberRegion, new Color(0, 0, 255, 20));
#endif
            spriteBatch.DrawStringOnCtrl(this, pageNumer, PageNumberFont, pageNumberRegion, Color.Black, false, HorizontalAlignment.Center);

            /* Draw turn buttons */
            LeftTurnButtonBounds = new Rectangle(SheetBounds.Left - SHEET_MARGIN - TurnPageSprite.Width, SheetBounds.Y + SheetBounds.Height / 2 - SHEET_MARGIN, (int)(1.2f * TurnPageSprite.Width), (int)(1.2f * TurnPageSprite.Height));
            RightTurnButtonBounds = new Rectangle(SheetBounds.Right + SHEET_MARGIN, SheetBounds.Y + SheetBounds.Height / 2 - SHEET_MARGIN, (int)(1.2f * TurnPageSprite.Width), (int)(1.2f * TurnPageSprite.Height));
            var leftTurnButtonColor = CurrentPageIndex > 0 ? _mouseOverTurnPageLeft ? Color.White : new Color(200, 200, 200) : new Color(100, 100, 100);
            var rightTurnButtonColor = CurrentPageIndex < PagesTotal - 1 ? _mouseOverTurnPageRight ? Color.White : new Color(200, 200, 200) : new Color(100, 100, 100);
            spriteBatch.DrawOnCtrl(this, TurnPageSprite, LeftTurnButtonBounds, TurnPageSprite.Bounds, leftTurnButtonColor, 0, Vector2.Zero, SpriteEffects.FlipHorizontally);
            spriteBatch.DrawOnCtrl(this, TurnPageSprite, RightTurnButtonBounds, TurnPageSprite.Bounds, rightTurnButtonColor);

            var deleteButtonTexture = _mouseOverDeleteBook ? DeleteButtonTextureHover : DeleteButtonTextureNormal;
            _deleteBookButtonBounds = new Rectangle(bounds.X + 12, bounds.Height - 32, 32, 32);
            spriteBatch.DrawOnCtrl(this, deleteButtonTexture, _deleteBookButtonBounds);
        }
    }
}
