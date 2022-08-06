using Blish_HUD;
using LiteDB;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nekres.Chat_Shorts.UI.Models
{
    public enum GameMode
    {
        All,
        PvE,
        WvW,
        PvP
    }

    internal class MacroEntity
    {
        [BsonId(true)]
        public int _id { get; set; }

        [BsonField("id")]
        public Guid Id { get; set; }

        [BsonField("title")]
        public string Title { get; set; }

        [BsonField("modifierKey")]
        public ModifierKeys ModifierKey { get; set; }

        [BsonField("primaryKey")]
        public Keys PrimaryKey { get; set; }

        [BsonField("squadBroadcast")]
        public bool SquadBroadcast { get; set; }

        [BsonField("mapId")]
        public List<int> MapIds { get; set; }

        [BsonField("excludedMapId")]
        public List<int> ExcludedMapIds { get; set; }

        [BsonField("gameMode")]
        public GameMode GameMode { get; set; }

        [BsonField("text")]
        public string Text { get; set; }

        public MacroEntity(Guid id)
        {
            this.Id = id;
            this.MapIds = new List<int>();
            this.ExcludedMapIds = new List<int>();
            this.ModifierKey = ModifierKeys.None;
            this.PrimaryKey = Keys.None;
            this.Title = string.Empty;
            this.Text = string.Empty;
        }

        public static bool CanActivate(MacroEntity e)
        {
            return (e.GameMode == MapUtil.GetCurrentGameMode() || e.GameMode == GameMode.All) &&
                   e.MapIds.Any(id => id == GameService.Gw2Mumble.CurrentMap.Id || !e.MapIds.Any()) &&
                   e.ExcludedMapIds.All(id => id != GameService.Gw2Mumble.CurrentMap.Id) &&
                   (!e.SquadBroadcast || GameService.Gw2Mumble.PlayerCharacter.IsCommander);
        }
    }
}
