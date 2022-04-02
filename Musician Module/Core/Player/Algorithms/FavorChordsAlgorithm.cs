using System;
using System.Diagnostics;
using System.Threading;
using Blish_HUD;
using Nekres.Musician.Core.Domain;

namespace Nekres.Musician.Core.Player.Algorithms
{
    public class FavorChordsAlgorithm : IPlayAlgorithm
    {
        private bool Abort = false;
        public void Dispose() { Abort = true; }
        public void Play(Instrument instrument, Metronome metronomeMark, ChordOffset[] melody) {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            for (var strumIndex = 0; strumIndex < melody.Length;)
            {
                if (Abort) return;

                var strum = melody[strumIndex];

                if (stopwatch.ElapsedMilliseconds > metronomeMark.WholeNoteLength.Multiply(strum.Offset).TotalMilliseconds)
                {
                    var chord = strum.Chord;

                    foreach (var note in chord.Notes)
                    {
                        instrument.GoToOctave(note);
                        instrument.PlayNote(note);
                    }

                    strumIndex++;
                }
                else
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(1));
                }
            }

            stopwatch.Stop();
        }
    }
}