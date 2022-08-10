using Blish_HUD.Controls;

namespace Nekres.Music_Mixer.Core.UI.Controls
{
    internal class RegionEntry : StandardButton
    {
        private int _regionId;
        public int RegionId
        {
            get => _regionId;
            set => SetProperty(ref _regionId, value);
        }

        private string _regionName;
        public string RegionName
        {
            get => _regionName;
            set => SetProperty(ref _regionName, value);
        }

        private bool _showId;
        public bool ShowId
        {
            get => _showId;
            set
            {
                if (!SetProperty(ref _showId, value)) return;
                this.Text = this.RegionName + (value ? $" ({this.RegionId})" : string.Empty);
            }
        }

        public RegionEntry(int regionId, string regionName)
        {
            this.RegionId = regionId;
            this.RegionName = regionName;
            this.Text = this.RegionName + (this.ShowId ? $" ({this.RegionId})" : string.Empty);
        }
    }
}
