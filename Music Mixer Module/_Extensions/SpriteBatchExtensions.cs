using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nekres.Music_Mixer
{
    internal static class SpriteBatchExtensions
    {
        public static void DrawBorderOnCtrl(this SpriteBatch spriteBatch, Control ctrl, Rectangle bounds, int borderSize)
        {
            spriteBatch.DrawOnCtrl(ctrl, ContentService.Textures.Pixel, new Rectangle(bounds.X - borderSize, bounds.Y - borderSize, bounds.Width + borderSize, borderSize), Color.Black);
            spriteBatch.DrawOnCtrl(ctrl, ContentService.Textures.Pixel, new Rectangle(bounds.X - borderSize, bounds.Y, borderSize, bounds.Height + borderSize), Color.Black);
            spriteBatch.DrawOnCtrl(ctrl, ContentService.Textures.Pixel, new Rectangle(bounds.X, bounds.Y + bounds.Height, bounds.Width + borderSize, borderSize), Color.Black);
            spriteBatch.DrawOnCtrl(ctrl, ContentService.Textures.Pixel, new Rectangle(bounds.X + bounds.Width, bounds.Y - borderSize, borderSize, bounds.Height + borderSize), Color.Black);
        }
    }
}
