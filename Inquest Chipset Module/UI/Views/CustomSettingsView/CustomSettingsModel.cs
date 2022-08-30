using Blish_HUD.Settings;

namespace Nekres.Inquest_Module.UI.Models
{
    public class CustomSettingsModel
    {
        public readonly string PolicyMacrosAndMacroUse = @"https://help.guildwars2.com/hc/articles/360013762153-Policy-Macros-and-Macro-Use";

        public SettingCollection Settings { get; private set; }

        public CustomSettingsModel(SettingCollection settings)
        {
            Settings = settings;
        }
    }
}
