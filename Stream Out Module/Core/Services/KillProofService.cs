using Blish_HUD;
using Blish_HUD.Modules.Managers;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Blish_HUD.Extended;

namespace Nekres.Stream_Out.Core.Services
{
    internal class KillProofService : ExportService
    {
        private DirectoriesManager DirectoriesManager => StreamOutModule.Instance?.DirectoriesManager;
        private ContentsManager ContentsManager => StreamOutModule.Instance?.ContentsManager;
        private string AccountName => StreamOutModule.Instance?.AccountName.Value;

        private const string KILLPROOF_ME_UNSTABLE_FRACTAL_ESSENCE = "unstable_fractal_essence.txt";
        private const string KILLPROOF_ME_LEGENDARY_DIVINATION = "legendary_divination.txt";
        private const string KILLPROOF_ME_LEGENDARY_INSIGHT = "legendary_insight.txt";
        private const string KILLPROOF_API_URL = "https://killproof.me/api/kp/";

        public KillProofService()
        {
        }

        public override async Task Initialize()
        {
            var moduleDir = DirectoriesManager.GetFullDirectoryPath("stream_out");
            await ContentsManager.Extract("legendary_divination.png", Path.Combine($@"{moduleDir}\static", "legendary_divination.png"));
            await ContentsManager.Extract("legendary_insight.png", Path.Combine($@"{moduleDir}\static", "legendary_insight.png"));
            await ContentsManager.Extract("unstable_fractal_essence.png", Path.Combine($@"{moduleDir}\static", "unstable_fractal_essence.png"));
        }

        protected override async Task Update()
        {
            await UpdateKillProofs();
        }

        private async Task UpdateKillProofs()
        {
            await TaskUtil.GetJsonResponse<dynamic>($"{KILLPROOF_API_URL}{AccountName}?lang={GameService.Overlay.UserLocale.Value}").ContinueWith(async task =>
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

        public override async Task Clear()
        {
            var dir = DirectoriesManager.GetFullDirectoryPath("stream_out");
            await FileUtil.DeleteAsync(Path.Combine(dir, KILLPROOF_ME_UNSTABLE_FRACTAL_ESSENCE));
            await FileUtil.DeleteAsync(Path.Combine(dir, KILLPROOF_ME_LEGENDARY_DIVINATION));
            await FileUtil.DeleteAsync(Path.Combine(dir, KILLPROOF_ME_LEGENDARY_INSIGHT));
            await FileUtil.DeleteDirectoryAsync(Path.Combine(Path.Combine(dir, "static")));
        }

        public override void Dispose()
        {
        }
    }
}
