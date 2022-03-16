using System;
using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nekres.Screenshot_Manager;
using Nekres.Screenshot_Manager.UI.Models;
using Nekres.Screenshot_Manager.UI.Views;

namespace Nekres.Screenshot_Manager_Module.Controls
{
    public class ScreenshotNotification : Panel
    {
        private const int HEADING_HEIGHT = 20;

        private const int PanelMargin = 10;

        private static int _visibleNotifications;

        private readonly AsyncTexture2D _thumbnail;

        private readonly Point _thumbnailSize;
        private Rectangle _layoutInspectIconBounds;

        private Rectangle _layoutThumbnailBounds;

        private static Texture2D _notificationBackgroundTexture = ScreenshotManagerModule.ModuleInstance.ContentsManager.GetTexture("ns-button.png");
        private static Texture2D _inspectIcon = ScreenshotManagerModule.ModuleInstance.ContentsManager.GetTexture("inspect.png");

        private ScreenshotNotification(AsyncTexture2D texture, string message)
        {
            _thumbnail = texture;

            Opacity = 0f;

            Size = new Point(350 + PanelMargin, 200 + HEADING_HEIGHT + PanelMargin);

            Location = new Point(60, 60 + (Size.Y + 15) * _visibleNotifications);

            ShowBorder = true;
            ShowTint = true;

            var borderPanel = new Panel
            {
                Parent = this,
                Size = new Point(Size.X, Size.Y + PanelMargin),
                Location = new Point(0, HEADING_HEIGHT),
                BackgroundColor = Color.Black,
                ShowTint = true,
                ShowBorder = true
            };
            var messageLbl = new Label
            {
                Parent = this,
                Location = new Point(0, 2),
                Size = Size,
                Font = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size14, ContentService.FontStyle.Regular),
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = message
            };
        }

        protected override CaptureType CapturesInput()
        {
            return CaptureType.Mouse;
        }

        /// <inheritdoc />
        public override void RecalculateLayout()
        {
            _layoutThumbnailBounds = new Rectangle(PanelMargin / 2, HEADING_HEIGHT + PanelMargin / 2, Size.X,Size.Y);
            _layoutInspectIconBounds = new Rectangle(Size.X / 2 - 32, Size.Y / 2 - 32 + HEADING_HEIGHT, 64, 64);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            spriteBatch.DrawOnCtrl(this, _notificationBackgroundTexture, bounds, Color.White * 0.85f);
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bound)
        {
            if (_thumbnail.HasTexture)
                spriteBatch.DrawOnCtrl(this, _thumbnail, new Rectangle((this.Width - _thumbnail.Texture.Width) / 2, (this.Height - _thumbnail.Texture.Height) / 2, _thumbnail.Texture.Width, _thumbnail.Texture.Height));
            else
                LoadingSpinnerUtil.DrawLoadingSpinner(this, spriteBatch, _layoutThumbnailBounds);

            spriteBatch.DrawOnCtrl(this, _inspectIcon, _layoutInspectIconBounds);
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

        public static void ShowNotification(AsyncTexture2D texture, string message, float duration, Action clickCallback)
        {
            var notif = new ScreenshotNotification(texture, message)
            {
                Parent = Graphics.SpriteScreen
            };
            notif.Click += (o, e) => clickCallback();
            notif.Show(duration);
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