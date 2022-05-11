using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nekres.Chat_Shorts.UI.Controls
{
    /// <inheritdoc />
    internal class ContextMenuStripItemSeparator : ContextMenuStripItem
    {
        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(0, bounds.Height / 2, bounds.Width, 1), Color.White * 0.8f);
        }
    }
}
