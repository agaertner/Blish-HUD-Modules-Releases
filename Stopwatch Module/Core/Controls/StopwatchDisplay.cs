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

        public StopwatchDisplay()
        {
            _font = Content.GetFont(ContentService.FontFace.Menomonia, StopwatchModule.ModuleInstance.FontSize.Value, ContentService.FontStyle.Regular);
        }

        protected override CaptureType CapturesInput() => CaptureType.Filter;

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            spriteBatch.DrawStringOnCtrl(this, _text, _font, bounds, this.Color, false, true,1, HorizontalAlignment.Center);
        }
    }
}
