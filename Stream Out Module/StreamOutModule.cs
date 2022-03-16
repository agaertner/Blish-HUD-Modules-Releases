using Blish_HUD;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Gw2Sharp.Models;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Blish_HUD.Graphics.UI;
using Gw2Sharp.WebApi.Exceptions;
using Nekres.Stream_Out.UI.Models;
using Nekres.Stream_Out.UI.Views;
using static Blish_HUD.GameService;
using Color = System.Drawing.Color;
using File = System.IO.File;
using Graphics = System.Drawing.Graphics;
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

        private SettingEntry<bool> _onlyLastDigitSettingEntry;
        private SettingEntry<UnicodeSigning> _addUnicodeSymbols;
        private SettingEntry<bool> _useCatmanderTag;

        private SettingEntry<DateTime?> _resetTimeWvW;
        private SettingEntry<DateTime?> _resetTimeDaily;

        private SettingEntry<int> _sessionKillsWvW;
        private SettingEntry<int> _sessionKillsWvwDaily;
        private SettingEntry<int> _sessionKillsPvP;

        private SettingEntry<int> _totalKillsAtResetWvW;
        private SettingEntry<int> _totalKillsAtResetPvP;

        private SettingEntry<int> _totalDeathsAtResetWvW;
        private SettingEntry<int> _totalDeathsAtResetDaily;

        private SettingEntry<int> _sessionDeathsWvW;
        private SettingEntry<int> _sessionDeathsDaily;

        private SettingEntry<Guid> _accountGuid;
        private SettingEntry<string> _accountName;

        private enum UnicodeSigning
        {
            None,
            Prefixed,
            Suffixed
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            _onlyLastDigitSettingEntry = settings.DefineSetting("OnlyLastDigits",true, () => "Only Output Last Digits of Server Address", () => "Only outputs the last digits of the server address you are currently connected to.\nThis is the address shown when entering \"/ip\" in chat.");
            _addUnicodeSymbols = settings.DefineSetting("UnicodeSymbols",UnicodeSigning.Suffixed, () => "Numeric Value Signing", () => "The way numeric values should be signed with unicode symbols.");
            _useCatmanderTag = settings.DefineSetting("CatmanderTag", false, () => "Use Catmander Tag", () => $"Replaces the {COMMANDER_ICON} with the Catmander icon if you tag up as Commander in-game.");

            var cache = settings.AddSubCollection("CachedValues");
            cache.RenderInUi = false;
            _accountGuid = cache.DefineSetting("AccountGuid", Guid.Empty);
            _accountName = cache.DefineSetting("AccountName", string.Empty);
            _resetTimeWvW = cache.DefineSetting<DateTime?>("ResetTimeWvW", null);
            _resetTimeDaily = cache.DefineSetting<DateTime?>("ResetTimeDaily", null);
            _sessionKillsWvW = cache.DefineSetting("SessionKillsWvW", 0);
            _sessionKillsWvwDaily = cache.DefineSetting("SessionsKillsWvWDaily", 0);
            _sessionKillsPvP = cache.DefineSetting("SessionKillsPvP", 0);
            _sessionDeathsWvW = cache.DefineSetting("SessionDeathsWvW", 0);
            _sessionDeathsDaily = cache.DefineSetting("SessionDeathsDaily", 0);
            _totalKillsAtResetWvW = cache.DefineSetting("TotalKillsAtResetWvW", 0);
            _totalKillsAtResetPvP = cache.DefineSetting("TotalKillsAtResetPvP", 0);
            _totalDeathsAtResetWvW = cache.DefineSetting("TotalDeathsAtResetWvW", 0);
            _totalDeathsAtResetDaily = cache.DefineSetting("TotalDeathsAtResetDaily", 0);
        }

        public override IView GetSettingsView()
        {
            return new CustomSettingsView(new CustomSettingsModel(SettingsManager.ModuleSettings));
        }

        private const string SERVER_ADDRESS     = "server_address.txt";
        private const string CHARACTER_NAME     = "character_name.txt";
        private const string MAP_TYPE           = "map_type.txt";
        private const string MAP_NAME           = "map_name.txt";
        private const string PROFESSION_ICON    = "profession_icon.png";
        private const string COMMANDER_ICON     = "commander_icon.png";
        private const string WALLET_COINS       = "wallet_coins.png";
        private const string WALLET_KARMA       = "wallet_karma.png";

        private const string GUILD_NAME         = "guild_name.txt";
        private const string GUILD_TAG          = "guild_tag.txt";
        private const string GUILD_EMBLEM       = "guild_emblem.png";
        private const string GUILD_MOTD         = "guild_motd.txt";

        private const string WVW_KILLS_WEEK     = "wvw_kills_week.txt";
        private const string WVW_KILLS_DAY      = "wvw_kills_day.txt";
        private const string WVW_KILLS_TOTAL    = "wvw_kills_total.txt";
        private const string WVW_RANK           = "wvw_rank.txt";

        private const string PVP_KILLS_TOTAL    = "pvp_kills_total.txt";
        private const string PVP_KILLS_DAY      = "pvp_kills_day.txt";
        private const string PVP_RANK           = "pvp_rank.txt";
        private const string PVP_RANK_ICON      = "pvp_rank_icon.png";
        private const string PVP_TIER_ICON      = "pvp_tier_icon.png";
        private const string PVP_WINRATE        = "pvp_winrate.txt";

        private const string KILLPROOF_ME_UNSTABLE_FRACTAL_ESSENCE = "unstable_fractal_essence.txt";
        private const string KILLPROOF_ME_LEGENDARY_DIVINATION = "legendary_divination.txt";
        private const string KILLPROOF_ME_LEGENDARY_INSIGHT = "legendary_insight.txt";

        private const string DEATHS_WEEK        = "deaths_week.txt";
        private const string DEATHS_DAY         = "deaths_day.txt";

        private const string SKULL = "\u2620"; // ☠
        private const string SWORDS = "\u2694"; // ⚔

        private const string KILLPROOF_API_URL = "https://killproof.me/api/kp/";

        private Bitmap Commander_Icon;
        private Bitmap Catmander_Icon;

        private Regex GUILD_MOTD_PUBLIC = new Regex(@"(?<=\[public\]).*(?=\[\/public\])", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        protected override void Initialize()
        {
            Gw2ApiManager.SubtokenUpdated += SubTokenUpdated;
        }

        private bool _hasSubToken;
        private void SubTokenUpdated(object o, ValueEventArgs<IEnumerable<TokenPermission>> e)
        {
            _hasSubToken = true;
        }

        protected override async Task LoadAsync()
        {
            // Generate some placeholder files for values that depend on a privileged API connection
            await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{WVW_KILLS_WEEK}", $"0{SWORDS}", false);
            await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{WVW_KILLS_TOTAL}", $"0{SWORDS}", false);
            await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{WVW_KILLS_DAY}", $"0{SWORDS}", false);
            await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{PVP_KILLS_DAY}", $"0{SWORDS}", false);
            await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{PVP_KILLS_TOTAL}", $"0{SWORDS}", false);
            await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{DEATHS_WEEK}", $"0{SKULL}", false);
            await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{DEATHS_DAY}", $"0{SKULL}", false);
            await Task.Run(() => Gw2Util.GeneratePvpTierImage($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{PVP_TIER_ICON}", 1, 3, false));
            await Task.Run(() => Gw2Util.GenerateCoinsImage($"{ModuleInstance.DirectoriesManager.GetFullDirectoryPath("stream_out")}/{WALLET_COINS}", 10000000, false));
            await Task.Run(() => Gw2Util.GenerateKarmaImage($"{ModuleInstance.DirectoriesManager.GetFullDirectoryPath("stream_out")}/{WALLET_KARMA}", 10000000, false));
            await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{PVP_RANK}", "Bronze I", false);
            await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{PVP_WINRATE}", "50%", false);
            await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{WVW_RANK}", "1 : Invader", false);

            var moduleDir = DirectoriesManager.GetFullDirectoryPath("stream_out");
            ExtractIcons("1614804.png", moduleDir, PVP_RANK_ICON);
            ExtractIcons(_useCatmanderTag.Value ? "catmander_tag_white.png" : "commander_tag_white.png", moduleDir, COMMANDER_ICON);
            if (!Gw2Mumble.PlayerCharacter.IsCommander)
                ClearImage($"{moduleDir}/{COMMANDER_ICON}");
            ExtractIcons("legendary_divination.png", $"{moduleDir}/static/", "legendary_divination.png");
            ExtractIcons("legendary_insight.png", $"{moduleDir}/static/", "legendary_insight.png");
            ExtractIcons("unstable_fractal_essence.png", $"{moduleDir}/static/", "unstable_fractal_essence.png");
        }
        private void ExtractIcons(string iconName, string outputDir, string iconOutputName)
        {
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);
            var fullname = outputDir + '/' + iconOutputName;
            if (File.Exists(fullname)) return;
            using var texStr = ContentsManager.GetFileStream(iconName);
            using var icon = new Bitmap(texStr);
            icon.Save(fullname, ImageFormat.Png);
        }

        protected override void OnModuleLoaded(EventArgs e)
        {
            Gw2Mumble.PlayerCharacter.NameChanged += OnNameChanged;
            Gw2Mumble.PlayerCharacter.SpecializationChanged += OnSpecializationChanged;
            Gw2Mumble.PlayerCharacter.IsCommanderChanged += OnIsCommanderChanged;
            Gw2Mumble.CurrentMap.MapChanged += OnMapChanged;
            _useCatmanderTag.SettingChanged += OnUseCatmanderTagSettingChanged;

            OnNameChanged(null, new ValueEventArgs<string>(Gw2Mumble.PlayerCharacter.Name));
            OnSpecializationChanged(null, new ValueEventArgs<int>(Gw2Mumble.PlayerCharacter.Specialization));
            OnMapChanged(null, new ValueEventArgs<int>(Gw2Mumble.CurrentMap.Id));

            // Base handler must be called
            base.OnModuleLoaded(e);
        }

        private DateTime? _prevApiRequestTime;
        private string _prevServerAddress = "";
        protected override async void Update(GameTime gameTime)
        {
            if (Gw2Mumble.IsAvailable && !_prevServerAddress.Equals(Gw2Mumble.Info.ServerAddress, StringComparison.InvariantCultureIgnoreCase))
            {
                _prevServerAddress = Gw2Mumble.Info.ServerAddress;
                await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{SERVER_ADDRESS}", string.IsNullOrEmpty(Gw2Mumble.Info.ServerAddress) ? string.Empty : 
                    _onlyLastDigitSettingEntry.Value ? '*' + Gw2Mumble.Info.ServerAddress.Substring(Gw2Mumble.Info.ServerAddress.LastIndexOf('.')) : Gw2Mumble.Info.ServerAddress);
            }

            if (_hasSubToken && (!_prevApiRequestTime.HasValue || DateTime.UtcNow.Subtract(_prevApiRequestTime.Value).TotalSeconds > 300))
            {
                _prevApiRequestTime = DateTime.UtcNow;

                await CheckForReset();
                await UpdateWallet();
                await UpdateStandingsForPvP();
                await UpdateStatsForPvp();
                await UpdateRankForWvw();
                await UpdateKillsAndDeaths();
                await UpdateKillProofs();
                await UpdateGuild();
            }
        }

        private async Task UpdateKillsAndDeaths()
        {
            var prefixKills = _addUnicodeSymbols.Value == UnicodeSigning.Prefixed ? SWORDS : string.Empty;
            var suffixKills = _addUnicodeSymbols.Value == UnicodeSigning.Suffixed ? SWORDS : string.Empty;
            var prefixDeaths = _addUnicodeSymbols.Value == UnicodeSigning.Prefixed ? SKULL : string.Empty;
            var suffixDeaths = _addUnicodeSymbols.Value == UnicodeSigning.Suffixed ? SKULL : string.Empty;

            // WvW kills
            var totalKillsWvW = await RequestTotalKillsForWvW();
            if (totalKillsWvW >= 0)
            {
                var currentKills = totalKillsWvW - _totalKillsAtResetWvW.Value;
                _sessionKillsWvW.Value = currentKills;
                _sessionKillsWvwDaily.Value = currentKills;
                await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{WVW_KILLS_WEEK}", $"{prefixKills}{_sessionKillsWvW.Value}{suffixKills}");
                await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{WVW_KILLS_TOTAL}", $"{prefixKills}{totalKillsWvW}{suffixKills}");
                await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{WVW_KILLS_DAY}", $"{prefixKills}{_sessionKillsWvwDaily.Value}{suffixKills}");
            }

            // PvP kills
            var totalKillsPvP = await RequestTotalKillsForPvP();
            if (totalKillsPvP >= 0)
            {
                _sessionKillsPvP.Value = totalKillsPvP - _totalKillsAtResetPvP.Value;
                await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{PVP_KILLS_DAY}", $"{prefixKills}{_sessionKillsPvP.Value}{suffixKills}");
                await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{PVP_KILLS_TOTAL}", $"{prefixKills}{totalKillsPvP}{suffixKills}");
            }

            // Deaths
            var totalDeaths = await RequestTotalDeaths();
            if (totalDeaths >= 0)
            {
                _sessionDeathsDaily.Value = totalDeaths - _totalDeathsAtResetDaily.Value;
                _sessionDeathsWvW.Value = totalDeaths - _totalDeathsAtResetWvW.Value;
                await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{DEATHS_WEEK}", $"{prefixDeaths}{_sessionDeathsWvW.Value}{suffixDeaths}");
                await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{DEATHS_DAY}", $"{prefixDeaths}{_sessionDeathsDaily.Value}{suffixDeaths}");
            }
        }

        /// <inheritdoc />
        protected override void Unload()
        {
            Gw2Mumble.PlayerCharacter.NameChanged -= OnNameChanged;
            Gw2Mumble.PlayerCharacter.SpecializationChanged -= OnSpecializationChanged;
            Gw2Mumble.CurrentMap.MapChanged -= OnMapChanged;
            Gw2Mumble.PlayerCharacter.IsCommanderChanged -= OnIsCommanderChanged;
            Gw2ApiManager.SubtokenUpdated -= SubTokenUpdated;
            _useCatmanderTag.SettingChanged -= OnUseCatmanderTagSettingChanged;
            // All static members must be manually unset
            ModuleInstance = null;
        }

        private async void OnMapChanged(object o, ValueEventArgs<int> e)
        {
            if (e.Value <= 0)
            {
                await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{MAP_NAME}", string.Empty);
                await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{MAP_TYPE}", string.Empty);
                return;
            }

            Map map;
            try
            {
                map = await Gw2ApiManager.Gw2ApiClient.V2.Maps.GetAsync(e.Value);
                if (map == null)
                    throw new NullReferenceException("Unknown error.");
            }
            catch (Exception ex) when (ex is UnexpectedStatusException or NullReferenceException)
            {
                Logger.Warn(CommonStrings.WebApiDown);
                return;
            }

            var location = map.Name;
            // Some instanced maps consist of just a single sector and hide their display name in it.
            if (map.Name.Equals(map.RegionName, StringComparison.InvariantCultureIgnoreCase))
            {
                var defaultSector = (await RequestSectors(map.ContinentId, map.DefaultFloor, map.RegionId, map.Id)).FirstOrDefault();
                if (defaultSector != null && !string.IsNullOrEmpty(defaultSector.Name))
                    location = defaultSector.Name.Replace("<br>", " ");
            }
            await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{MAP_NAME}", location);

            var type = string.Empty;
            switch (map.Type.Value)
            {
                case MapType.Center:
                case MapType.BlueHome:
                case MapType.GreenHome:
                case MapType.RedHome:
                case MapType.JumpPuzzle:
                case MapType.EdgeOfTheMists:
                case MapType.WvwLounge:
                    type = "WvW";
                    break;
                case MapType.PublicMini:
                case MapType.Public:
                    type = map.Id != 350 ? "PvE" : "PvP"; // Edge of the Mists
                    break;
                case MapType.Pvp:
                    type = "PvP";
                    break;
                case MapType.Gvg:
                    type = "GvG";
                    break;
                case MapType.CharacterCreate:
                case MapType.Tutorial:
                case MapType.Instance:
                case MapType.Tournament:
                case MapType.UserTournament:
                case MapType.FortunesVale:
                    type = map.Type.Value.ToDisplayString();
                    break;
                default: break;
            }
            await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{MAP_TYPE}", type);
        }

        private async void OnNameChanged(object o, ValueEventArgs<string> e)
        {
            await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{CHARACTER_NAME}", e.Value ?? string.Empty);
        }

        private async void OnSpecializationChanged(object o, ValueEventArgs<int> e)
        {
            if (e.Value <= 0)
            {
                ClearImage($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{PROFESSION_ICON}");
                return;
            }

            try
            {
                var specialization = await Gw2ApiManager.Gw2ApiClient.V2.Specializations.GetAsync(e.Value);
                var profession = await Gw2ApiManager.Gw2ApiClient.V2.Professions.GetAsync(Gw2Mumble.PlayerCharacter.Profession);
                await SaveToImage(specialization.Elite ? specialization.ProfessionIconBig : profession.IconBig, $"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{PROFESSION_ICON}");
            }
            catch (UnexpectedStatusException)
            {
                Logger.Warn(CommonStrings.WebApiDown);
            }
        }

        private async void OnIsCommanderChanged(object o, ValueEventArgs<bool> e)
        {
            if (!e.Value)
            {
                ClearImage($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{COMMANDER_ICON}");
                return;
            }
            SaveCommanderIcon(_useCatmanderTag.Value);
        }

        private async void SaveCommanderIcon(bool useCatmanderIcon)
        {
            if (useCatmanderIcon)
            {
                if (Catmander_Icon == null)
                {
                    using var catmanderIconStream = ContentsManager.GetFileStream("catmander_tag_white.png");
                    Catmander_Icon = new Bitmap(catmanderIconStream);
                    await catmanderIconStream.FlushAsync();
                }
                Catmander_Icon.Save($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{COMMANDER_ICON}", ImageFormat.Png);
                return;
            }

            if (Commander_Icon == null)
            {
                using var commanderIconStream = ContentsManager.GetFileStream("commander_tag_white.png");
                Commander_Icon = new Bitmap(commanderIconStream);
                await commanderIconStream.FlushAsync();
            }
            Commander_Icon.Save($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{COMMANDER_ICON}", ImageFormat.Png);
        }

        private void OnUseCatmanderTagSettingChanged(object o, ValueChangedEventArgs<bool> e)
        {
            if (Gw2Mumble.PlayerCharacter.IsCommander)
            {
                SaveCommanderIcon(e.NewValue);
            }
        }

        private async Task SaveToImage(string renderUri, string path)
        {
            await Gw2ApiManager.Gw2ApiClient.Render.DownloadToByteArrayAsync(renderUri).ContinueWith(textureDataResponse =>
                {
                    if (textureDataResponse.IsFaulted)
                    {
                        Logger.Warn($"Request to render service for {renderUri} failed.");
                        return;
                    }
                    using var textureStream = new MemoryStream(textureDataResponse.Result);
                    using var bitmap = new Bitmap(textureStream);
                    bitmap.SaveOnNetworkShare(path, ImageFormat.Png);
                });
        }

        private void ClearImage(string path)
        {
            if (!File.Exists(path)) return;
            using var stream = new MemoryStream(File.ReadAllBytes(path));
            using var bitmap = (Bitmap)Image.FromStream(stream);
            using (var gfx = Graphics.FromImage(bitmap))
            {
                gfx.Clear(Color.Transparent);
                gfx.Flush();
            }
            bitmap.SaveOnNetworkShare(path, ImageFormat.Png);
        }

        private async Task<IEnumerable<ContinentFloorRegionMapSector>> RequestSectors(int continentId, int floor, int regionId, int mapId)
        {
            return await Gw2ApiManager.Gw2ApiClient.V2.Continents[continentId].Floors[floor].Regions[regionId].Maps[mapId].Sectors.AllAsync()
                .ContinueWith(task => task.IsFaulted ? Enumerable.Empty<ContinentFloorRegionMapSector>() : task.Result);
        }

        private async Task<int> RequestTotalDeaths()
        {
            if (!Gw2ApiManager.HasPermissions(new[] { TokenPermission.Account, TokenPermission.Characters }))
                return -1;
            return await Gw2ApiManager.Gw2ApiClient.V2.Characters.AllAsync().ContinueWith(task => task.IsFaulted ? -1 : task.Result.Sum(x => x.Deaths));
            
        }

        private async Task UpdateGuild()
        {
            if (!Gw2Mumble.IsAvailable || !Gw2ApiManager.HasPermissions(new[] { TokenPermission.Account, TokenPermission.Characters, TokenPermission.Guilds }))
                return;

            var guildId = await Gw2ApiManager.Gw2ApiClient.V2.Characters[Gw2Mumble.PlayerCharacter.Name].Core.GetAsync().ContinueWith(task => task.IsFaulted ? Guid.Empty : task.Result.Guild);

            if (guildId.Equals(Guid.Empty))
            {
                await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{GUILD_NAME}", string.Empty);
                await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{GUILD_TAG}", string.Empty);
                await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{GUILD_MOTD}", string.Empty);
                ClearImage($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{GUILD_EMBLEM}");
                return;
            }

            await Gw2ApiManager.Gw2ApiClient.V2.Guild[guildId].GetAsync().ContinueWith(async task =>
            {
                if (task.IsFaulted)
                    return;
                var name = task.Result.Name;
                var tag = task.Result.Tag;
                var motd = task.Result.Motd ?? string.Empty;

                await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{GUILD_NAME}", name);
                await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{GUILD_TAG}", $"[{tag}]");
                await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{GUILD_MOTD}", GUILD_MOTD_PUBLIC.Match(motd).Value);

                var emblem = task.Result.Emblem;
                if (emblem == null)
                {
                    ClearImage($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{GUILD_EMBLEM}");
                    return;
                }

                var bg = await Gw2ApiManager.Gw2ApiClient.V2.Emblem.Backgrounds.GetAsync(emblem.Background.Id);
                var fg = await Gw2ApiManager.Gw2ApiClient.V2.Emblem.Foregrounds.GetAsync(emblem.Foreground.Id);

                var layersCombined = new List<Gw2Sharp.WebApi.RenderUrl>();
                if (bg != null)
                    layersCombined.AddRange(bg.Layers);
                if (fg != null)
                    layersCombined.AddRange(fg.Layers.Skip(1));
                var layers = new List<Bitmap>();
                foreach (var renderUrl in layersCombined)
                {
                    using var textureStream = new MemoryStream();
                    await renderUrl.DownloadToStreamAsync(textureStream);
                    layers.Add(new Bitmap(textureStream));
                }
                if (!layers.Any())
                {
                    ClearImage($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{GUILD_EMBLEM}");
                    return;
                }

                // combine for a single API request
                var colorsCombined = new List<int>();
                colorsCombined.AddRange(emblem.Background.Colors);
                colorsCombined.AddRange(emblem.Foreground.Colors);
                //TODO: ManyAsync does not keep duplicates. Same indexing as layers is required for application.
                //var colors = await Gw2ApiManager.Gw2ApiClient.V2.Colors.ManyAsync(colorsCombined);
                var colors = new List<Gw2Sharp.WebApi.V2.Models.Color>();
                foreach (var color in colorsCombined)
                    colors.Add(await Gw2ApiManager.Gw2ApiClient.V2.Colors.GetAsync(color));

                Bitmap result = new Bitmap(256, 256);
                for (var i = 0; i < layers.Count; i++)
                {
                    var layer = layers[i].FitTo(result);

                    if (colors.Any())
                    {
                        var color = Color.FromArgb(colors[i].Cloth.Rgb[0], colors[i].Cloth.Rgb[1], colors[i].Cloth.Rgb[2]);
                        // apply colors
                        layer.Colorize(color);
                    }

                    // apply flags
                    if (bg != null && i < bg.Layers.Count)
                        layer.Flip(emblem.Flags.Any(x => x == GuildEmblemFlag.FlipBackgroundHorizontal), emblem.Flags.Any(x => x == GuildEmblemFlag.FlipBackgroundVertical));
                    else
                        layer.Flip(emblem.Flags.Any(x => x == GuildEmblemFlag.FlipForegroundHorizontal), emblem.Flags.Any(x => x == GuildEmblemFlag.FlipForegroundVertical));

                    // merge layer with result
                    var merged = result.Merge(layer);
                    result.Dispose();
                    layer.Dispose();
                    result = merged;
                }
                result?.Save($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{GUILD_EMBLEM}", ImageFormat.Png);
            });
        }

        private async Task<int> RequestTotalKillsForWvW()
        {
            if (!Gw2ApiManager.HasPermissions(new[] { TokenPermission.Account, TokenPermission.Progression }))
                return -1;
            return await Gw2ApiManager.Gw2ApiClient.V2.Account.Achievements.GetAsync().ContinueWith(response =>
            {
                if (response.IsFaulted) return -1;
                return response.Result.Single(x => x.Id == 283).Current; // Realm Avenger
            });
        }

        private async Task<int> RequestTotalKillsForPvP()
        {
            if (!Gw2ApiManager.HasPermissions(new[] {TokenPermission.Account, TokenPermission.Progression}))
                return -1;
            return await Gw2ApiManager.Gw2ApiClient.V2.Account.Achievements.GetAsync().ContinueWith(response =>
            {
                if (response.IsFaulted) return -1;
                return response.Result.Single(x => x.Id == 239).Current; // Slayer
            });
        }

        private async Task ResetWorldVersusWorld(int worldId, bool force = false)
        {
            if (!force && _resetTimeWvW.Value.HasValue && DateTime.UtcNow < _resetTimeWvW.Value) return;

            _resetTimeWvW.Value = await GetWvWResetTime(worldId);
            _sessionKillsWvW.Value = 0;
            _sessionDeathsWvW.Value = 0;
            _totalKillsAtResetWvW.Value = await RequestTotalKillsForWvW();
            _totalDeathsAtResetWvW.Value = await RequestTotalDeaths();
        }

        private async Task ResetDaily(bool force = false)
        {
            if (!force && _resetTimeDaily.Value.HasValue && DateTime.UtcNow < _resetTimeDaily.Value) return;

            _resetTimeDaily.Value = GetDailyResetTime();
            _sessionKillsPvP.Value = 0;
            _sessionDeathsDaily.Value = 0;
            _sessionKillsWvwDaily.Value = 0;
            _totalKillsAtResetPvP.Value = await RequestTotalKillsForPvP();
            _totalDeathsAtResetDaily.Value = await RequestTotalDeaths();
        }

        private DateTime GetDailyResetTime()
        {
            var nextDay = DateTime.UtcNow.AddDays(1);
            return new DateTime(nextDay.Year, nextDay.Month, nextDay.Day, 2, 0, 0).ToUniversalTime(); // UTC+2
        }

        private async Task<DateTime?> GetWvWResetTime(int worldId)
        {
            return await Gw2ApiManager.Gw2ApiClient.V2.Wvw.Matches.World(worldId).GetAsync().ContinueWith(r =>
            {
                if (r.IsFaulted)
                    return new DateTime?();
                return r.Result.EndTime.UtcDateTime;
            });
        }

        private async Task CheckForReset()
        {
            if (!Gw2ApiManager.HasPermission(TokenPermission.Account))
                return;

            await Gw2ApiManager.Gw2ApiClient.V2.Account.GetAsync().ContinueWith(async response =>
            {
                if (response.IsFaulted)
                    return;

                var isNewAcc = !response.Result.Id.Equals(_accountGuid.Value);
                _accountName.Value = response.Result.Name;
                _accountGuid.Value = response.Result.Id;
                await ResetWorldVersusWorld(response.Result.World, isNewAcc);
                await ResetDaily(isNewAcc);
            });
        }

        private async Task UpdateRankForWvw()
        {
            if (!Gw2ApiManager.HasPermissions(new[] { TokenPermission.Account, TokenPermission.Progression }))
                return;

            await Gw2ApiManager.Gw2ApiClient.V2.Account.GetAsync().ContinueWith(async response =>
            {
                if (response.IsFaulted) return;
                var wvwRank = response.Result.WvwRank;
                if (!wvwRank.HasValue || wvwRank <= 0) return;
                await Gw2ApiManager.Gw2ApiClient.V2.Wvw.Ranks.AllAsync().ContinueWith(async t =>
                {
                    if (t.IsFaulted) return;
                    var wvwRankObj = t.Result.MaxBy(y => wvwRank >= y.MinRank);
                    await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{WVW_RANK}", $"{wvwRank:N0} : {wvwRankObj.Title}");
                });
            });
        }

        private async Task UpdateStandingsForPvP()
        {
            if (!Gw2ApiManager.HasPermissions(new[] { TokenPermission.Account, TokenPermission.Pvp }))
                return;

            await Gw2ApiManager.Gw2ApiClient.V2.Pvp.Seasons.AllAsync().ContinueWith(async task =>
            {
                if (task.IsFaulted) return;
                var season = task.Result.OrderByDescending(x => x.End).First();

                await Gw2ApiManager.Gw2ApiClient.V2.Pvp.Standings.GetAsync().ContinueWith(async t =>
                {
                    if (t.IsFaulted) return;
                    
                    var standing = t.Result.FirstOrDefault(x => x.SeasonId.Equals(season.Id));

                    if (standing?.Current.Rating == null)
                        return;

                    var rank = season.Ranks.First();
                    var tier = 1;
                    var found = false;
                    var ranksTotal = season.Ranks.Count;

                    var tiers = season.Ranks.SelectMany(x => x.Tiers).ToList();
                    var maxRating = tiers.MaxBy(y => y.Rating).Rating;
                    var minRating = tiers.MinBy(y => y.Rating).Rating;

                    // overshoots
                    if (standing.Current.Rating > maxRating)
                    {
                        rank = season.Ranks.Last();
                        tier = rank.Tiers.Count;
                        found = true;
                    }

                    // undershoots
                    if (standing.Current.Rating < minRating)
                    {
                        rank = season.Ranks.First();
                        tier = 1;
                        found = true;
                    }

                    for (var i = 0; i < ranksTotal; i++)
                    {
                        if (found) break;
                        var currentRank = season.Ranks[i];
                        var tiersTotal = currentRank.Tiers.Count;

                        for (var j = 0; j < tiersTotal; j++)
                        {
                            var nextTierRating = currentRank.Tiers[j].Rating;

                            if (standing.Current.Rating > nextTierRating)
                                continue;

                            tier = j + 1;
                            rank = currentRank;
                            found = true;
                            break;
                        }
                    }

                    await Task.Run(() => Gw2Util.GeneratePvpTierImage($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{PVP_TIER_ICON}", tier, rank.Tiers.Count));

                    await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{PVP_RANK}",$"{rank.Name} {tier.ToRomanNumeral()}");
                    await SaveToImage(rank.OverlaySmall, $"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{PVP_RANK_ICON}");
                });
            });
        }

        private async Task UpdateStatsForPvp()
        {
            if (!Gw2ApiManager.HasPermissions(new[] { TokenPermission.Account, TokenPermission.Pvp }))
                return;

            await Gw2ApiManager.Gw2ApiClient.V2.Pvp.Stats.GetAsync().ContinueWith(async task =>
            {
                if (task.IsFaulted) return;

                var ranked = task.Result.Ladders.Where(x => !x.Key.Contains("unranked") && x.Key.Contains("ranked")).ToArray();
                var wins = ranked.Sum(x => x.Value.Wins);
                var losses = ranked.Sum(x => x.Value.Losses);
                //var forfeits = ranked.Sum(x => x.Value.Forfeits); // Doesn't count as win nor loss.
                var byes = ranked.Sum(x => x.Value.Byes);
                var desertions = ranked.Sum(x => x.Value.Desertions);
                double totalGames = wins + losses + desertions + byes;
                if (totalGames <= 0) return;
                var winRatio = (wins + byes) / totalGames * 100;
                await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{PVP_WINRATE}",$"{Math.Round(winRatio).ToString(CultureInfo.InvariantCulture)}%");
            });
        }

        private async Task UpdateWallet()
        {
            if (!Gw2ApiManager.HasPermissions(new[] { TokenPermission.Account, TokenPermission.Wallet }))
                return;

            await Gw2ApiManager.Gw2ApiClient.V2.Account.Wallet.GetAsync().ContinueWith(async task =>
            {
                if (task.IsFaulted) return;
                var coins = task.Result.First(x => x.Id == 1).Value; // Coins
                await Task.Run(() => Gw2Util.GenerateCoinsImage($"{ModuleInstance.DirectoriesManager.GetFullDirectoryPath("stream_out")}/{WALLET_COINS}", coins));

                var karma = task.Result.First(x => x.Id == 2).Value; // Karma
                await Task.Run(() => Gw2Util.GenerateKarmaImage($"{ModuleInstance.DirectoriesManager.GetFullDirectoryPath("stream_out")}/{WALLET_KARMA}", karma));
            });
        }

        private async Task UpdateKillProofs()
        {
            await TaskUtil.GetJsonResponse<dynamic>($"{KILLPROOF_API_URL}{_accountName.Value}?lang={Overlay.UserLocale.Value}").ContinueWith(async task =>
                {
                    if (task.IsFaulted || !task.Result.Item1) return;
                    IEnumerable<dynamic> killProofs = task.Result.Item2.killproofs;
                    if (killProofs.IsNullOrEmpty()) return;

                    var killproofDir = $"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/killproof.me";
                    if (!Directory.Exists(killproofDir))
                        Directory.CreateDirectory(killproofDir);

                    var count = 0;
                    foreach (var killProof in killProofs)
                    {
                        int id = killProof.id;
                        switch (id)
                        {
                            case 88485: // Legendary Divination
                                await FileUtil.WriteAllTextAsync($"{killproofDir}/{KILLPROOF_ME_LEGENDARY_DIVINATION}", killProof.amount.ToString());
                                count++;
                                break;
                            case 77302: // Legendary Insight
                                await FileUtil.WriteAllTextAsync($"{killproofDir}/{KILLPROOF_ME_LEGENDARY_INSIGHT}", killProof.amount.ToString());
                                count++;
                                break;
                            case 94020: // Unstable Fractal Essence
                                await FileUtil.WriteAllTextAsync($"{killproofDir}/{KILLPROOF_ME_UNSTABLE_FRACTAL_ESSENCE}", killProof.amount.ToString());
                                count++;
                                break;
                            default: break;
                        }
                        if (count == 3) break;
                    }
                });
        }
    }
}
