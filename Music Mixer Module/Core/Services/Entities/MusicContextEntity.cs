using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        [BsonField("artist")]
        public string Artist { get; set; }

        [BsonField("uri")]
        public string Uri { get; set; }

        [BsonField("duration")]
        public TimeSpan Duration { get; set; }

        [BsonField("mapIds")]
        public List<int> MapIds { get; set; }

        [BsonField("excludedMapIds")]
        public List<int> ExcludedMapIds { get; set; }

        [BsonField("sectorIds")]
        public List<int> SectorIds { get; set; }

        [BsonField("dayTimes")]
        public List<TyrianTime> DayTimes { get; set; }

        [BsonField("mountTypes")]
        public List<MountType> MountTypes { get; set; }

        [BsonField("states")]
        public List<Gw2StateService.State> States { get; set; }

        public MusicContextModel ToModel()
        {
            return new MusicContextModel(this.Title, this.Artist, this.Uri, this.Duration)
            {
                Id = this.Id,
                MapIds = new ObservableCollection<int>(this.MapIds),
                SectorIds = new ObservableCollection<int>(this.SectorIds),
                DayTimes = new ObservableCollection<TyrianTime>(this.DayTimes),
                MountTypes = new ObservableCollection<MountType>(this.MountTypes),
                States = new ObservableCollection<Gw2StateService.State>(this.States)
            };
        }

        public static bool CanPlay(MusicContextEntity entity)
        {
            return (!entity.DayTimes.Any() || entity.DayTimes.Contains(TyrianTimeUtil.GetCurrentDayCycle()))
                   && (!entity.MapIds.Any() || entity.MapIds.Contains(GameService.Gw2Mumble.CurrentMap.Id))
                   && (!entity.ExcludedMapIds.Any() || !entity.ExcludedMapIds.Contains(GameService.Gw2Mumble.CurrentMap.Id))
                   && (!entity.MountTypes.Any() || entity.MountTypes.Contains(GameService.Gw2Mumble.PlayerCharacter.CurrentMount))
                   && (!entity.States.Any() || entity.States.Contains(MusicMixer.Instance.Gw2State.CurrentState));
        }

        public static MusicContextEntity FromModel(MusicContextModel model)
        {
            return new MusicContextEntity
            {
                Id = model.Id,
                Title = model.Title,
                Artist = model.Artist,
                Uri = model.Uri,
                Duration = model.Duration,
                MapIds = model.MapIds.ToList(),
                ExcludedMapIds = model.ExcludedMapIds.ToList(),
                SectorIds = model.SectorIds.ToList(),
                DayTimes = model.DayTimes.ToList(),
                MountTypes = model.MountTypes.ToList(),
                States = model.States.ToList(),
            };
        }
    }
}
