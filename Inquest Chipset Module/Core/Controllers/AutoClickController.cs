using Blish_HUD;
using Blish_HUD.Controls.Intern;
using Blish_HUD.Input;
using Microsoft.Xna.Framework.Audio;
using Nekres.Inquest_Module.UI.Controls;
using System;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
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
        private System.Drawing.Point _togglePos;
        private bool _toggleActive;
        private Color _redShift;

        private TaskIndicator _indicator;
        private ClickIndicator _clickIndicator;

        public AutoClickController()
        {
            _redShift = new Color(255, 57, 57);
            AutoClickHoldKey.Enabled = true;
            AutoClickHoldKey.Activated += OnHoldActivated;
            AutoClickToggleKey.Enabled = true;
            AutoClickToggleKey.Activated += OnToggleActivate;
            _doubleClickSfx = new[]
            {
                InquestModule.ModuleInstance.ContentsManager.GetSound(@"audio\double-click-1.wav"),
                InquestModule.ModuleInstance.ContentsManager.GetSound(@"audio\double-click-2.wav"),
                InquestModule.ModuleInstance.ContentsManager.GetSound(@"audio\double-click-3.wav")
            };
            GameService.Gw2Mumble.PlayerCharacter.IsInCombatChanged += OnIsInCombatChanged;
            GameService.GameIntegration.Gw2Instance.IsInGameChanged += OnIsInGameChanged;
        }

        private void OnIsInCombatChanged(object o, ValueEventArgs<bool> e)
        {
            if (e.Value) DeactivateToggle();
        }
        private void OnIsInGameChanged(object o, ValueEventArgs<bool> e)
        {
            if (!e.Value) DeactivateToggle();
        }
        private void OnHoldActivated(object o, EventArgs e)
        {
            if (_toggleActive) DeactivateToggle();
        }

        private void SaveTogglePosition()
        {
            _togglePos = Mouse.GetPosition();

            _clickIndicator ??= new ClickIndicator
            {
                Parent = GameService.Graphics.SpriteScreen,
                Size = new Point(32, 32),
                Location = new Point(GameService.Input.Mouse.Position.X - 14, GameService.Input.Mouse.Position.Y - 14),
                ZIndex = 1000
            };
        }

        private void OnToggleActivate(object o, EventArgs e)
        {
            if (_toggleActive || IsBusy())
            {
                DeactivateToggle();
                return;
            }

            SaveTogglePosition();
            NumericInputPrompt.ShowPrompt(OnToggleInputPromptCallback, "Enter an interval in seconds:");
        }

        private void DeactivateToggle()
        {
            _toggleActive = false;
            _clickIndicator?.Dispose();
            _clickIndicator = null;
            _indicator?.Dispose();
            _indicator = null;
        }

        private void OnToggleInputPromptCallback(bool confirmed, double input)
        {
            if (!confirmed)
            {
                DeactivateToggle();
                return;
            }
            _toggleActive = true;
            _toggleIntervalMs = Math.Min(300000, Math.Max(250, (int)(input * 1000)));
            _nextToggleClick = DateTime.UtcNow;

            _indicator ??= new TaskIndicator
            {
                Parent = GameService.Graphics.SpriteScreen,
                Size = new Point(50, 50),
                AttachToCursor = false,
                Location = new Point(_clickIndicator.Location.X + 25, _clickIndicator.Location.Y - 32),
            };
        }

        public void UpdateIndicator()
        {
            if (!_toggleActive) return;

            var isBusy = IsBusy();
            _clickIndicator.Paused = isBusy;
            _indicator.Paused = isBusy;
            _indicator.Visible = !isBusy;
            var remainingTime = DateTime.UtcNow.Subtract(_nextToggleClick);
            _indicator.Text = remainingTime.ToString(remainingTime.TotalSeconds > -1 ? @"\.ff" : remainingTime.TotalMinutes > -1 ? "ss" : @"m\:ss").TrimStart('0');
            _indicator.TextColor = Color.Lerp(Color.White, _redShift, 1 + (float)remainingTime.TotalMilliseconds / _toggleIntervalMs);

        }

        public void Update()
        {
            UpdateIndicator();

            if (IsBusy())
            {
                _nextToggleClick = DateTime.UtcNow.Add(_pausedRemainingTime);
                return;
            }

            if (!_toggleActive && AutoClickHoldKey.IsTriggering && DateTime.UtcNow > _nextHoldClick && GameService.GameIntegration.Gw2Instance.Gw2HasFocus)
            {
                if (!InquestModule.ModuleInstance.AutoClickSoundDisabledSetting.Value) DoubleClickSfx.Play(GameService.GameIntegration.Audio.Volume, 0, 0);
                Mouse.DoubleClick(MouseButton.LEFT, -1, -1, true);
                _nextHoldClick = DateTime.UtcNow.AddMilliseconds(50);
            }

            if (_toggleActive && DateTime.UtcNow > _nextToggleClick)
            {
                _clickIndicator.LeftClick();
                if (!InquestModule.ModuleInstance.AutoClickSoundDisabledSetting.Value) DoubleClickSfx.Play(GameService.GameIntegration.Audio.Volume,0,0);
                Mouse.DoubleClick(MouseButton.LEFT, _togglePos.X, _togglePos.Y);
                Mouse.Click(MouseButton.LEFT, _togglePos.X, _togglePos.Y); // WM_BUTTONDBLCLK (0x0203) jams message queue. Unjam with followup click.
                _nextToggleClick = DateTime.UtcNow.AddMilliseconds(_toggleIntervalMs);
            }
        }

        private bool IsBusy()
        {
            var isBusy = !GameService.GameIntegration.Gw2Instance.Gw2IsRunning
                         || !GameService.GameIntegration.Gw2Instance.IsInGame
                         || GameService.Gw2Mumble.UI.IsTextInputFocused
                         || GameService.Input.Mouse.CameraDragging
                         || GameService.Gw2Mumble.PlayerCharacter.IsInCombat;

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
            DeactivateToggle();
            foreach (var sfx in _doubleClickSfx) sfx?.Dispose();
            AutoClickToggleKey.Activated -= OnToggleActivate;
            GameService.Gw2Mumble.PlayerCharacter.IsInCombatChanged -= OnIsInCombatChanged;
            GameService.GameIntegration.Gw2Instance.IsInGameChanged -= OnIsInGameChanged;
        }
    }
}
