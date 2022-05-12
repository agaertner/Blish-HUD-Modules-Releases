using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using System;
using System.IO;
using System.Threading.Tasks;
using static Blish_HUD.GameService;

namespace Nekres.Stream_Out.Core.Services
{
    internal class ClientService : ExportService
    {
        private DirectoriesManager DirectoriesManager => StreamOutModule.Instance?.DirectoriesManager;
        private SettingEntry<bool> OnlyLastDigitSettingEntry => StreamOutModule.Instance?.OnlyLastDigitSettingEntry;

        private const string SERVER_ADDRESS = "server_address.txt";
        private string _prevServerAddress;

        public ClientService()
        {
            _prevServerAddress = string.Empty;
        }

        protected override async Task Update()
        {
            if (Gw2Mumble.IsAvailable && !_prevServerAddress.Equals(Gw2Mumble.Info.ServerAddress, StringComparison.InvariantCultureIgnoreCase))
            {
                _prevServerAddress = Gw2Mumble.Info.ServerAddress;
                await FileUtil.WriteAllTextAsync($"{DirectoriesManager.GetFullDirectoryPath("stream_out")}/{SERVER_ADDRESS}", string.IsNullOrEmpty(Gw2Mumble.Info.ServerAddress) ? string.Empty :
                    OnlyLastDigitSettingEntry.Value ? '*' + Gw2Mumble.Info.ServerAddress.Substring(Gw2Mumble.Info.ServerAddress.LastIndexOf('.')) : Gw2Mumble.Info.ServerAddress);
            }
        }

        public override async Task Clear()
        {
            var dir = DirectoriesManager.GetFullDirectoryPath("stream_out");
            await FileUtil.DeleteAsync(Path.Combine(dir, SERVER_ADDRESS));
        }

        public override void Dispose()
        {
        }
    }
}
