using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using Nekres.Screenshot_Manager.UI.Controls;
using System;
using System.IO;

namespace Nekres.Screenshot_Manager_Module.Controls
{
    public class ScreenshotNotification : Panel
    {
        private static int _visibleNotifications;

        private static readonly BitmapFont Font = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size24, ContentService.FontStyle.Regular);
        private static readonly BitmapFont TitleFont = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size22, ContentService.FontStyle.Regular);

        private readonly ThumbnailBase _thumbnail;
        private readonly string _message;

        private ScreenshotNotification(AsyncTexture2D texture, string fileName, string message)
        {
            _thumbnail = new ThumbnailBase(texture, fileName)
            {
                Parent = this,
                Location = new Point(0, HEADER_HEIGHT),
                Size = new Point(256, 144)
            };

            _message = message;

            Opacity = 0f;

            Size = new Point(256, 144 + HEADER_HEIGHT);
            Location = new Point(60, 60 + this.Height * _visibleNotifications);

            ShowBorder = true;
            ShowTint = true;
        }

        protected override CaptureType CapturesInput() => CaptureType.Mouse;

        public override void RecalculateLayout() => this.Location = new Point(60, 60 + this.Height * _visibleNotifications);

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bound)
        {
            base.PaintBeforeChildren(spriteBatch, bound);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, bound, Color.Black * 0.4f);
            spriteBatch.DrawStringOnCtrl(this, _message, Font, new Rectangle(0,0, this.Width, HEADER_HEIGHT), Color.White, false, true, 2, HorizontalAlignment.Center);
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bound)
        {
            base.PaintAfterChildren(spriteBatch, bound);
            spriteBatch.DrawStringOnCtrl(this, $"\u201c{Path.GetFileNameWithoutExtension(_thumbnail.FileName)}\u201d", TitleFont, new Rectangle(0, HEADER_HEIGHT + 2, this.Width, HEADER_HEIGHT), Color.White, false, true, 1, HorizontalAlignment.Center);
        }

        private void Show(float duration)
        {
            Content.PlaySoundEffectByName(@"audio/color-change");

            Animation.Tweener
                .Tween(this, new {Opacity = 1f}, 0.2f)
                .Repeat(1)
                .RepeatDelay(duration)
                .Reflect()
                .OnComplete(Dispose);
        }

        public static void ShowNotification(AsyncTexture2D texture, string fileName, string message, float duration, Action clickCallback)
        {
            var notif = new ScreenshotNotification(texture, fileName, message)
            {
                Parent = Graphics.SpriteScreen
            };
            notif.Click += (o, e) => clickCallback();
            notif.Show(duration);
            _visibleNotifications++;
        }

        protected override void DisposeControl()
        {
            _visibleNotifications--;
            base.DisposeControl();
        }
        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);
            this.Dispose();
        }
    }
}