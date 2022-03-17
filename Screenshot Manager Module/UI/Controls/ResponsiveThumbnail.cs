using System;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Blish_HUD;
using Blish_HUD.Input;
using Nekres.Screenshot_Manager.Properties;

namespace Nekres.Screenshot_Manager.UI.Controls
{
    public class ResponsiveThumbnail : ThumbnailBase
    {
        public event EventHandler<EventArgs> OnInspect;
        public event EventHandler<ValueEventArgs<bool>> FavoriteChanged;
        public event EventHandler<EventArgs> OnDelete; 

        private Rectangle _nameTextBoxBounds;
        private TextBox _nameTextBox;

        private const int MaxFileNameLength = 50;

        private readonly IEnumerable<char> _invalidFileNameCharacters;

        private static Texture2D _completeHeartIcon = ScreenshotManagerModule.ModuleInstance.ContentsManager.GetTexture("complete_heart.png");
        private static Texture2D _incompleteHeartIcon = ScreenshotManagerModule.ModuleInstance.ContentsManager.GetTexture("incomplete_heart.png");

        private static Texture2D _trashcanClosedIcon64 = ScreenshotManagerModule.ModuleInstance.ContentsManager.GetTexture("trashcanClosed_icon_64x64.png");
        private static Texture2D _trashcanOpenIcon64 = ScreenshotManagerModule.ModuleInstance.ContentsManager.GetTexture("trashcanOpen_icon_64x64.png");
        //private static Texture2D _trashcanClosedIcon128 = ScreenshotManagerModule.ModuleInstance.ContentsManager.GetTexture("trashcanClosed_icon_128x128.png");
        //private static Texture2D _trashcanOpenIcon128 = ScreenshotManagerModule.ModuleInstance.ContentsManager.GetTexture("trashcanOpen_icon_128x128.png");

        private static Texture2D _inspectIcon = ScreenshotManagerModule.ModuleInstance.ContentsManager.GetTexture("inspect.png");

        private bool _mouseOverFavButton;
        private Rectangle _favButtonBounds;

        private bool _mouseOverDelButton;
        private Rectangle _delButtonBounds;

        private bool _mouseOverInspect;
        private Rectangle _inspectButtonBounds;

        private bool _isFavorite;
        public bool IsFavorite
        {
            get => _isFavorite;
            set
            {
                FavoriteChanged?.Invoke(this, new ValueEventArgs<bool>(value));
                SetProperty(ref _isFavorite, value);
            }
        }

        public ResponsiveThumbnail(AsyncTexture2D texture, string fileName) : base(texture, fileName)
        {
            _invalidFileNameCharacters = Path.GetInvalidFileNameChars().Union(Path.GetInvalidPathChars());
        }

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            var relPos = RelativeMousePosition;
            _mouseOverFavButton = _favButtonBounds.Contains(relPos);
            _mouseOverDelButton = _delButtonBounds.Contains(relPos);
            _mouseOverInspect = _inspectButtonBounds.Contains(relPos);

            if (_mouseOverFavButton)
                this.BasicTooltipText = this.IsFavorite ? Resources.Unfavourite : Resources.Favourite;
            else if (_mouseOverDelButton)
                this.BasicTooltipText = Resources.Delete_Image_;
            else if (_mouseOverInspect)
                this.BasicTooltipText = Resources.Click_To_Zoom;
            else
                this.BasicTooltipText = string.Empty;

            base.OnMouseMoved(e);
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            if (_mouseOverInspect)
            {
                this.OnInspect?.Invoke(this, EventArgs.Empty);
                return;
            }

            if (_mouseOverFavButton)
                this.IsFavorite = !IsFavorite;

            if (_mouseOverDelButton) 
                this.OnDelete?.Invoke(this, EventArgs.Empty);
        }

