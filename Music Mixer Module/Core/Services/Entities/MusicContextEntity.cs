using Blish_HUD;
using Gw2Sharp.Models;
using LiteDB;
using Nekres.Music_Mixer.Core.UI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nekres.Music_Mixer.Core.Services.Entities
{
    internal class MusicContextEntity
    {
        [BsonField("id")]
        public Guid Id { get; set; }

        [BsonField("state")]
        public Gw2StateService.State State { get; set; }

        [BsonField("title")]
        public string Title { get; set; }

        [BsonField("artist")]
        public string Artist { get; set; }

        [BsonField("uri")]
        public string Uri { get; set; }

        [BsonField("audioUrl")]
        public string AudioUrl { get; set; }

        [BsonField("duration")]
        public TimeSpan Duration { get; set; }

        [BsonField("regionIds")]
        public List<int> RegionIds { get; set; }

        [BsonField("excludedMapIds")]
        public List<int> ExcludedMapIds { get; set; }

        [BsonField("dayTimes")]
        public List<TyrianTime> DayTimes { get; set; }

        [BsonField("mountTypes")]
        public List<MountType> MountTypes { get; set; }

        public MusicContextModel ToModel()
        {
            return new MusicContextModel(this.State, this.Title, this.Artist, this.Uri, this.Duration)
            {
                Id = this.Id,
                AudioUrl = this.AudioUrl,
                RegionIds = new ObservableCollection<int>(this.RegionIds),
                ExcludedMapIds = new ObservableCollection<int>(this.ExcludedMapIds),
                DayTimes = new ObservableCollection<TyrianTime>(this.DayTimes),
                MountTypes = new ObservableCollection<MountType>(this.MountTypes)
            };
        }

        public static bool CanPlay(MusicContextEntity entity)
        {
            return (!entity.DayTimes.Any() || entity.DayTimes.Contains(TyrianTimeUtil.GetCurrentDayCycle()))
                   && (!entity.RegionIds.Any() || entity.RegionIds.Contains(MusicMixer.Instance.MapService.CurrentRegion))
                   && (!entity.ExcludedMapIds.Any() || !entity.ExcludedMapIds.Contains(GameService.Gw2Mumble.CurrentMap.Id))
                   && (!entity.MountTypes.Any() || entity.MountTypes.Contains(GameService.Gw2Mumble.PlayerCharacter.CurrentMount))
                   && entity.State == MusicMixer.Instance.Gw2State.CurrentState;
        }

        public static MusicContextEntity FromModel(MusicContextModel model)
        {
            return new MusicContextEntity
            {
                Id = model.Id,
                Title = model.Title,
                Artist = model.Artist,
                Uri = model.Uri,
                AudioUrl = model.AudioUrl,
                Duration = model.Duration,
                State = model.State,
                RegionIds = model.RegionIds.ToList(),
                ExcludedMapIds = model.ExcludedMapIds.ToList(),
                DayTimes = model.DayTimes.ToList(),
                MountTypes = model.MountTypes.ToList()
            };
        }
    }
}
