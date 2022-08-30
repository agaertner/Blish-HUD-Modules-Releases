using Blish_HUD;
using Blish_HUD.Extended.Core.Views;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using Nekres.Stream_Out.Core.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace Nekres.Stream_Out
{
    [Export(typeof(Module))]
    public class StreamOutModule : Module
    {

        internal static readonly Logger Logger = Logger.GetLogger<StreamOutModule>();

        internal static StreamOutModule Instance;

        #region Service Managers
        internal SettingsManager SettingsManager => ModuleParameters.SettingsManager;
        internal ContentsManager ContentsManager => ModuleParameters.ContentsManager;
        internal DirectoriesManager DirectoriesManager => ModuleParameters.DirectoriesManager;
        internal Gw2ApiManager Gw2ApiManager => ModuleParameters.Gw2ApiManager;
        #endregion

        // Settings
        internal SettingEntry<bool> OnlyLastDigitSettingEntry;
        internal SettingEntry<UnicodeSigning> AddUnicodeSymbols;
        internal SettingEntry<bool> UseCatmanderTag;

        // Export Toggles
        internal SettingEntry<bool> ExportClientInfo;
        internal SettingEntry<bool> ExportMapInfo;
        internal SettingEntry<bool> ExportWalletInfo;
        internal SettingEntry<bool> ExportPvpInfo;
        internal SettingEntry<bool> ExportWvwInfo;
        internal SettingEntry<bool> ExportGuildInfo;
        internal SettingEntry<bool> ExportCharacterInfo;
        internal SettingEntry<bool> ExportKillProofs;

        // Hidden settings cache
        internal SettingEntry<DateTime> ResetTimeWvW;
        internal SettingEntry<DateTime> ResetTimeDaily;

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

        internal string WebApiDown = "Unable to connect to the official Guild Wars 2 WebApi. Check if the WebApi is down for maintenance.";

        internal enum UnicodeSigning
        {
            None,
            Prefixed,
            Suffixed
        }

        private List<ExportService> _allExportServices;

        [ImportingConstructor]
        public StreamOutModule([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters)
        {
            Instance = this;
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            var general = settings.AddSubCollection("General", true, false);
            OnlyLastDigitSettingEntry = general.DefineSetting("OnlyLastDigits",true, () => "Only Output Last Digits of Server Address", () => "Only outputs the last digits of the server address you are currently connected to.\nThis is the address shown when entering \"/ip\" in chat.");
            UseCatmanderTag = general.DefineSetting("CatmanderTag", false, () => "Use Catmander Tag", () => $"Replaces the Commander icon with the Catmander icon if you tag up as Commander in-game.");
            AddUnicodeSymbols = general.DefineSetting("UnicodeSymbols", UnicodeSigning.Suffixed, () => "Numeric Value Signing", () => "The way numeric values should be signed with unicode symbols.");

            var toggles = settings.AddSubCollection("Export Toggles", true, false);
            ExportClientInfo = toggles.DefineSetting("clientInfo", true, 
                () => "Export Client Info", 
                () => "Client info such as server address.");
            ExportCharacterInfo = toggles.DefineSetting("characterInfo", true,
                () => "Export Character Info",
                () => "Character info such as name, deaths, profession and commander tag.");

            ExportGuildInfo = toggles.DefineSetting("guildInfo", true,
                () => "Export Guild Info",
                () => "Guild info such as guild name, tag, emblem and part of the Message of the Day that is surrounded by [public]<text>[/public].");

            ExportKillProofs = toggles.DefineSetting("killsProofs", true,
                () => "Export Kill Proofs",
                () => "Kill Proofs such as Legendary Divination, Legendary Insight and Unstable Fractal Essence.\nFor more info visit: www.killproof.me");

            ExportMapInfo = toggles.DefineSetting("mapInfo", true,
                () => "Export Map Info",
                () => "Map info such as map name and map type.");

            ExportPvpInfo = toggles.DefineSetting("pvpInfo", true,
                () => "Export PvP Info",
                () => "PvP info such as rank, tier, win rate and kills.");

            ExportWvwInfo = toggles.DefineSetting("wvwInfo", true,
                () => "Export WvW info",
                () => "WvW info such as rank and kills");

            ExportWalletInfo = toggles.DefineSetting("walletInfo", true, 
                () => "Export Wallet Info",
                () => "Currencies such as coins and karma.");

            var cache = settings.AddSubCollection("CachedValues", false, false);
            AccountGuid = cache.DefineSetting("AccountGuid", Guid.Empty);
            AccountName = cache.DefineSetting("AccountName", string.Empty);
            ResetTimeWvW = cache.DefineSetting("ResetTimeWvW", DateTime.UtcNow);
            ResetTimeDaily = cache.DefineSetting("ResetTimeDaily", DateTime.UtcNow);
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
            return new SocialsSettingsView(new SocialsSettingsModel(SettingsManager.ModuleSettings, "https://pastebin.com/raw/Kk9DgVmL"));
        }

        protected override void Initialize()
        {
            _allExportServices = new List<ExportService>();

            ExportCharacterInfo.SettingChanged += ToggleService<CharacterService>;
            ExportClientInfo.SettingChanged += ToggleService<ClientService>;
            ExportGuildInfo.SettingChanged += ToggleService<GuildService>;
            ExportKillProofs.SettingChanged += ToggleService<KillProofService>;
            ExportMapInfo.SettingChanged += ToggleService<MapService>;
            ExportPvpInfo.SettingChanged += ToggleService<PvpService>;
            ExportWvwInfo.SettingChanged += ToggleService<WvwService>;
            ExportWalletInfo.SettingChanged += ToggleService<WalletService>;

            Gw2ApiManager.SubtokenUpdated += SubTokenUpdated;
        }

        private async void ToggleService<TType>(object o, ValueChangedEventArgs<bool> e) where TType : ExportService => await ToggleService<TType>(e.NewValue);
        private async Task ToggleService<TType>(bool enabled) where TType : ExportService
        {
            var service = _allExportServices.FirstOrDefault(x => x.GetType() == typeof(TType));
            if (enabled && service == null)
            {
                service = (TType)Activator.CreateInstance(typeof(TType));
                _allExportServices.Add(service);
                await service.Initialize();
            }
            else if (!enabled && service != null)
            {
                _allExportServices.Remove(service);
                await service.Clear();
                service.Dispose();
            }
        }

        private void SubTokenUpdated(object o, ValueEventArgs<IEnumerable<TokenPermission>> e)
        {
            HasSubToken = true;
        }

        protected override async Task LoadAsync()
        {
            await ToggleService<CharacterService>(ExportCharacterInfo.Value);
            await ToggleService<ClientService>(ExportClientInfo.Value);
            await ToggleService<GuildService>(ExportGuildInfo.Value);
            await ToggleService<KillProofService>(ExportKillProofs.Value);
            await ToggleService<MapService>(ExportMapInfo.Value);
            await ToggleService<PvpService>(ExportPvpInfo.Value);
            await ToggleService<WvwService>(ExportWvwInfo.Value);
            await ToggleService<WalletService>(ExportWalletInfo.Value);
        }

        protected override void OnModuleLoaded(EventArgs e)
        {
            // Base handler must be called
            base.OnModuleLoaded(e);
        }

        protected override async void Update(GameTime gameTime)
        {
            if (!HasSubToken) return;
            try
            {
                foreach (var service in _allExportServices.ToList())
                {
                    if (service == null) continue;
                    await service.DoUpdate();
                }
            }
            catch (Exception e) when (e is InvalidOperationException or NullReferenceException)
            {
                /* NOOP */
            }
        }

        /// <inheritdoc />
        protected override void Unload()
        {
            ExportCharacterInfo.SettingChanged -= ToggleService<CharacterService>;
            ExportClientInfo.SettingChanged -= ToggleService<ClientService>;
            ExportGuildInfo.SettingChanged -= ToggleService<GuildService>;
            ExportKillProofs.SettingChanged -= ToggleService<KillProofService>;
            ExportMapInfo.SettingChanged -= ToggleService<MapService>;
            ExportPvpInfo.SettingChanged -= ToggleService<PvpService>;
            ExportWvwInfo.SettingChanged -= ToggleService<WvwService>;
            ExportWalletInfo.SettingChanged -= ToggleService<WalletService>;
            Gw2ApiManager.SubtokenUpdated -= SubTokenUpdated;
            foreach (var service in _allExportServices) service?.Dispose();
            // All static members must be manually unset
            Instance = null;
        }
    }
}
