using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
namespace Nekres.Stream_Out.UI.Models
{
    public class CustomSettingsModel
    {
        public enum Social
        {
            KoFi,
            Twitch
        }

        private readonly Dictionary<Social, string> _socialUrls;
        private readonly Dictionary<Social, Texture2D> _socialLogos;

        public SettingCollection Settings { get; }

        private ContentsManager ContentsManager => StreamOutModule.Instance.ContentsManager;
        public CustomSettingsModel(SettingCollection settings)
        {
            Settings = settings;
            _socialUrls = new Dictionary<Social, string>
            {
                {Social.KoFi, "https://ko-fi.com/TypoTiger"},
                {Social.Twitch, "https://twitch.com/sNekCmd"}
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
