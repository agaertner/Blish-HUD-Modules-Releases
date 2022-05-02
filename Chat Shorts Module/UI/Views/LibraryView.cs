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

        protected override async Task<bool> Load(IProgress<string> progress)
        {
            this.Presenter.Model.MacroModels = (await ChatShorts.Instance.DataService.GetAll()).Select(MacroModel.FromEntity).ToList();
            return !ChatShorts.Instance.DataService.Loading;
        }

        protected override void Build(Container buildPanel)
        {
            this.MacroPanel = new FlowPanel
            {
                Parent = buildPanel,
                Size = new Point(buildPanel.ContentRegion.Width - 10, buildPanel.ContentRegion.Height - 150),
                Location = new Point(0, 0),
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
                Location = new Point((buildPanel.ContentRegion.Width - 100) / 2, this.MacroPanel.Height + MARGIN_BOTTOM),
                Size = new Point(100, 50),
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
    }
}
