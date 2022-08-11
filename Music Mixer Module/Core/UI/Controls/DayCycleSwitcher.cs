using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Nekres.Music_Mixer.Core.UI.Controls
{
    internal class DayCycleSwitcher : Control
    {
        private int _borderSize = 10;

        private Dictionary<TyrianTime, Texture2D> _cycleTextures;

        private Color _textColor = new Color(238, 221, 171);
        public Color TextColor
        {
            get => _textColor;
            set => SetProperty(ref _textColor, value);
        }

        private int _dayCycle;
        public TyrianTime DayCycle
        {
            get => (TyrianTime)_dayCycle;
            set
            {
                if ((int)value < 1 || (int)value > 4) return;
                _dayCycle = (int)value;
            }
        }

        public DayCycleSwitcher()
        {
            _cycleTextures = new Dictionary<TyrianTime, Texture2D>
            {
                { TyrianTime.Dawn, MusicMixer.Instance.ContentsManager.GetTexture("regions/skybox/197504_cropped.png")},
                { TyrianTime.Day, MusicMixer.Instance.ContentsManager.GetTexture("regions/skybox/193070_cropped.png")},
                { TyrianTime.Dusk, MusicMixer.Instance.ContentsManager.GetTexture("regions/skybox/276493_cropped.png")},
                { TyrianTime.Night, MusicMixer.Instance.ContentsManager.GetTexture("regions/skybox/198036_cropped.png")}
            };
            _dayCycle = 1;
        }

        public TyrianTime NextCycle()
        {
            _dayCycle++;
            if (_dayCycle > 4)
            {
                _dayCycle = 1;
            }
            return this.DayCycle;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            var tex = _cycleTextures[(TyrianTime) _dayCycle];
            var centerX = this.Width > tex.Width ? (this.Width - tex.Width) / 2 + _borderSize : _borderSize;
            var imageBounds = new Rectangle(centerX, _borderSize, tex.Width - _borderSize * 2, this.Height - _borderSize * 2);
            spriteBatch.DrawOnCtrl(this, tex, imageBounds, new Rectangle(0, 0, tex.Width - _borderSize * 2, tex.Height - _borderSize * 2));
            spriteBatch.DrawBorderOnCtrl(this, imageBounds, _borderSize);
            spriteBatch.DrawStringOnCtrl(this, ((TyrianTime)_dayCycle).ToString(), Content.DefaultFont32, new Rectangle(0, 0, this.Width, this.Height), _textColor, false, true, 1, HorizontalAlignment.Center);
        }

        protected override void DisposeControl()
        {
            foreach (var tex in _cycleTextures.Values)
            {
                tex?.Dispose();
            }
            base.DisposeControl();
        }
    }
}
