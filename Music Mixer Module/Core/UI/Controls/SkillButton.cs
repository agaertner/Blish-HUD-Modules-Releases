using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nekres.Music_Mixer.Core.UI.Controls
{
    internal class SkillButton : Control
    {
        private bool _active;
        public bool Active
        {
            get => _active;
            set
            {
                if (!SetProperty(ref _active, value)) return;
                _grayscaleEffect?.Parameters["Intensity"].SetValue(value ? 0f : 1f);
            }

        }

        private Effect _grayscaleEffect;
        public AsyncTexture2D Texture;

        public SkillButton()
        {
            _grayscaleEffect = MusicMixer.Instance.ContentsManager.GetEffect<Effect>(@"effects\grayscale.mgfx");
            _spriteBatchParameters = new SpriteBatchParameters
            {
                Effect = _grayscaleEffect
            };
        }
        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (this.Texture == null || !this.Texture.HasTexture) return;
            spriteBatch.DrawOnCtrl(this, this.Texture, new Rectangle(0,0,this.Width,this.Height));
        }

        protected override void OnMouseEntered(MouseEventArgs e)
        {
            _grayscaleEffect?.Parameters["Intensity"].SetValue(0f);
            base.OnMouseEntered(e);
        }

        protected override void OnMouseLeft(MouseEventArgs e)
        {
            _grayscaleEffect?.Parameters["Intensity"].SetValue(_active ? 0f : 1f);
            base.OnMouseLeft(e);
        }

        protected override void DisposeControl()
        {
            this.Texture?.Dispose();
            _grayscaleEffect?.Dispose();
            base.DisposeControl();
        }
    }
}
