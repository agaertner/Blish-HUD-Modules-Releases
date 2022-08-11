using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nekres.Music_Mixer.Core.UI.Models;
using System;
using SpriteBatch = Microsoft.Xna.Framework.Graphics.SpriteBatch;

namespace Nekres.Music_Mixer.Core.UI.Controls
{
    internal class MusicContextDetails : DetailsButton
    {
        public event EventHandler<MouseEventArgs> EditClick;

        private const int MARGIN = 10;
        private const int BOTTOMSECTION_HEIGHT = 35;

        //private static Texture2D _dividerSprite = GameService.Content.GetTexture("157218");
        private static Texture2D _iconBoxSprite = GameService.Content.GetTexture("controls/detailsbutton/605003");

        private static Texture2D _editMacroTex = MusicMixer.Instance.ContentsManager.GetTexture("155941.png");
        private static Texture2D _editMacroTexHover = MusicMixer.Instance.ContentsManager.GetTexture("155940.png");
        private static Texture2D _editMacroTexActive = MusicMixer.Instance.ContentsManager.GetTexture("155942.png");
        private static Texture2D _editMacroTexDisabled = MusicMixer.Instance.ContentsManager.GetTexture("155939.png");
        private static Texture2D _trashCanClosed = MusicMixer.Instance.ContentsManager.GetTexture("trashcanClosed_icon_64x64.png");
        private static Texture2D _trashCanOpen = MusicMixer.Instance.ContentsManager.GetTexture("trashcanOpen_icon_64x64.png");

        private bool _active;
        public bool Active
        {
            get => _active;
            set => SetProperty(ref _active, value);
        }

        private bool _editable;
        public bool Editable
        {
            get => _editable;
            set => SetProperty(ref _editable, value);
        }

        private bool _deletable;
        public bool Deletable
        {
            get => _deletable;
            set => SetProperty(ref _deletable, value);
        }

        private Rectangle _editButtonBounds;
        private bool _mouseOverEditButton;

        private Rectangle _delButtonBounds;
        private bool _mouseOverDelButton;

        public MusicContextModel Model { get; }

        public MusicContextDetails(MusicContextModel model, bool editable = true, bool deletable = true)
        {
            _editable = editable;
            _deletable = deletable;
            this.Title = model.Title;
            this.Model = model;
        }

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            base.OnMouseMoved(e);
            var relPos = RelativeMousePosition;
            if (this.Editable)
            {
                _mouseOverEditButton = _editButtonBounds.Contains(relPos);
            }
            if (this.Deletable)
            {
                _mouseOverDelButton = _delButtonBounds.Contains(relPos);
            }
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);
            if (this.Editable) {
                GameService.Content.PlaySoundEffectByName("button-click");
                if (_mouseOverEditButton && !this.Active) EditClick?.Invoke(this, e);
            };
            if (this.Deletable && _mouseOverDelButton)
            {
                this.Model.Delete();
                this.Dispose();
            }
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            // Draw background
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, bounds, Color.Black * 0.25f);

            // Draw bottom section (overlap to make background darker here)
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(0, bounds.Height - BOTTOMSECTION_HEIGHT, bounds.Width - BOTTOMSECTION_HEIGHT, BOTTOMSECTION_HEIGHT), Color.Black * 0.1f);

            // Draw bottom section separator
            //spriteBatch.DrawOnCtrl(this, _dividerSprite, new Rectangle(0, bounds.Height - 40, bounds.Width, 8), Color.White);

            var thumbnailBounds = new Rectangle(MARGIN, (bounds.Height - 36) / 2, 64, 36);
            if (this.Model.Thumbnail.HasTexture)
                spriteBatch.DrawOnCtrl(this, this.Model.Thumbnail, thumbnailBounds, Color.White);
            else
                LoadingSpinnerUtil.DrawLoadingSpinner(this, spriteBatch, new Rectangle(thumbnailBounds.Center.X - 16, thumbnailBounds.Center.Y - 16, 32, 32));

            // Draw icon box
            spriteBatch.DrawOnCtrl(this, _iconBoxSprite, thumbnailBounds, Color.White);

            // Draw edit button
            _editButtonBounds = new Rectangle(this.Width - 66, (bounds.Height - BOTTOMSECTION_HEIGHT - 64) / 2, 64, 64);
            if (this.Editable)
            {
                var editIcon = this.Active ? _editMacroTexActive : _mouseOverEditButton ? _editMacroTexHover : this.Enabled ? _editMacroTex : _editMacroTexDisabled;
                spriteBatch.DrawOnCtrl(this, editIcon, _editButtonBounds, Color.White);
            }

            // Wrap text
            var titleBounds = new Rectangle(thumbnailBounds.Right + MARGIN, 0, bounds.Width - thumbnailBounds.Width - _editButtonBounds.Width - MARGIN * 2, this.Height - BOTTOMSECTION_HEIGHT);
            var wrappedText = DrawUtil.WrapText(Content.DefaultFont14, this.Title, titleBounds.Width);
            spriteBatch.DrawStringOnCtrl(this, wrappedText, Content.DefaultFont14, titleBounds, Color.White, false, true, 2);

            // Draw bottom text
            var bottomTextBounds = new Rectangle(titleBounds.X, bounds.Height - BOTTOMSECTION_HEIGHT, titleBounds.Width, BOTTOMSECTION_HEIGHT);
            if (!string.IsNullOrEmpty(this.Model.Artist))
            {
                spriteBatch.DrawStringOnCtrl(this, this.Model.Artist, Content.DefaultFont12, bottomTextBounds, Color.LightGray, false, false, 0);
            };

            // Draw delete button
            _delButtonBounds = new Rectangle(bounds.Width - 28, bounds.Height - BOTTOMSECTION_HEIGHT + 3, 28, 28);
            if (this.Deletable)
            {
                if (_mouseOverDelButton)
                    spriteBatch.DrawOnCtrl(this, _trashCanOpen, _delButtonBounds, Color.White);
                else
                    spriteBatch.DrawOnCtrl(this, _trashCanClosed, _delButtonBounds, Color.White);
            }

            // Draw duration
            if (TimeSpan.Zero.Equals(this.Model.Duration)) return;
            var durationBounds = new Rectangle(bounds.Width - 100, bounds.Height - BOTTOMSECTION_HEIGHT, 100, BOTTOMSECTION_HEIGHT);
            spriteBatch.DrawStringOnCtrl(this, this.Model.Duration.ToShortForm(), Content.DefaultFont12, durationBounds, Color.LightGray, false, false, 0, HorizontalAlignment.Center);
        }
    }
}
