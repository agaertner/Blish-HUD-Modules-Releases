using Blish_HUD;
using Blish_HUD.Controls.Intern;
using Blish_HUD.Input;
using Microsoft.Xna.Framework.Audio;
using Nekres.Inquest_Module.UI.Controls;
using System;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Color = Microsoft.Xna.Framework.Color;
using Mouse = Blish_HUD.Controls.Intern.Mouse;
using Point = Microsoft.Xna.Framework.Point;
namespace Nekres.Inquest_Module.Core.Controllers
{
    internal class AutoClickController : IDisposable
    {
        private float _soundVolume;
        public float SoundVolume
        {
            get => Math.Min(GameService.GameIntegration.Audio.Volume, _soundVolume);
            set => _soundVolume = value;
        }

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
        private bool _inTogglePrompt;

        private TaskIndicator _indicator;
        private ClickIndicator _clickIndicator;

        private bool _isDisposing;
        public AutoClickController()
        {
            this.SoundVolume = 1;
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
            if (e.Value) Deactivate();
        }
        private void OnIsInGameChanged(object o, ValueEventArgs<bool> e)
        {
            if (!e.Value) Deactivate();
        }
        private void OnHoldActivated(object o, EventArgs e)
        {
            if (_toggleActive) Deactivate();
        }

        private void CreateClickIndicator(bool attachToCursor, bool forceRecreation = false)
        {
            if (forceRecreation) Deactivate();
            _clickIndicator ??= new ClickIndicator(attachToCursor)
            {
                Parent = GameService.Graphics.SpriteScreen,
                Size = new Point(32, 32),
                Location = new Point(GameService.Input.Mouse.Position.X - 14, GameService.Input.Mouse.Position.Y - 14),
            };
        }

        private void OnToggleActivate(object o, EventArgs e)
        {
            if (_toggleActive || IsBusy())
            {
                Deactivate();
                return;
            }
            _inTogglePrompt = true;
            _togglePos = WindowUtil.GetCursorPosition(GameService.GameIntegration.Gw2Instance.Gw2WindowHandle);
            CreateClickIndicator(false, true);
            NumericInputPrompt.ShowPrompt(OnToggleInputPromptCallback, "Enter an interval in seconds:", InquestModule.ModuleInstance.AutoClickToggleInterval.Value);
        }

        private void OnToggleInputPromptCallback(bool confirmed, double input)
        {
            _inTogglePrompt = false;
            if (!confirmed)
            {
                Deactivate();
                return;
            }
            _toggleActive = true;
            _toggleIntervalMs = Math.Min(300000, Math.Max(250, (int)(input * 1000)));
            _nextToggleClick = DateTime.UtcNow;
            InquestModule.ModuleInstance.AutoClickToggleInterval.Value = _toggleIntervalMs / 1000.0;

            _indicator ??= new TaskIndicator(false)
            {
                Parent = GameService.Graphics.SpriteScreen,
                Size = new Point(50, 50),
                Location = new Point(_clickIndicator.Location.X + 25, _clickIndicator.Location.Y - 32),
            };
        }

        private bool HoldIsTriggering() => (InquestModule.ModuleInstance.HoldKeyWithLeftClickEnabledSetting.Value
            ? AutoClickHoldKey.IsTriggering && GameService.Input.Mouse.State.LeftButton == ButtonState.Pressed
            : AutoClickHoldKey.IsTriggering) && !_inTogglePrompt && !_isDisposing;

        public void Update()
        {
            if (_toggleActive)
            {
                var isBusy = IsBusy();

                _clickIndicator.Paused = isBusy;
                _indicator.Paused = isBusy;
                _indicator.Visible = !isBusy;

                if (isBusy)
                {
                    _nextToggleClick = DateTime.UtcNow.Add(_pausedRemainingTime);
                    return;
                }

                var remainingTime = DateTime.UtcNow.Subtract(_nextToggleClick);
                _indicator.Text = remainingTime.ToString(remainingTime.TotalSeconds > -1 ? @"\.ff" : remainingTime.TotalMinutes > -1 ? "ss" : @"m\:ss").TrimStart('0');
                _indicator.TextColor = Color.Lerp(Color.White, _redShift, 1 + (float)remainingTime.TotalMilliseconds / _toggleIntervalMs);

                if (DateTime.UtcNow <= _nextToggleClick) return;
                _clickIndicator.LeftClick();
                if (!InquestModule.ModuleInstance.AutoClickSoundDisabledSetting.Value) DoubleClickSfx.Play(SoundVolume, 0, 0);
                Mouse.DoubleClick(MouseButton.LEFT, _togglePos.X, _togglePos.Y);
                Mouse.Click(MouseButton.LEFT, _togglePos.X, _togglePos.Y); // WM_BUTTONDBLCLK (0x0203) jams message queue. Unjam with followup click.
                _nextToggleClick = DateTime.UtcNow.AddMilliseconds(_toggleIntervalMs);
            }
            else if (HoldIsTriggering())
            {
                CreateClickIndicator(true);
                if (DateTime.UtcNow <= _nextHoldClick || !GameService.GameIntegration.Gw2Instance.Gw2HasFocus) return;
                _clickIndicator.LeftClick(40);
                if (!InquestModule.ModuleInstance.AutoClickSoundDisabledSetting.Value) DoubleClickSfx.Play(SoundVolume, 0, 0);
                var pos = WindowUtil.GetCursorPosition(GameService.GameIntegration.Gw2Instance.Gw2WindowHandle);
                Mouse.DoubleClick(MouseButton.LEFT, pos.X, pos.Y);
                Mouse.Click(MouseButton.LEFT, pos.X, pos.Y);
                _nextHoldClick = DateTime.UtcNow.AddMilliseconds(50);
            }
            else if (!_inTogglePrompt)
            {
                Deactivate();
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

        private void Deactivate()
        {
            _toggleActive = false;
            _clickIndicator?.Dispose();
            _clickIndicator = null;
            _indicator?.Dispose();
            _indicator = null;
        }

        public void Dispose()
        {
            _isDisposing = true;
            AutoClickToggleKey.Activated -= OnToggleActivate;
            GameService.Gw2Mumble.PlayerCharacter.IsInCombatChanged -= OnIsInCombatChanged;
            GameService.GameIntegration.Gw2Instance.IsInGameChanged -= OnIsInGameChanged;
            Deactivate();
            foreach (var sfx in _doubleClickSfx) sfx?.Dispose();
        }
    }
}
