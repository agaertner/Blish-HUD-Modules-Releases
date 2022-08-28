using Blish_HUD;
using Gw2Sharp.Models;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Color = Microsoft.Xna.Framework.Color;
using Point = System.Drawing.Point;

namespace Nekres.Mistwar.Entities
{
    public enum WvwObjectiveTier
    {
        Supported,
        Secured,
        Reinforced,
        Fortified
    }

    public class WvwObjectiveEntity
    {
        private static readonly Texture2D TextureFortified = MistwarModule.ModuleInstance.ContentsManager.GetTexture("1324351.png");
        private static readonly Texture2D TextureReinforced = MistwarModule.ModuleInstance.ContentsManager.GetTexture("1324350.png");
        private static readonly Texture2D TextureSecured = MistwarModule.ModuleInstance.ContentsManager.GetTexture("1324349.png");
        private static readonly Texture2D TextureClaimed = MistwarModule.ModuleInstance.ContentsManager.GetTexture("1304078.png");
        private static readonly Texture2D TextureClaimedRepGuild = MistwarModule.ModuleInstance.ContentsManager.GetTexture("1304077.png");
        private static readonly Texture2D TextureBuff = MistwarModule.ModuleInstance.ContentsManager.GetTexture("righteous_indignation.png");

        private static readonly Texture2D TextureRuinEstate = MistwarModule.ModuleInstance.ContentsManager.GetTexture("ruin_estate.png");
        private static readonly Texture2D TextureRuinTemple = MistwarModule.ModuleInstance.ContentsManager.GetTexture("ruin_temple.png");
        private static readonly Texture2D TextureRuinOverlook = MistwarModule.ModuleInstance.ContentsManager.GetTexture("ruin_overlook.png");
        private static readonly Texture2D TextureRuinHollow = MistwarModule.ModuleInstance.ContentsManager.GetTexture("ruin_hollow.png");
        private static readonly Texture2D TextureRuinAscent = MistwarModule.ModuleInstance.ContentsManager.GetTexture("ruin_ascent.png");
        private static readonly Texture2D TextureRuinOther = MistwarModule.ModuleInstance.ContentsManager.GetTexture("ruin_other.png");

        private static readonly Texture2D TextureWayPoint = MistwarModule.ModuleInstance.ContentsManager.GetTexture("157353.png");
        private static readonly Texture2D TextureWayPointHover = MistwarModule.ModuleInstance.ContentsManager.GetTexture("60970.png");
        private static readonly Texture2D TextureWayPointContested = MistwarModule.ModuleInstance.ContentsManager.GetTexture("102349.png");

        private static readonly IReadOnlyDictionary<string, Texture2D> _ruinsTexLookUp = new Dictionary<string, Texture2D>
        {
            {"95-62", TextureRuinTemple}, // Temple of the Fallen
            {"96-62", TextureRuinTemple}, // Temple of Lost Prayers
            {"1099-121", TextureRuinOther}, // Darra's Maze

            {"96-66", TextureRuinAscent}, // Carver's Ascent
            {"95-66", TextureRuinAscent}, // Patrick's Ascent
            {"1099-118", TextureRuinOther}, // Higgins's Ascent

            {"96-63", TextureRuinHollow}, // Battle's Hollow
            {"95-63", TextureRuinHollow}, // Norfolk's Hollow
            {"1099-119", TextureRuinOther}, // Bearce's Dwelling

            {"96-65", TextureRuinOverlook}, // Orchard Overlook
            {"95-65", TextureRuinOverlook}, // Cohen's Overlook
            {"1099-120", TextureRuinOther}, // Zak's Overlook

            {"96-64", TextureRuinEstate}, // Bauer's Estate
            {"95-64", TextureRuinEstate}, // Gertzz's Estate 
            {"1099-122", TextureRuinOther} // Tilly's Encampment
        };

        private static readonly Color ColorRed = new Color(213, 71, 67);
        private static readonly Color ColorGreen = new Color(73, 190, 111);
        private static readonly Color ColorBlue = new Color(100, 164, 228);
        private static readonly Color ColorNeutral = Color.DimGray;
        public static readonly Color BrightGold = new Color(223, 194, 149, 255);

        private readonly WvwObjective _internalObjective;
        private readonly ContinentFloorRegionMapSector _internalSector;

        public int MapId { get; }

        public string Id => _internalObjective.Id;

        public string Name => _internalObjective.Name;

        public WvwObjectiveType Type => _internalObjective.Type;

        /// <summary>
        /// Sector bounds this objective belongs to.
        /// </summary>
        public IEnumerable<Point> Bounds { get; }

        /// <summary>
        /// Center coordinates of the objective on the world map.
        /// </summary>
        public Point Center { get; }

        /// <summary>
        /// Position of the objective in the game world.
        /// </summary>
        public Vector3 WorldPosition { get; }

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
        /// Number of Dolyaks delivered to the objective.
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
        public Texture2D ClaimedTexture => ClaimedBy.Equals(MistwarModule.ModuleInstance.WvwService.CurrentGuild) ? TextureClaimedRepGuild : TextureClaimed;

        /// <summary>
        /// Texture of the protection buff.
        /// </summary>
        public Texture2D BuffTexture => TextureBuff;

        private float _opacity;
        /// <summary>
        /// Opacity of icon and text when drawn.
        /// </summary>
        public float Opacity => GetOpacity();

