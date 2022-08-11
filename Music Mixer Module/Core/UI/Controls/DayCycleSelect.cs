using System;
using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nekres.Music_Mixer.Core.UI.Controls
{
    internal class DayCycleSelect : Control
    {
        private int _borderSize = 10;

        private Texture2D _texture;

        private Color _textColor = new Color(238, 221, 171);
        public Color TextColor
        {
            get => _textColor;
            set => SetProperty(ref _textColor, value);
        }

        private bool _active;
        public bool Active
        {
            get => _active;
            set
            {
                if (!SetProperty(ref _active, value)) return;
                _grayscaleEffect?.Parameters["Intensity"].SetValue(value ? 0f : 1f);
            }

        }

        public readonly TyrianTime DayCycle;

        private Effect _grayscaleEffect;

        private SpriteBatchParameters _defaultSpriteBatchParameters;

        public DayCycleSelect(TyrianTime dayCycle)
        {
            this.DayCycle = dayCycle;
            switch (dayCycle)
            {
                case TyrianTime.Dawn:
                    _texture = MusicMixer.Instance.ContentsManager.GetTexture("regions/skybox/197504_cropped.png");
                    break;
                case TyrianTime.Day:
                    _texture = MusicMixer.Instance.ContentsManager.GetTexture("regions/skybox/193070_cropped.png");
                    break;
                case TyrianTime.Dusk:
                    _texture = MusicMixer.Instance.ContentsManager.GetTexture("regions/skybox/276493_cropped.png");
                    break;
                case TyrianTime.Night:
                    _texture = MusicMixer.Instance.ContentsManager.GetTexture("regions/skybox/198036_cropped.png");
                    break;
                default:
                    this.DayCycle = TyrianTime.Day;
                    _texture = MusicMixer.Instance.ContentsManager.GetTexture("regions/skybox/193070_cropped.png");
                    break;
            }

            _grayscaleEffect = MusicMixer.Instance.ContentsManager.GetEffect<Effect>(@"effects\grayscale.mgfx");
            _grayscaleEffect.Parameters["Intensity"].SetValue(_active ? 0f : 1f);
            _spriteBatchParameters = new SpriteBatchParameters
            {
                Effect = _grayscaleEffect
            };
            _defaultSpriteBatchParameters = new SpriteBatchParameters();
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            var centerX = this.Width > _texture.Width ? (this.Width - _texture.Width) / 2 + _borderSize : _borderSize;

            var sourceRect = new Rectangle(
                MathHelper.Clamp(_texture.Width / 2 - Math.Abs(_texture.Width / 2 - this.Width), 0, _texture.Width / 2),
                0,
                MathHelper.Clamp(_texture.Width / 2 - Math.Abs(_texture.Width / 2 - this.Width) - _borderSize * 2, 0, _texture.Width / 2 - _borderSize * 2),
                _texture.Height - _borderSize * 2);

            var imageBounds = new Rectangle(
                centerX, 
                _borderSize,
                sourceRect.Width,
                sourceRect.Height);

            spriteBatch.DrawOnCtrl(this, _texture, imageBounds, sourceRect);
            
            // Exclude everything below from grayscale effect.
            spriteBatch.End();
            spriteBatch.Begin(_defaultSpriteBatchParameters);

            spriteBatch.DrawBorderOnCtrl(this, imageBounds, _borderSize);
            spriteBatch.DrawStringOnCtrl(this, this.DayCycle.ToString(), Content.DefaultFont32, new Rectangle(0, 0, this.Width, this.Height), _textColor, false, true, 1, HorizontalAlignment.Center);
            
            spriteBatch.End();
            spriteBatch.Begin(_spriteBatchParameters);
        }

        protected override void DisposeControl()
        {
            _texture?.Dispose();
            base.DisposeControl();
        }
    }
}
