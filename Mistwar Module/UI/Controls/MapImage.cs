using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using Nekres.Mistwar.Entities;
using System.Collections.Generic;
using System.Linq;
using Gw2Sharp.WebApi.V2.Models;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
namespace Nekres.Mistwar.UI.Controls
{
    internal class MapImage : Container
    {
        public IEnumerable<WvwObjectiveEntity> WvwObjectives;

        public float TextureOpacity { get; private set; }

        public float ScaleRatio { get; private set; } = MathHelper.Clamp(MistwarModule.ModuleInstance.ScaleRatioSetting.Value / 100f, 0, 1);

        protected AsyncTexture2D _texture;
        public AsyncTexture2D Texture
        {
            get => _texture;
            private init => SetProperty(ref _texture, value);
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

        private Effect _grayscaleEffect;

        private SpriteBatchParameters _grayscaleSpriteBatchParams;

        private BitmapFont _font;

        public MapImage()
        {
            _font = Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size24, ContentService.FontStyle.Regular);
            _spriteBatchParameters = new SpriteBatchParameters();
            _grayscaleEffect = MistwarModule.ModuleInstance.ContentsManager.GetEffect<Effect>(@"effects\grayscale.mgfx");
            _grayscaleSpriteBatchParams = new SpriteBatchParameters
            {
                Effect = _grayscaleEffect
            };
            this.Texture = new AsyncTexture2D();
            this.Texture.TextureSwapped += OnTextureSwapped;
            MistwarModule.ModuleInstance.ScaleRatioSetting.SettingChanged += OnScaleRatioChanged;
        }

        public void Toggle(float tDuration = 0.1f, bool silent = false)
        {
            if (_visible)
            {
                _visible = false;
                if (silent)
                {
                    this.Hide();
                    return;
                }
                GameService.Content.PlaySoundEffectByName("window-close");
                GameService.Animation.Tweener.Tween(this, new { Opacity = 0.0f }, tDuration).OnComplete(this.Hide);
                return;
            }
            _visible = true;
            this.Show();
            if (silent) return;
            GameService.Content.PlaySoundEffectByName("page-open-" + RandomUtil.GetRandom(1, 3));
            GameService.Animation.Tweener.Tween(this, new { Opacity = 1.0f }, 0.35f);
        }

        internal void SetOpacity(float opacity)
        {
            TextureOpacity = opacity;
            _grayscaleEffect.Parameters["Opacity"].SetValue(opacity);
        }

        public void SetColorIntensity(float colorIntensity)
        {
            _grayscaleEffect.Parameters["Intensity"].SetValue(MathHelper.Clamp(colorIntensity, 0, 1));
        }

        private void OnScaleRatioChanged(object o, ValueChangedEventArgs<float> e)
        {
            this.ScaleRatio = MathHelper.Clamp(e.NewValue / 100f, 0, 1);
            if (!_texture.HasTexture) return;
            this.Size = Blish_HUD.PointExtensions.ResizeKeepAspect(_texture.Texture.Bounds.Size, (int)(ScaleRatio * GameService.Graphics.SpriteScreen.Width), (int)(ScaleRatio * GameService.Graphics.SpriteScreen.Height));
            this.Location = new Point(this.Parent.Size.X / 2 - this.Size.X / 2, this.Parent.Size.Y / 2 - this.Size.Y / 2);
        }

        protected override void DisposeControl()
        {
            if (_texture != null)
            {
                _texture.TextureSwapped -= OnTextureSwapped;
                _texture.Dispose();
            }
            _grayscaleEffect?.Dispose();
            MistwarModule.ModuleInstance.ScaleRatioSetting.SettingChanged -= OnScaleRatioChanged;
            base.DisposeControl();
        }

        private void OnTextureSwapped(object o, ValueChangedEventArgs<Texture2D> e)
        {
            this.SourceRectangle = e.NewValue.Bounds;
            this.Size = Blish_HUD.PointExtensions.ResizeKeepAspect(e.NewValue.Bounds.Size, (int)(ScaleRatio * GameService.Graphics.SpriteScreen.Width), (int)(ScaleRatio * GameService.Graphics.SpriteScreen.Height));
            this.Location = new Point(this.Parent.Size.X / 2 - this.Size.X / 2, this.Parent.Size.Y / 2 - this.Size.Y / 2);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (!this.Visible || !_texture.HasTexture || WvwObjectives == null) return;

            spriteBatch.End();
            spriteBatch.Begin(_grayscaleSpriteBatchParams);

            // Draw the texture
            spriteBatch.DrawOnCtrl(this,
                _texture,
                bounds,
                this.SourceRectangle,
                _tint,
                0f,
                Vector2.Zero,
                _spriteEffects);

            spriteBatch.End();
            spriteBatch.Begin(_spriteBatchParameters); // Exclude everything below from greyscale effect.

            var widthRatio = bounds.Width / (float)SourceRectangle.Width;
            var heightRatio = bounds.Height / (float)SourceRectangle.Height;

            if (MistwarModule.ModuleInstance.DrawSectorsSetting.Value)
            {
                // Draw sector boundaries
                // These need to be iterated separately to be drawn before any other content to avoid overlapping.
                foreach (var objectiveEntity in WvwObjectives)
                {
                    var teamColor = objectiveEntity.TeamColor.GetColorBlindType(MistwarModule.ModuleInstance.ColorTypeSetting.Value, (int)(TextureOpacity * 255));

                    var sectorBounds = objectiveEntity.Bounds.Select(p =>
                    {
                        var r = new Point((int)(widthRatio * p.X), (int)(heightRatio * p.Y)).ToBounds(this.AbsoluteBounds);
                        return new Vector2(r.X, r.Y);
                    }).ToArray();

                    spriteBatch.DrawPolygon(new Vector2(0, 0), sectorBounds, teamColor, 3);
                }
            }

            foreach (var objectiveEntity in WvwObjectives)
            {
                if (objectiveEntity.Icon == null) continue;

                if (!MistwarModule.ModuleInstance.DrawRuinMapSetting.Value && objectiveEntity.Type == WvwObjectiveType.Ruins) continue;

                // Calculate draw bounds.
                var width = (int)(ScaleRatio * objectiveEntity.Icon.Width);
                var height = (int)(ScaleRatio * objectiveEntity.Icon.Height);
                var dest = new Rectangle((int)(widthRatio * objectiveEntity.Center.X), (int)(heightRatio * objectiveEntity.Center.Y), width, height);

                // Draw the objective.
                spriteBatch.DrawWvwObjectiveOnCtrl(this, objectiveEntity, dest, 1f, _font);
            }
        }
    }
}
