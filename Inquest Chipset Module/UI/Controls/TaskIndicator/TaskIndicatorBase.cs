using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nekres.Inquest_Module.UI.Controls
{
    internal abstract class TaskIndicatorBase : Control
    {
        private static readonly Texture2D StopIcon = GameService.Content.GetTexture("common/154982");
        private static readonly Texture2D MouseIcon = GameService.Content.GetTexture("156734");

        private Point _mousePos => GameService.Input.Mouse.Position;

        private bool _paused;
        public bool Paused
        {
            get => _paused;
            set => SetProperty(ref _paused, value);
        }

        private bool _attachToCursor;
        public bool AttachToCursor
        {
            get => _attachToCursor;
            set => SetProperty(ref _attachToCursor, value);
        }

        protected TaskIndicatorBase(bool attachToCursor = true)
        {
            this.AttachToCursor = attachToCursor;
            base.ZIndex = 1000;
        }

        protected override CaptureType CapturesInput() => CaptureType.Filter;

        private void Update()
        {
            if (!_attachToCursor) return;
            Location = new Point(_mousePos.X + this.Width / 2, _mousePos.Y - this.Height / 2);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            Update();

            if (!_paused) return;
            spriteBatch.DrawOnCtrl(this, MouseIcon, new Rectangle((bounds.Width - MouseIcon.Width) / 2, (bounds.Height - MouseIcon.Height) / 2, MouseIcon.Width, MouseIcon.Height), MouseIcon.Bounds);
            spriteBatch.DrawOnCtrl(this, StopIcon, new Rectangle((bounds.Width - StopIcon.Width) / 2, (bounds.Height - StopIcon.Height) / 2, StopIcon.Width, StopIcon.Height), StopIcon.Bounds, Color.White * 0.65f);
        }
    }
}
