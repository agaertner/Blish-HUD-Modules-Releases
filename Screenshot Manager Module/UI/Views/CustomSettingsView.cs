using System;
using System.Linq;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Blish_HUD.Settings.UI.Views;
using Microsoft.Xna.Framework;
using Nekres.Screenshot_Manager.UI.Models;
using Nekres.Screenshot_Manager.UI.Presenters;

namespace Nekres.Screenshot_Manager.UI.Views
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

            _settingFlowPanel = new FlowPanel
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

            var troubleShootLabel = new Label
            {
                Parent = _settingFlowPanel,
                Size = new Point(_settingFlowPanel.Width / 2, 100),
                Text = "Troubleshooting:\nIf you only see file icons (ie. symbols) instead of thumbnail previews (ie. miniature image previews), please disable \"Always show icons, never thumbnails\" in the File Explorer Options dialogue.",
                WrapText = true,
                Font = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size14, ContentService.FontStyle.Regular)
            };

            var openFolderDiaBttn = new StandardButton
            {
                Parent = _settingFlowPanel,
                Size = new Point(200, 60),
                Text = "Open File Explorer Options",
                BackgroundColor = Color.LightBlue
            };

            openFolderDiaBttn.Click += (_,_) =>
            {
                var process = new System.Diagnostics.Process();
                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                    FileName = "cmd.exe",
                    Arguments = "/C rundll32.exe shell32.dll,Options_RunDLL 7"
                };
                process.StartInfo = startInfo;
                process.Start();
            };
        }

        private void _bttn_Click(object sender, MouseEventArgs e)
        {
            this.SocialButtonClicked?.Invoke(sender, e);
        }

        private void _bttn_MouseEntered(object sender, MouseEventArgs e)
        {
            ((Image)sender).Tint = Color.Gray;
        }

        private void _bttn_MouseLeft(object sender, MouseEventArgs e)
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
