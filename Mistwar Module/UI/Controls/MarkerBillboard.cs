using Blish_HUD;
using Blish_HUD.Controls;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using Nekres.Mistwar.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Nekres.Mistwar.UI.Controls
{
    internal class MarkerBillboard : Control
    {
        public IEnumerable<WvwObjectiveEntity> WvwObjectives;

        private BitmapFont _font;

        public MarkerBillboard()
        {
            _font = Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size24, ContentService.FontStyle.Regular);
            _spriteBatchParameters = new SpriteBatchParameters();
        }

        public void Toggle(float tDuration = 0.1f, bool silent = false)
        {
            if (_visible)
            {
                if (!GameUtil.IsUiAvailable()) return;
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

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (!this.Visible || WvwObjectives == null) return;

            this.Size = Parent.AbsoluteBounds.Size; // Always keep at screen size.

            // Order by distance.
            var distanceSort = WvwObjectives.Where(x => x.Icon != null).OrderBy(x => x.WorldPosition.Distance(GameService.Gw2Mumble.PlayerCamera.Position)).ToList();
            if (MistwarModule.ModuleInstance.HideInCombatSetting.Value && GameService.Gw2Mumble.PlayerCharacter.IsInCombat)
            {
                distanceSort = new List<WvwObjectiveEntity> { distanceSort[0] }; // Show only the closest objective during combat.
            }

            foreach (var objectiveEntity in distanceSort)
            {
                if (objectiveEntity.Icon == null) continue;

                if (!MistwarModule.ModuleInstance.DrawRuinMarkersSetting.Value && objectiveEntity.Type == WvwObjectiveType.Ruins) continue;

                var dir = GameService.Gw2Mumble.PlayerCamera.Position - objectiveEntity.WorldPosition;
                var angle = MathUtil.RadToDeg * GameService.Gw2Mumble.PlayerCamera.Forward.Angle(dir);

                if (Math.Abs(angle) < 90) // Objective is behind player.
                {
                    continue;
                }

                // Project onto screen space.
                var trs = Matrix.CreateScale(1) * Matrix.CreateTranslation(objectiveEntity.WorldPosition);
                var transformed = Vector3.Transform(trs.Translation, GameService.Gw2Mumble.PlayerCamera.WorldViewProjection).Flatten();

                // Calculate draw bounds.
                var width = objectiveEntity.Icon.Width;
                var height = objectiveEntity.Icon.Height;
                var dest = new Rectangle((int)transformed.X, (int)transformed.Y, width, height);

                // Draw the objective.
                spriteBatch.DrawWvwObjectiveOnCtrl(this, objectiveEntity, dest, objectiveEntity.Opacity, _font);
            }
        }

        protected override CaptureType CapturesInput()
        {
            return CaptureType.None;
        }
    }
}