        private void CreateNameTextBox()
        {
            if (_nameTextBox != null) return;
            _nameTextBox = new TextBox
            {
                Parent = this,
                MaxLength = MaxFileNameLength,
                Size = _nameTextBoxBounds.Size,
                Location = _nameTextBoxBounds.Location,
                Text = Path.GetFileNameWithoutExtension(this.FileName),
                BasicTooltipText = Resources.Rename_Image
            };
            _nameTextBox.InputFocusChanged += async (o, e) =>
            {
                if (e.Value || _nameTextBox.Text.Equals(Path.GetFileNameWithoutExtension(this.FileName))) return;

                if (string.IsNullOrEmpty(_nameTextBox.Text))
                {
                    ScreenNotification.ShowNotification(Resources.Image_name_cannot_be_empty_, ScreenNotification.NotificationType.Error);
                    _nameTextBox.Text = Path.GetFileNameWithoutExtension(this.FileName);
                    return;
                }

                if (_nameTextBox.Text.Length > MaxFileNameLength)
                {
                    ScreenNotification.ShowNotification(Resources.Please_enter_a_different_image_name_, ScreenNotification.NotificationType.Error);
                    _nameTextBox.Text = Path.GetFileNameWithoutExtension(this.FileName);
                    return;
                }

                if (_nameTextBox.Text.Any(x => _invalidFileNameCharacters.Any(y => y.Equals(x))))
                {
                    ScreenNotification.ShowNotification(Resources.The_image_name_contains_invalid_characters_, ScreenNotification.NotificationType.Error);
                    _nameTextBox.Text = Path.GetFileNameWithoutExtension(this.FileName);
                    return;
                }

                var ext = Path.GetExtension(this.FileName);
                var path = Path.GetDirectoryName(this.FileName);
                if (path == null) return;
                var newName = Path.Combine(path, _nameTextBox.Text + ext);

                if (File.Exists(newName))
                {
                    ScreenNotification.ShowNotification(Resources.A_duplicate_image_name_was_specified_, ScreenNotification.NotificationType.Error);
                    _nameTextBox.Text = Path.GetFileNameWithoutExtension(this.FileName);
                    return;
                }

                if (!await FileUtil.MoveAsync(this.FileName, newName))
                {
                    ScreenNotification.ShowNotification(string.Format(Resources.Unable_to_rename_image__0__, $"\u201c{Path.GetFileNameWithoutExtension(this.FileName)}\u201d"), ScreenNotification.NotificationType.Error);
                    _nameTextBox.Text = Path.GetFileNameWithoutExtension(this.FileName);
                    return;
                }

                this.FileName = newName;
            };
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            if (_mouseOverInspect)
            {
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, bounds, Color.Black  * 0.8f);
            }

            _inspectButtonBounds = new Rectangle((this.Width - _inspectIcon.Width / 2) / 2, (this.Height - _inspectIcon.Height / 2) / 2, _inspectIcon.Width / 2, _inspectIcon.Height / 2);
            spriteBatch.DrawOnCtrl(this, _inspectIcon, _inspectButtonBounds, Color.White * (_mouseOverInspect ? 1.0f : 0.25f));

            var delTexture = _mouseOverDelButton ? _trashcanOpenIcon64 : _trashcanClosedIcon64;

            _nameTextBoxBounds = new Rectangle(0, this.Height - 30, this.Width - delTexture.Width / 2 - 5, 30);

            var favTexture = IsFavorite ? _completeHeartIcon : _incompleteHeartIcon;
            _favButtonBounds = new Rectangle(this.Width - favTexture.Width - 10, 5, favTexture.Width, favTexture.Height);
            spriteBatch.DrawOnCtrl(this, favTexture, _favButtonBounds);

            _delButtonBounds = new Rectangle(this.Width - delTexture.Width / 2 - 8, this.Height - delTexture.Height / 2, delTexture.Width / 2, delTexture.Height / 2);
            spriteBatch.DrawOnCtrl(this, delTexture, _delButtonBounds);

            CreateNameTextBox();
        }
    }
}
