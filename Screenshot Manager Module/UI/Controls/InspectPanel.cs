using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Blish_HUD.Input;
using MonoGame.Extended.BitmapFonts;

namespace Nekres.Screenshot_Manager.UI.Controls
{
    internal sealed class InspectPanel : Panel
    {
        private static readonly BitmapFont Font = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size36, ContentService.FontStyle.Regular);
        private readonly AsyncTexture2D _texture;
        private Rectangle _textureBoundsFitted;
        private readonly string _label;
        public InspectPanel(AsyncTexture2D texture, string label)
        {
            _texture = texture;
            _label = label;
            Parent = GameService.Graphics.SpriteScreen;
            Size = new Point(1, 1);
            Location = new Point(this.Parent.Width / 2, this.Parent.Height / 2);
            BackgroundColor = Color.Black;
            ZIndex = 9999;
            ShowTint = true;

            _texture.TextureSwapped += OnTextureSwapped;
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);
            this.Dispose();
        }

        protected override void DisposeControl()
        {
            _texture.Dispose();
            base.DisposeControl();
        }

        public void OnTextureSwapped(object o, EventArgs e)
        {
            Size = new Point(_texture.Texture.Width + 10, _texture.Texture.Height + 10);
            Location = new Point((GameService.Graphics.SpriteScreen.Width - this.Width) / 2, (GameService.Graphics.SpriteScreen.Height - this.Height) / 2);
            _textureBoundsFitted = _texture.Texture.Bounds.Fit(this.LocalBounds);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            if (_texture.HasTexture)
                spriteBatch.DrawOnCtrl(this, _texture.Texture, _textureBoundsFitted);
            else
                LoadingSpinnerUtil.DrawLoadingSpinner(this,spriteBatch, bounds);

            spriteBatch.DrawStringOnCtrl(this, $"\u201c{_label}\u201d", Font, new Rectangle(0, HEADER_HEIGHT, this.Width, HEADER_HEIGHT), Color.White, false, true, 3, HorizontalAlignment.Center);
        }
    }
}
