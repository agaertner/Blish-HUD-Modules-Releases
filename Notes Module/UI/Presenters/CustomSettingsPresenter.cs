using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Flurl.Http;
using Nekres.Notes.Core.Models;
using Nekres.Notes.UI.Core;
using Nekres.Notes.UI.Models;
using Nekres.Notes.UI.Views;
using Newtonsoft.Json;

namespace Nekres.Notes.UI.Presenters
{
    public class CustomSettingsPresenter : Presenter<CustomSettingsView, CustomSettingsModel>
    {
        private readonly string _subTokenClaimType;
        private readonly CallbackListener _listener;
        private readonly JwtSecurityTokenHandler _tokenHandler;

        public CustomSettingsPresenter(CustomSettingsView view, CustomSettingsModel model) : base(view, model)
        {
            _subTokenClaimType = "gw2:tokens";
            _listener = new CallbackListener();
            _tokenHandler = new JwtSecurityTokenHandler();
        }

        protected override Task<bool> Load(IProgress<string> progress)
        {
            this.View.SocialButtonClicked += View_SocialButtonClicked;
            this.View.LoginButtonClicked += View_LoginButtonClicked;
            return base.Load(progress);
        }

        protected override void Unload()
        {
            _listener.Stop();
            this.View.SocialButtonClicked -= View_SocialButtonClicked;
        }

        private void View_SocialButtonClicked(object sender, EventArgs e)
        {
            BrowserUtil.OpenInDefaultBrowser(((Control)sender).BasicTooltipText);
        }

        private async void View_LoginButtonClicked(object sender, EventArgs e)
        {
            _listener.Start(OnAuthorizedCallback);
            BrowserUtil.OpenInDefaultBrowser($"https://localhost:7168/api/Gw2Auth/auth?displayName={Environment.MachineName}");
        }

        private async void OnAuthorizedCallback(HttpListenerContext context)
        {
            var response = AuthResponseModel.FromQuery(context.Request.QueryString);
            if (response.IsError())
                NotesModule.Logger.Info(response.ErrorDescription);

            var result = await $"https://localhost:7168/api/Gw2Auth/login?code={response.Code}&state={response.State}".GetStringAsync();

            var userLogin = UserLoginModel.FromResponse(result);

            NotesModule.Logger.Info($"Successfully authorized through GW2Auth. Expires {userLogin.Expires}");

            var cache = Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GW2Auth"));

            File.WriteAllText(Path.Combine(cache.FullName, "auth.json"), JsonConvert.SerializeObject(userLogin));
        }

        private Dictionary<string, JwtSubtokenModel>? GetTokensFromJwt(string jwt)
        {
            var jwtToken = _tokenHandler.ReadJwtToken(jwt);

            var claim = jwtToken.Claims.FirstOrDefault(x => x.Type.Equals(_subTokenClaimType));

            return claim == null ? null : JsonConvert.DeserializeObject<Dictionary<string, JwtSubtokenModel>>(claim.Value);
        }
    }
}
