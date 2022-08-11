using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Gw2Sharp.Models;
using Microsoft.Xna.Framework;
using Nekres.Music_Mixer.Core.UI.Controls;
using Nekres.Music_Mixer.Core.UI.Models;
using System;
using System.Linq;

namespace Nekres.Music_Mixer.Core.UI.Views
{
    internal class MountView : View
    {
        private const int TAB_WIDTH = 42;

        private MainModel _model;

        private ViewContainer _library;

        public MountView(MainModel model)
        {
            _model = model;
        }

        protected override void Build(Container buildPanel)
        {
            var mountTabs = new FlowPanel
            {
                Parent = buildPanel,
                Width = 80,
                Height = buildPanel.ContentRegion.Height,
                Location = new Point(buildPanel.ContentRegion.X, 0),
                FlowDirection = ControlFlowDirection.LeftToRight,
                ControlPadding = new Vector2(5, 5),
                CanScroll = true
            };
            foreach (var type in Enum.GetValues(typeof(MountType)).Cast<MountType>())
            {
                if (type == MountType.None) continue;
                var mountTab = new SkillButton
                {
                    Parent = mountTabs,
                    Texture = MusicMixer.Instance.ContentsManager.GetTexture($"tabs/mounts/skills/{type.ToString().ToLowerInvariant()}_skill.png"),
                    Width = 64,
                    Height = 64,
                    Active = type == _model.MountType,
                    BasicTooltipText = type.ToString()
                };
                mountTab.Click += (o, _) => {
                    GameService.Content.PlaySoundEffectByName($"tab-swap-{RandomUtil.GetRandom(1, 5)}");
                    _model.MountType = type;
                    foreach (var tab in mountTabs.Children.OfType<SkillButton>())
                    {
                        tab.Active = tab == o;
                    }
                };
            }

            var dayCycle = new DayCycleSwitcher
            {
                Parent = buildPanel,
                Width = buildPanel.ContentRegion.Width - mountTabs.Width - TAB_WIDTH - Panel.RIGHT_PADDING * 2,
                Height = 100,
                Location = new Point(mountTabs.Right + Panel.RIGHT_PADDING, 0),
                DayCycle = _model.DayCycle
            };
            dayCycle.Click += (_, _) =>
            {
                GameService.Content.PlaySoundEffectByName($"tab-swap-{RandomUtil.GetRandom(1, 5)}");
                _model.DayCycle = dayCycle.NextCycle();
            };

            _library = new ViewContainer
            {
                Parent = buildPanel,
                Width = buildPanel.ContentRegion.Width - mountTabs.Width - TAB_WIDTH - Panel.RIGHT_PADDING * 2,
                Height = buildPanel.ContentRegion.Height - dayCycle.Height - TAB_WIDTH - Panel.BOTTOM_PADDING * 2,
                Location = new Point(mountTabs.Right + Panel.RIGHT_PADDING, dayCycle.Bottom + Panel.BOTTOM_PADDING)
            };
            _library.Show(new LibraryView(_model));

            base.Build(buildPanel);
        }
    }
}
