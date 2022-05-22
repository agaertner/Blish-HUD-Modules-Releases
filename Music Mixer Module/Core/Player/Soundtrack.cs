using Blish_HUD;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Nekres.Music_Mixer.Core.Player.Source;
using System;
using System.IO;
using System.Threading.Tasks;
using Nekres.Music_Mixer.Core.Player.Source.DSP;
using Nekres.Music_Mixer.Core.Player.Source.Equalizer;

namespace Nekres.Music_Mixer.Core.Player
{
    internal class Soundtrack : IDisposable
    {
        enum StreamingPlaybackState
        {
            Stopped,
            Playing,
            Buffering,
            Paused
        }

        private WasapiOut _outputDevice;
        private MediaFoundationReader _mediaProvider;
        private VolumeSampleProvider _volumeProvider;
        private FadeInOutSampleProvider _fadeInOut;
        private BiQuadFilterSource _lowPassFilter;
        private Equalizer _equalizer;

        private volatile StreamingPlaybackState _playbackState;

        private Stream _stream;

        private float _volume;
        public float Volume
        {
            get => _volume;
            set
            {
                _volume = value;
                if (_volumeProvider == null) return;
                _volumeProvider.Volume = value;
            }
        }

        private Soundtrack(string url, float volume)
        {
            _volume = volume;
            _outputDevice = new WasapiOut(GameService.GameIntegration.Audio.AudioDevice, AudioClientShareMode.Shared, false, 100);
            _mediaProvider = new MediaFoundationReader(url);
            
        }

        public static bool TryGetStream(string url, float volume, out Soundtrack soundTrack)
        {
            try {
                soundTrack = new Soundtrack(url, volume);
                return true;
            } catch (InvalidCastException e)
            {
                soundTrack = null;
                MusicMixer.Logger.Error(e, e.Message);
                return false;
            }
        }

        private void Play(int fadeInDuration = 500)
        {
            _playbackState = StreamingPlaybackState.Playing;

            _volumeProvider = new VolumeSampleProvider(_mediaProvider.ToSampleProvider())
            {
                Volume = this.Volume
            };

            _fadeInOut = new FadeInOutSampleProvider(_volumeProvider);

            // Filter is toggled when submerged.
            _lowPassFilter = new BiQuadFilterSource(_fadeInOut)
            {
                Filter = new LowPassFilter(_fadeInOut.WaveFormat.SampleRate, 400)
            };
            _equalizer = Equalizer.Create10BandEqualizer(_lowPassFilter);

            _outputDevice.Init(_equalizer);
            _outputDevice.Play();

            _fadeInOut.BeginFadeIn(fadeInDuration);
        }

        public void ToggleSubmergedFx(bool enable)
        {
            if (_equalizer == null) return;
            _lowPassFilter.Enabled = enable;
            _equalizer.SampleFilters[1].AverageGainDB = enable ? 19.5f : 0; // Bass
            _equalizer.SampleFilters[9].AverageGainDB = enable ? 13.4f : 0; // Treble
        }

        public void FadeIn(int durationMs = 500)
        {
            this.Play(durationMs);
        }

        public async Task FadeOut(int durationMs = 500)
        {
            _fadeInOut.BeginFadeOut(durationMs);
            await Task.Delay(durationMs).ContinueWith(_ => this.Dispose());
        }

        public void Dispose()
        {
            _outputDevice?.Dispose();
            _mediaProvider?.Dispose();
            _stream?.Dispose();
            _playbackState = StreamingPlaybackState.Stopped;
        }
    }
}
