using Blish_HUD;
using Gw2Sharp.Models;
using Nekres.Music_Mixer.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nekres.Music_Mixer.Core.UI.Models
{
    internal class MusicContextModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }

        public string Uri { get; set; }

        public List<int> MapIds { get; set; }

        public List<int> SectorIds { get; set; }

        public List<TyrianTime> DayTimes { get; set; }

        public List<MountType> MountTypes { get; set; }

        public List<Gw2StateService.State> States { get; set; }

        public MusicContextModel()
        {
            this.Id = Guid.NewGuid();
            this.Title = string.Empty;
            this.Uri = string.Empty;
            this.MapIds = new List<int>();
            this.SectorIds = new List<int>();
            this.DayTimes = new List<TyrianTime>();
            this.MountTypes = new List<MountType>();
            this.States = new List<Gw2StateService.State>();
        }

        public static bool CanPlay(MusicContextModel model)
        {
            return (!model.DayTimes.Any() || model.DayTimes.Contains(TyrianTimeUtil.GetCurrentDayCycle()))
                && (!model.MapIds.Any() || model.MapIds.Contains(GameService.Gw2Mumble.CurrentMap.Id))
                && (!model.MountTypes.Any() || model.MountTypes.Contains(GameService.Gw2Mumble.PlayerCharacter.CurrentMount))
                && (!model.States.Any() || model.States.Contains(MusicMixerModule.ModuleInstance.Gw2State.CurrentState));
        }
    }
}
