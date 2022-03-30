using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;

namespace Nekres.Inquest_Module.UI.Controls
{
    internal sealed class TaskIndicator : TaskIndicatorBase
    {
        private static BitmapFont _font = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size24, ContentService.FontStyle.Regular);

        private string _text;
        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }

        private Color _textColor;
        public Color TextColor
        {
            get => _textColor;
            set => SetProperty(ref _textColor, value);
        }

        public TaskIndicator(bool attachToCursor = true) : base(attachToCursor)
        {
            _textColor = Color.White;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.Paint(spriteBatch, bounds);
            if (this.Paused) return;
            LoadingSpinnerUtil.DrawLoadingSpinner(this, spriteBatch, bounds);
            spriteBatch.DrawStringOnCtrl(this, _text, _font, bounds, _textColor, false, true, 1, HorizontalAlignment.Center);
        }
    }
}
