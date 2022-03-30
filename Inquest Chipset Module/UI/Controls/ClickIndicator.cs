using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Nekres.Inquest_Module.UI.Controls
{
    internal sealed class ClickIndicator : Control
    {
        private static Texture2D _mouseIcon = GameService.Content.GetTexture("156734");
        private static Texture2D _stopIcon = GameService.Content.GetTexture("common/154982");
        private static Texture2D _mouseIdleTex = InquestModule.ModuleInstance.ContentsManager.GetTexture("mouse-idle.png");
        private static Texture2D _mouseLeftClickTex = InquestModule.ModuleInstance.ContentsManager.GetTexture("mouse-left-click.png");

        private bool _paused;
        public bool Paused
        {
            get => _paused;
            set => SetProperty(ref _paused, value);
        }

        private bool _leftClick;
        private DateTime _clickEnd;
        public ClickIndicator()
        {
            this.ZIndex = 1000;
        }

        public void LeftClick()
        {
            _leftClick = true;
            _clickEnd = DateTime.UtcNow.AddMilliseconds(150);
        }

        protected override CaptureType CapturesInput() => CaptureType.Filter;

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (_paused) {
                spriteBatch.DrawOnCtrl(this, _mouseIcon, new Rectangle((bounds.Width - _mouseIcon.Width) / 2, (bounds.Height - _mouseIcon.Height) / 2, _mouseIcon.Width, _mouseIcon.Height), _mouseIcon.Bounds);
                spriteBatch.DrawOnCtrl(this, _stopIcon, new Rectangle((bounds.Width - _stopIcon.Width) / 2, (bounds.Height - _stopIcon.Height) / 2, _stopIcon.Width, _stopIcon.Height), _stopIcon.Bounds, Color.White * 0.65f);
            }
            else
            {
                if (_leftClick && DateTime.UtcNow < _clickEnd)
                    spriteBatch.DrawOnCtrl(this, _mouseLeftClickTex, new Rectangle((bounds.Width - _mouseIdleTex.Width) / 2, (bounds.Height - _mouseIdleTex.Height) / 2, _mouseIdleTex.Width, _mouseIdleTex.Height), _mouseIdleTex.Bounds);
                else
                    spriteBatch.DrawOnCtrl(this, _mouseIdleTex, new Rectangle((bounds.Width - _mouseIdleTex.Width) / 2, (bounds.Height - _mouseIdleTex.Height) / 2, _mouseIdleTex.Width, _mouseIdleTex.Height), _mouseIdleTex.Bounds);
            }
        }
    }
}
