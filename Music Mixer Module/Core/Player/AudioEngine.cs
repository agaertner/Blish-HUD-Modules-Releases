using Nekres.Music_Mixer.Core.Player.API;
using System;
using System.Threading.Tasks;
using Blish_HUD;

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

        public AudioEngine()
        {
        }

        public async Task Play(string url)
        {
            if (this.Loading) return;
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
            _soundtrack.FadeIn();
            this.Loading = false;
        }

        public void ToggleSubmerged(bool enable) => _soundtrack?.ToggleSubmergedFx(enable);

        public void Stop()
        {
            _soundtrack?.FadeOut();
        }

        public void Dispose()
        {
            _soundtrack?.Dispose();
        }
    }
}
