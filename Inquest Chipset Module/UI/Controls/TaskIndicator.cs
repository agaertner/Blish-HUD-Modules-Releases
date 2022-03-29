using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;

namespace Nekres.Inquest_Module.UI.Controls
{
    internal sealed class TaskIndicator : Control
    {
        private static Texture2D _stopIcon = GameService.Content.GetTexture("common/154982");
        private static Texture2D _mouseIcon = GameService.Content.GetTexture("156734");
        private static BitmapFont _font = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size24, ContentService.FontStyle.Regular);

        private Point _mousePos => GameService.Input.Mouse.Position;

        private bool _paused;
        public bool Paused
        {
            get => _paused;
            set => SetProperty(ref _paused, value);
        }

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
        
        public TaskIndicator()
        {
            _textColor = Color.White;
            this.ZIndex = 1000;
            Update();
        }

        protected override CaptureType CapturesInput() => CaptureType.Filter;

        private void Update()
        {
            Location = new Point(_mousePos.X + this.Width / 2, _mousePos.Y - this.Height / 2);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            Update();

            if (_paused) {
                spriteBatch.DrawOnCtrl(this, _mouseIcon, new Rectangle((bounds.Width - _mouseIcon.Width) / 2, (bounds.Height - _mouseIcon.Height) / 2, _mouseIcon.Width, _mouseIcon.Height), _mouseIcon.Bounds);
                spriteBatch.DrawOnCtrl(this, _stopIcon, new Rectangle((bounds.Width - _stopIcon.Width) / 2, (bounds.Height - _stopIcon.Height) / 2, _stopIcon.Width, _stopIcon.Height), _stopIcon.Bounds, Color.White * 0.65f);
            }
            else
            {
                LoadingSpinnerUtil.DrawLoadingSpinner(this, spriteBatch, bounds);
                spriteBatch.DrawStringOnCtrl(this, _text, _font, bounds, _textColor, false, true, 1, HorizontalAlignment.Center);
            }
        }
    }
}
