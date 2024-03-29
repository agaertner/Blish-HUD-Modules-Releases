﻿using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Nekres.Notes.Properties;

namespace Nekres.Notes.UI.Controls
{
    internal class EditableBook : BookBase
    {
        public event EventHandler<EventArgs> OnChanged;

        // Edit Button
        private static readonly Texture2D EditButtonTextureDisabled = NotesModule.ModuleInstance.ContentsManager.GetTexture("155939.png");
        private static readonly Texture2D EditButtonTextureHover = NotesModule.ModuleInstance.ContentsManager.GetTexture("155940.png");
        private static readonly Texture2D EditButtonTextureNormal = NotesModule.ModuleInstance.ContentsManager.GetTexture("155941.png");
        private static readonly Texture2D EditButtonTextureClick = NotesModule.ModuleInstance.ContentsManager.GetTexture("155942.png");

        // Add/Remove Page Textures
        private static readonly Texture2D AddPageButtonTexture = GameService.Content.GetTexture("plus");
        private static readonly Texture2D RemovePageButtonTexture = GameService.Content.GetTexture("minus");

        private const int MaxCharacterCountTitle = 35;
        private const int MaxCharacterCountContent = 400;

        private bool _editingBookTitle;
        private TextBox _bookTitlePromptInputBox;

        // Add/Remove Page Buttons
        private Rectangle _rightAddPageButtonBounds;
        private Rectangle _leftAddPageButtonBounds;
        private Rectangle _removePageButtonBounds;
        private bool _mouseOverRightAddPageButton;
        private bool _mouseOverLeftAddPageButton;
        private bool _mouseOverRemovePageButton;

        // Edit BookTitle Button
        private Rectangle _editTitleButtonBounds;
        private bool _mouseOverEditBookTitle;

        // Title Input
        private TextBox _editTitleTextBox;

        // Content Input
        private MultilineTextBox _editContentTextBox;

        public EditableBook(Guid id, string title, IList<(string, string)> pages) : base(id, title, pages)
        {
        }

        public EditableBook(Guid id, string title, IEnumerable<string> contentPages) : base(id, title, contentPages)
        {
        }

        public EditableBook(Guid id, string title, string content) : base(id, title, content)
        {
        }

        public EditableBook(Guid id, string title) : base(id, title)
        {
        }

        protected override void TurnPage(int index)
        {
            base.TurnPage(index);
            this._editTitleTextBox.Text = this.Pages[this.CurrentPageIndex].Item1;
            this._editContentTextBox.Text = this.Pages[this.CurrentPageIndex].Item2;
            for (var i = this.CurrentPageIndex - 1; i >= 0; i--)
            {
                var mostRecentTitle = this.Pages[i].Item1;
                if (string.IsNullOrEmpty(mostRecentTitle))
                    continue;
                this._editTitleTextBox.PlaceholderText = mostRecentTitle;
                break;
            }
        }

        public void AddPage(int index, string content = "", string title = "")
        {
            if (index < 0)
                index = 0;
            else if (index > PagesTotal)
                index = this.PagesTotal;
            this.Pages.Insert(index, (title ?? string.Empty, content ?? string.Empty));
            this.TurnPage(index);
            this.OnChanged?.Invoke(this, EventArgs.Empty);
        }

        public void RemovePage(int index)
        {
            if (this.PagesTotal == 1) return;
            var pageTitle = this.Pages[this.CurrentPageIndex].Item1;
            ConfirmationPrompt.ShowPrompt(confirmed =>
            {
                if (!confirmed) return;
                if (index < 0)
                    index = 0;
                else if (index >= PagesTotal)
                    index = PagesTotal - 1;
                this.Pages.RemoveAt(index);
                this.TurnPage(index >= PagesTotal ? PagesTotal - 1 : index);
                this.OnChanged?.Invoke(this, EventArgs.Empty);
            }, string.Format(Resources.You_are_about_to_permanently_destroy__0__, $"\u201c{(string.IsNullOrEmpty(pageTitle) ? Resources.Empty_Page : pageTitle)}\u201d ({Resources.Page} {this.CurrentPageIndex + 1})") + '\n' + Resources.Are_you_sure_,
                Resources.Yes, Resources.Cancel);
        }

