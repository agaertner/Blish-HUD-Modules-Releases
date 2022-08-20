using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Gw2Sharp.Models;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Nekres.Mistwar.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
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

        private Texture2D _playerArrow;

        public MapImage()
        {
            _playerArrow = MistwarModule.ModuleInstance.ContentsManager.GetTexture("156081.png");
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
            _playerArrow?.Dispose();
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
                spriteBatch.DrawWvwObjectiveOnCtrl(this, objectiveEntity, dest, 1f, 1f, MistwarModule.ModuleInstance.DrawObjectiveNamesSetting.Value);
            }

            // Draw player position indicator (camera transforms are used because avatar transforms are not exposed in competitive modes.)
            if (!MistwarModule.ModuleInstance.WvwService.TryGetMap(GameService.Gw2Mumble.CurrentMap.Id, out var map)) return;
            var v = GameService.Gw2Mumble.PlayerCamera.Position * 39.37008f; // world meters to inches.
            var worldInchesMap = new Vector2(
                (float)(map.ContinentRect.TopLeft.X + (v.X - map.MapRect.TopLeft.X) / map.MapRect.Width * map.ContinentRect.Width), 
                (float)(map.ContinentRect.TopLeft.Y - (v.Y - map.MapRect.TopLeft.Y) / map.MapRect.Height * map.ContinentRect.Height)); // clamp to map bounds
            var mapCenter = GameService.Gw2Mumble.UI.MapCenter.ToXnaVector2(); // might be (0,0) in competitive..
            var pos = Vector2.Transform(worldInchesMap - mapCenter, Matrix.CreateRotationZ(0f));
            var fit = MapUtil.Refit(new Coordinates2(pos.X, pos.Y), map.ContinentRect.TopLeft); // refit to our 256x256 tiled map
            var tDest = new Rectangle((int)(widthRatio * fit.X), (int)(heightRatio * fit.Y), (int)(ScaleRatio * _playerArrow.Width), (int)(ScaleRatio * _playerArrow.Height)); // apply user scale
            var rot = Math.Atan2(GameService.Gw2Mumble.PlayerCamera.Forward.X, GameService.Gw2Mumble.PlayerCamera.Forward.Y) * 3.6f / Math.PI; // rotate the arrow in the forward direction
            spriteBatch.DrawOnCtrl(this, _playerArrow, new Rectangle(tDest.X + tDest.Width / 4, tDest.Y + tDest.Height / 4, tDest.Width, tDest.Height), _playerArrow.Bounds, Color.White, (float)rot, new Vector2(_playerArrow.Width / 2f, _playerArrow.Height / 2f));
        }
    }
}
