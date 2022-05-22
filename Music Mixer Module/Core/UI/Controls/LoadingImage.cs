using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nekres.Music_Mixer.Core.UI.Controls
{
    internal class LoadingImage : Image
    {
        public LoadingImage() : base()
        {
        }

        public LoadingImage(AsyncTexture2D image) : base(image)
        {
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (this.Texture == null || !this.Texture.HasTexture)
            {
                LoadingSpinnerUtil.DrawLoadingSpinner(this, spriteBatch, new Rectangle(bounds.Center.X - 32, bounds.Center.Y - 32, 64,64));
                return;
            }
            base.Paint(spriteBatch, bounds);
        }
    }
}
