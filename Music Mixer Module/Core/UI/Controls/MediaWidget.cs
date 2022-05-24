using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nekres.Music_Mixer.Core.UI.Models;
using System;
using Glide;

namespace Nekres.Music_Mixer.Core.UI.Controls
{
    internal class MediaWidget : Container
    {
        private const int MARGIN = 15;

        private Texture2D _texBg = MusicMixer.Instance.ContentsManager.GetTexture("1060353-gray.png");
        private Texture2D _texPause = MusicMixer.Instance.ContentsManager.GetTexture("media_pause.png");
        private Texture2D _texNextPrev = MusicMixer.Instance.ContentsManager.GetTexture("media_seek.png");

        private static Texture2D _boxSprite = GameService.Content.GetTexture("controls/detailsbutton/605003");

        private bool _mouseOverNext;
        private Rectangle _btnNextBounds;

        private bool _mouseOverPrev;
        private Rectangle _btnPrevBounds;

        private bool _mouseOverPause;
        private Rectangle _btnPauseBounds;

        private bool _mouseOverDragBar;
        private Rectangle _dragBounds;

        private bool _isDragging;
        private Point _dragPos;

        private MusicContextModel _model;
        public MusicContextModel Model
        {
            get => _model;
            set => SetProperty(ref _model, value);
        }

        private readonly TrackBar _valueTrackBar;

        public MediaWidget()
        {
            this.Size = new Point(387, 82);

            _valueTrackBar = new TrackBar
            {
                Location = new Point(64 + MARGIN * 2, this.Height - 16 - MARGIN),
                Size = new Point(this.Width - 64 - MARGIN * 3, 16),
                Parent = this
            };
            _valueTrackBar.ValueChanged += HandleTrackBarChanged;

            this.RefreshValue(MusicMixer.Instance.MasterVolumeSetting.Value);
            MusicMixer.Instance.MasterVolumeSetting.SettingChanged += OnMasterVolumeSettingChanged;
        }

        private void RefreshValue(float value)
        {
            _valueTrackBar.MinValue = Math.Min(_valueTrackBar.MinValue, value);
            _valueTrackBar.MaxValue = Math.Max(_valueTrackBar.MaxValue, value);

            _valueTrackBar.Value = value;
        }

        protected void HandleTrackBarChanged(object sender, ValueEventArgs<float> e)
        {
            MusicMixer.Instance.MasterVolumeSetting.Value = e.Value;
        }

        private void OnMasterVolumeSettingChanged(object o, ValueChangedEventArgs<float> e)
        {
            this.RefreshValue(e.NewValue);
        }

        protected override void DisposeControl()
        {
            MusicMixer.Instance.MasterVolumeSetting.SettingChanged -= OnMasterVolumeSettingChanged;
            _valueTrackBar.ValueChanged -= HandleTrackBarChanged;
            _valueTrackBar.Dispose();
            base.DisposeControl();
        }

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            var relPos = RelativeMousePosition;
            _mouseOverNext = _btnNextBounds.Contains(relPos);
            _mouseOverPause = _btnPauseBounds.Contains(relPos);
            _mouseOverPrev = _btnPrevBounds.Contains(relPos);
            _mouseOverDragBar = _dragBounds.Contains(relPos);
            base.OnMouseMoved(e);
        }

        protected override void OnLeftMouseButtonPressed(MouseEventArgs e)
        {
            if (_mouseOverDragBar)
            {
                _dragPos = GameService.Input.Mouse.Position;
                _isDragging = true;
            }

            base.OnLeftMouseButtonPressed(e);
        }

        protected override void OnLeftMouseButtonReleased(MouseEventArgs e)
        {
            if (_isDragging)
            {
                MusicMixer.Instance.MediaWidgetLocation.Value = this.Location;
            }
            _isDragging = false;
            base.OnLeftMouseButtonReleased(e);
        }

        protected override void OnClick(MouseEventArgs e)
        {
            if (_mouseOverNext)
            {

            } else if (_mouseOverPause)
            {

            } else if (_mouseOverPrev)
            {

            }
            base.OnClick(e);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (_isDragging) {
                var nOffset = Input.Mouse.Position - _dragPos;
                this.Location += nOffset;
                _dragPos = Input.Mouse.Position;
            }
            _dragBounds = new Rectangle(0, 0, bounds.Width, 20);

            // Draw background
            spriteBatch.DrawOnCtrl(this, _texBg, bounds, _texBg.Bounds, Color.White);

            // Draw thumbnail
            var thumbnailBounds = new Rectangle(MARGIN, (bounds.Height - 36) / 2, 64, 36);
            if (this.Model.Thumbnail.HasTexture)
                spriteBatch.DrawOnCtrl(this, this.Model.Thumbnail, thumbnailBounds, Color.White);
            else
                LoadingSpinnerUtil.DrawLoadingSpinner(this, spriteBatch, new Rectangle(thumbnailBounds.Center.X - 16, thumbnailBounds.Center.Y - 16, 32, 32));

            // Draw icon box
            spriteBatch.DrawOnCtrl(this, _boxSprite, thumbnailBounds, Color.White);

            // Draw title
            var titleBounds = new Rectangle(thumbnailBounds.Right + MARGIN, 0, bounds.Width - thumbnailBounds.Width - 100 - MARGIN * 2, bounds.Height / 2 - 10);
            spriteBatch.DrawStringOnCtrl(this, this.Model.Title, Content.DefaultFont14, titleBounds, Color.White, false, true, 2, HorizontalAlignment.Left, VerticalAlignment.Bottom);

            // Draw artist
            var bottomTextBounds = new Rectangle(titleBounds.X,  titleBounds.Bottom, titleBounds.Width, bounds.Height / 2);
            if (string.IsNullOrEmpty(this.Model.Artist)) return;
            spriteBatch.DrawStringOnCtrl(this, this.Model.Artist, Content.DefaultFont12, bottomTextBounds, Color.LightGray, false, false, 0, HorizontalAlignment.Left, VerticalAlignment.Top);

            // Draw duration
            if (TimeSpan.Zero.Equals(this.Model.Duration)) return;
            var durationBounds = new Rectangle(bounds.Width - 100, 0, 100, bounds.Height);
            spriteBatch.DrawStringOnCtrl(this, this.Model.Duration.ToShortForm(), Content.DefaultFont12, durationBounds, Color.LightGray, false, false, 0, HorizontalAlignment.Center);
            
            base.PaintBeforeChildren(spriteBatch, bounds);
        }
    }
}
