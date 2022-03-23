using Blish_HUD;
using Blish_HUD.Controls;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using Nekres.Mistwar.Entities;
using System.Collections.Generic;
using System.Linq;
using MonoGame.Extended.Triangulation;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using RectangleF = MonoGame.Extended.RectangleF;
namespace Nekres.Mistwar.UI.Controls
{
    internal class MapImageDynamic : Control
    {
        private IEnumerable<WvwObjectiveEntity> WvwObjectives => ((MapImage)Parent).WvwObjectives;
        private Rectangle SourceRectangle => ((MapImage)Parent).SourceRectangle;
        private float ScaleRatio => ((MapImage)Parent).ScaleRatio;
        private float TextureOpacity => ((MapImage)Parent).TextureOpacity;

        private BitmapFont _font;

        private SpriteEffects _spriteEffects;
        public SpriteEffects SpriteEffects
        {
            get => _spriteEffects;
            set => SetProperty(ref _spriteEffects, value);
        }

        private Color _tint = Color.White;
        public Color Tint
        {
            get => _tint;
            set => SetProperty(ref _tint, value);
        }
        public MapImageDynamic(MapImage parent)
        {
            this.Parent = parent;
            this.Size = Parent.Size;
            this.Visible = Parent.Visible;
            _font = Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size24, ContentService.FontStyle.Regular);
        }

        protected override CaptureType CapturesInput() => CaptureType.DoNotBlock;

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (WvwObjectives == null) return;

            var widthRatio = bounds.Width / (float)SourceRectangle.Width;
            var heightRatio = bounds.Height / (float)SourceRectangle.Height;

            // draw player position (Unfortunately, Guild Wars 2's MumbleLink API does not provide real time positionals in competitive modes.
            //var rawPlayerPos = MapUtil.Refit(new Coordinates2(widthRatio * GameService.Gw2Mumble.RawClient.PlayerLocationMap.X, heightRatio * GameService.Gw2Mumble.RawClient.PlayerLocationMap.Y), new Coordinates2(SourceRectangle.Left, SourceRectangle.Top));
            //var playerPos = new Point(rawPlayerPos.X, rawPlayerPos.Y).ToBounds(this.AbsoluteBounds);
            //spriteBatch.DrawCircle(new CircleF(new Point2(playerPos.X, playerPos.Y), 20), 360, WvwObjectiveEntity.BrightGold, 20); ;

            if (MistwarModule.ModuleInstance.DrawSectorsSetting.Value)
            {
                // draw sector boundaries
                // these need to be iterated separately to be drawn before any other content to avoid overlapping.
                foreach (var objectiveEntity in WvwObjectives)
                {
                    var teamColor = objectiveEntity.TeamColor.GetColorBlindType(MistwarModule.ModuleInstance.ColorTypeSetting.Value, (int)(TextureOpacity * 255));

                    var sectorBounds = objectiveEntity.Bounds.Select(p =>
                    {
                        var r = new Point((int) (widthRatio * p.X), (int) (heightRatio * p.Y)).ToBounds(this.AbsoluteBounds);
                        return new Vector2(r.X, r.Y);
                    }).ToArray();

                    spriteBatch.DrawPolygon(new Vector2(0, 0), sectorBounds, teamColor, 3);
                }
            }

