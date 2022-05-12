using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Gw2Sharp.WebApi.V2.Models;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Nekres.Stream_Out.Core.Services
{
    internal class PvpService : ExportService
    {
        private Gw2ApiManager Gw2ApiManager => StreamOutModule.Instance?.Gw2ApiManager;
        private DirectoriesManager DirectoriesManager => StreamOutModule.Instance?.DirectoriesManager;
        private ContentsManager ContentsManager => StreamOutModule.Instance?.ContentsManager;
        private StreamOutModule.UnicodeSigning UnicodeSigning => StreamOutModule.Instance?.AddUnicodeSymbols.Value ?? StreamOutModule.UnicodeSigning.Suffixed;
        private SettingEntry<int> SessionKillsPvP => StreamOutModule.Instance?.SessionKillsPvP;
        private SettingEntry<int> TotalKillsAtResetPvP => StreamOutModule.Instance?.TotalKillsAtResetPvP;

        private const string PVP_KILLS_TOTAL = "pvp_kills_total.txt";
        private const string PVP_KILLS_DAY = "pvp_kills_day.txt";
        private const string PVP_RANK = "pvp_rank.txt";
        private const string PVP_RANK_ICON = "pvp_rank_icon.png";
        private const string PVP_TIER_ICON = "pvp_tier_icon.png";
        private const string PVP_WINRATE = "pvp_winrate.txt";

        private const string SWORDS = "\u2694"; // ⚔

        public PvpService()
        {
        }

        public override async Task Initialize()
        {
            await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{PVP_RANK}", "Bronze I", false);
            await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{PVP_WINRATE}", "50%", false);
            await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{PVP_KILLS_DAY}", $"0{SWORDS}", false);
            await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{PVP_KILLS_TOTAL}", $"0{SWORDS}", false);
            await Gw2Util.GeneratePvpTierImage($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{PVP_TIER_ICON}", 1, 3, false);

            var moduleDir = DirectoriesManager.GetFullDirectoryPath("stream_out");
            ContentsManager.ExtractIcons("1614804.png", Path.Combine(moduleDir, PVP_RANK_ICON));
        }

        private async Task<int> RequestTotalKillsForPvP()
        {
            if (!Gw2ApiManager.HasPermissions(new[] { TokenPermission.Account, TokenPermission.Progression }))
                return -1;
            return await Gw2ApiManager.Gw2ApiClient.V2.Account.Achievements.GetAsync().ContinueWith(response =>
            {
                if (response.IsFaulted) return -1;
                return response.Result.Single(x => x.Id == 239).Current; // Slayer
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

                    await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{PVP_RANK}", $"{rank.Name} {tier.ToRomanNumeral()}");
                    await TextureUtil.SaveToImage(rank.OverlaySmall, $"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{PVP_RANK_ICON}");
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
                await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{PVP_WINRATE}", $"{Math.Round(winRatio).ToString(CultureInfo.InvariantCulture)}%");
            });
        }

        protected override async Task ResetDaily()
        {
            SessionKillsPvP.Value = 0;
            TotalKillsAtResetPvP.Value = await RequestTotalKillsForPvP();
        }

        protected override async Task Update()
        {
            await UpdateStandingsForPvP();
            await UpdateStatsForPvp();

            var prefixKills = UnicodeSigning == StreamOutModule.UnicodeSigning.Prefixed ? SWORDS : string.Empty;
            var suffixKills = UnicodeSigning == StreamOutModule.UnicodeSigning.Suffixed ? SWORDS : string.Empty;

            var totalKillsPvP = await RequestTotalKillsForPvP();
            if (totalKillsPvP >= 0)
            {
                SessionKillsPvP.Value = totalKillsPvP - TotalKillsAtResetPvP.Value;
                await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{PVP_KILLS_DAY}", $"{prefixKills}{SessionKillsPvP.Value}{suffixKills}");
                await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{PVP_KILLS_TOTAL}", $"{prefixKills}{totalKillsPvP}{suffixKills}");
            }
        }

        public override async Task Clear()
        {
            var dir = DirectoriesManager.GetFullDirectoryPath("stream_out");
            await FileUtil.DeleteAsync(Path.Combine(dir, PVP_KILLS_TOTAL));
            await FileUtil.DeleteAsync(Path.Combine(dir, PVP_KILLS_DAY));
            await FileUtil.DeleteAsync(Path.Combine(dir, PVP_RANK));
            await FileUtil.DeleteAsync(Path.Combine(dir, PVP_RANK_ICON));
            await FileUtil.DeleteAsync(Path.Combine(dir, PVP_TIER_ICON));
            await FileUtil.DeleteAsync(Path.Combine(dir, PVP_WINRATE));
        }

        public override void Dispose()
        {
        }
    }
}
