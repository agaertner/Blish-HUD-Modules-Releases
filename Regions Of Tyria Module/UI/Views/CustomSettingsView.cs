using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Settings.UI.Views;
using Microsoft.Xna.Framework;
using Nekres.Regions_Of_Tyria.UI.Models;
using Nekres.Regions_Of_Tyria.UI.Presenters;
using System;
using System.Linq;
namespace Nekres.Regions_Of_Tyria.UI.Views
{
    public class CustomSettingsView : View<CustomSettingsPresenter>
    {
        public event EventHandler<EventArgs> SocialButtonClicked;

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

            foreach (CustomSettingsModel.Social social in Enum.GetValues(typeof(CustomSettingsModel.Social)))
            {
                var tex = Presenter.Model.GetSocialLogo(social);
                var btn = new Image
                {
                    Parent = _socialFlowPanel,
                    Texture = tex,
                    Size = PointExtensions.ResizeKeepAspect(tex.Bounds.Size, 54, _socialFlowPanel.Height - (int)_socialFlowPanel.ControlPadding.Y * 2),
                    BasicTooltipText = Presenter.Model.GetSocialUrl(social)
                };
                btn.Click += _bttn_Click;
                btn.MouseEntered += _bttn_MouseEntered;
                btn.MouseLeft += _bttn_MouseLeft;
            }

            _settingFlowPanel = new FlowPanel()
            {
                Size = new Point(buildPanel.Width, buildPanel.Height - _socialFlowPanel.Height),
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
                    _lastSettingContainer = new ViewContainer()
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

        private void _bttn_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            this.SocialButtonClicked?.Invoke(sender, e);
        }

        private void _bttn_MouseEntered(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            ((Image)sender).Tint = Color.Gray;
        }

        private void _bttn_MouseLeft(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            ((Image)sender).Tint = Color.White;
        }

        protected override void Unload()
        {
            foreach (var btn in _socialFlowPanel.Children)
            {
                btn.Click -= _bttn_Click;
                btn.MouseEntered -= _bttn_MouseEntered;
                btn.MouseLeft -= _bttn_MouseLeft;
            }
        }
    }
}
