using Nekres.Music_Mixer.Core.Services;
namespace Nekres.Music_Mixer
{
    internal static class Gw2StateExtensions
    {
        public static bool IsIntermediate(this Gw2StateService.State state)
        {
            return state is Gw2StateService.State.Battle 
                or Gw2StateService.State.Mounted 
                or Gw2StateService.State.Submerged 
                or Gw2StateService.State.Victory 
                or Gw2StateService.State.Defeated 
                or Gw2StateService.State.StandBy;
        }
    }
}