        private void EditTitle(int index, string newTitle)
        {
            if (index < 0 || index > PagesTotal) return;
            this.Pages[index] = (newTitle ?? string.Empty, this.Pages[index].Item2);
            this.OnChanged?.Invoke(this, EventArgs.Empty);
        }

        private void EditContent(int index, string newContent)
        {
            if (index < 0 || index > PagesTotal) return;
            var page = this.Pages[index];
            this.Pages[index] = (this.Pages[index].Item1, newContent ?? string.Empty);
            this.OnChanged?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            var relPos = RelativeMousePosition;
            _mouseOverRightAddPageButton = _rightAddPageButtonBounds.Contains(relPos);
            _mouseOverLeftAddPageButton = _leftAddPageButtonBounds.Contains(relPos);
            _mouseOverRemovePageButton = _removePageButtonBounds.Contains(relPos);
            _mouseOverEditBookTitle = _editTitleButtonBounds.Contains(relPos);

            if (_mouseOverRightAddPageButton)
                this.BasicTooltipText = Resources.Add_new_page_right;
            else if (_mouseOverLeftAddPageButton)
                this.BasicTooltipText = Resources.Add_new_page_left;
            else if (_mouseOverRemovePageButton)
                this.BasicTooltipText = Resources.Remove_current_page;
            else if (_mouseOverEditBookTitle)
                this.BasicTooltipText = Resources.Edit_book_title;
            else
                this.BasicTooltipText = string.Empty;

            base.OnMouseMoved(e);
        }

        private void AddOrUpdateEditTextBox()
        {
            if (_editTitleTextBox == null)
            {
                _editTitleTextBox = new TextBox
                {
                    Parent = this,
                    Text = this.Pages[0].Item1,
                    MaxLength = MaxCharacterCountTitle,
                    Size = new Point(400, TitleRegion.Height),
                    Location = new Point((ContentRegion.Width - 400) / 2, TitleRegion.Y - TitleRegion.Height),
                    Font = TitleFont,
                    ForeColor = Color.White,
                    Visible = true,
                    Enabled = true,
                    BasicTooltipText = Resources.Edit_page_title,
                    HideBackground = true,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    PlaceholderText = this.Pages[0].Item1,
                };
                _editTitleTextBox.InputFocusChanged += (o, e) =>
                {
                    if (e.Value) return;
                    EditTitle(this.CurrentPageIndex, _editTitleTextBox.Text);
                };
            }
            _editTitleTextBox.Location = new Point((ContentRegion.Width - 400) / 2, TitleRegion.Y - TitleRegion.Height);
            _editTitleTextBox.Size = new Point(400, TitleRegion.Height);

            if (_editContentTextBox == null)
            {
                _editContentTextBox = new MultilineTextBox
                {
                    Parent = this,
                    Text = Pages[CurrentPageIndex].Item2,
                    MaxLength = MaxCharacterCountContent,
                    Size = SheetContentRegion.Size,
                    Location = new Point(SheetContentRegion.X, SheetContentRegion.Y - 30),
                    Font = TextFont,
                    ForeColor = Color.Black,
                    Visible = true,
                    Enabled = true,
                    HideBackground = true,
                    PlaceholderText = Resources.Write_something___
                };
                _editContentTextBox.InputFocusChanged += (o, e) =>
                {
                    if (e.Value) return;
                    EditContent(CurrentPageIndex, _editContentTextBox.Text);
                };
            }
            _editContentTextBox.Location = new Point(SheetContentRegion.X, SheetContentRegion.Y - 30);
            _editContentTextBox.Size = SheetContentRegion.Size;
        }

