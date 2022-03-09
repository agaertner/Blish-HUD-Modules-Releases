using System;
using System.Collections.Generic;
using System.Linq;
using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
namespace Nekres.Regions_Of_Tyria.UI.Controls
{
    internal sealed class MapNotification : Container
    {
        private static readonly BitmapFont SmallFont;
        private static readonly BitmapFont MediumFont;
        private const int TopMargin = 20;
        private const int StrokeDist = 1;
        private const int UnderlineSize = 1;
        private static readonly Color BrightGold;

        private const int NotificationCooldownMs = 2000;
        private static DateTime _lastNotificationTime;
        
        private static readonly SynchronizedCollection<MapNotification> ActiveMapNotifications;

        static MapNotification()
        {
            _lastNotificationTime = DateTime.Now;
            ActiveMapNotifications = new SynchronizedCollection<MapNotification>();

            SmallFont = Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size24, ContentService.FontStyle.Regular);
            MediumFont = Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size36, ContentService.FontStyle.Regular);
            BrightGold = new Color(223, 194, 149, 255);
        }

        #region Public Fields

        private IEnumerable<string> _headerLines;
        private string _header;
        public string Header
        {
            get => _header;
            set
            {
                _headerLines = value?.Split(new[]{"<br>"}, StringSplitOptions.RemoveEmptyEntries).ForEach(x => x.Trim());
                SetProperty(ref _header, value);
            }
        }

        private IEnumerable<string> _textLines;
        private string _text;
        public string Text
        {
            get => _text;
            set
            {
                _textLines = value?.Split(new[]{"<br>"}, StringSplitOptions.RemoveEmptyEntries).ForEach(x => x.Trim());
                SetProperty(ref _text, value);
            }
        }

        private float _showDuration;
        public float ShowDuration {
            get => _showDuration;
            set => SetProperty(ref _showDuration, value);
        }


        private float _fadeInDuration;
        public float FadeInDuration {
            get => _fadeInDuration;
            set => SetProperty(ref _fadeInDuration, value);
        }

        private float _fadeOutDuration;
        public float FadeOutDuration {
            get => _fadeOutDuration;
            set => SetProperty(ref _fadeOutDuration, value);
        }

        #endregion

        // ReSharper disable once NotAccessedField.Local
        #pragma warning disable IDE0052 // Remove unread private members
        private Glide.Tween _animFadeLifecycle;
        private int _targetTop;

        private MapNotification(string header, string text, float showDuration = 4, float fadeInDuration = 2, float fadeOutDuration = 2) {
            _showDuration = showDuration;
            _fadeInDuration = fadeInDuration;
            _fadeOutDuration = fadeOutDuration;

            Text = text;
            Header = header;
            ClipsBounds = true;
            Opacity = 0f;
            Size = new Point(1000, 1000);
            ZIndex = Screen.MENUUI_BASEINDEX;
            Location = new Point(Graphics.SpriteScreen.Width / 2 - Size.X / 2, Graphics.SpriteScreen.Height / 4 - Size.Y / 4);

            _targetTop = Top;

            Resized += UpdateLocation;
        }

        public void UpdateLocation(object o, ResizedEventArgs e) => Location = new Point(Graphics.SpriteScreen.Width / 2 - Size.X / 2, Graphics.SpriteScreen.Height / 4 - Size.Y / 2);

        /// <inheritdoc />
        protected override CaptureType CapturesInput()
        {
            return CaptureType.Filter;
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds) {
            var height = 0;
            var rect = Rectangle.Empty;

            if (!string.IsNullOrEmpty(Header) && !Header.Equals(Text, StringComparison.InvariantCultureIgnoreCase))
            {
                int width = 0;
                foreach (var headerLine in _headerLines)
                {
                    width = (int)SmallFont.MeasureString(headerLine).Width;
                    rect = new Rectangle(0, TopMargin + height, bounds.Width, bounds.Height);
                    height += SmallFont.LetterSpacing + (int)SmallFont.MeasureString(headerLine).Height;
                    spriteBatch.DrawStringOnCtrl(this, headerLine, SmallFont, rect, BrightGold, false, true, StrokeDist, HorizontalAlignment.Center, VerticalAlignment.Top);
                }

                // Underline
                rect = new Rectangle(Size.X / 2 - width / 2 - 1, rect.Y + height + 2, width + 2, UnderlineSize + 2);
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, rect, new Color(0, 0, 0, 200));
                rect = new Rectangle(rect.X + 1, rect.Y + 1, width, UnderlineSize);
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, rect, BrightGold);

                height += TopMargin;
            }

            if (!string.IsNullOrEmpty(Text)) 
            {
                foreach (var textLine in _textLines)
                {
                    rect = new Rectangle(0, TopMargin + height, bounds.Width, bounds.Height);
                    height += MediumFont.LetterSpacing + (int)MediumFont.MeasureString(textLine).Height;
                    spriteBatch.DrawStringOnCtrl(this, textLine, MediumFont, rect, BrightGold, false, true, StrokeDist, HorizontalAlignment.Center, VerticalAlignment.Top);
                }
            }
        }

        /// <inheritdoc />
        public override void Show() {
            //Nesting instead so we are able to set a different duration per fade direction.
            _animFadeLifecycle = Animation.Tweener
                .Tween(this, new { Opacity = 1f }, FadeInDuration)
                    .OnComplete(() => { _animFadeLifecycle = Animation.Tweener.Tween(this, new { Opacity = 1f }, ShowDuration)
                        .OnComplete(() => { _animFadeLifecycle = Animation.Tweener.Tween(this, new { Opacity = 0f }, FadeOutDuration)
                            .OnComplete(Dispose);
                        });
                });
             
            base.Show();
        }

        private void SlideDown(int distance) {
            _targetTop += distance;

            Animation.Tweener.Tween(this, new {Top = _targetTop}, FadeOutDuration);

            if (_opacity < 1f) return;

            _animFadeLifecycle = Animation.Tweener
                                          .Tween(this, new {Opacity = 0f}, FadeOutDuration)
                                          .OnComplete(Dispose);
        }

        /// <inheritdoc />
        protected override void DisposeControl() {
            ActiveMapNotifications.Remove(this);

            base.DisposeControl();
        }

        public static void ShowNotification(string header, string footer, Texture2D icon = null, float showDuration = 4, float fadeInDuration = 2, float fadeOutDuration = 2) {
            if (DateTime.Now.Subtract(_lastNotificationTime).TotalMilliseconds < NotificationCooldownMs)
                return;

            _lastNotificationTime = DateTime.Now;

            var nNot = new MapNotification(header, footer, showDuration, fadeInDuration, fadeOutDuration) {
                Parent = Graphics.SpriteScreen
            };

            nNot.ZIndex = ActiveMapNotifications.DefaultIfEmpty(nNot).Max(n => n.ZIndex) + 1;

            foreach (var activeScreenNotification in ActiveMapNotifications) {
                activeScreenNotification.SlideDown((int)(SmallFont.LineHeight + MediumFont.LineHeight + TopMargin * 1.05f));
            }

            ActiveMapNotifications.Add(nNot);

            nNot.Show();
        }
    }
}
