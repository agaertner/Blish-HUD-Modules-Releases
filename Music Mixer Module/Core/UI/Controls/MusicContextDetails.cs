using System;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nekres.Music_Mixer.Core.UI.Models;

namespace Nekres.Music_Mixer.Core.UI.Controls
{
    internal class MusicContextDetails : DetailsButton
    {
        public event EventHandler<MouseEventArgs> EditClick;

        private const int MARGIN = 10;
        private const int BUTTON_WIDTH = 345;
        private const int BUTTON_HEIGHT = 100;
        private const int USER_WIDTH = 75;
        private const int BOTTOMSECTION_HEIGHT = 35;

        private static Texture2D _dividerSprite = GameService.Content.GetTexture("157218");
        private static Texture2D _iconBoxSprite = GameService.Content.GetTexture("controls/detailsbutton/605003");

        private static Texture2D _editMacroTex = MusicMixer.Instance.ContentsManager.GetTexture("155941.png");
        private static Texture2D _editMacroTexHover = MusicMixer.Instance.ContentsManager.GetTexture("155940.png");
        private static Texture2D _editMacroTexActive = MusicMixer.Instance.ContentsManager.GetTexture("155942.png");
        private static Texture2D _editMacroTexDisabled = MusicMixer.Instance.ContentsManager.GetTexture("155939.png");

        private bool _active;
        public bool Active
        {
            get => _active;
            set => SetProperty(ref _active, value);
        }

        private Rectangle _editButtonBounds;
        private bool _mouseOverEditButton;

        public MusicContextModel Model { get; }

        public MusicContextDetails(MusicContextModel model)
        {
            this.Title = model.Title;
            this.Model = model;
            this.Model.Changed += OnModelChanged;
        }

        private void OnModelChanged(object o, EventArgs e)
        {
            this.Title = this.Model.Title;
        }

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            var relPos = RelativeMousePosition;
            _mouseOverEditButton = _editButtonBounds.Contains(relPos);
            base.OnMouseMoved(e);
        }

        protected override void OnClick(MouseEventArgs e)
        {
            GameService.Content.PlaySoundEffectByName("button-click");
            if (_mouseOverEditButton && !this.Active) EditClick?.Invoke(this, e);
            base.OnClick(e);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            // Draw background
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, bounds, Color.Black * 0.25f);

            // Draw bottom section (overlap to make background darker here)
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(0, bounds.Height - BOTTOMSECTION_HEIGHT, bounds.Width - BOTTOMSECTION_HEIGHT, BOTTOMSECTION_HEIGHT), Color.Black * 0.1f);

            // Draw bottom section separator
            spriteBatch.DrawOnCtrl(this, _dividerSprite, new Rectangle(0, bounds.Height - 40, bounds.Width, 8), Color.White);

            var thumbnailBounds = new Rectangle(MARGIN, (bounds.Height - 36) / 2, 64, 36);
            if (this.Model.Thumbnail.HasTexture)
                spriteBatch.DrawOnCtrl(this, this.Model.Thumbnail, thumbnailBounds, Color.White);
            else
                LoadingSpinnerUtil.DrawLoadingSpinner(this, spriteBatch, new Rectangle(thumbnailBounds.Center.X - 16, thumbnailBounds.Center.Y - 16, 32, 32));

            // Draw icon box
            spriteBatch.DrawOnCtrl(this, _iconBoxSprite, thumbnailBounds, Color.White);

            // Draw edit button
            _editButtonBounds = new Rectangle(BUTTON_WIDTH - 66, (bounds.Height - BOTTOMSECTION_HEIGHT - 64) / 2, 64, 64);
            var editIcon = this.Active ? _editMacroTexActive : _mouseOverEditButton ? _editMacroTexHover : this.Enabled ? _editMacroTex : _editMacroTexDisabled;
            spriteBatch.DrawOnCtrl(this, editIcon, _editButtonBounds, Color.White);

            // Wrap text
            var titleBounds = new Rectangle(thumbnailBounds.Right + MARGIN, 0, bounds.Width - thumbnailBounds.Width - _editButtonBounds.Width - MARGIN * 2, this.Height - BOTTOMSECTION_HEIGHT);
            var wrappedText = DrawUtil.WrapText(Content.DefaultFont14, this.Title, titleBounds.Width);
            spriteBatch.DrawStringOnCtrl(this, wrappedText, Content.DefaultFont14, titleBounds, Color.White, false, true, 2);

            // Draw bottom text
            var bottomTextBounds = new Rectangle(titleBounds.X, bounds.Height - BOTTOMSECTION_HEIGHT, titleBounds.Width, BOTTOMSECTION_HEIGHT);
            if (string.IsNullOrEmpty(this.Model.Artist)) return;
            spriteBatch.DrawStringOnCtrl(this, this.Model.Artist, Content.DefaultFont12, bottomTextBounds, Color.LightGray, false, false, 0);

            // Draw duration
            if (TimeSpan.Zero.Equals(this.Model.Duration)) return;
            var durationBounds = new Rectangle(bounds.Width - 100, bounds.Height - BOTTOMSECTION_HEIGHT, 100, BOTTOMSECTION_HEIGHT);
            spriteBatch.DrawStringOnCtrl(this, this.Model.Duration.ToShortForm(), Content.DefaultFont12, durationBounds, Color.LightGray, false, false, 0, HorizontalAlignment.Center);
        }
    }
}
