using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Nekres.Music_Mixer.Core.Player;

namespace Nekres.Music_Mixer.Core.UI.Controls
{
    internal sealed class AudioVolumeWidget : Container
    {
        private const int MARGIN = 10;
        private Texture2D _texAudioBtn = GameService.Content.GetTexture("156738");
        private Texture2D _texAudioMuted = GameService.Content.GetTexture("common/154982");
        private Texture2D _texTooltip = GameService.Content.GetTexture("tooltip");

        private TrackBar _volumeTrackBar;

        private bool _mouseOverAudioBtn;
        private Rectangle _audioBtnBounds;

        private MediaWidget _mediaWidget;
        private Soundtrack Soundtrack => _mediaWidget.Soundtrack;

        private Rectangle _activeBounds;

        public AudioVolumeWidget(MediaWidget widget)
        {
            _mediaWidget = widget;
            this.Size = new Point(250, 64);
            _volumeTrackBar = new TrackBar
            {
                Parent = this,
                Location = new Point(64, (this.Height - 16) / 2),
                Size = new Point(this.Width - _texAudioBtn.Width - 64 - MARGIN * 2, 16),
                Visible = true
            };
            _volumeTrackBar.ValueChanged += OnValueChanged;
            _activeBounds = _mediaWidget.AbsoluteBounds.Add(this.AbsoluteBounds);
            this.ZIndex = 99999;
        }

        private void OnValueChanged(object o, ValueEventArgs<float> e)
        {
            MusicMixer.Instance.MasterVolumeSetting.Value = e.Value;
        }

        private void OnMasterVolumeSettingChanged(object o, ValueChangedEventArgs<float> e)
        {
            this.RefreshValue(e.NewValue);
        }

        private void RefreshValue(float value)
        {
            _volumeTrackBar.MinValue = Math.Min(_volumeTrackBar.MinValue, value);
            _volumeTrackBar.MaxValue = Math.Max(_volumeTrackBar.MaxValue, value);

            _volumeTrackBar.Value = value;
        }

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            var relPos = RelativeMousePosition;
            _mouseOverAudioBtn = _audioBtnBounds.Contains(relPos);
            if (_mouseOverAudioBtn)
            {
                this.BasicTooltipText = "Mute";
            }
            else
            {
                this.BasicTooltipText = string.Empty;
            }
            base.OnMouseMoved(e);
        }

        protected override void OnClick(MouseEventArgs e)
        {
            if (_mouseOverAudioBtn)
            {
                this.Soundtrack?.ToggleMuted();
            }
            base.OnClick(e);
        }

        protected override void DisposeControl()
        {
            _volumeTrackBar.ValueChanged -= OnValueChanged;
            MusicMixer.Instance.MasterVolumeSetting.SettingChanged -= OnMasterVolumeSettingChanged;
            _volumeTrackBar.Dispose();
            base.DisposeControl();
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (!_activeBounds.Contains(GameService.Graphics.SpriteScreen.RelativeMousePosition))
            {
                this.Dispose();
                return;
            }

            spriteBatch.DrawOnCtrl(this, _texTooltip, bounds, _texTooltip.Bounds, Color.White);
            // Draw audio button
            _audioBtnBounds = new Rectangle(MARGIN, (bounds.Height - _texAudioBtn.Height) / 2, _texAudioBtn.Width, _texAudioBtn.Height);
            spriteBatch.DrawOnCtrl(this, _texAudioBtn, _audioBtnBounds, _texAudioBtn.Bounds, Color.White);
            if (this.Soundtrack.IsMuted)
                spriteBatch.DrawOnCtrl(this, _texAudioMuted, _audioBtnBounds, _texAudioMuted.Bounds, Color.White * 0.5f);

            var volumeBounds = new Rectangle(_volumeTrackBar.Right, 0, 50, bounds.Height);
            spriteBatch.DrawStringOnCtrl(this, $"{Math.Ceiling(this.Soundtrack.Volume * 1000)}", Content.DefaultFont18, volumeBounds, Color.White, false, false, 1, HorizontalAlignment.Center);
            
            // Draw border
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(0,0,bounds.Width, 1), Color.Black);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(0, 0, 1, bounds.Height), Color.Black);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Width - 1, 0, 1, bounds.Height), Color.Black);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(0, bounds.Height - 1, bounds.Width, 1), Color.Black);
            base.PaintBeforeChildren(spriteBatch, bounds);
        }
    }
}
