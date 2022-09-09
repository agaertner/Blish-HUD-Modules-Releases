using Blish_HUD;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Nekres.Music_Mixer.Core.Player.Source;
using Nekres.Music_Mixer.Core.Player.Source.DSP;
using Nekres.Music_Mixer.Core.Player.Source.Equalizer;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Nekres.Music_Mixer.Core.Player
{
    internal class Soundtrack : IDisposable
    {
        public event EventHandler<EventArgs> Finished;

        private WasapiOut _outputDevice;
        private MediaFoundationReader _mediaProvider;
        private EndOfStreamProvider _endOfStream;
        private VolumeSampleProvider _volumeProvider;
        private FadeInOutSampleProvider _fadeInOut;
        private BiQuadFilterSource _lowPassFilter;
        private Equalizer _equalizer;
        private bool _initialized;

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

        private bool _muted;
        public bool Muted
        {
            get => _muted;
            set
            {
                if (value == _muted) return;
                _muted = value;
                if (_muted)
                {
                    this.Volume = 0;
                }
                else
                {
                    this.Volume = MusicMixer.Instance.MasterVolume;
                }
            }
        }

        public readonly string SourceUri;
        public TimeSpan CurrentTime => _mediaProvider.CurrentTime;
        public TimeSpan TotalTime => _mediaProvider.TotalTime;
        public bool IsBuffering => _endOfStream.IsBuffering;

        private Soundtrack(string url, float volume)
        {
            _volume = volume;
            _outputDevice = new WasapiOut(GameService.GameIntegration.Audio.AudioDevice, AudioClientShareMode.Shared, false, 100);
            _mediaProvider = new MediaFoundationReader(url);
            this.SourceUri = url;

            _endOfStream = new EndOfStreamProvider(_mediaProvider);
            _endOfStream.Ended += OnEndOfStreamReached;

            _volumeProvider = new VolumeSampleProvider(_endOfStream)
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
        }

        public static bool TryGetStream(string url, float volume, out Soundtrack soundTrack)
        {
            soundTrack = null;
            if (string.IsNullOrEmpty(url)) return false;
            var timeout = DateTime.UtcNow.AddMilliseconds(500);
            while (DateTime.UtcNow < timeout)
            {
                try
                {
                    soundTrack = new Soundtrack(url, volume);
                    return true;
                }
                catch (Exception e) when (e is InvalidCastException or UnauthorizedAccessException or COMException)
                {
                    if (DateTime.UtcNow < timeout) continue;
                    MusicMixer.Logger.Error(e, e.Message);
                }
                break;
            }
            return false;
        }

        public void Play(int fadeInDuration = 500)
        {
            var timeout = DateTime.UtcNow.AddMilliseconds(500);
            while (DateTime.UtcNow < timeout)
            {
                try
                {
                    if (!_initialized) {
                        _initialized = true;
                        _outputDevice.Init(_equalizer);
                    }
                    _outputDevice.Play();
                    _fadeInOut.BeginFadeIn(fadeInDuration);
                    this.ToggleSubmergedFx(MusicMixer.Instance.Gw2State.IsSubmerged);
                }
                catch (Exception e) when (e is InvalidCastException or UnauthorizedAccessException or COMException)
                {
                    if (DateTime.UtcNow < timeout) continue;
                    MusicMixer.Logger.Error(e, e.Message);
                }
                break;
            }
        }

        public void Seek(float seconds)
        {
            _mediaProvider?.SetPosition(seconds);
        }

        public void Seek(TimeSpan timespan)
        {
            _mediaProvider?.SetPosition(timespan);
        }

        public void Pause()
        {
            if (_outputDevice == null || _outputDevice.PlaybackState == PlaybackState.Paused) return;
            _outputDevice.Pause();
        }

        public void Resume()
        {
            if (_outputDevice == null || _outputDevice.PlaybackState != PlaybackState.Paused) return;
            _outputDevice.Play();
        }

        private void OnEndOfStreamReached(object o, EventArgs e)
        {
            _endOfStream.Ended -= OnEndOfStreamReached;
            this.Finished?.Invoke(this, EventArgs.Empty);
        }

        public void ToggleSubmergedFx(bool enable)
        {
            if (_equalizer == null) return;
            _lowPassFilter.Enabled = enable;
            _equalizer.SampleFilters[1].AverageGainDB = enable ? 19.5f : 0; // Bass
            _equalizer.SampleFilters[9].AverageGainDB = enable ? 13.4f : 0; // Treble
        }

        public void Dispose()
        {
            _endOfStream.Ended -= OnEndOfStreamReached;
            try
            {
                _outputDevice?.Dispose();
                _mediaProvider?.Dispose();
            }
            catch (Exception e) when (e is NullReferenceException or ObjectDisposedException)
            {
                /* NOOP - Module was unloaded */
            }
        }
    }
}
