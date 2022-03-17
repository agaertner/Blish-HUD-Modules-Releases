using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using Nekres.Screenshot_Manager;
using Nekres.Screenshot_Manager.UI.Controls;
using System;
namespace Nekres.Screenshot_Manager_Module.Controls
{
    public class ScreenshotNotification : Panel
    {
        private static int _visibleNotifications;

        private static BitmapFont _font = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size24, ContentService.FontStyle.Regular);
        private readonly ThumbnailBase _thumbnail;
        private string _message;

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
            Location = new Point(60, 60 + 144 * _visibleNotifications);

            ShowBorder = true;
            ShowTint = true;
        }

        protected override CaptureType CapturesInput()
        {
            return CaptureType.Mouse;
        }

        /// <inheritdoc />
        public override void RecalculateLayout()
        {
            this.Location = new Point(60, 60 + 144 * _visibleNotifications);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bound)
        {
            spriteBatch.DrawStringOnCtrl(this, _message, _font, new Rectangle(0,0, this.Width, HEADER_HEIGHT), Color.White, false, true, 1, HorizontalAlignment.Center);
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bound)
        {
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