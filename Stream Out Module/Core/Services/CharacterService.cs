using Blish_HUD;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Gw2Sharp.WebApi.Exceptions;
using Gw2Sharp.WebApi.V2.Models;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Blish_HUD.GameService;
namespace Nekres.Stream_Out.Core.Services
{
    internal class CharacterService : ExportService
    {
        private static Gw2ApiManager Gw2ApiManager => StreamOutModule.Instance?.Gw2ApiManager;
        private DirectoriesManager DirectoriesManager => StreamOutModule.Instance?.DirectoriesManager;
        private ContentsManager ContentsManager => StreamOutModule.Instance?.ContentsManager;
        private SettingEntry<int> SessionDeathsWvW => StreamOutModule.Instance?.SessionDeathsWvW;
        private SettingEntry<int> TotalDeathsAtResetWvW => StreamOutModule.Instance?.TotalDeathsAtResetWvW;
        private SettingEntry<int> SessionDeathsDaily => StreamOutModule.Instance?.SessionDeathsDaily;
        private SettingEntry<int> TotalDeathsAtResetDaily => StreamOutModule.Instance?.TotalDeathsAtResetDaily;
        private StreamOutModule.UnicodeSigning UnicodeSigning => StreamOutModule.Instance?.AddUnicodeSymbols.Value ?? StreamOutModule.UnicodeSigning.Suffixed;

        private const string CHARACTER_NAME = "character_name.txt";
        private const string PROFESSION_ICON = "profession_icon.png";
        private const string COMMANDER_ICON = "commander_icon.png";
        private const string DEATHS_WEEK = "deaths_week.txt";
        private const string DEATHS_DAY = "deaths_day.txt";

        private const string SKULL = "\u2620"; // ☠

        private Bitmap _commanderIcon;
        private Bitmap _catmanderIcon;

        private SettingEntry<bool> UseCatmanderTag => StreamOutModule.Instance.UseCatmanderTag;

        public CharacterService()
        {
            Gw2Mumble.PlayerCharacter.NameChanged += OnNameChanged;
            Gw2Mumble.PlayerCharacter.SpecializationChanged += OnSpecializationChanged;
            Gw2Mumble.PlayerCharacter.IsCommanderChanged += OnIsCommanderChanged;

            UseCatmanderTag.SettingChanged += OnUseCatmanderTagSettingChanged;
            OnNameChanged(null, new ValueEventArgs<string>(Gw2Mumble.PlayerCharacter.Name));
            OnSpecializationChanged(null, new ValueEventArgs<int>(Gw2Mumble.PlayerCharacter.Specialization));
        }

        public override async Task Initialize()
        {
            await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{DEATHS_WEEK}", $"0{SKULL}", false);
            await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{DEATHS_DAY}", $"0{SKULL}", false);

            var moduleDir = DirectoriesManager.GetFullDirectoryPath("stream_out");
            ContentsManager.ExtractIcons(UseCatmanderTag.Value ? "catmander_tag_white.png" : "commander_tag_white.png", Path.Combine(moduleDir, COMMANDER_ICON));
            
            if (Gw2Mumble.PlayerCharacter.IsCommander) return;
            await TextureUtil.ClearImage($"{moduleDir}/{COMMANDER_ICON}");
        }

        private async void OnNameChanged(object o, ValueEventArgs<string> e)
        {
            await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{CHARACTER_NAME}", e.Value ?? string.Empty);
        }

