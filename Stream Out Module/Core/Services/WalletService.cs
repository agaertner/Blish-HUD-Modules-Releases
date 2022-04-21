﻿using Blish_HUD;
using Blish_HUD.Modules.Managers;
using Gw2Sharp.WebApi.V2.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Nekres.Stream_Out.Core.Services
{
    internal class WalletService : IExportService
    {
        private Logger Logger => StreamOutModule.Logger;
        private Gw2ApiManager Gw2ApiManager => StreamOutModule.ModuleInstance?.Gw2ApiManager;
        private DirectoriesManager DirectoriesManager => StreamOutModule.ModuleInstance?.DirectoriesManager;

        private const string WALLET_COINS = "wallet_coins.png";
        private const string WALLET_KARMA = "wallet_karma.png";

        public WalletService()
        {
        }

        public async Task Update()
        {
            await UpdateWallet();
        }

        public async Task Initialize()
        {
            await Gw2Util.GenerateCoinsImage($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{WALLET_COINS}", 10000000, false);
            await Gw2Util.GenerateKarmaImage($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{WALLET_KARMA}", 10000000, false);
        }

        public async Task ResetDaily()
        {
        }

        private async Task UpdateWallet()
        {
            if (!Gw2ApiManager.HasPermissions(new[] { TokenPermission.Account, TokenPermission.Wallet }))
                return;

            await Gw2ApiManager.Gw2ApiClient.V2.Account.Wallet.GetAsync().ContinueWith(async task =>
            {
                if (task.IsFaulted) return;
                var coins = task.Result.First(x => x.Id == 1).Value; // Coins
                await Gw2Util.GenerateCoinsImage($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{WALLET_COINS}", coins);

                var karma = task.Result.First(x => x.Id == 2).Value; // Karma
                await Gw2Util.GenerateKarmaImage($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{WALLET_KARMA}", karma);
            });
        }

        public void Dispose()
        {
        }
    }
}