        protected override void OnLeftMouseButtonReleased(MouseEventArgs e)
        {
            base.OnLeftMouseButtonReleased(e);

            if (!_editingBookTitle)
            {
                if (_mouseOverLeftAddPageButton)
                    AddPage(this.CurrentPageIndex);
                else if (_mouseOverRightAddPageButton)
                    AddPage(this.CurrentPageIndex + 1);
                else if (_mouseOverRemovePageButton)
                    RemovePage(this.CurrentPageIndex);

                if (!_mouseOverEditBookTitle) return;
                // Show file renaming overlay. Disable other inputs.
                _editingBookTitle = true;
                _editTitleTextBox.Hide();
                _editContentTextBox.Enabled = false;
                _bookTitlePromptInputBox = new TextBox
                {
                    Parent = this,
                    Text = this.Title,
                    MaxLength = MaxCharacterCountTitle,
                    Size = new Point(400, this.TitleRegion.Height),
                    Location = new Point((ContentRegion.Width - 400) / 2, (Height - this.TitleRegion.Height) / 2),
                    Font = TitleFont,
                    ForeColor = Color.White,
                    Visible = true,
                    Enabled = true,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    PlaceholderText = this.Title
                };
                _bookTitlePromptInputBox.InputFocusChanged += (o, e) =>
                {
                    if (e.Value) return;
                    if (string.IsNullOrEmpty(_bookTitlePromptInputBox.Text)) return;
                    this.Title = _bookTitlePromptInputBox.Text;
                    _editTitleTextBox.Show();
                    _editContentTextBox.Enabled = true;
                    _bookTitlePromptInputBox.Dispose();
                    _editingBookTitle = false;
                    this.OnChanged?.Invoke(this, EventArgs.Empty);
                };
                GameService.Content.PlaySoundEffectByName("button-click");
            }
            else if (_mouseOverEditBookTitle)
            {
                GameService.Content.PlaySoundEffectByName("button-click");
                // Hide file renaming overlay. Enable other inputs.
                _editContentTextBox.Enabled = true;
                _editTitleTextBox.Show();
                _bookTitlePromptInputBox.Dispose();
                _editingBookTitle = false;
            }
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            /* Draw add/remove buttons */
            _rightAddPageButtonBounds = new Rectangle(RightTurnButtonBounds.Center.X - AddPageButtonTexture.Width / 2, RightTurnButtonBounds.Bottom + AddPageButtonTexture.Height, AddPageButtonTexture.Width, AddPageButtonTexture.Height);
            spriteBatch.DrawOnCtrl(this, AddPageButtonTexture, _rightAddPageButtonBounds, AddPageButtonTexture.Bounds, _mouseOverRightAddPageButton ? Color.White : new Color(200, 200, 200));
            _leftAddPageButtonBounds = new Rectangle(LeftTurnButtonBounds.Center.X, LeftTurnButtonBounds.Bottom + AddPageButtonTexture.Height, AddPageButtonTexture.Width, AddPageButtonTexture.Height);
            spriteBatch.DrawOnCtrl(this, AddPageButtonTexture, _leftAddPageButtonBounds, AddPageButtonTexture.Bounds, _mouseOverLeftAddPageButton ? Color.White : new Color(200, 200, 200));
            _removePageButtonBounds = new Rectangle(this.SheetBounds.Center.X - RemovePageButtonTexture.Width / 2, this.SheetBounds.Bottom - RemovePageButtonTexture.Height / 2, RemovePageButtonTexture.Width, RemovePageButtonTexture.Height);
            spriteBatch.DrawOnCtrl(this, RemovePageButtonTexture, _removePageButtonBounds, RemovePageButtonTexture.Bounds, _mouseOverRemovePageButton ? Color.White : new Color(200, 200, 200));

            if (_editingBookTitle)
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, bounds, Color.Black * 0.8f);

            var editBookTitleButtonTexture = !_editingBookTitle ? _mouseOverEditBookTitle ? Mouse.GetState().LeftButton == ButtonState.Pressed ? EditButtonTextureClick : EditButtonTextureHover : EditButtonTextureNormal : EditButtonTextureDisabled;
            _editTitleButtonBounds = new Rectangle(bounds.X - 5, bounds.Y - 10, editBookTitleButtonTexture.Width, editBookTitleButtonTexture.Height);
            spriteBatch.DrawOnCtrl(this, editBookTitleButtonTexture, _editTitleButtonBounds);

            AddOrUpdateEditTextBox();
        }

        private void Upload()
        {
            throw new NotImplementedException(); //TODO: Upload functionality
        }
    }
}
