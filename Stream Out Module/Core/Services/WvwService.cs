using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Gw2Sharp.WebApi.V2.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Nekres.Stream_Out.Core.Services
{
    internal class WvwService : ExportService
    {
        private Gw2ApiManager Gw2ApiManager => StreamOutModule.Instance?.Gw2ApiManager;
        private DirectoriesManager DirectoriesManager => StreamOutModule.Instance?.DirectoriesManager;
        private StreamOutModule.UnicodeSigning UnicodeSigning => StreamOutModule.Instance?.AddUnicodeSymbols.Value ?? StreamOutModule.UnicodeSigning.Suffixed;
        private SettingEntry<string> AccountName => StreamOutModule.Instance?.AccountName;
        private SettingEntry<Guid> AccountGuid => StreamOutModule.Instance?.AccountGuid;
        private SettingEntry<DateTime> ResetTimeWvW => StreamOutModule.Instance.ResetTimeWvW;
        private SettingEntry<int> SessionKillsWvW => StreamOutModule.Instance?.SessionKillsWvW;
        private SettingEntry<int> SessionDeathsWvW => StreamOutModule.Instance?.SessionDeathsWvW;
        private SettingEntry<int> SessionKillsWvwDaily => StreamOutModule.Instance?.SessionKillsWvwDaily;
        private SettingEntry<int> TotalKillsAtResetWvW => StreamOutModule.Instance?.TotalKillsAtResetWvW;
        private SettingEntry<int> TotalDeathsAtResetWvW => StreamOutModule.Instance?.TotalDeathsAtResetWvW;

        private const string WVW_KILLS_WEEK = "wvw_kills_week.txt";
        private const string WVW_KILLS_DAY = "wvw_kills_day.txt";
        private const string WVW_KILLS_TOTAL = "wvw_kills_total.txt";
        private const string WVW_RANK = "wvw_rank.txt";

        private const string SWORDS = "\u2694"; // ⚔
        public WvwService()
        {
        }

        public override async Task Initialize()
        {
            await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{WVW_KILLS_WEEK}", $"0{SWORDS}", false);
            await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{WVW_KILLS_TOTAL}", $"0{SWORDS}", false);
            await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{WVW_KILLS_DAY}", $"0{SWORDS}", false);
            await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{WVW_RANK}", "1 : Invader", false);
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

        private async Task ResetWorldVersusWorld(int worldId, bool force = false)
        {
            if (!force && DateTime.UtcNow < ResetTimeWvW.Value) return;

            ResetTimeWvW.Value = await GetWvWResetTime(worldId);
            SessionKillsWvW.Value = 0;
            SessionDeathsWvW.Value = 0;
            TotalKillsAtResetWvW.Value = await RequestTotalKillsForWvW();
            TotalDeathsAtResetWvW.Value = await CharacterService.RequestTotalDeaths();
        }

        private async Task<DateTime> GetWvWResetTime(int worldId)
        {
            return await Gw2ApiManager.Gw2ApiClient.V2.Wvw.Matches.World(worldId).GetAsync().ContinueWith(r => r.IsFaulted ? DateTime.UtcNow : r.Result.EndTime.UtcDateTime);
        }

        protected override async Task ResetDaily()
        {
            SessionKillsWvwDaily.Value = 0;
        }

        protected override async Task Update()
        {
            if (!Gw2ApiManager.HasPermission(TokenPermission.Account))
                return;

            await UpdateRankForWvw();

            await Gw2ApiManager.Gw2ApiClient.V2.Account.GetAsync().ContinueWith(async response =>
            {
                if (response.IsFaulted) return;

                var isNewAcc = !response.Result.Id.Equals(AccountGuid.Value);
                AccountName.Value = response.Result.Name;
                AccountGuid.Value = response.Result.Id;
                await ResetWorldVersusWorld(response.Result.World, isNewAcc).ContinueWith(async _ =>
                {
                    var prefixKills = UnicodeSigning == StreamOutModule.UnicodeSigning.Prefixed ? SWORDS : string.Empty;
                    var suffixKills = UnicodeSigning == StreamOutModule.UnicodeSigning.Suffixed ? SWORDS : string.Empty;

                    // WvW kills
                    var totalKillsWvW = await RequestTotalKillsForWvW();
                    if (totalKillsWvW >= 0)
                    {
                        var currentKills = totalKillsWvW - TotalKillsAtResetWvW.Value;
                        SessionKillsWvW.Value = currentKills;
                        SessionKillsWvwDaily.Value = currentKills;
                        await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{WVW_KILLS_WEEK}", $"{prefixKills}{SessionKillsWvW.Value}{suffixKills}");
                        await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{WVW_KILLS_TOTAL}", $"{prefixKills}{totalKillsWvW}{suffixKills}");
                        await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{WVW_KILLS_DAY}", $"{prefixKills}{SessionKillsWvwDaily.Value}{suffixKills}");
                    }
                });
            });
        }

        public override async Task Clear()
        {
            var dir = DirectoriesManager.GetFullDirectoryPath("stream_out");
            await FileUtil.DeleteAsync(Path.Combine(dir, WVW_KILLS_WEEK));
            await FileUtil.DeleteAsync(Path.Combine(dir, WVW_KILLS_DAY));
            await FileUtil.DeleteAsync(Path.Combine(dir, WVW_KILLS_TOTAL));
            await FileUtil.DeleteAsync(Path.Combine(dir, WVW_RANK));
        }

        public override void Dispose()
        {
        }
    }
}