using System.Collections.Generic;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework.Graphics;
using Stopwatch;

namespace Nekres.Stopwatch.UI.Models
{
    public class CustomSettingsModel
    {
        public SettingCollection Settings { get; private set; }

        public enum Social
        {
            KoFi
        }

        private readonly Dictionary<Social, string> _socialUrls;
        private readonly Dictionary<Social, Texture2D> _socialLogos;

        private ContentsManager ContentsManager => StopwatchModule.ModuleInstance.ContentsManager;
        public CustomSettingsModel(SettingCollection settings)
        {
            Settings = settings;
            _socialUrls = new Dictionary<Social, string>
            {
                {Social.KoFi, "https://ko-fi.com/TypoTiger"},
            };
            _socialLogos = new Dictionary<Social, Texture2D>
            {
                {Social.KoFi, ContentsManager.GetTexture(@"socials\ko-fi-logo.png")},
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
