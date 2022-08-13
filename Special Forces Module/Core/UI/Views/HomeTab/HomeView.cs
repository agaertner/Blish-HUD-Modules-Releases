using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using Nekres.Special_Forces.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nekres.Special_Forces.Core.Services.Persistance;

namespace Nekres.Special_Forces.Core.UI.Views.HomeTab
{
    internal class HomeView : View<HomePresenter>
    {
        private List<RawTemplate> _templates;

        private string DD_TITLE;
        private string DD_PROFESSION;

        private FlowPanel _templatePanel;

        public HomeView(HomeModel model)
        {
            DD_TITLE = "Title";
            DD_PROFESSION = "Profession";
            this.WithPresenter(new HomePresenter(this, model));
        }

        protected override async Task<bool> Load(IProgress<string> progress)
        {
            _templates = SpecialForcesModule.Instance.TemplateReader.LoadDirectory(SpecialForcesModule.Instance.DirectoriesManager.GetFullDirectoryPath("special_forces"));
            return !SpecialForcesModule.Instance.RenderService.IsLoading;
        }

        protected override void Build(Container buildPanel)
        {
            _templatePanel = new FlowPanel
            {
                Parent = buildPanel,
                Location = new Point(buildPanel.ContentRegion.X, 0),
                Width = buildPanel.ContentRegion.Width,
                Height = buildPanel.ContentRegion.Height - 100,
                ShowTint = true,
                ShowBorder = true,
                CanScroll = true,
                ControlPadding = new Vector2(5,5)
            };
            foreach (var template in _templates)
            {
                AddTemplate(template, _templatePanel);
            }

            var ddSortMethod = new Dropdown
            {
                Parent = buildPanel,
                Location = new Point(buildPanel.ContentRegion.Right - 150 - 10, 5),
                Width = 150
            };
            ddSortMethod.Items.Add(DD_TITLE);
            ddSortMethod.Items.Add(DD_PROFESSION);
            ddSortMethod.ValueChanged += UpdateSort;
            ddSortMethod.SelectedItem = DD_TITLE;
            UpdateSort(ddSortMethod, EventArgs.Empty);

            var sortShowAll = new Checkbox
            {
                Parent = buildPanel,
                Location = new Point(ddSortMethod.Left - 140, 10),
                Text = "Show All",
                Checked = SpecialForcesModule.Instance.LibraryShowAll.Value
            };
            sortShowAll.CheckedChanged += delegate (object sender, CheckChangedEvent e)
            {
                SpecialForcesModule.Instance.LibraryShowAll.Value = e.Checked;
                UpdateSort(ddSortMethod, EventArgs.Empty);
            };
            var import_button = new StandardButton
            {
                Parent = buildPanel,
                Location = new Point(buildPanel.ContentRegion.Right - 150, _templatePanel.Bottom + Panel.BOTTOM_PADDING),
                Text = "Import Json Url",
                Size = new Point(150, 30)
            };
            base.Build(buildPanel);
        }
        private void UpdateSort(object sender, EventArgs e)
        {
        }

        private void AddTemplate(RawTemplate template, Panel parent)
        {
            var build = template.GetBuildChatLink();

            var prof = build.Profession;
            var elite = build.Specialization3Id;

            var isElite = SpecialForcesModule.Instance.RenderService.IsEliteSpec(elite);
            var tex = isElite
                ? SpecialForcesModule.Instance.RenderService.GetEliteRender(elite)
                : SpecialForcesModule.Instance.RenderService.GetProfessionRender(prof);
            var name = isElite 
                ? SpecialForcesModule.Instance.RenderService.GetEliteSpecName(elite) 
                : SpecialForcesModule.Instance.RenderService.GetProfessionName(prof);

            var button = new TemplateButton(template)
            {
                Parent = parent,
                Icon = tex,
                IconSize = DetailsIconSize.Small,
                Text = template.Title,
                BottomText = name
            };
            button.PlayClick += delegate
            {
                SpecialForcesModule.Instance.TemplatePlayer.Play(template);
            };
        }
    }
}