        public List<ContinentFloorRegionMapPoi> WayPoints { get; }

        public WvwObjectiveEntity(WvwObjective objective, ContinentFloorRegionMap map)
        {
            _internalObjective = objective;
            _internalSector = map.Sectors[objective.SectorId];
            _opacity = 1f;
            Icon = GetTexture(objective.Type);
            MapId = map.Id;
            Bounds = _internalSector.Bounds.Select(coord => MapUtil.Refit(coord, map.ContinentRect.TopLeft));
            Center = MapUtil.Refit(_internalSector.Coord, map.ContinentRect.TopLeft);
            LastFlipped = DateTime.MinValue.ToUniversalTime();
            BuffDuration = new TimeSpan(0, 5, 0);
            WorldPosition = CalculateWorldPosition(map);

            WayPoints = map.PointsOfInterest.Values.Where(x => x.Type == PoiType.Waypoint).Where(y =>
                PolygonUtil.InBounds(new Vector2((float) y.Coord.X, (float) y.Coord.Y), _internalSector.Bounds.Select(z => new Vector2((float)z.X, (float)z.Y)).ToList())).ToList();

            foreach (var wp in WayPoints)
            {
                var fit = MapUtil.Refit(wp.Coord, map.ContinentRect.TopLeft);
                wp.Coord = new Coordinates2(fit.X, fit.Y);
            }
        }

        public Texture2D GetWayPointIcon(bool hover)
        {
            return this.Owner == MistwarModule.ModuleInstance.WvwService.CurrentTeam ? 
                hover ? TextureWayPointHover : TextureWayPoint : TextureWayPointContested;
        }

        public bool IsOwned()
        {
            return (int)this.Owner <= 1;
        }

        public bool IsClaimed()
        {
            return !ClaimedBy.Equals(Guid.Empty);
        }

        public bool HasGuildUpgrades()
        {
            return !GuildUpgrades.IsNullOrEmpty();
        }

        public bool HasUpgraded()
        {
            return YaksDelivered >= 20;
        }

        public bool HasEmergencyWaypoint()
        {
            return HasGuildUpgrades() && GuildUpgrades.Contains(178);
        }

        public bool HasRegularWaypoint()
        {
            return IsSpawn() || GetTier() == WvwObjectiveTier.Fortified && (Type == WvwObjectiveType.Keep || Type == WvwObjectiveType.Castle);
        }

        public bool IsSpawn()
        {
            return this.Type == WvwObjectiveType.Spawn;
        }

        public WvwObjectiveTier GetTier()
        {
            return YaksDelivered >= 140 ? WvwObjectiveTier.Fortified : YaksDelivered >= 60 ? WvwObjectiveTier.Reinforced : YaksDelivered >= 20 ? WvwObjectiveTier.Secured : WvwObjectiveTier.Supported;
        }

        public bool HasBuff(out TimeSpan remainingTime)
        {
            var buffTime = DateTime.UtcNow.Subtract(LastFlipped);
            remainingTime = BuffDuration.Subtract(buffTime);
            return remainingTime.Ticks > 0;
        }

        public float GetDistance()
        {
            return WorldPosition.Distance(GameService.Gw2Mumble.PlayerCamera.Position);
        }

        private Vector3 CalculateWorldPosition(ContinentFloorRegionMap map)
        {
            var v = _internalObjective.Coord;
            if (_internalObjective.Id.Equals("38-15") && Math.Abs(v.X - 11766.3) < 1 && Math.Abs(v.Y - 14793.5) < 1 && Math.Abs(v.Z - (-2133.39)) < 1) 
            {
                v = new Coordinates3(11462.5f, 15600 - 2650 / 24, _internalObjective.Coord.Z - 500); // Langor fix-hack
            }
            var r = map.ContinentRect;
            var offset = new Vector3(
                (float)((r.TopLeft.X + r.BottomRight.X) / 2.0f),
                0,
                (float)((r.TopLeft.Y + r.BottomRight.Y) / 2.0f));
            return new Vector3(
                WorldUtil.GameToWorldCoord((float)((v.X - offset.X) * 24)),
                WorldUtil.GameToWorldCoord((float)(-(v.Y - offset.Z) * 24)),
                WorldUtil.GameToWorldCoord((float)-v.Z));
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
                case WvwObjectiveType.Ruins:
                    return _ruinsTexLookUp.TryGetValue(this.Id, out var tex) ? tex : null; 
                default: return null;
            }
        }

        private Color GetColor()
        {
            return Owner switch
            {
                WvwOwner.Red => ColorRed,
                WvwOwner.Blue => ColorBlue,
                WvwOwner.Green => ColorGreen,
                _ => ColorNeutral
            };
        }

        private Texture2D GetUpgradeTierTexture()
        {
            return GetTier() switch
            {
                WvwObjectiveTier.Fortified => TextureFortified,
                WvwObjectiveTier.Reinforced => TextureReinforced,
                WvwObjectiveTier.Secured => TextureSecured,
                _ => ContentService.Textures.TransparentPixel
            };
        }

        private float GetOpacity()
        {
            _opacity = MathUtil.Clamp(MathUtil.Map((GameService.Gw2Mumble.PlayerCamera.Position - this.WorldPosition).Length(), MistwarModule.ModuleInstance.MaxViewDistanceSetting.Value * 50, _opacity, 0f, 1f), 0f, 1f);
            return _opacity;
        }
    }
}
