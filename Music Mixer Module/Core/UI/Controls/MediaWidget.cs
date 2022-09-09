using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nekres.Music_Mixer.Core.Player;
using Nekres.Music_Mixer.Core.UI.Models;
using System.Linq;

namespace Nekres.Music_Mixer.Core.UI.Controls
{
    internal class MediaWidget : Container
    {
        private const int MARGIN = 15;

        private Texture2D _texBg = MusicMixer.Instance.ContentsManager.GetTexture("1060353-gray.png");
        private Texture2D _texPause = MusicMixer.Instance.ContentsManager.GetTexture("media_pause.png");
        private Texture2D _texNextPrev = MusicMixer.Instance.ContentsManager.GetTexture("media_seek.png");
        private Texture2D _texAudioBtn = GameService.Content.GetTexture("156738");
        private Texture2D _texAudioMuted = GameService.Content.GetTexture("common/154982");
        private Texture2D _texClose = GameService.Content.GetTexture("button-exit");
        private Texture2D _texCloseActive = GameService.Content.GetTexture("button-exit-active");
        private Texture2D _boxSprite = GameService.Content.GetTexture("controls/detailsbutton/605003");

        /*
        private bool _mouseOverNext;
        private Rectangle _btnNextBounds;

        private bool _mouseOverPrev;
        private Rectangle _btnPrevBounds;

        private bool _mouseOverPause;
        private Rectangle _btnPauseBounds;
        */

        private bool _mouseOverCloseBtn;
        private Rectangle _closeBounds;

        private bool _mouseOverAudioBtn;
        private Rectangle _audioBtnBounds;

        private bool _mouseOverTitle;
        private Rectangle _titleBounds;

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

        private Soundtrack _soundtrack;
        public Soundtrack Soundtrack {
            get => _soundtrack;
            set
            {
                SetProperty(ref _soundtrack, value);
                if (value == null) return;
                _seekTrackBar.MaxValue = (float)value.TotalTime.TotalSeconds;
                _seekTrackBar.MinValue = 0;
            }
        }

        private readonly TrackBar2 _seekTrackBar;

        private AudioVolumeWidget _volumeWidget;

        public MediaWidget()
        {
            this.Size = new Point(387, 82);

            _seekTrackBar = new TrackBar2
            {
                Location = new Point(64 + MARGIN * 2, this.Height - 16 - MARGIN),
                Size = new Point(this.Width - 64 - MARGIN * 4 - _texAudioBtn.Width, 16),
                Parent = this
            };
            _seekTrackBar.DraggingStopped += OnDraggingStopped;
        }

        public void Change(MusicContextModel model, Soundtrack track)
        {
            this.Model = model;
            this.Soundtrack = track;
            MusicMixer.Instance.DataService.GetThumbnail(model);
        }

        private void OnDraggingStopped(object o, ValueEventArgs<float> e)
        {
            _soundtrack?.Seek(e.Value);
        }

        protected override void DisposeControl()
        {
            _volumeWidget?.Dispose();
            _seekTrackBar?.Dispose();
            _texBg?.Dispose();
            _texPause?.Dispose();
            _texNextPrev?.Dispose();
            _texAudioBtn?.Dispose();
            _texAudioMuted?.Dispose();
            _texClose?.Dispose();
            _texCloseActive?.Dispose();
            _boxSprite?.Dispose();
            base.DisposeControl();
        }

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            var relPos = RelativeMousePosition;
            _mouseOverDragBar = _dragBounds.Contains(relPos);
            _mouseOverAudioBtn = _audioBtnBounds.Contains(relPos);
            _mouseOverTitle = _titleBounds.Contains(relPos);
            _mouseOverCloseBtn = _closeBounds.Contains(relPos);

            if (_mouseOverAudioBtn)
            {
                this.BasicTooltipText = "Volume";
            }
            else if (_mouseOverTitle)
            {
                this.BasicTooltipText = this.Model.Title;
            }
            else if (_mouseOverCloseBtn)
            {
                this.BasicTooltipText = "Hide (Right-Click module icon to show.)";
            }
            else
            {
                this.BasicTooltipText = string.Empty;
            }
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
            if (_mouseOverAudioBtn)
            {
                _volumeWidget?.Dispose();
                _volumeWidget = null;
                _volumeWidget = new AudioVolumeWidget(this)
                {
                    Parent = GameService.Graphics.SpriteScreen,
                    Location = new Point(this.Right, this.Top)
                };
            } 
            else if (_mouseOverCloseBtn)
            {
                this.Hide();
            }
            base.OnClick(e);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            // Draw background
            spriteBatch.DrawOnCtrl(this, _texBg, bounds, _texBg.Bounds, Color.White);

