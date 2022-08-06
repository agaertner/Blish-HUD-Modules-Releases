using System;
using System.Linq;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Gw2Sharp.Models;
using Microsoft.Xna.Framework;
using Nekres.Music_Mixer.Core.Services;
using Nekres.Music_Mixer.Core.UI.Controls;
using Nekres.Music_Mixer.Core.UI.Models;
using Nekres.Music_Mixer.Core.UI.Presenters;

namespace Nekres.Music_Mixer.Core.UI.Views
{
    internal class ContextView : View<ContextPresenter>
    {
        public ViewContainer Library { get; private set; }

        public ContextView(MainModel model)
        {
            this.WithPresenter(new ContextPresenter(this, model));
        }

        protected override void Build(Container buildPanel)
        {
            FlowPanel mountTabs = null;
            if (this.Presenter.Model.State == Gw2StateService.State.Mounted)
            {
                mountTabs = new FlowPanel
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
                        Active = type == MountType.Raptor
                    };
                    mountTab.Click += (o,_) => {
                        this.Presenter.Model.MountType = type;

                        foreach (var tab in mountTabs.Children.OfType<SkillButton>())
                        {
                            tab.Active = tab == o;
                        }
                    };
                }
            }

            var day = new Image
            {
                Parent = buildPanel,
                Width = 64,
                Height = 64,
                Location = new Point((mountTabs?.Right ?? 0) + buildPanel.ContentRegion.Width / 2 - 64, 0),
                Texture = MusicMixer.Instance.ContentsManager.GetTexture("icons/sun.png")
            };
            day.Click += (_, _) => this.Presenter.Model.DayCycle = TyrianTime.Day;
            var night = new Image
            {
                Parent = buildPanel,
                Width = 64,
                Height = 64,
                Location = new Point((mountTabs?.Right ?? 0) + buildPanel.ContentRegion.Width / 2, 0),
                Texture = MusicMixer.Instance.ContentsManager.GetTexture("icons/moon.png")
            };
            night.Click += (_, _) => this.Presenter.Model.DayCycle = TyrianTime.Night;

            this.Library = new ViewContainer
            {
                Parent = buildPanel,
                Width = buildPanel.ContentRegion.Width - (mountTabs?.Width ?? 0),
                Height = buildPanel.ContentRegion.Height - 64,
                Location = new Point(mountTabs?.Width ?? buildPanel.ContentRegion.X, 0),
                ShowTint = true
            };

            base.Build(buildPanel);
        }
    }
}
