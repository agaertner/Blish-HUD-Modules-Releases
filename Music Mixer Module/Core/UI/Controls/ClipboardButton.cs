using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework.Graphics;

namespace Nekres.Music_Mixer.Core.UI.Controls
{
    internal class ClipboardButton : Image
    {
        private Texture2D _clipboard;
        private Texture2D _clipboardHover;
        private Texture2D _clipboardDisabled;

        private new bool _enabled;
        public new bool Enabled
        {
            get => _enabled;
            set
            {
                SetProperty(ref _enabled, value);
                this.Texture = value ? _clipboard : _clipboardDisabled;
            }
        }

        public ClipboardButton()
        {
            _clipboard = MusicMixer.Instance.ContentsManager.GetTexture("clipboard_hover.png");
            _clipboardHover = MusicMixer.Instance.ContentsManager.GetTexture("clipboard.png");
            _clipboardDisabled = MusicMixer.Instance.ContentsManager.GetTexture("clipboard_disabled.png");
            this.Texture = _clipboard;
        }
        protected override void OnMouseEntered(MouseEventArgs e)
        {
            if (!this.Enabled) return;
            this.Texture = _clipboardHover;
            base.OnMouseMoved(e);
        }

        protected override void OnMouseLeft(MouseEventArgs e)
        {
            if (!this.Enabled) return;
            this.Texture = _clipboard;
            base.OnMouseLeft(e);
        }

        protected override void DisposeControl()
        {
            _clipboard.Dispose();
            _clipboardHover.Dispose();
            _clipboardDisabled.Dispose();
            base.DisposeControl();
        }
    }
}