            if (_isDragging) {
                var nOffset = Input.Mouse.Position - _dragPos;
                this.Location += nOffset;
                _dragPos = Input.Mouse.Position;
            }
            _dragBounds = new Rectangle(0, 0, bounds.Width, 20);

            // Draw close button
            _closeBounds = new Rectangle(bounds.Width - _texClose.Width - 2, 1, _texClose.Width, _texClose.Height);
            spriteBatch.DrawOnCtrl(this, _mouseOverCloseBtn ? _texCloseActive : _texClose, _closeBounds);

            // Draw thumbnail
            var thumbnailBounds = new Rectangle(MARGIN, (bounds.Height - 36) / 2, 64, 36);
            if (this.Model.Thumbnail.HasTexture)
                spriteBatch.DrawOnCtrl(this, this.Model.Thumbnail, thumbnailBounds, Color.White);
            else
                LoadingSpinnerUtil.DrawLoadingSpinner(this, spriteBatch, new Rectangle(thumbnailBounds.Center.X - 16, thumbnailBounds.Center.Y - 16, 32, 32));

            // Draw icon box
            spriteBatch.DrawOnCtrl(this, _boxSprite, thumbnailBounds, Color.White);

            // Draw title
            var title = this.Model.Title;
            if (title.Length > 33)
                title = new string(title.Take(33).ToArray()) + "...";
            _titleBounds = new Rectangle(thumbnailBounds.Right + MARGIN, 0, bounds.Width - thumbnailBounds.Width - 100 - MARGIN * 2, bounds.Height / 2 - 10);
            spriteBatch.DrawStringOnCtrl(this, title, Content.DefaultFont14, _titleBounds, Color.White, false, true, 2, HorizontalAlignment.Left, VerticalAlignment.Bottom);

            // Draw artist
            var bottomTextBounds = new Rectangle(_titleBounds.X, _titleBounds.Bottom, _titleBounds.Width, bounds.Height / 2);
            if (string.IsNullOrEmpty(this.Model.Artist)) return;
            spriteBatch.DrawStringOnCtrl(this, this.Model.Artist, Content.DefaultFont12, bottomTextBounds, Color.LightGray, false, false, 0, HorizontalAlignment.Left, VerticalAlignment.Top);

            // Draw audio button
            _audioBtnBounds = new Rectangle(_seekTrackBar.Right + MARGIN, _seekTrackBar.Location.Y - _texAudioBtn.Height / 4, _texAudioBtn.Width, _texAudioBtn.Height);
            spriteBatch.DrawOnCtrl(this, _texAudioBtn, _audioBtnBounds, _texAudioBtn.Bounds, Color.White);
            if (this.Soundtrack != null)
            {
                if (this.Soundtrack.Muted) spriteBatch.DrawOnCtrl(this, _texAudioMuted, _audioBtnBounds, _texAudioMuted.Bounds, Color.White * 0.5f);
                if (!_seekTrackBar.Dragging) _seekTrackBar.Value = (float)_soundtrack.CurrentTime.TotalSeconds;

                // Draw duration
                var durationBounds = new Rectangle(_seekTrackBar.Right - 100, _seekTrackBar.Top - 16, 100, bounds.Height);
                var text = $"{this.Soundtrack.CurrentTime.ToShortForm()} / {this.Soundtrack.TotalTime.ToShortForm()}";
                spriteBatch.DrawStringOnCtrl(this, text, Content.DefaultFont12, durationBounds, Color.LightGray, false, false, 0, HorizontalAlignment.Right, VerticalAlignment.Top);
            }

            if (MusicMixer.Instance.AudioEngine.Loading || MusicMixer.Instance.AudioEngine.IsBuffering)
            {
                var spinnerBounds = new Rectangle((bounds.Width - 64) / 2, (bounds.Height - 64) / 2, 64, 64);
                LoadingSpinnerUtil.DrawLoadingSpinner(this, spriteBatch, spinnerBounds);
            }

            base.PaintBeforeChildren(spriteBatch, bounds);
        }
    }
}
