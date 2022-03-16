using System.ComponentModel;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using Nekres.Screenshot_Manager.UI.Models;
using Nekres.Screenshot_Manager.UI.Presenters;
using Container = Blish_HUD.Controls.Container;

namespace Nekres.Screenshot_Manager.UI.Views
{
    public class ScreenshotManagerView : View<ScreenshotManagerPresenter>
    {
        public ScreenshotManagerView(ScreenshotManagerModel model)
        {
            this.WithPresenter(new ScreenshotManagerPresenter(this, model));
        }

        public ScreenshotManagerView()
        {
            /* NOOP */
        }

        protected override void Build(Container buildPanel)
        {
            var thumbnailFlowPanel = new FlowPanel
            {
                Parent = buildPanel,
                Size = new Point(buildPanel.ContentRegion.Size.X - 70, buildPanel.ContentRegion.Size.Y - 130),
                Location = new Point(35, 50),
                FlowDirection = ControlFlowDirection.LeftToRight,
                ControlPadding = new Vector2(5, 5),
                CanCollapse = false,
                CanScroll = true,
                Collapsed = false,
                ShowTint = true,
                ShowBorder = true
            };
            thumbnailFlowPanel.PropertyChanged += delegate (object o, PropertyChangedEventArgs e) {
                if (!e.PropertyName.Equals(nameof(thumbnailFlowPanel.VerticalScrollOffset))) return;
                //TODO: Load/Unload displayed thumbnails that are (not) in view while scrolling.

            };
            var searchBox = new TextBox
            {
                Parent = buildPanel,
                Location = new Point(thumbnailFlowPanel.Location.X, thumbnailFlowPanel.Location.Y - 40),
                Size = new Point(200, 40),
                //PlaceholderText = SearchBoxPlaceHolder
            };
        }
        protected override void Unload()
        {
        }
    }
}
