using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using Nekres.Music_Mixer.Core.UI.Controls;
using Nekres.Music_Mixer.Core.UI.Models;
using Nekres.Music_Mixer.Core.UI.Presenters;
using Color = Microsoft.Xna.Framework.Color;

namespace Nekres.Music_Mixer.Core.UI.Views
{
    internal class ConfigView : View<MusicContextConfigPresenter>
    {
        public Container Parent { get; private set; }

        private const int MARGIN = 10;

        public ConfigView(MusicContextModel model)
        {
            this.WithPresenter(new MusicContextConfigPresenter(this, model));
        }

        protected override void Build(Container buildPanel)
        {
            this.Parent = buildPanel;

            var thumbnail = new LoadingImage(this.Presenter.Model.Thumbnail)
            {
                Parent = buildPanel,
                Size = new Point(128, 72),
                Location = new Point(Panel.LEFT_PADDING, Panel.TOP_PADDING)
            };

            var artistHeight = GameService.Content.DefaultFont12.MeasureString(this.Presenter.Model.Artist).Height;

            var title = new Label
            {
                Parent = buildPanel,
                Text = this.Presenter.Model.Title,
                Size = new Point(buildPanel.Width - thumbnail.Width - MARGIN * 2 - 100, thumbnail.Height - (int)artistHeight),
                Location = new Point(thumbnail.Right + MARGIN, thumbnail.Location.Y),
                Font = GameService.Content.DefaultFont18,
                StrokeText = true,
                WrapText = true,
                VerticalAlignment = VerticalAlignment.Top
            };

            var artist = new Label
            {
                Parent = buildPanel,
                Text = this.Presenter.Model.Artist,
                Size = new Point(title.Width, title.Height),
                Location = new Point(title.Location.X, thumbnail.Bottom - (int)artistHeight),
                Font = GameService.Content.DefaultFont12,
                TextColor = Color.LightGray,
                VerticalAlignment = VerticalAlignment.Top
            };

            var duration = new Label
            {
                Parent = buildPanel,
                Text = this.Presenter.Model.Duration.ToShortForm(),
                Size = new Point(buildPanel.ContentRegion.Width - thumbnail.Width - title.Width, thumbnail.Height),
                Location = new Point(title.Right - 5, thumbnail.Location.Y),
                Font = GameService.Content.DefaultFont16,
                TextColor = Color.LightGray
            };

            var volumeLabel = new Label
            {
                Parent = buildPanel,
                Text = "Volume",
                Location = new Point(buildPanel.ContentRegion.X, thumbnail.Bottom + Panel.BOTTOM_PADDING * 2),
                Width = buildPanel.ContentRegion.Width / 4 - Panel.RIGHT_PADDING,
                Height = 32,
                HorizontalAlignment = HorizontalAlignment.Center,
                Font = GameService.Content.DefaultFont16
            };

            var volumeTrackBar = new TrackBar2
            {
                Parent = buildPanel,
                Location = new Point(volumeLabel.Right + Panel.RIGHT_PADDING, volumeLabel.Location.Y + (volumeLabel.Height - 16) / 2),
                Width = buildPanel.ContentRegion.Width / 2 - volumeLabel.Width,
                Height = 16,
                MinValue = 0f,
                MaxValue = 100f,
                Value = MathHelper.Clamp(this.Presenter.Model.Volume * 1000f, 0f, 100f)
            };
            volumeTrackBar.DraggingStopped += (_, e) => this.Presenter.Model.Volume = e.Value / 1000f;
        }
    }
}