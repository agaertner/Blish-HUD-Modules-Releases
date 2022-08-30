using Blish_HUD.Settings;

namespace Nekres.Stopwatch.UI.Models
{
    public class CustomSettingsModel
    {
        public SettingCollection Settings { get; private set; }

        public CustomSettingsModel(SettingCollection settings)
        {
            Settings = settings;
        }
    }
}
