using Blish_HUD;
using Blish_HUD.Controls.Intern;
using Blish_HUD.Input;
using Microsoft.Xna.Framework.Audio;
using Nekres.Inquest_Module.UI.Controls;
using System;
using System.Drawing;
using Color = Microsoft.Xna.Framework.Color;

namespace Nekres.Inquest_Module.Core.Controllers
{
    internal class AutoClickController : IDisposable
    {
        private KeyBinding AutoClickHoldKey => InquestModule.ModuleInstance.AutoClickHoldKeySetting.Value;
        private KeyBinding AutoClickToggleKey => InquestModule.ModuleInstance.AutoClickToggleKeySetting.Value;

        private SoundEffect[] _doubleClickSfx;
        public SoundEffect DoubleClickSfx => _doubleClickSfx[RandomUtil.GetRandom(0, 2)];

        private DateTime _nextHoldClick = DateTime.UtcNow;

        private DateTime _nextToggleClick = DateTime.UtcNow;

        private bool _paused;
        private TimeSpan _pausedRemainingTime;

        private int _toggleIntervalMs;
        private Point _togglePos;
        private bool _toggleActive;
        private Color _redShift;

        private TaskIndicator _indicator;
        public AutoClickController()
        {
            _redShift = new Color(255, 105, 105);
            AutoClickHoldKey.Enabled = true;
            AutoClickToggleKey.Enabled = true;
            AutoClickToggleKey.Activated += OnToggleActivate;
            _doubleClickSfx = new[]
            {
                InquestModule.ModuleInstance.ContentsManager.GetSound(@"audio\double-click-1.wav"),
                InquestModule.ModuleInstance.ContentsManager.GetSound(@"audio\double-click-2.wav"),
                InquestModule.ModuleInstance.ContentsManager.GetSound(@"audio\double-click-3.wav")
            };
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

        private void OnToggleInputPromptCallback(bool confirmed, double input)
        {
            if (!confirmed) return;
            _toggleActive = true;
            _toggleIntervalMs = Math.Min(300000, Math.Max(250, (int)(input * 1000)));
            _nextToggleClick = DateTime.UtcNow;
        }

        public void UpdateIndicator()
        {
            if (!_toggleActive) return;

            if (_indicator != null)
            {
                var remainingTime = DateTime.UtcNow.Subtract(_nextToggleClick);
                _indicator.Paused = IsBusy();
                _indicator.Text = remainingTime.ToString(remainingTime.TotalSeconds > -1 ? @"\.ff" : remainingTime.TotalMinutes > -1 ? "ss" : @"m\:ss").TrimStart('0');
                _indicator.TextColor = Color.Lerp(Color.White, _redShift, 1 + (float)remainingTime.TotalMilliseconds / _toggleIntervalMs);
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
                if (!InquestModule.ModuleInstance.AutoClickSoundDisabledSetting.Value) DoubleClickSfx.Play();
                Mouse.DoubleClick(MouseButton.LEFT, -1, -1, true);
                _nextHoldClick = DateTime.UtcNow.AddMilliseconds(50);
            }

            if (_toggleActive && DateTime.UtcNow > _nextToggleClick)
            {
                var savePos = Mouse.GetPosition();
                Mouse.SetPosition(_togglePos.X, _togglePos.Y, true);
                if (!InquestModule.ModuleInstance.AutoClickSoundDisabledSetting.Value) DoubleClickSfx.Play();
                Mouse.DoubleClick(MouseButton.LEFT, _togglePos.X, _togglePos.Y, true);
                _nextToggleClick = DateTime.UtcNow.AddMilliseconds(_toggleIntervalMs);
                Mouse.SetPosition(savePos.X, savePos.Y, true);
            }
        }

        private bool IsBusy()
        {
            var isBusy = !GameService.GameIntegration.Gw2Instance.Gw2IsRunning
                         || !GameService.GameIntegration.Gw2Instance.Gw2HasFocus
                         || !GameService.Gw2Mumble.IsAvailable
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
            foreach (var sfx in _doubleClickSfx) sfx?.Dispose();
            AutoClickToggleKey.Activated -= OnToggleActivate;
        }
    }
}
