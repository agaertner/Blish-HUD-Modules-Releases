using Blish_HUD;
using Blish_HUD.Controls;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using Nekres.Mistwar.Entities;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Nekres.Mistwar
{
    public static class SpriteBatchExtensions
    {
        public static void DrawWvwObjectiveOnCtrl(this SpriteBatch spriteBatch, Control control, WvwObjectiveEntity objectiveEntity, Rectangle dest, float opacity = 1f, BitmapFont font = null)
        {
			if (objectiveEntity.Icon == null) return;
            font ??= GameService.Content.DefaultFont18;

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
					    var aScale = 1.3f;
					    var shapeRectangle = new RectangleF(tDest.X - aScale * tDest.Width / 2, tDest.Y - aScale * tDest.Height / 2, aScale * tDest.Width, aScale * tDest.Height);
					    spriteBatch.FillRectangle(shapeRectangle, teamColor);
					    spriteBatch.DrawRectangle(shapeRectangle, borderColor, 2);
					    break;
				    case WvwOwner.Green:
					    // draw polygon
					    var aWidth = 1.7f * tDest.Width;
					    var aHeight = 1.7f * tDest.Height;
					    var shapeDiamond = new[]
					    {
						    new Vector2(aWidth / 2, aWidth), new Vector2(0, aWidth / 2),
						    new Vector2(aWidth / 2, 0), new Vector2( aWidth, aWidth / 2)
                        };
					    spriteBatch.DrawPolygon(new Vector2(tDest.X - aWidth / 2, tDest.Y - aHeight / 2), shapeDiamond, teamColor, dest.Width);
					    spriteBatch.DrawPolygon(new Vector2(tDest.X - aWidth / 2, tDest.Y - aHeight / 2), shapeDiamond, borderColor, 2);
					    break;
				    default:
					    // draw circle
					    var circleDest = new CircleF(new Point2(tDest.X, tDest.Y), (int)(0.7 * dest.Width));
					    spriteBatch.DrawCircle(circleDest, 360, teamColor, dest.Width);
					    spriteBatch.DrawCircle(circleDest, 360, borderColor, 2);
					    break;
			    }
            }

			// draw type icon
			spriteBatch.DrawOnCtrl(control, objectiveEntity.Icon, new Rectangle(dest.X - dest.Width / 2, dest.Y - dest.Height / 2, dest.Width, dest.Height), objectiveEntity.Icon.Bounds, teamColor);

			// draw remaining duration of the protection buff
			if (objectiveEntity.HasBuff(out var remainingTime))
			{
				var text = remainingTime.ToString(@"m\:ss");
				var size = font.MeasureString(text);
				var texSize = Blish_HUD.PointExtensions.ResizeKeepAspect(new Point(objectiveEntity.BuffTexture.Width, objectiveEntity.BuffTexture.Height), (int)size.Width, (int)size.Height);
				spriteBatch.DrawOnCtrl(control, objectiveEntity.BuffTexture, new Rectangle(dest.X + texSize.X / 2, dest.Y - texSize.Y + 1, texSize.X, texSize.Y), objectiveEntity.BuffTexture.Bounds, whiteColor);
                spriteBatch.DrawStringOnCtrl(control, text, font, new Rectangle(dest.X - (int)size.Width / 2, dest.Y - (int)size.Height - dest.Height / 2 - 10, dest.Width, dest.Height), textColor, false, opacity >= 0.99f);
            }

            // draw claimed indicator
			if (objectiveEntity.IsClaimed())
			{
				spriteBatch.DrawOnCtrl(control, objectiveEntity.ClaimedTexture, new Rectangle(dest.X + (int)(0.6 * dest.Width) - objectiveEntity.ClaimedTexture.Width / 2, dest.Y + (int)(0.9 * dest.Height) - dest.Height / 2, objectiveEntity.ClaimedTexture.Width, objectiveEntity.ClaimedTexture.Height), objectiveEntity.ClaimedTexture.Bounds, whiteColor);
			}

			// draw upgrade tier indicator
			if (objectiveEntity.HasUpgraded())
			{
				spriteBatch.DrawOnCtrl(control, objectiveEntity.UpgradeTexture, new Rectangle(dest.X - objectiveEntity.UpgradeTexture.Width / 2, dest.Y - dest.Height / 2 - (int)(0.7 * objectiveEntity.UpgradeTexture.Height), objectiveEntity.UpgradeTexture.Width, objectiveEntity.UpgradeTexture.Height), objectiveEntity.UpgradeTexture.Bounds, whiteColor);
			}

			// draw name
			if (MistwarModule.ModuleInstance.DrawObjectiveNamesSetting.Value)
            {
                var nameSize = font.MeasureString(objectiveEntity.Name);
                spriteBatch.DrawStringOnCtrl(control, objectiveEntity.Name, font, new Rectangle(dest.X - (int)nameSize.Width / 2, dest.Y + dest.Height / 2 + 3, (int)nameSize.Width, (int)nameSize.Height), textColor, false, opacity >= 0.99f);
            }
		}
	}
}
