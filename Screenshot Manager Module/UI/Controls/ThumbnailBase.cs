using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nekres.Screenshot_Manager.UI.Controls
{
    public class ThumbnailBase : Container
    {
        private AsyncTexture2D _texture;
        public string FileName { get; set; }

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
                spriteBatch.DrawOnCtrl(this, _texture.Texture, _texture.Texture.Bounds.Fit(bounds));
            else
                LoadingSpinnerUtil.DrawLoadingSpinner(this, spriteBatch, bounds);
        }
    }
}