            foreach (var objectiveEntity in WvwObjectives)
            {
                var teamColor = objectiveEntity.TeamColor.GetColorBlindType(MistwarModule.ModuleInstance.ColorTypeSetting.Value);

                if (objectiveEntity.Icon == null) continue;

                var width = (int)(ScaleRatio * objectiveEntity.Icon.Width);
                var height = (int)(ScaleRatio * objectiveEntity.Icon.Height);
                var dest = new Rectangle((int)(widthRatio * objectiveEntity.Center.X), (int)(heightRatio * objectiveEntity.Center.Y), width, height);
                var tDest = dest.ToBounds(this.AbsoluteBounds); //necessary for native DrawShape calls.

                switch (objectiveEntity.Owner)
                {
                    case WvwOwner.Blue:
                        // draw rectangle
                        var aScale = 1.4f;
                        var shapeRectangle = new RectangleF(tDest.X - aScale * tDest.Width / 2, tDest.Y - aScale * tDest.Height / 2, aScale * tDest.Width, aScale * tDest.Height);
                        spriteBatch.FillRectangle(shapeRectangle, teamColor);
                        spriteBatch.DrawRectangle(shapeRectangle, Color.Black, 2);
                        break;
                    case WvwOwner.Green:
                        // draw diamond (we have to fake thickness to fill the diamond because the thickness param draws outside. >_>)
                        // Note: Drawing on a bitmap with Graphics.FillPolygon, saving it into a stream and drawing a Texture2D created from that stream is too resource intensive and will cause the UI thread to stall.
                        // Unfortunately, the source of Monogame Extension's FillRectangle just scales a single white pixel and doesn't offer a rectangle to transform.
                        var aWidth = 1.6f * tDest.Width;
                        var aHeight = 1.6f * tDest.Height;
                        var shapeDiamond = new[]
                        {
                            new Vector2( aWidth, aWidth / 2), new Vector2(aWidth / 2, 0),
                            new Vector2(0, aWidth / 2), new Vector2(aWidth / 2, aWidth)
                        };
                        spriteBatch.DrawPolygonFill(new Vector2(tDest.X - aWidth / 2, tDest.Y - aHeight / 2), shapeDiamond, Color.Black);
                        break;
                    default:
                        // draw circle
                        var circleDest = new CircleF(new Point2(tDest.X, tDest.Y), (int)(0.8 * width));
                        spriteBatch.DrawCircle(circleDest, 360, teamColor, width);
                        spriteBatch.DrawCircle(circleDest, 360, Color.Black, 2);
                        break;
                }

                // draw remaining duration of the protection buff
                if (objectiveEntity.HasBuff(out var remainingTime))
                {
                    var text = remainingTime.ToString(@"m\:ss");
                    var size = _font.MeasureString(text);
                    var texSize = Blish_HUD.PointExtensions.ResizeKeepAspect(new Point(objectiveEntity.BuffTexture.Width, objectiveEntity.BuffTexture.Height), (int)size.Width, (int)size.Height);
                    spriteBatch.DrawOnCtrl(this, objectiveEntity.BuffTexture, new Rectangle(dest.X + texSize.X / 2, dest.Y - texSize.Y + 1, texSize.X, texSize.Y), objectiveEntity.BuffTexture.Bounds);
                    spriteBatch.DrawStringOnCtrl(this, text, _font, new Rectangle(dest.X - (int)size.Width / 2, dest.Y - (int)size.Height - dest.Height / 2 - 10, dest.Width, dest.Height), WvwObjectiveEntity.BrightGold, false, true);
                }

                // draw type icon
                spriteBatch.DrawOnCtrl(this, objectiveEntity.Icon, new Rectangle(dest.X - width / 2, dest.Y - height / 2, width, height), objectiveEntity.Icon.Bounds, teamColor);

                // draw claimed indicator
                var scale = 0.5;
                if (objectiveEntity.IsClaimed())
                {
                    if (MistwarModule.ModuleInstance.UseCustomIconsSetting.Value)
                        spriteBatch.DrawOnCtrl(this, objectiveEntity.CustomClaimedTexture,
                            new Rectangle(dest.X - width - 5, dest.Y + 10, (int)(scale * objectiveEntity.CustomClaimedTexture.Width), (int)(scale * objectiveEntity.CustomClaimedTexture.Height)), objectiveEntity.CustomClaimedTexture.Bounds);
                    else
                        spriteBatch.DrawOnCtrl(this, objectiveEntity.ClaimedTexture,
                            new Rectangle(dest.X + (int)(0.6 * width) - objectiveEntity.ClaimedTexture.Width / 2, dest.Y + (int)(0.9 * height) - height / 2, objectiveEntity.ClaimedTexture.Width, objectiveEntity.ClaimedTexture.Height), objectiveEntity.ClaimedTexture.Bounds);

                }

                // draw upgrade tier indicator
                if (objectiveEntity.HasUpgraded())
                {
                    if (MistwarModule.ModuleInstance.UseCustomIconsSetting.Value)
                        spriteBatch.DrawOnCtrl(this, objectiveEntity.CustomUpgradeTexture,
                            new Rectangle(dest.X + width / 2 - 2, dest.Y + 5, (int)(scale * objectiveEntity.CustomUpgradeTexture.Width), (int)(scale * objectiveEntity.CustomUpgradeTexture.Height)), objectiveEntity.CustomUpgradeTexture.Bounds);
                    else
                        spriteBatch.DrawOnCtrl(this, objectiveEntity.UpgradeTexture, new Rectangle(dest.X - objectiveEntity.UpgradeTexture.Width / 2, dest.Y - height / 2 - (int)(0.7 * objectiveEntity.UpgradeTexture.Height), objectiveEntity.UpgradeTexture.Width, objectiveEntity.UpgradeTexture.Height), objectiveEntity.UpgradeTexture.Bounds);
                }

                // draw name
                if (MistwarModule.ModuleInstance.DrawObjectiveNamesSetting.Value)
                {
                    var nameSize = _font.MeasureString(objectiveEntity.Name);
                    spriteBatch.DrawStringOnCtrl(this, objectiveEntity.Name, _font, new Rectangle(dest.X - (int) nameSize.Width / 2, dest.Y + height / 2 + 3, (int) nameSize.Width, (int) nameSize.Height), WvwObjectiveEntity.BrightGold, false, true);
                }
            }
        }
    }
}
