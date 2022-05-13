using System;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nekres.Chat_Shorts.UI.Models;

namespace Nekres.Chat_Shorts.UI.Controls
{
    internal class MacroDetails : DetailsButton
    {
        public event EventHandler<MouseEventArgs> EditClick;

        private const int BUTTON_WIDTH = 345;
        private const int BUTTON_HEIGHT = 100;
        private const int USER_WIDTH = 75;
        private const int BOTTOMSECTION_HEIGHT = 35;

        private static Texture2D _dividerSprite = GameService.Content.GetTexture("157218");

        private static Texture2D _editMacroTex = ChatShorts.Instance.ContentsManager.GetTexture("155941.png");
        private static Texture2D _editMacroTexHover = ChatShorts.Instance.ContentsManager.GetTexture("155940.png");
        private static Texture2D _editMacroTexActive = ChatShorts.Instance.ContentsManager.GetTexture("155942.png");
        private static Texture2D _editMacroTexDisabled = ChatShorts.Instance.ContentsManager.GetTexture("155939.png");

        private string _keys;
        public string Keys
        {
            get => _keys;
            set => SetProperty(ref _keys, value);
        }

        private bool _active;
        public bool Active
        {
            get => _active;
            set => SetProperty(ref _active, value);
        }

        private Rectangle _editButtonBounds;
        private bool _mouseOverEditButton;

        public MacroModel Model { get; }

        private bool _mouseOverSendButton;
        private Rectangle _sendButtonBounds;

        public MacroDetails(MacroModel model)
        {
            this.Keys = model.KeyBinding.GetBindingDisplayText();
            this.Title = model.Title;
            this.BasicTooltipText = model.Text;
            this.Model = model;
            this.Model.Changed += OnModelChanged;
        }

        private void OnModelChanged(object o, EventArgs e)
        {
            this.Keys = this.Model.KeyBinding.GetBindingDisplayText();
            this.Title = this.Model.Title;
            this.BasicTooltipText = this.Model.Text;
        }

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            var relPos = RelativeMousePosition;
            _mouseOverEditButton = _editButtonBounds.Contains(relPos);
            _mouseOverSendButton = _sendButtonBounds.Contains(relPos);
            base.OnMouseMoved(e);
        }

        protected override async void OnClick(MouseEventArgs e)
        {
            GameService.Content.PlaySoundEffectByName("button-click");
            if (_mouseOverEditButton && !this.Active) EditClick?.Invoke(this, e);
            if (_mouseOverSendButton) await ChatShorts.Instance.ChatService.Send(this.Model.Text, this.Model.SquadBroadcast);
            base.OnClick(e);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            // Draw background
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, bounds, Color.Black * 0.25f);

            // Draw bottom section (overlap to make background darker here)
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(0, bounds.Height - BOTTOMSECTION_HEIGHT, bounds.Width - BOTTOMSECTION_HEIGHT, BOTTOMSECTION_HEIGHT), Color.Black * 0.1f);

            // Draw bottom section separator
            spriteBatch.DrawOnCtrl(this, _dividerSprite, new Rectangle(0, bounds.Height - 40, bounds.Width, 8), Color.White);

            // Draw edit button
            _editButtonBounds = new Rectangle(BUTTON_WIDTH - 66, (this.Height - BOTTOMSECTION_HEIGHT - 64) / 2, 64, 64);
            var editIcon = this.Active ? _editMacroTexActive : _mouseOverEditButton ? _editMacroTexHover : this.Enabled ? _editMacroTex : _editMacroTexDisabled;
            spriteBatch.DrawOnCtrl(this, editIcon, _editButtonBounds, Color.White);

            // Wrap text
            var wrappedText = DrawUtil.WrapText(Content.DefaultFont14, this.Title, BUTTON_WIDTH - 40 - 20);
            spriteBatch.DrawStringOnCtrl(this, wrappedText, Content.DefaultFont14, new Rectangle(89, 0, 216, this.Height - BOTTOMSECTION_HEIGHT), Color.White, false, true, 2);

            // Draw the user;
            _sendButtonBounds = new Rectangle(5, bounds.Height - BOTTOMSECTION_HEIGHT, USER_WIDTH, 35);
            if (string.IsNullOrEmpty(this.Model.Text)) return;
            var sendText = string.IsNullOrEmpty(this.Keys) ? "Click to send" : $"Click to send ({this.Keys})";
            spriteBatch.DrawStringOnCtrl(this, sendText, Content.DefaultFont14, _sendButtonBounds, Color.White, false, false, 0);
        }
    }
}
