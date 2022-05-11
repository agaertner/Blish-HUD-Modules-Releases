using System.Collections.Generic;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework.Graphics;

namespace Nekres.Inquest_Module.UI.Models
{
    public class CustomSettingsModel
    {
        public string PolicyMacrosAndMacroUse = @"https://help.guildwars2.com/hc/articles/360013762153-Policy-Macros-and-Macro-Use";

        public SettingCollection Settings { get; private set; }

        public enum Social
        {
            KoFi,
            Twitch
        }

        private readonly Dictionary<Social, string> _socialUrls;
        private readonly Dictionary<Social, Texture2D> _socialLogos;

        private ContentsManager ContentsManager => InquestModule.ModuleInstance.ContentsManager;
        public CustomSettingsModel(SettingCollection settings)
        {
            Settings = settings;
            _socialUrls = new Dictionary<Social, string>
            {
                {Social.KoFi, "https://ko-fi.com/TypoTiger"},
                {Social.Twitch, "https://www.twitch.tv/sNekCmd"},
            };
            _socialLogos = new Dictionary<Social, Texture2D>
            {
                {Social.KoFi, ContentsManager.GetTexture(@"socials\ko-fi-logo.png")},
                {Social.Twitch, ContentsManager.GetTexture(@"socials\twitch-logo.png")}
            };
        }

        public Texture2D GetSocialLogo(Social social)
        {
            return _socialLogos[social];
        }

        public string GetSocialUrl(Social social)
        {
            return _socialUrls[social];
        }
    }
}
