using System;
using System.Collections.Generic;
using System.Linq;
using Blish_HUD;
using Gw2Sharp.Models;
using LiteDB;
using Nekres.Music_Mixer.Core.UI.Models;

namespace Nekres.Music_Mixer.Core.Services.Entities
{
    internal class MusicContextEntity
    {
        [BsonId(true)]
        public long _id { get; set; }

        [BsonField("id")]
        public Guid Id { get; set; }

        [BsonField("title")]
        public string Title { get; set; }

        [BsonField("uri")]
        public string Uri { get; set; }

        [BsonField("mapIds")]
        public List<int> MapIds { get; set; }

        [BsonField("sectorIds")]
        public List<int> SectorIds { get; set; }

        [BsonField("dayTimes")]
        public List<TyrianTime> DayTimes { get; set; }

        [BsonField("mountTypes")]
        public List<MountType> MountTypes { get; set; }

        [BsonField("states")]
        public List<Gw2StateService.State> States { get; set; }

        public static bool CanPlay(MusicContextEntity entity)
        {
            return (!entity.DayTimes.Any() || entity.DayTimes.Contains(TyrianTimeUtil.GetCurrentDayCycle()))
                   && (!entity.MapIds.Any() || entity.MapIds.Contains(GameService.Gw2Mumble.CurrentMap.Id))
                   && (!entity.MountTypes.Any() || entity.MountTypes.Contains(GameService.Gw2Mumble.PlayerCharacter.CurrentMount))
                   && (!entity.States.Any() || entity.States.Contains(MusicMixerModule.ModuleInstance.Gw2State.CurrentState));
        }

        public static MusicContextEntity FromModel(MusicContextModel model)
        {
            return new MusicContextEntity
            {
                Id = model.Id,
                Title = model.Title,
                MapIds = model.MapIds,
                SectorIds = model.SectorIds,
                Uri = model.Uri,
                DayTimes = model.DayTimes,
                MountTypes = model.MountTypes,
                States = model.States,
            };
        }
    }
}
