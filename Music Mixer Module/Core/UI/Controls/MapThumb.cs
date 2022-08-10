using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nekres.Music_Mixer.Core.UI.Controls
{
    internal class MapThumb : Control
    {
        private static Color _sandColor = new Color(238, 221, 171);

        private AsyncTexture2D _texture;
        public AsyncTexture2D Texture
        {
            get => _texture;
            set => SetProperty(ref _texture, value);
        }

        private int _Id;
        public int Id
        {
            get => _Id;
            set => SetProperty(ref _Id, value);
        }

        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

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

        private const int _borderSize = 10;

        private Effect _grayscaleEffect;

        private SpriteBatchParameters _defaultSpriteBatchParameters;

        public MapThumb(int mapId, string mapName, AsyncTexture2D texture)
        {
            this.Id = mapId;
            this.Name = mapName;
            this.Texture = texture;
            _grayscaleEffect = MusicMixer.Instance.ContentsManager.GetEffect<Effect>(@"effects\grayscale.mgfx");
            _grayscaleEffect.Parameters["Intensity"].SetValue(_active ? 0f : 1f);
            _spriteBatchParameters = new SpriteBatchParameters
            {
                Effect = _grayscaleEffect
            };
            _defaultSpriteBatchParameters = new SpriteBatchParameters();
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

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            var imageBounds = new Rectangle(_borderSize, _borderSize, this.Width - _borderSize * 2, this.Height - _borderSize * 2);
            spriteBatch.DrawOnCtrl(this, this.Texture, imageBounds);

            // Exclude everything below from grayscale effect.
            spriteBatch.End();
            spriteBatch.Begin(_defaultSpriteBatchParameters);

            spriteBatch.DrawBorderOnCtrl(this, imageBounds, _borderSize);
            spriteBatch.DrawStringOnCtrl(this, this.Name, Content.DefaultFont18, new Rectangle(0, _borderSize * 2, this.Width, this.Height - _borderSize * 2), _sandColor, false, true, 1, HorizontalAlignment.Center, VerticalAlignment.Top);
            
            spriteBatch.End();
            spriteBatch.Begin(_spriteBatchParameters);
        }

        protected override void DisposeControl()
        {
            this.Texture?.Dispose();
            _grayscaleEffect?.Dispose();
            base.DisposeControl();
        }
    }
}
