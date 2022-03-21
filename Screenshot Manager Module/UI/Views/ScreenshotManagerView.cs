using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using Nekres.Screenshot_Manager.Properties;
using Nekres.Screenshot_Manager.UI.Controls;
using Nekres.Screenshot_Manager.UI.Models;
using Nekres.Screenshot_Manager.UI.Presenters;
using Container = Blish_HUD.Controls.Container;

namespace Nekres.Screenshot_Manager.UI.Views
{
    public class ScreenshotManagerView : View<ScreenshotManagerPresenter>
    {
        public FlowPanel ThumbnailFlowPanel { get; private set; }
        public TextBox SearchBox { get; private set; }

        public ScreenshotManagerView(ScreenshotManagerModel model)
        {
            this.WithPresenter(new ScreenshotManagerPresenter(this, model));
        }

        public ScreenshotManagerView()
        {
            /* NOOP */
        }

        protected override async void Build(Container buildPanel)
        {
            ThumbnailFlowPanel = new FlowPanel
            {
                Parent = buildPanel,
                Size = new Point(buildPanel.ContentRegion.Size.X, buildPanel.ContentRegion.Size.Y - 90),
                Location = new Point(0, 50),
                FlowDirection = ControlFlowDirection.LeftToRight,
                ControlPadding = new Vector2(5, 5),
                CanCollapse = false,
                CanScroll = true,
                Collapsed = false,
                ShowTint = true,
                ShowBorder = true
            };
            SearchBox = new TextBox
            {
                Parent = buildPanel,
                Location = new Point(ThumbnailFlowPanel.Location.X, ThumbnailFlowPanel.Location.Y - 40),
                Size = new Point(200, 40),
                PlaceholderText = Resources.Search___
            };

            SearchBox.TextChanged += (_,_) => this.ThumbnailFlowPanel.SortChildren<ResponsiveThumbnail>(this.Presenter.SortThumbnails);

            // Load existing screenshots and do initial sorting
            foreach (var fileName in this.Presenter.Model.FileWatcherFactory.Index)
            {
                var texture = new AsyncTexture2D();
                this.Presenter.CreateThumbnail(this.ThumbnailFlowPanel, texture, fileName);
                await this.Presenter.LoadTexture(texture, fileName);
                this.ThumbnailFlowPanel.SortChildren<ResponsiveThumbnail>(this.Presenter.SortThumbnails);
            }
        }

        protected override void Unload()
        {
        }
    }
}
