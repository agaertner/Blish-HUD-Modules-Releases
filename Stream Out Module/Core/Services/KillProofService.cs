using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Modules.Managers;

namespace Nekres.Stream_Out.Core.Services
{
    internal class KillProofService : IExportService, IDisposable
    {
        private Logger Logger => StreamOutModule.Logger;
        private DirectoriesManager DirectoriesManager => StreamOutModule.ModuleInstance?.DirectoriesManager;
        private ContentsManager ContentsManager => StreamOutModule.ModuleInstance?.ContentsManager;
        private string AccountName => StreamOutModule.ModuleInstance?.AccountName.Value;

        private const string KILLPROOF_ME_UNSTABLE_FRACTAL_ESSENCE = "unstable_fractal_essence.txt";
        private const string KILLPROOF_ME_LEGENDARY_DIVINATION = "legendary_divination.txt";
        private const string KILLPROOF_ME_LEGENDARY_INSIGHT = "legendary_insight.txt";
        private const string KILLPROOF_API_URL = "https://killproof.me/api/kp/";

        public KillProofService()
        {

        }

        public async Task Initialize()
        {
            var moduleDir = DirectoriesManager.GetFullDirectoryPath("stream_out");
            ContentsManager.ExtractIcons("legendary_divination.png", Path.Combine($@"{moduleDir}\static", "legendary_divination.png"));
            ContentsManager.ExtractIcons("legendary_insight.png", Path.Combine($@"{moduleDir}\static", "legendary_insight.png"));
            ContentsManager.ExtractIcons("unstable_fractal_essence.png", Path.Combine($@"{moduleDir}\static", "unstable_fractal_essence.png"));
        }

        public Task ResetDaily()
        {
            return Task.CompletedTask;
        }

        public async Task Update()
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

        public void Dispose()
        {
        }
    }
}
