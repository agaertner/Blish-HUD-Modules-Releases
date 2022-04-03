using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using Stopwatch;

namespace Nekres.Stopwatch.Core.Controls
{
    internal class StopwatchDisplay : Control
    {

        private BitmapFont _font;

        private string _text;

        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }

        private ContentService.FontSize _fontSize;

        public ContentService.FontSize FontSize
        {
            get => _fontSize;
            set
            {
                if (SetProperty(ref _fontSize, value))
                    _font = Content.GetFont(ContentService.FontFace.Menomonia, value, ContentService.FontStyle.Regular);
            }
        }

        private Color _color;

        public Color Color
        {
            get => _color;
            set => SetProperty(ref _color, value);
        }

        private float _backgroundOpacity;
        public float BackgroundOpacity
        {
            get => _backgroundOpacity;
            set => SetProperty(ref _backgroundOpacity, value);
        }

        public StopwatchDisplay()
        {
            _font = Content.GetFont(ContentService.FontFace.Menomonia, StopwatchModule.ModuleInstance.FontSize.Value, ContentService.FontStyle.Regular);
        }

        protected override CaptureType CapturesInput() => CaptureType.Filter;

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, bounds, Color.Black * BackgroundOpacity);
            var size = _font.MeasureString(_text);
            spriteBatch.DrawStringOnCtrl(this, _text, _font, new Rectangle((bounds.Width - (int)size.Width) / 2, (bounds.Height - (int)size.Height) / 2, (int)size.Width, (int)size.Height), this.Color, false, true,2, HorizontalAlignment.Center, VerticalAlignment.Top);
        }
    }
}
