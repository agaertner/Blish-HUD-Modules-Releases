using System.IO;
using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;

namespace Nekres.Screenshot_Manager.UI.Controls
{
    public class ThumbnailBase : Container
    {
        private static BitmapFont _font = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size36, ContentService.FontStyle.Regular);

        private AsyncTexture2D _texture;

        private string _fileName;
        public string FileName
        {
            get => _fileName;
            set => SetProperty(ref _fileName, value ?? string.Empty);
        }

        public ThumbnailBase(AsyncTexture2D texture, string fileName)
        {
            _texture = texture;
            FileName = fileName;
        }

        protected override void DisposeControl()
        {
            _texture.Dispose();
            base.DisposeControl();
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, bounds, new Color(44, 47, 51));
            if (_texture.HasTexture)
            {
                spriteBatch.DrawOnCtrl(this, _texture.Texture, _texture.Texture.Bounds.Fit(bounds));
                spriteBatch.DrawStringOnCtrl(this, 
                    Path.GetExtension(this.FileName).TrimStart('.').ToUpperInvariant(), _font, 
                    new Rectangle(bounds.X + 10, bounds.Y + 3, bounds.Width - 20, bounds.Height - 6), 
                    new Color(Color.White, 0.5f), false, false, 1, HorizontalAlignment.Left, VerticalAlignment.Top);
            }
            else
                LoadingSpinnerUtil.DrawLoadingSpinner(this, spriteBatch, bounds);
        }
    }
}
