using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Color = Microsoft.Xna.Framework.Color;

namespace Nekres.Mistwar.Entities
{
    internal class WvwObjectiveEntity
    {
        private static readonly Texture2D TextureFortified = MistwarModule.ModuleInstance.ContentsManager.GetTexture("1324351.png");
        private static readonly Texture2D TextureReinforced = MistwarModule.ModuleInstance.ContentsManager.GetTexture("1324350.png");
        private static readonly Texture2D TextureSecured = MistwarModule.ModuleInstance.ContentsManager.GetTexture("1324349.png");
        private static readonly Texture2D TextureClaimed = MistwarModule.ModuleInstance.ContentsManager.GetTexture("1304078.png");
        private static readonly Texture2D TextureBuff = MistwarModule.ModuleInstance.ContentsManager.GetTexture("righteous_indignation.png");
        private static readonly Color ColorRed = new Color(213, 71, 67);
        private static readonly Color ColorGreen = new Color(73, 190, 111);
        private static readonly Color ColorBlue = new Color(100, 164, 228);
        private static readonly Color ColorNeutral = Color.DimGray;
        public static readonly Color BrightGold = new Color(223, 194, 149, 255);

        private readonly WvwObjective _internalObjective;

        public int MapId { get; }

        public string Id => _internalObjective.Id;

        public string Name => _internalObjective.Name;

        public WvwObjectiveType Type => _internalObjective.Type;

        /// <summary>
        /// Sector bounds this objective belongs to.
        /// </summary>
        public IEnumerable<Point> Bounds { get; }

        /// <summary>
        /// Center coordinates of the objective.
        /// </summary>
        public Point Center { get; }

        /// <summary>
        /// The timestamp of when the last time a change of ownership has occurred.
        /// </summary>
        public DateTime LastFlipped { get; set; }

        /// <summary>
        /// The objective owner.
        /// </summary>
        public WvwOwner Owner { get; set; }

        /// <summary>
        /// Color of the objective's owning team.
        /// </summary>
        public Color TeamColor => GetColor();

        /// <summary>
        /// Id of the guild that has claimed the objective.
        /// </summary>
        public Guid ClaimedBy { get; set; }

        /// <summary>
        /// List of guild upgrade ids.
        /// </summary>
        public IReadOnlyList<int> GuildUpgrades { get; set; }

        /// <summary>
        /// Number of dolyaks delivered to the objective.
        /// </summary>
        public int YaksDelivered { get; set; }

        /// <summary>
        /// Icon of the objective's type.
        /// </summary>
        public Texture2D Icon { get; }

        /// <summary>
        /// Duration of the protection buff applied when a change of ownership occurs.
        /// </summary>
        public TimeSpan BuffDuration { get; }

        /// <summary>
        /// Texture reflecting the upgrade tier of the objective.
        /// </summary>
        public Texture2D UpgradeTexture => GetUpgradeTierTexture();

        /// <summary>
        /// Texture indicating that a guild has claimed the objective.
        /// </summary>
        public Texture2D ClaimedTexture => TextureClaimed;

        /// <summary>
        /// Texture of the protection buff.
        /// </summary>
        public Texture2D BuffTexture => TextureBuff;

        public WvwObjectiveEntity(WvwObjective objective, Map map, ContinentFloorRegionMapSector sector)
        {
            _internalObjective = objective;

            Icon = GetTexture(objective.Type);
            MapId = map.Id;
            Bounds = sector.Bounds.Select(coord => MapUtil.Refit(coord, map.ContinentRect.TopLeft));
            Center = MapUtil.Refit(sector.Coord, map.ContinentRect.TopLeft);
            LastFlipped = DateTime.MinValue.ToUniversalTime();
            BuffDuration = new TimeSpan(0, 5, 0);
        }

        private Texture2D GetTexture(WvwObjectiveType type)
        {
            switch (type)
            {
                case WvwObjectiveType.Camp:
                case WvwObjectiveType.Castle:
                case WvwObjectiveType.Keep:
                case WvwObjectiveType.Tower:
                    return MistwarModule.ModuleInstance.ContentsManager.GetTexture($"{type}.png");
                default: return null;
            }
        }

        private Color GetColor()
        {
            switch (Owner)
            {
                case WvwOwner.Red:
                    return ColorRed;
                case WvwOwner.Blue:
                    return ColorBlue;
                case WvwOwner.Green:
                    return ColorGreen;
                default:
                    return ColorNeutral;
            }
        }

        public bool IsClaimed()
        {
            return !ClaimedBy.Equals(Guid.Empty);
        }

        public bool HasGuildUpgrades()
        {
            return GuildUpgrades.IsNullOrEmpty();
        }

        public bool HasUpgraded()
        {
            return YaksDelivered >= 20;
        }

        public bool HasBuff(out TimeSpan remainingTime)
        {
            var buffTime = DateTime.UtcNow.Subtract(LastFlipped);
            remainingTime = BuffDuration.Subtract(buffTime);
            return remainingTime.Ticks > 0;
        }

        private Texture2D GetUpgradeTierTexture()
        {
            return YaksDelivered >= 140 ? TextureFortified : YaksDelivered >= 60 ? TextureReinforced : TextureSecured;
        }
    }
}
