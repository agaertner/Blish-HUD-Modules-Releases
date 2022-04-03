using System;
using Blish_HUD;
using Microsoft.Xna.Framework;
using Nekres.Stopwatch.Core.Controls;
using Stopwatch;

namespace Nekres.Stopwatch.Core.Controllers
{
    internal class StopwatchController : IDisposable
    {
        private System.Diagnostics.Stopwatch _stopwatch;

        private TimeSpan _startTime;

        private StopwatchDisplay _display;

        public Color FontColor { get; set; }
        public ContentService.FontSize FontSize { get; set; }

        public StopwatchController()
        {
            _stopwatch = new System.Diagnostics.Stopwatch();
        }

        public void StartAt()
        {
            TimeSpanInputPrompt.ShowPrompt(TimeSpanInputPromptCallback, "Enter a start time:");
        }

        public void Toggle()
        {
            if (_stopwatch.IsRunning)
                _stopwatch.Stop();
            else
                Start();
        }

        private void TimeSpanInputPromptCallback(bool confirmed, TimeSpan time)
        {
            if (!confirmed) return;
            Start(time);
        }

        private void Start(TimeSpan? start = null)
        {
            _display ??= new StopwatchDisplay
            {
                Parent = GameService.Graphics.SpriteScreen,
                Size = new Point(300, 250),
                Location = StopwatchModule.ModuleInstance.Position.Value,
                Color = this.FontColor,
                FontSize = this.FontSize
            };

            if (start.HasValue)
                _startTime = start.Value;

            _stopwatch.Start();
        }

        public void Stop()
        {
            _display?.Dispose();
            _display = null;
            _stopwatch.Stop();
        }

        public void Reset()
        {
            _display?.Dispose();
            _display = null;
            _stopwatch.Reset();
        }

        public void Update()
        {
            if (_display == null) return;

            _display.Text = _startTime.Subtract(_stopwatch.Elapsed).ToString(@"hh\:mm\:ss\.fff");
        }

        public void Dispose()
        {
            _display?.Dispose();
        }
    }
}