        private async void OnSpecializationChanged(object o, ValueEventArgs<int> e)
        {
            if (e.Value <= 0)
            {
                await TextureUtil.ClearImage($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{PROFESSION_ICON}");
                return;
            }

            try
            {
                var specialization = await Gw2ApiManager.Gw2ApiClient.V2.Specializations.GetAsync(e.Value);
                var profession = await Gw2ApiManager.Gw2ApiClient.V2.Professions.GetAsync(Gw2Mumble.PlayerCharacter.Profession);
                await TextureUtil.SaveToImage(specialization.Elite ? specialization.ProfessionIconBig : profession.IconBig, $"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{PROFESSION_ICON}");
            }
            catch (UnexpectedStatusException)
            {
                StreamOutModule.Logger.Warn(StreamOutModule.Instance.WebApiDown);
            }
        }

        private async void OnIsCommanderChanged(object o, ValueEventArgs<bool> e)
        {
            if (!e.Value)
            {
                await TextureUtil.ClearImage($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{COMMANDER_ICON}");
                return;
            }
            await SaveCommanderIcon(UseCatmanderTag.Value);
        }

        private async Task SaveCommanderIcon(bool useCatmanderIcon)
        {
            if (useCatmanderIcon)
            {
                if (_catmanderIcon == null)
                {
                    using var catmanderIconStream = ContentsManager.GetFileStream("catmander_tag_white.png");
                    _catmanderIcon = new Bitmap(catmanderIconStream);
                    await catmanderIconStream.FlushAsync();
                }
                await _catmanderIcon.SaveOnNetworkShare($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{COMMANDER_ICON}", ImageFormat.Png);
                return;
            }

            if (_commanderIcon == null)
            {
                using var commanderIconStream = ContentsManager.GetFileStream("commander_tag_white.png");
                _commanderIcon = new Bitmap(commanderIconStream);
                await commanderIconStream.FlushAsync();
            }
            await _commanderIcon.SaveOnNetworkShare($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{COMMANDER_ICON}", ImageFormat.Png);
        }

        private async void OnUseCatmanderTagSettingChanged(object o, ValueChangedEventArgs<bool> e)
        {
            if (!Gw2Mumble.PlayerCharacter.IsCommander) return;
            await SaveCommanderIcon(e.NewValue);
        }

        public static async Task<int> RequestTotalDeaths()
        {
            if (!Gw2ApiManager.HasPermissions(new[] { TokenPermission.Account, TokenPermission.Characters }))
                return -1;
            return await Gw2ApiManager.Gw2ApiClient.V2.Characters.AllAsync().ContinueWith(task => task.IsFaulted ? -1 : task.Result.Sum(x => x.Deaths));
        }

        protected override async Task ResetDaily()
        {
            SessionDeathsDaily.Value = 0;
            TotalDeathsAtResetDaily.Value = await RequestTotalDeaths();
        }

        protected override async Task Update()
        {
            var prefixDeaths = UnicodeSigning == StreamOutModule.UnicodeSigning.Prefixed ? SKULL : string.Empty;
            var suffixDeaths = UnicodeSigning == StreamOutModule.UnicodeSigning.Suffixed ? SKULL : string.Empty;

            // Deaths
            var totalDeaths = await RequestTotalDeaths();
            if (totalDeaths >= 0)
            {
                SessionDeathsDaily.Value = totalDeaths - TotalDeathsAtResetDaily.Value;
                SessionDeathsWvW.Value = totalDeaths - TotalDeathsAtResetWvW.Value;
                await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{DEATHS_WEEK}", $"{prefixDeaths}{SessionDeathsWvW.Value}{suffixDeaths}");
                await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{DEATHS_DAY}", $"{prefixDeaths}{SessionDeathsDaily.Value}{suffixDeaths}");
            }
        }

        public override async Task Clear()
        {
            var dir = DirectoriesManager.GetFullDirectoryPath("stream_out");
            await FileUtil.DeleteAsync(Path.Combine(dir, DEATHS_DAY));
            await FileUtil.DeleteAsync(Path.Combine(dir, DEATHS_WEEK));
            await FileUtil.DeleteAsync(Path.Combine(dir, CHARACTER_NAME));
            await FileUtil.DeleteAsync(Path.Combine(dir, PROFESSION_ICON));
            await FileUtil.DeleteAsync(Path.Combine(dir, COMMANDER_ICON));
        }

        public override void Dispose()
        {
            _commanderIcon?.Dispose();
            _catmanderIcon?.Dispose();
            Gw2Mumble.PlayerCharacter.NameChanged -= OnNameChanged;
            Gw2Mumble.PlayerCharacter.SpecializationChanged -= OnSpecializationChanged;
            UseCatmanderTag.SettingChanged -= OnUseCatmanderTagSettingChanged;
            Gw2Mumble.PlayerCharacter.IsCommanderChanged -= OnIsCommanderChanged;
        }
    }
}
