using Blish_HUD;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Nekres.Inquest_Module.UI.Controls
{
    internal sealed class ClickIndicator : TaskIndicatorBase
    {
        private static readonly Texture2D MouseIdleTex = InquestModule.ModuleInstance.ContentsManager.GetTexture("mouse-idle.png");
        private static readonly Texture2D MouseLeftClickTex = InquestModule.ModuleInstance.ContentsManager.GetTexture("mouse-left-click.png");

        private DateTime _clickEnd;

        public ClickIndicator(bool attachToCursor = true) : base(attachToCursor) { /* NOOP */ }

        public void LeftClick(int durationMs = 150)
        {
            _clickEnd = DateTime.UtcNow.AddMilliseconds(durationMs);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.Paint(spriteBatch, bounds);
            if (this.Paused) return;
            if (DateTime.UtcNow < _clickEnd)
                spriteBatch.DrawOnCtrl(this, MouseLeftClickTex, new Rectangle((bounds.Width - MouseLeftClickTex.Width) / 2, (bounds.Height - MouseLeftClickTex.Height) / 2, MouseLeftClickTex.Width, MouseLeftClickTex.Height), MouseLeftClickTex.Bounds);
            else
                spriteBatch.DrawOnCtrl(this, MouseIdleTex, new Rectangle((bounds.Width - MouseIdleTex.Width) / 2, (bounds.Height - MouseIdleTex.Height) / 2, MouseIdleTex.Width, MouseIdleTex.Height), MouseIdleTex.Bounds);
        }
    }
}
