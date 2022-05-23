using Nekres.Music_Mixer.Core.Player.API;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nekres.Music_Mixer.Core.Player
{
    internal class AudioEngine : IDisposable
    {
        private Soundtrack _soundtrack;

        private float _volume;
        public float Volume
        {
            get => _volume;
            set
            {
                _volume = value;
                if (_soundtrack == null) return;
                _soundtrack.Volume = value;
            }
        }

        public bool Loading { get; private set; }

        private TaskScheduler _scheduler;
        public AudioEngine()
        {
            _scheduler = System.Threading.Tasks.TaskScheduler.FromCurrentSynchronizationContext();
        }

        public async Task Play(string url)
        {
            if (this.Loading || string.IsNullOrEmpty(url)) return;
            this.Loading = true;
            AudioUrlReceived(await youtube_dl.Instance.GetAudioOnlyUrl(url));
        }

        private void AudioUrlReceived(string url)
        {
            if (!Soundtrack.TryGetStream(url, this.Volume, out _soundtrack))
            {
                this.Loading = false;
                return;
            }
            _soundtrack.Finished += OnSoundtrackFinished;
            _soundtrack.FadeIn();
            this.Loading = false;
        }

        public void ToggleSubmerged(bool enable) => _soundtrack?.ToggleSubmergedFx(enable);

        public void Stop()
        {
            _soundtrack?.FadeOut();
        }

        private async void OnSoundtrackFinished(object o, EventArgs e)
        {
            _soundtrack.Finished -= OnSoundtrackFinished;
            if (this.Loading) return;
            await Task.Factory.StartNew(
                async () => await this.Play((await MusicMixer.Instance.DataService.GetRandom())?.Uri), 
                CancellationToken.None, TaskCreationOptions.None, _scheduler).Unwrap();
        }

        public void Dispose()
        {
            _soundtrack?.Dispose();
        }
    }
}
