using Blish_HUD;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using Nekres.Stream_Out.Core.Services;
using Nekres.Stream_Out.UI.Models;
using Nekres.Stream_Out.UI.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
namespace Nekres.Stream_Out
{
    [Export(typeof(Module))]
    public class StreamOutModule : Module
    {

        internal static readonly Logger Logger = Logger.GetLogger<StreamOutModule>();

        internal static StreamOutModule ModuleInstance;

        #region Service Managers
        internal SettingsManager SettingsManager => ModuleParameters.SettingsManager;
        internal ContentsManager ContentsManager => ModuleParameters.ContentsManager;
        internal DirectoriesManager DirectoriesManager => ModuleParameters.DirectoriesManager;
        internal Gw2ApiManager Gw2ApiManager => ModuleParameters.Gw2ApiManager;
        #endregion

        [ImportingConstructor]
        public StreamOutModule([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters) { ModuleInstance = this; }

        internal SettingEntry<bool> OnlyLastDigitSettingEntry;
        internal SettingEntry<UnicodeSigning> AddUnicodeSymbols;
        internal SettingEntry<bool> UseCatmanderTag;

        internal SettingEntry<DateTime?> ResetTimeWvW;
        internal SettingEntry<DateTime?> ResetTimeDaily;

        internal SettingEntry<int> SessionKillsWvW;
        internal SettingEntry<int> SessionKillsWvwDaily;
        internal SettingEntry<int> SessionKillsPvP;

        internal SettingEntry<int> TotalKillsAtResetWvW;
        internal SettingEntry<int> TotalKillsAtResetPvP;

        internal SettingEntry<int> TotalDeathsAtResetWvW;
        internal SettingEntry<int> TotalDeathsAtResetDaily;

        internal SettingEntry<int> SessionDeathsWvW;
        internal SettingEntry<int> SessionDeathsDaily;

        internal SettingEntry<Guid> AccountGuid;
        internal SettingEntry<string> AccountName;

        internal bool HasSubToken { get; private set; }

        private DateTime? _prevApiRequestTime;

        internal enum UnicodeSigning
        {
            None,
            Prefixed,
            Suffixed
        }

        private List<IExportService> _allExportServices;

        protected override void DefineSettings(SettingCollection settings)
        {
            OnlyLastDigitSettingEntry = settings.DefineSetting("OnlyLastDigits",true, () => "Only Output Last Digits of Server Address", () => "Only outputs the last digits of the server address you are currently connected to.\nThis is the address shown when entering \"/ip\" in chat.");
            AddUnicodeSymbols = settings.DefineSetting("UnicodeSymbols",UnicodeSigning.Suffixed, () => "Numeric Value Signing", () => "The way numeric values should be signed with unicode symbols.");
            UseCatmanderTag = settings.DefineSetting("CatmanderTag", false, () => "Use Catmander Tag", () => $"Replaces the Commander icon with the Catmander icon if you tag up as Commander in-game.");

            var cache = settings.AddSubCollection("CachedValues");
            cache.RenderInUi = false;
            AccountGuid = cache.DefineSetting("AccountGuid", Guid.Empty);
            AccountName = cache.DefineSetting("AccountName", string.Empty);
            ResetTimeWvW = cache.DefineSetting<DateTime?>("ResetTimeWvW", null);
            ResetTimeDaily = cache.DefineSetting<DateTime?>("ResetTimeDaily", null);
            SessionKillsWvW = cache.DefineSetting("SessionKillsWvW", 0);
            SessionKillsWvwDaily = cache.DefineSetting("SessionsKillsWvWDaily", 0);
            SessionKillsPvP = cache.DefineSetting("SessionKillsPvP", 0);
            SessionDeathsWvW = cache.DefineSetting("SessionDeathsWvW", 0);
            SessionDeathsDaily = cache.DefineSetting("SessionDeathsDaily", 0);
            TotalKillsAtResetWvW = cache.DefineSetting("TotalKillsAtResetWvW", 0);
            TotalKillsAtResetPvP = cache.DefineSetting("TotalKillsAtResetPvP", 0);
            TotalDeathsAtResetWvW = cache.DefineSetting("TotalDeathsAtResetWvW", 0);
            TotalDeathsAtResetDaily = cache.DefineSetting("TotalDeathsAtResetDaily", 0);
        }

        public override IView GetSettingsView()
        {
            return new CustomSettingsView(new CustomSettingsModel(SettingsManager.ModuleSettings));
        }

        protected override void Initialize()
        {
            Gw2ApiManager.SubtokenUpdated += SubTokenUpdated;

            _allExportServices = new List<IExportService>
            {
                new CharacterService(),
                new ClientService(),
                new GuildService(),
                new KillProofService(),
                new MapService(),
                new PvpService(),
                new WalletService(),
                new WvwService()
            };
        }

        private void SubTokenUpdated(object o, ValueEventArgs<IEnumerable<TokenPermission>> e)
        {
            HasSubToken = true;
        }

        protected override async Task LoadAsync()
        {
            // Generate some placeholder files for values that depend on a privileged API connection
            foreach (var service in _allExportServices) await service.Initialize();
        }

        protected override void OnModuleLoaded(EventArgs e)
        {
            // Base handler must be called
            base.OnModuleLoaded(e);
        }

        protected override async void Update(GameTime gameTime)
        {
            if (!HasSubToken) return;

            await ResetDaily();

            if (!HasSubToken || _prevApiRequestTime.HasValue && DateTime.UtcNow.Subtract(_prevApiRequestTime.Value).TotalSeconds < 300) return;
            _prevApiRequestTime = DateTime.UtcNow;

            foreach (var service in _allExportServices) await service.Update();
        }

        /// <inheritdoc />
        protected override void Unload()
        {
            Gw2ApiManager.SubtokenUpdated -= SubTokenUpdated;
            foreach (var service in _allExportServices) service?.Dispose();

            // All static members must be manually unset
            ModuleInstance = null;
        }

        private async Task ResetDaily()
        {
            if (ResetTimeDaily.Value.HasValue && DateTime.UtcNow < ResetTimeDaily.Value) return;

            foreach (var service in _allExportServices) await service.ResetDaily();
            ResetTimeDaily.Value = Gw2Util.GetDailyResetTime();
        }
    }
}
