using Blish_HUD.Controls;

namespace Nekres.Music_Mixer.Core.UI.Controls
{
    internal class MapEntry : StandardButton
    {
        private int _mapId;
        public int MapId
        {
            get => _mapId;
            set => SetProperty(ref _mapId, value);
        }

        private string _mapName;
        public string MapName
        {
            get => _mapName;
            set => SetProperty(ref _mapName, value);
        }

        public MapEntry(int mapId, string mapName)
        {
            this.MapId = mapId;
            this.MapName = mapName;
            this.Text = $"{this.MapName} ({this.MapId})";
        }
    }
}
