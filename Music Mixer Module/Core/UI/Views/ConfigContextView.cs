using System;
using System.Linq;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using Nekres.Music_Mixer.Core.UI.Controls;
using Nekres.Music_Mixer.Core.UI.Models;

namespace Nekres.Music_Mixer.Core.UI.Views
{
    internal class ConfigContextView : View
    {
        public ViewContainer Library { get; private set; }

        private ConfigModel _model;

        public ConfigContextView(ConfigModel model)
        {
            _model = model;
        }

        protected override void Build(Container buildPanel)
        {
            var width = (int)(0.225 * buildPanel.ContentRegion.Width);
            var dayCyclePanel = new FlowPanel
            {
                Parent = buildPanel,
                Width = width * 4 + 20,
                Height = 100,
                Location = new Point((buildPanel.ContentRegion.Width - (width * 4 + 20)) / 2, 2),
                FlowDirection = ControlFlowDirection.LeftToRight,
                ControlPadding = new Vector2(5,5),
                CanScroll = false,
                CanCollapse = false
            };
            foreach (var dayCycle in Enum.GetValues(typeof(TyrianTime)).Cast<TyrianTime>())
            {
                if (dayCycle == TyrianTime.None) continue;
                var dayCycleBttn = new DayCycleSelect(dayCycle)
                {
                    Parent = dayCyclePanel,
                    Width = width,
                    Height = 100,
                    Active = _model.MusicContextModel.DayTimes.Contains(dayCycle)
                };
                dayCycleBttn.Click += (_, _) =>
                {
                    if (_model.MusicContextModel.DayTimes.Contains(dayCycle))
                    {
                        if (_model.MusicContextModel.DayTimes.Count == 1)
                        {
                            ScreenNotification.ShowNotification("You may not disable all day times!", ScreenNotification.NotificationType.Error);
                            GameService.Content.PlaySoundEffectByName("error");
                            return;
                        }
                        _model.MusicContextModel.DayTimes.Remove(dayCycle);
                        dayCycleBttn.Active = false;
                    }
                    else
                    {
                        _model.MusicContextModel.DayTimes.Add(dayCycle);
                        dayCycleBttn.Active = true;
                    }
                    GameService.Content.PlaySoundEffectByName("color-change");
                };
            }

            this.Library = new ViewContainer
            {
                Parent = buildPanel,
                Width = buildPanel.ContentRegion.Width - Panel.RIGHT_PADDING,
                Height = buildPanel.ContentRegion.Height - 100 - Panel.BOTTOM_PADDING,
                Location = new Point(buildPanel.ContentRegion.X, 100 + Panel.BOTTOM_PADDING)
            };
            this.Library.Show(new ConfigMapSelectionView(_model));

            base.Build(buildPanel);
        }
    }
}