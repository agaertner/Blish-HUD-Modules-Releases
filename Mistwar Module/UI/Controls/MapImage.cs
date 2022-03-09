using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nekres.Mistwar.Entities;
using System.Collections.Generic;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
namespace Nekres.Mistwar.UI.Controls
{
    internal class MapImage : Container
    {
        public IEnumerable<WvwObjectiveEntity> WvwObjectives;

        protected AsyncTexture2D _texture;
        public AsyncTexture2D Texture
        {
            get => _texture;
            set => SetProperty(ref _texture, value);
        }

        private SpriteEffects _spriteEffects;
        public SpriteEffects SpriteEffects
        {
            get => _spriteEffects;
            set => SetProperty(ref _spriteEffects, value);
        }

        private Rectangle? _sourceRectangle;
        public Rectangle SourceRectangle
        {
            get => _sourceRectangle ?? _texture.Texture.Bounds;
            set => SetProperty(ref _sourceRectangle, value);
        }

        private Color _tint = Color.White;
        public Color Tint
        {
            get => _tint;
            set => SetProperty(ref _tint, value);
        }

        private float _grayscaleIntensity;
        public float GrayscaleIntensity
        {
            get => _grayscaleIntensity;
            set
            {
                SetProperty(ref _grayscaleIntensity, value);
                _grayscaleEffect.Parameters["Intensity"].SetValue(MathHelper.Clamp(value, 0, 1));
            }
        }

        private Effect _grayscaleEffect;

        public float ScaleRatio { get; private set; } = MathHelper.Clamp(MistwarModule.ModuleInstance.ScaleRatioSetting.Value / 100f, 0, 1);

        private MapImageDynamic _dynamicLayer;
        public float TextureOpacity { get; private set; }
        public MapImage()
        {
            this.Texture = new AsyncTexture2D();
            this.Size = _texture.Texture.Bounds.Size;
            _grayscaleEffect ??= MistwarModule.ModuleInstance.ContentsManager.GetEffect<Effect>(@"effects\grayscale.mgfx");
            _spriteBatchParameters = new SpriteBatchParameters
            {
                Effect = _grayscaleEffect
            };
            _texture.TextureSwapped += OnTextureSwapped;
            _dynamicLayer = new MapImageDynamic(this)
            {
                Size = this.Size
            };
            MistwarModule.ModuleInstance.ScaleRatioSetting.SettingChanged += OnScaleRatioChanged;
        }

        internal void SetOpacity(float opacity)
        {
            TextureOpacity = opacity;
            _grayscaleEffect.Parameters["Opacity"].SetValue(opacity);
        }

        private void OnScaleRatioChanged(object o, ValueChangedEventArgs<float> e)
        {
            this.ScaleRatio = MathHelper.Clamp(e.NewValue / 100f, 0, 1);
            if (!_texture.HasTexture) return;
            this.Size = Blish_HUD.PointExtensions.ResizeKeepAspect(_texture.Texture.Bounds.Size, (int)(ScaleRatio * GameService.Graphics.SpriteScreen.Width), (int)(ScaleRatio * GameService.Graphics.SpriteScreen.Height));
            this.Location = new Point(this.Parent.Size.X / 2 - this.Size.X / 2, this.Parent.Size.Y / 2 - this.Size.Y / 2);
            _dynamicLayer.Size = this.Size;
        }
        private new void Dispose()
        {
            _dynamicLayer?.Dispose();
            Texture.TextureSwapped -= OnTextureSwapped;
            MistwarModule.ModuleInstance.ScaleRatioSetting.SettingChanged -= OnScaleRatioChanged;
            base.Dispose();
        }

        private void OnTextureSwapped(object o, ValueChangedEventArgs<Texture2D> e)
        {
            this.SourceRectangle = e.NewValue.Bounds;
            this.Size = Blish_HUD.PointExtensions.ResizeKeepAspect(e.NewValue.Bounds.Size, (int)(ScaleRatio * GameService.Graphics.SpriteScreen.Width), (int)(ScaleRatio * GameService.Graphics.SpriteScreen.Height));
            this.Location = new Point(this.Parent.Size.X / 2 - this.Size.X / 2, this.Parent.Size.Y / 2 - this.Size.Y / 2);
            _dynamicLayer.Size = this.Size;
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (!_texture.HasTexture || WvwObjectives == null) return;

            // Draw the texture
            spriteBatch.DrawOnCtrl(this,
                _texture,
                bounds,
                this.SourceRectangle,
                _tint,
                0f,
                Vector2.Zero,
                _spriteEffects);
        }
    }
}
