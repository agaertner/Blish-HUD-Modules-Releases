﻿using System;
using Blish_HUD;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Nekres.Stopwatch.Core.Controls;
using Stopwatch;

namespace Nekres.Stopwatch.Core.Controllers
{
    internal class StopwatchController : IDisposable
    {
        private System.Diagnostics.Stopwatch _stopwatch;

        private TimeSpan _startTime;

        private StopwatchDisplay _display;

        private SoundEffect[] _rewindSfx;
        private SoundEffect RewindSfx => _rewindSfx[RandomUtil.GetRandom(0, _rewindSfx.Length - 1)];

        private SoundEffect _startSfx;

        private SoundEffect _beepSfx;
        private SoundEffect _longBeepSfx;

        private SoundEffect _tickSfx;
        private TimeSpan _prevTick;

        private Color _fontColor;
        public Color FontColor
        {
            get => _fontColor;
            set
            {
                _fontColor = value;
                if (_display == null) return;
                _display.Color = value;
            }
        }

        private ContentService.FontSize _fontSize;
        public ContentService.FontSize FontSize
        {
            get => _fontSize;
            set
            {
                _fontSize = value;
                if (_display == null) return;
                _display.FontSize = value;
            }
        }

        private Point _position;
        public Point Position
        {
            get => _position;
            set
            {
                _position = value;
                if (_display == null) return;
                _display.Location = value;
            }
        }

        public float AudioVolume { get; set; }

        private Color _redShift;

        private bool _inInputPrompt;
        public StopwatchController()
        {
            _rewindSfx = new[]
            {
                StopwatchModule.ModuleInstance.ContentsManager.GetSound(@"audio\stopwatch-rewind-1.wav"),
                StopwatchModule.ModuleInstance.ContentsManager.GetSound(@"audio\stopwatch-rewind-2.wav"),
                StopwatchModule.ModuleInstance.ContentsManager.GetSound(@"audio\stopwatch-rewind-3.wav"),
                StopwatchModule.ModuleInstance.ContentsManager.GetSound(@"audio\stopwatch-rewind-4.wav")
            };
            _startSfx = StopwatchModule.ModuleInstance.ContentsManager.GetSound(@"audio\stopwatch-start.wav");
            _tickSfx = StopwatchModule.ModuleInstance.ContentsManager.GetSound(@"audio\stopwatch-tick.wav");
            _beepSfx = StopwatchModule.ModuleInstance.ContentsManager.GetSound(@"audio\beep.wav");
            _longBeepSfx = StopwatchModule.ModuleInstance.ContentsManager.GetSound(@"audio\long-beep.wav");

            _redShift = new Color(255, 57, 57);
            _stopwatch = new System.Diagnostics.Stopwatch();
        }

        public void StartAt()
        {
            var prevValue = StopwatchModule.ModuleInstance.StartTime.Value;
            _inInputPrompt = true;
            Reset();
            TimeSpanInputPrompt.ShowPrompt(TimeSpanInputPromptCallback, "Enter a start time:", 
                prevValue.Equals(TimeSpan.Zero) ? string.Empty : prevValue.ToString(@"hh\:mm\:ss\.fff"));
        }

        public void Toggle()
        {
            if (_inInputPrompt) return;
            if (_stopwatch.IsRunning)
                _stopwatch.Stop();
            else
                Start();
        }

        private void TimeSpanInputPromptCallback(bool confirmed, TimeSpan time)
        {
            _inInputPrompt = false;
            if (!confirmed) return;
            StopwatchModule.ModuleInstance.StartTime.Value = time;
            Start(time);
        }
        private void Start(TimeSpan? start = null)
        {
            _display ??= new StopwatchDisplay
            {
                Parent = GameService.Graphics.SpriteScreen,
                Size = new Point(300, 250),
                Location = this.Position,
                Color = this.FontColor,
                FontSize = this.FontSize
            };

            if (start.HasValue)
                _startTime = start.Value;

            _startSfx.Play(AudioVolume, 0, 0);
            _stopwatch.Start();
            _prevTick = TimeSpan.Zero;
        }

        public void Stop()
        {
            _display?.Dispose();
            _display = null;
            _stopwatch.Stop();
        }

        public void Reset()
        {
            RewindSfx.Play(AudioVolume, 0, 0);
            _display?.Dispose();
            _display = null;
            _stopwatch.Reset();
        }

        public void Update()
        {
            if (_display == null) return;

            if (!StopwatchModule.ModuleInstance.TickingSoundDisabledSetting.Value && _stopwatch.Elapsed.Subtract(_prevTick).TotalMilliseconds > 500)
            {
                _tickSfx.Play(AudioVolume, 0, 0);
                _prevTick = _stopwatch.Elapsed;
            }

            if (_startTime.Equals(TimeSpan.Zero))
            {
                _display.Text = _stopwatch.Elapsed.ToString(@"hh\:mm\:ss\.fff");
                return;
            }

            var current = _startTime.Subtract(_stopwatch.Elapsed);
            _display.Text = (current.Ticks < 0 ? "-" : "") + _startTime.Subtract(_stopwatch.Elapsed).ToString(@"hh\:mm\:ss\.fff");
            _display.Color = Color.Lerp(Color.White, _redShift, _stopwatch.ElapsedMilliseconds / (float)_startTime.TotalMilliseconds);
        }

        public void Dispose()
        {
            _display?.Dispose();
            _stopwatch.Stop();
            foreach (var sfx in _rewindSfx) sfx.Dispose();
            _startSfx.Dispose();
            _tickSfx.Dispose();
            _beepSfx.Dispose();
            _longBeepSfx.Dispose();
        }
    }
}
