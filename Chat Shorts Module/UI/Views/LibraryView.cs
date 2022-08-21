using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Nekres.Chat_Shorts.UI.Controls;
using Nekres.Chat_Shorts.UI.Models;
using Nekres.Chat_Shorts.UI.Presenters;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Nekres.Chat_Shorts.UI.Views
{
    internal class LibraryView : View<LibraryPresenter>
    {
        internal event EventHandler<EventArgs> AddNewClick;

        internal FlowPanel MacroPanel;

        private const int MARGIN_BOTTOM = 10;

        private const string FILTER_ALL = "All";
        private const string FILTER_PVP = "PvP";
        private const string FILTER_WVW = "WvW";
        private const string FILTER_PVE = "PvE";
        private const string FILTER_MAP = "Current Map";

        public LibraryView(LibraryModel model)
        {
            this.WithPresenter(new LibraryPresenter(this, model));
            ChatShorts.Instance.DataService.MacroDeleted += OnMacroDeleted;
        }

        private void OnMacroDeleted(object o, ValueEventArgs<Guid> e)
        {
            var ctrl = this.MacroPanel?.Children
                            .Where(x => x.GetType() == typeof(MacroDetails))
                            .Cast<MacroDetails>().FirstOrDefault(x => x.Model.Id.Equals(e.Value));
            ctrl?.Dispose();
        }

        protected override void Build(Container buildPanel)
        {
            var searchBar = new TextBox
            {
                Parent = buildPanel,
                MaxLength = 256,
                Location = new Point(0, 5),
                Size = new Point(150, 32),
                PlaceholderText = "Search..."
            };
            searchBar.EnterPressed += OnSearchFilterChanged;

            // Sort drop down
            var ddSortMethod = new Dropdown
            {
                Parent = buildPanel,
                Location = new Point(buildPanel.ContentRegion.Width - 5 - 150, 5),
                Width = 150
            };
            ddSortMethod.Items.Add(FILTER_ALL);
            ddSortMethod.Items.Add(FILTER_PVE);
            ddSortMethod.Items.Add(FILTER_PVP);
            ddSortMethod.Items.Add(FILTER_WVW);
            ddSortMethod.Items.Add(FILTER_MAP);
            ddSortMethod.SelectedItem = FILTER_ALL;
            ddSortMethod.ValueChanged += OnSortChanged;
            
            this.MacroPanel = new FlowPanel
            {
                Parent = buildPanel,
                Size = new Point(buildPanel.ContentRegion.Width - 10, buildPanel.ContentRegion.Height - 150),
                Location = new Point(0, ddSortMethod.Bottom + MARGIN_BOTTOM),
                FlowDirection = ControlFlowDirection.LeftToRight,
                ControlPadding = new Vector2(5, 5),
                CanCollapse = false,
                CanScroll = true,
                Collapsed = false,
                ShowTint = true,
                ShowBorder = true
            };

            var btnAddNew = new StandardButton
            {
                Parent = buildPanel,
                Location = new Point((buildPanel.ContentRegion.Width - 100) / 2, this.MacroPanel.Bottom + MARGIN_BOTTOM),
                Size = new Point(100, 35),
                Text = "Add Macro"
            };
            btnAddNew.Click += BtnAddNew_Click;

            foreach (var model in this.Presenter.Model.MacroModels) this.Presenter.AddMacro(model);

        }

        private void BtnAddNew_Click(object o, MouseEventArgs e)
        {
            GameService.Content.PlaySoundEffectByName("button-click");
            AddNewClick?.Invoke(this, EventArgs.Empty);
        }

        private void OnSearchFilterChanged(object o, EventArgs e)
        {
            var text = ((TextBox)o).Text;
            text = string.IsNullOrEmpty(text) ? text : text.ToLowerInvariant();
            this.MacroPanel.SortChildren<MacroDetails>((x, y) =>
            {
                x.Visible = string.IsNullOrEmpty(text) || x.Title.ToLowerInvariant().Contains(text);
                y.Visible = string.IsNullOrEmpty(text) || y.Title.ToLowerInvariant().Contains(text);
                if (!x.Visible || !y.Visible) return 0;
                return string.Compare(x.Title, y.Title, StringComparison.InvariantCultureIgnoreCase);
            });
        }

        private void OnSortChanged(object o, ValueChangedEventArgs e)
        {
            var filter = ((Dropdown)o).SelectedItem;
            this.MacroPanel.SortChildren<MacroDetails>((x, y) =>
            {
                x.Visible = filter.Equals(FILTER_ALL) || x.Model.Mode.ToString().Equals(filter) || x.Model.MapIds.Contains(GameService.Gw2Mumble.CurrentMap.Id);
                y.Visible = filter.Equals(FILTER_ALL) || y.Model.Mode.ToString().Equals(filter) || y.Model.MapIds.Contains(GameService.Gw2Mumble.CurrentMap.Id); ;
                if (!x.Visible || !y.Visible) return 0;
                return string.Compare(x.Title, y.Title, StringComparison.InvariantCultureIgnoreCase);
            });
        }
    }
}
