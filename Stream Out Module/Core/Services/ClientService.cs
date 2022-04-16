using Blish_HUD;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using System;
using System.Threading.Tasks;
using static Blish_HUD.GameService;

namespace Nekres.Stream_Out.Core.Services
{
    internal class ClientService : IExportService, IDisposable
    {
        private Logger Logger => StreamOutModule.Logger;
        private DirectoriesManager DirectoriesManager => StreamOutModule.ModuleInstance?.DirectoriesManager;
        private SettingEntry<bool> OnlyLastDigitSettingEntry => StreamOutModule.ModuleInstance?.OnlyLastDigitSettingEntry;

        private const string SERVER_ADDRESS = "server_address.txt";
        private string _prevServerAddress;

        public ClientService()
        {
            _prevServerAddress = string.Empty;
        }

        public async Task Update()
        {
            if (Gw2Mumble.IsAvailable && !_prevServerAddress.Equals(Gw2Mumble.Info.ServerAddress, StringComparison.InvariantCultureIgnoreCase))
            {
                _prevServerAddress = Gw2Mumble.Info.ServerAddress;
                await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{SERVER_ADDRESS}", string.IsNullOrEmpty(Gw2Mumble.Info.ServerAddress) ? string.Empty :
                    OnlyLastDigitSettingEntry.Value ? '*' + Gw2Mumble.Info.ServerAddress.Substring(Gw2Mumble.Info.ServerAddress.LastIndexOf('.')) : Gw2Mumble.Info.ServerAddress);
            }
        }

        public Task Initialize()
        {
            return Task.CompletedTask;
        }

        public Task ResetDaily()
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {

        }
    }
}
