using Blish_HUD.Controls.Intern;
using Blish_HUD.Input;
using System;
using System.Drawing;
using Blish_HUD;
using Nekres.Inquest_Module.UI.Controls;

namespace Nekres.Inquest_Module.Core.Controllers
{
    internal class AutoClickController : IDisposable
    {
        private KeyBinding AutoClickHoldKey => InquestModule.ModuleInstance.AutoClickHoldKeySetting.Value;
        private KeyBinding AutoClickToggleKey => InquestModule.ModuleInstance.AutoClickToggleKeySetting.Value;

        private DateTime _nextHoldClick = DateTime.UtcNow;

        private DateTime _nextToggleClick = DateTime.UtcNow;

        private bool _paused;
        private TimeSpan _pausedRemainingTime;

        private int _toggleIntervalMs;
        private Point _togglePos;
        private bool _toggleActive;

        private TaskIndicator _indicator;
        public AutoClickController()
        {
            AutoClickHoldKey.Enabled = true;
            AutoClickToggleKey.Enabled = true;
            AutoClickToggleKey.Activated += OnToggleActivate;
        }

        private void OnToggleActivate(object o, EventArgs e)
        {
            if (_toggleActive)
            {
                _toggleActive = false;
                _indicator?.Dispose();
                _indicator = null;
                return;
            }

            _togglePos = Mouse.GetPosition();
            NumericInputPrompt.ShowPrompt(OnToggleInputPromptCallback, "Enter an interval in seconds:");
        }

        private void OnToggleInputPromptCallback(bool confirmed, int input)
        {
            if (!confirmed) return;
            _toggleActive = true;
            _toggleIntervalMs = Math.Min(300000, Math.Max(75, input * 1000));
            _nextToggleClick = DateTime.UtcNow;
        }

        public void UpdateIndicator()
        {
            if (!_toggleActive) return;

            if (_indicator != null)
            {
                _indicator.Paused = IsBusy();
                _indicator.Text = DateTime.UtcNow.Subtract(_nextToggleClick).ToString(@"m\:ss");
                _indicator.Visible = !GameService.Input.Mouse.CameraDragging;
                return;
            }

            _indicator = new TaskIndicator
            {
                Parent = GameService.Graphics.SpriteScreen,
                Size = new Microsoft.Xna.Framework.Point(50, 50)
            };
        }

        public void Update()
        {
            UpdateIndicator();

            if (IsBusy())
            {
                _nextToggleClick = DateTime.UtcNow.Add(_pausedRemainingTime);
                return;
            }

            if (!_toggleActive && AutoClickHoldKey.IsTriggering && DateTime.UtcNow > _nextHoldClick)
            {
                Mouse.DoubleClick(MouseButton.LEFT, -1, -1, true);
                _nextHoldClick = DateTime.UtcNow.AddMilliseconds(50);
            }

            if (_toggleActive && DateTime.UtcNow > _nextToggleClick)
            {
                Mouse.SetPosition(_togglePos.X, _togglePos.Y, true);
                Mouse.DoubleClick(MouseButton.LEFT, -1, -1, true);
                _nextToggleClick = DateTime.UtcNow.AddMilliseconds(_toggleIntervalMs);
            }
        }

        private bool IsBusy()
        {
            var isBusy = !GameService.GameIntegration.Gw2Instance.Gw2IsRunning
                         || !GameService.GameIntegration.Gw2Instance.Gw2HasFocus
                         || GameService.Gw2Mumble.UI.IsTextInputFocused
                         || GameService.Input.Mouse.CameraDragging;

            if (isBusy)
            {
                if (_paused) return true;
                _paused = true;
                _pausedRemainingTime = _nextToggleClick.Subtract(DateTime.UtcNow);
                return true;
            }
            if (_paused) _paused = false;
            return false;
        }

        public void Dispose()
        {
            AutoClickToggleKey.Activated -= OnToggleActivate;
        }
    }
}
