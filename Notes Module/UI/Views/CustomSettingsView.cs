using System;
using System.Linq;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Settings.UI.Views;
using Microsoft.Xna.Framework;
using Nekres.Notes.UI.Models;
using Nekres.Notes.UI.Presenters;
namespace Nekres.Notes.UI.Views
{
    public class CustomSettingsView : View<CustomSettingsPresenter>
    {
        public event EventHandler<EventArgs> SocialButtonClicked;

        public event EventHandler<EventArgs> LoginButtonClicked;

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

        private StandardButton _loginButton;

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
                    Size = Blish_HUD.PointExtensions.ResizeKeepAspect(tex.Bounds.Size, 54, _socialFlowPanel.Height - (int)_socialFlowPanel.ControlPadding.Y * 2),
                    BasicTooltipText = Presenter.Model.GetSocialUrl(social)
                };
                btn.Click += OnSocialButtonClick;
                btn.MouseEntered += OnSocialButtonMouseEntered;
                btn.MouseLeft += OnSocialButtonMouseLeft;
            }

            /*_loginButton = new StandardButton
            {
                Parent = buildPanel,
                Size = new Point(100, 50),
                Location = new Point(0, _socialFlowPanel.Height),
                Text = "Login with GW2Auth"
            };
            _loginButton.Click += OnLoginButtonClick;*/

            _settingFlowPanel = new FlowPanel
            {
                Size = new Point(buildPanel.Width, buildPanel.Height - _socialFlowPanel.Height),
                Location = new Point(0, _loginButton?.Bottom ?? _socialFlowPanel.Height),
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

        private void OnSocialButtonClick(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            this.SocialButtonClicked?.Invoke(sender, e);
        }

        private void OnSocialButtonMouseEntered(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            ((Image)sender).Tint = Color.Gray;
        }

        private void OnSocialButtonMouseLeft(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            ((Image)sender).Tint = Color.White;
        }

        private void OnLoginButtonClick(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            this.LoginButtonClicked?.Invoke(sender, e);
        }
        protected override void Unload()
        {
            foreach (var btn in _socialFlowPanel.Children)
            {
                btn.Click -= OnSocialButtonClick;
                btn.MouseEntered -= OnSocialButtonMouseEntered;
                btn.MouseLeft -= OnSocialButtonMouseLeft;
            }
            //_loginButton.Click -= OnLoginButtonClick;
        }
    }
}
