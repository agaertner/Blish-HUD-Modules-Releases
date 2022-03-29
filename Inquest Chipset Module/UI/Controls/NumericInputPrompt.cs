﻿using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Globalization;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Nekres.Inquest_Module.UI.Controls
{
    internal sealed class NumericInputPrompt : Container
    {
        private static Texture2D _bgTexture = GameService.Content.GetTexture("tooltip");
        private static BitmapFont _font = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size24, ContentService.FontStyle.Regular);

        private static NumericInputPrompt _singleton;

        private Rectangle _confirmButtonBounds;
        private Rectangle _cancelButtonBounds;
        private Rectangle _inputTextBoxBounds;

        private StandardButton _confirmButton;
        private StandardButton _cancelButton;
        private TextBox _inputTextBox;

        private readonly Action<bool, double> _callback;
        private readonly string _text;
        private readonly string _confirmButtonText;
        private readonly string _cancelButtonButtonText;

        private NumericInputPrompt(Action<bool, double> callback, string text, string confirmButtonText, string cancelButtonText)
        {
            _callback = callback;
            _text = text;
            _confirmButtonText = confirmButtonText;
            _cancelButtonButtonText = cancelButtonText;
            this.ZIndex = 999;
        }

        public static void ShowPrompt(Action<bool, double> callback, string text, string confirmButtonText = "Confirm", string cancelButtonText = "Cancel")
        {
            if (_singleton != null) return;
            _singleton = new NumericInputPrompt(callback, text, confirmButtonText, cancelButtonText)
            {
                Parent = Graphics.SpriteScreen,
                Location = Point.Zero,
                Size = Graphics.SpriteScreen.Size
            };
            _singleton.Show();
        }

        private void CreateButtons()
        {
            if (_confirmButton == null)
            {
                _confirmButton = new StandardButton
                {
                    Parent = this,
                    Text = _confirmButtonText,
                    Size = _confirmButtonBounds.Size,
                    Location = _confirmButtonBounds.Location,
                    Enabled = false
                };
                _confirmButton.Click += (_, _) =>
                {
                    GameService.Content.PlaySoundEffectByName("button-click");
                    _callback(true, double.Parse(_inputTextBox.Text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture));
                    _singleton = null;
                    this.Dispose();
                };
            }

            if (_cancelButton == null)
            {
                _cancelButton = new StandardButton
                {
                    Parent = this,
                    Text = _cancelButtonButtonText,
                    Size = _cancelButtonBounds.Size,
                    Location = _cancelButtonBounds.Location
                };
                _cancelButton.Click += (_, _) =>
                {
                    GameService.Content.PlaySoundEffectByName("button-click");
                    _callback(false, 0);
                    _singleton = null;
                    this.Dispose();
                };
            }
        }

        private void CreateTextInput()
        {
            if (_inputTextBox != null) return;
            _inputTextBox = new TextBox
            {
                Parent = this,
                Size = _inputTextBoxBounds.Size,
                Location = _inputTextBoxBounds.Location,
                Font = _font,
                Focused = true,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            _inputTextBox.TextChanged += (o, _) =>
            {
                _confirmButton.Enabled = double.TryParse(((TextBox)o).Text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var _);
            };
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            var textSize = _font.MeasureString(_text);

            // Darken background outside container
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, bounds, Color.Black * 0.8f);

            // Calculate background bounds
            var bgTextureSize = new Point((int)textSize.Width + 12, (int)textSize.Height + 125);
            var bgTexturePos = new Point((bounds.Width - bgTextureSize.X) / 2, (bounds.Height - bgTextureSize.Y) / 2);
            var bgBounds = new Rectangle(bgTexturePos, bgTextureSize);

            // Draw border
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bgBounds.X - 1, bgBounds.Y - 1, bgBounds.Width + 1, 1), Color.Black);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bgBounds.X - 1, bgBounds.Y - 1, 1, bgBounds.Height + 1), Color.Black);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bgBounds.X + bgBounds.Width, bgBounds.Y, 1, bgBounds.Height + 1), Color.Black);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bgBounds.X, bgBounds.Y + bgBounds.Height, bgBounds.Width, 1), Color.Black);

            // Draw Background
            spriteBatch.DrawOnCtrl(this, _bgTexture, bgBounds, Color.White);

            // Draw text
            spriteBatch.DrawStringOnCtrl(this, _text, _font, new Rectangle(bgBounds.X + 6, bgBounds.Y + 5, bgBounds.Width - 11, bgBounds.Height), Color.White, true, HorizontalAlignment.Left, VerticalAlignment.Top);

            // Set button bounds
            _confirmButtonBounds = new Rectangle(bgBounds.Left + 5, bgBounds.Bottom - 50, 100, 45);
            _cancelButtonBounds = new Rectangle(_confirmButtonBounds.Right + 10, _confirmButtonBounds.Y, 100, 45);
            _inputTextBoxBounds = new Rectangle(_confirmButtonBounds.X, _confirmButtonBounds.Y - 55, bgBounds.Width - 10, 45);

            this.CreateTextInput();
            this.CreateButtons();
        }
    }
}