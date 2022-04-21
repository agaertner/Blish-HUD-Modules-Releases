using Microsoft.Xna.Framework.Audio;
using Nekres.Musician_Module.Controls.Instrument;
using Nekres.Musician_Module.Domain;
using Nekres.Musician_Module.Player.Algorithms;
using System;
using System.Linq;
using System.Threading;
using NAudio.Wave;

namespace Nekres.Musician_Module.Player
{
    public class MusicPlayer : IDisposable
    {
        public Thread Worker { get; private set; }
        public IPlayAlgorithm Algorithm { get; private set; }

        private SoundEffectInstance _activeSfx;

        private float _audioVolume => MusicianModule.ModuleInstance.audioVolume.Value / 1000;
        public void PlaySound(SoundEffectInstance sfx, bool loops = false) {
            if (loops) {
                StopSound();
                sfx.IsLooped = true;
            }
            _activeSfx = sfx;
            sfx.Volume = _audioVolume;
            sfx.Play();
        }

        public void StopSound() {
            _activeSfx?.Stop();
        }

        public MusicPlayer(MusicSheet musicSheet, Instrument instrument, IPlayAlgorithm algorithm)
        {
            Algorithm = algorithm;
            Worker = new Thread(() => algorithm.Play(instrument, musicSheet.MetronomeMark, musicSheet.Melody.ToArray()));
        }

        public void Dispose()
        {
            StopSound();
            Algorithm.Dispose();
        }
    }
}