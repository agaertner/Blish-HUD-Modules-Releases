using System;
using System.Linq;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Blish_HUD.Settings.UI.Views;
using Microsoft.Xna.Framework;
using Nekres.Musician.UI.Models;
using Nekres.Musician.UI.Presenters;

namespace Nekres.Musician.UI.Views
{
    public class CustomSettingsView : View<CustomSettingsPresenter>
    {
        public event EventHandler<EventArgs> BrowserButtonClick;
        #region SettingsView Defaults

        private FlowPanel _settingFlowPanel;

        private bool _lockBounds = true;

        public bool LockBounds
        {
            get => _lockBounds;
            set
            {
                if (_lockBounds == value) return;

                _lockBounds = value;

                UpdateBoundsLocking(_lockBounds);
            }
        }

        private ViewContainer _lastSettingContainer;

        private void UpdateBoundsLocking(bool locked)
        {
            if (_settingFlowPanel == null) return;

            _settingFlowPanel.ShowBorder = !locked;
            _settingFlowPanel.CanCollapse = !locked;
        }

        #endregion

        private FlowPanel _socialFlowPanel;

        public CustomSettingsView()
        {
            /* NOOP */
        }

        public CustomSettingsView(CustomSettingsModel model)
        {
            this.WithPresenter(new CustomSettingsPresenter(this, model));
        }

        protected override void Build(Container buildPanel)
        {
            _socialFlowPanel = new FlowPanel
            {
                Size = new Point(buildPanel.Width, 78),
                Location = new Point(0, 0),
                FlowDirection = ControlFlowDirection.SingleRightToLeft,
                ControlPadding = new Vector2(27, 2),
                OuterControlPadding = new Vector2(27, 2),
                WidthSizingMode = SizingMode.Fill,
                ShowBorder = true,
                Parent = buildPanel
            };

            var donateBtn = new StandardButton
            {
                Parent = _socialFlowPanel,
                Size = new Point(160,46),
                Text = "Support Me on Ko-Fi",
                Icon = this.Presenter.Model.GetSocialLogo(CustomSettingsModel.Social.KoFi),
                ResizeIcon = true,
                BasicTooltipText = this.Presenter.Model.GetSocialUrl(CustomSettingsModel.Social.KoFi)
            };
            donateBtn.Click += OnBrowserButtonClick;

            var twitch = new StandardButton
            {
                Parent = _socialFlowPanel,
                Size = new Point(100, 46),
                Text = "twitch",
                Icon = this.Presenter.Model.GetSocialLogo(CustomSettingsModel.Social.Twitch),
                ResizeIcon = true,
                BasicTooltipText = this.Presenter.Model.GetSocialUrl(CustomSettingsModel.Social.Twitch)
            };
            twitch.Click += OnBrowserButtonClick;

            _settingFlowPanel = new FlowPanel
            {
                Size = new Point(buildPanel.Width, buildPanel.Height),
                Location = new Point(0, _socialFlowPanel.Height),
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new Vector2(5, 2),
                OuterControlPadding = new Vector2(10, 15),
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                AutoSizePadding = new Point(0, 15),
                Parent = buildPanel
            };

            foreach (var setting in Presenter.Model.Settings.Where(s => s.SessionDefined))
            {
                IView settingView;

                if ((settingView = SettingView.FromType(setting, _settingFlowPanel.Width)) != null)
                {
                    _lastSettingContainer = new ViewContainer
                    {
                        WidthSizingMode = SizingMode.Fill,
                        HeightSizingMode = SizingMode.AutoSize,
                        Parent = _settingFlowPanel
                    };

                    _lastSettingContainer.Show(settingView);

                    if (settingView is SettingsView subSettingsView)
                    {
                        subSettingsView.LockBounds = false;
                    }
                }
            }
        }

        private void OnBrowserButtonClick(object sender, MouseEventArgs e)
        {
            this.BrowserButtonClick?.Invoke(sender, e);
        }

        protected override void Unload()
        {
        }
    }
}
