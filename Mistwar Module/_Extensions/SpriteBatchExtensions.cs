using Blish_HUD;
using Blish_HUD.Controls;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Nekres.Mistwar.Entities;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Nekres.Mistwar
{
    public static class SpriteBatchExtensions
    {
        public static void DrawWvwObjectiveOnCtrl(this SpriteBatch spriteBatch, Control control, WvwObjectiveEntity objectiveEntity, Rectangle dest, float opacity = 1f, float scale = 1f, bool drawName = true, bool drawDistance = false)
        {
			if (objectiveEntity.Icon == null) return;

            var font = scale < 0.25 ? GameService.Content.DefaultFont12 : 
                       scale < 0.5f ? GameService.Content.DefaultFont14 :
                       scale < 0.75f ? GameService.Content.DefaultFont16 : GameService.Content.DefaultFont18;

            // Configure colors and apply opacity
			var teamColor = objectiveEntity.TeamColor.GetColorBlindType(MistwarModule.ModuleInstance.ColorTypeSetting.Value) * opacity;
            var borderColor = Color.Black * opacity;
            var textColor = WvwObjectiveEntity.BrightGold * (opacity + 0.2f);
            var whiteColor = Color.White * opacity;

            var tDest = dest.ToBounds(control.AbsoluteBounds); // Clamped destination. Native DrawShape calls expect it.

			if (objectiveEntity.Type != WvwObjectiveType.Ruins) {
                var shapeType = MistwarModule.ModuleInstance.TeamShapesSetting.Value ? objectiveEntity.Owner : WvwOwner.Red;
			    switch (shapeType)
			    {
				    case WvwOwner.Blue:
					    // draw rectangle
					    var aScale = scale * 1.3f;
					    var shapeRectangle = new RectangleF(tDest.X - aScale * tDest.Width / 2, tDest.Y - aScale * tDest.Height / 2, aScale * tDest.Width, aScale * tDest.Height);
					    spriteBatch.FillRectangle(shapeRectangle, teamColor);
					    spriteBatch.DrawRectangle(shapeRectangle, borderColor, 2);
					    break;
				    case WvwOwner.Green:
					    // draw polygon
					    var aWidth = scale * 1.7f * tDest.Width;
					    var aHeight = scale * 1.7f * tDest.Height;
					    var shapeDiamond = new[]
					    {
						    new Vector2(aWidth / 2, aWidth), new Vector2(0, aWidth / 2),
						    new Vector2(aWidth / 2, 0), new Vector2( aWidth, aWidth / 2)
                        };
					    spriteBatch.DrawPolygon(new Vector2(tDest.X - aWidth / 2, tDest.Y - aHeight / 2), shapeDiamond, teamColor, scale * dest.Width);
					    spriteBatch.DrawPolygon(new Vector2(tDest.X - aWidth / 2, tDest.Y - aHeight / 2), shapeDiamond, borderColor, 2);
					    break;
				    default:
					    // draw circle
					    var circleDest = new CircleF(new Point2(tDest.X, tDest.Y), (int)(scale * 0.7 * dest.Width));
					    spriteBatch.DrawCircle(circleDest, 360, teamColor, scale * dest.Width);
					    spriteBatch.DrawCircle(circleDest, 360, borderColor, 2);
					    break;
			    }
            }

			// draw type icon
			spriteBatch.DrawOnCtrl(control, objectiveEntity.Icon, new Rectangle(dest.X - (int)(scale * dest.Width) / 2, dest.Y - (int)(scale * dest.Height) / 2, (int)(scale * dest.Width), (int)(scale * dest.Height)), objectiveEntity.Icon.Bounds, teamColor);

			// draw remaining duration of the protection buff
			if (objectiveEntity.HasBuff(out var remainingTime))
			{
				var text = remainingTime.ToString(@"m\:ss");
				var size = font.MeasureString(text);
				var texSize = Blish_HUD.PointExtensions.ResizeKeepAspect(new Point(objectiveEntity.BuffTexture.Width, objectiveEntity.BuffTexture.Height), (int)(scale * size.Width), (int)(scale * size.Height));
                var iconBnds = new Rectangle(dest.X - 20, (int)(dest.Y - scale * 80), texSize.X, texSize.Y);
                var textBnds = new Rectangle(iconBnds.Right + 3, iconBnds.Y, iconBnds.Width, iconBnds.Height);
                spriteBatch.DrawOnCtrl(control, objectiveEntity.BuffTexture, iconBnds, objectiveEntity.BuffTexture.Bounds, whiteColor);
                spriteBatch.DrawStringOnCtrl(control, text, font, textBnds, textColor, false, opacity >= 0.99f);
            }

            // draw claimed indicator
			if (objectiveEntity.IsClaimed())
			{
				spriteBatch.DrawOnCtrl(control, objectiveEntity.ClaimedTexture, new Rectangle(dest.X + (int)(scale * 0.6 * dest.Width) - (int)(scale * objectiveEntity.ClaimedTexture.Width) / 2, dest.Y + (int)(scale * 0.9 * dest.Height) - dest.Height / 2, (int)(scale * objectiveEntity.ClaimedTexture.Width), (int)(scale * objectiveEntity.ClaimedTexture.Height)), objectiveEntity.ClaimedTexture.Bounds, whiteColor);
			}

			// draw upgrade tier indicator
			if (objectiveEntity.HasUpgraded())
			{
				spriteBatch.DrawOnCtrl(control, objectiveEntity.UpgradeTexture, new Rectangle(dest.X - (int)(scale * objectiveEntity.UpgradeTexture.Width) / 2, dest.Y - (int)(scale * dest.Height) / 2 - (int)(scale * 0.7 * objectiveEntity.UpgradeTexture.Height), (int)(scale * objectiveEntity.UpgradeTexture.Width), (int)(scale * objectiveEntity.UpgradeTexture.Height)), objectiveEntity.UpgradeTexture.Bounds, whiteColor);
			}

			// draw name
			if (drawName)
            {
                var nameSize = font.MeasureString(objectiveEntity.Name);
                spriteBatch.DrawStringOnCtrl(control, objectiveEntity.Name, font, new Rectangle(dest.X - (int)nameSize.Width / 2, dest.Y + dest.Height / 2 + (int)(scale * 12), (int)nameSize.Width, (int)nameSize.Height), textColor, false, opacity >= 0.99f);
            }

            if (!drawDistance) return;
            var distance = objectiveEntity.GetDistance() - 25;
            if (distance > 1)
            {
                var text = distance >= 1000 ? $"{distance / 1000:N2}km" : $"{distance:N0}m";
                var size = font.MeasureString(text);
                spriteBatch.DrawStringOnCtrl(control, text, font, new Rectangle(dest.X - (int)size.Width / 2, dest.Y - (int)size.Height - dest.Height / 2 - (int)(scale * 80), dest.Width, dest.Height), textColor, false, opacity >= 0.99f);
            }
		}
    }
}
