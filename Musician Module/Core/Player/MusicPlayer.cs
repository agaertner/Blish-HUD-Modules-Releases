using System;
using System.Linq;
using System.Threading;
using CSCore;
using CSCore.SoundOut;
using Nekres.Musician.Core.Player.Algorithms;
using Nekres.Musician.Core.Models;

namespace Nekres.Musician.Core.Player
{
    public class MusicPlayer : IDisposable
    {
        public Thread Worker { get; private set; }
        public IPlayAlgorithm Algorithm { get; private set; }
        public WasapiOut OutputDevice { get; private set; }

        public void PlaySound(ISampleSource sampleSource) {
            StopSound();
            OutputDevice.Initialize(sampleSource.ToWaveSource().Loop());
            OutputDevice.Play();
        }

        public void StopSound() {
            OutputDevice.Stop();
        }

        public MusicPlayer(MusicSheet musicSheet, Instrument.Instrument instrument, IPlayAlgorithm algorithm)
        {
            Algorithm = algorithm;
            Worker = new Thread(() => algorithm.Play(instrument, musicSheet.Tempo, musicSheet.Melody.ToArray()));

            OutputDevice = new WasapiOut();
        }
        public void Dispose() {
            Algorithm.Dispose();
            OutputDevice.Stop();
            OutputDevice.Dispose();
        }
    }
}