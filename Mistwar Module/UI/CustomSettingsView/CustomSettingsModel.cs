using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Nekres.Mistwar.UI.CustomSettingsView
{
    public class CustomSettingsModel
    {
        public SettingCollection Settings { get; private set; }

        public enum Social
        {
            KoFi
        }

        private readonly IReadOnlyDictionary<Social, string> _socialUrls;
        private readonly IReadOnlyDictionary<Social, Texture2D> _socialLogos;
        private readonly IReadOnlyDictionary<Social, string> _socialTexts;

        private readonly ContentsManager _contentsManager;

        public CustomSettingsModel(SettingCollection settings, ContentsManager contentsManager)
        {
            Settings = settings;
            _contentsManager = contentsManager;
            _socialUrls = new Dictionary<Social, string>
            {
                {Social.KoFi, "https://ko-fi.com/Nekres"}
            };
            _socialLogos = new Dictionary<Social, Texture2D>
            {
                {Social.KoFi, _contentsManager.GetTexture(@"socials\ko-fi-logo.png")}
            };
            _socialTexts = new Dictionary<Social, string>
            {
                {Social.KoFi, "Support Me on Ko-Fi"}
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

        public string GetSocialText(Social social)
        {
            return _socialTexts[social];
        }
    }
}
