﻿using System;
using System.Collections.Generic;
using System.Threading;
using Blish_HUD.Controls.Intern;
using Nekres.Musician.Core.Domain;

namespace Nekres.Musician.Core.Instrument.Bell
{
    public class Bell : Musician.Core.Instrument.Instrument
    {
        private readonly TimeSpan NoteTimeout = TimeSpan.FromMilliseconds(5);
        private readonly TimeSpan OctaveTimeout = TimeSpan.FromTicks(500);
        private readonly Dictionary<BellNote.Keys, GuildWarsControls> NoteMap = new Dictionary<BellNote.Keys, GuildWarsControls>
        {
            {BellNote.Keys.Note1, GuildWarsControls.WeaponSkill1},
            {BellNote.Keys.Note2, GuildWarsControls.WeaponSkill2},
            {BellNote.Keys.Note3, GuildWarsControls.WeaponSkill3},
            {BellNote.Keys.Note4, GuildWarsControls.WeaponSkill4},
            {BellNote.Keys.Note5, GuildWarsControls.WeaponSkill5},
            {BellNote.Keys.Note6, GuildWarsControls.HealingSkill},
            {BellNote.Keys.Note7, GuildWarsControls.UtilitySkill1},
            {BellNote.Keys.Note8, GuildWarsControls.UtilitySkill2}
        };

        private BellNote.Octaves CurrentOctave = BellNote.Octaves.Low;

        public Bell() {
            Preview = new BellPreview();
        }


        public override void PlayNote(Note note)
        {
            var bellNote = BellNote.From(note);

            if (RequiresAction(bellNote))
            {
                if (bellNote.Key == BellNote.Keys.None)
                {
                    PressNote(GuildWarsControls.EliteSkill);
                }
                else
                {
                    bellNote = OptimizeNote(bellNote);
                    PressNote(NoteMap[bellNote.Key]);
                }
            }
        }


        public override void GoToOctave(Note note)
        {
            var bellNote = BellNote.From(note);

            if (RequiresAction(bellNote))
            {
                bellNote = OptimizeNote(bellNote);

                while (CurrentOctave != bellNote.Octave)
                {
                    if (CurrentOctave < bellNote.Octave)
                    {
                        IncreaseOctave();
                    }
                    else
                    {
                        DecreaseOctave();
                    }
                }
            }
        }


        private static bool RequiresAction(BellNote bellNote)
        {
            return bellNote.Key != BellNote.Keys.None;
        }


        private BellNote OptimizeNote(BellNote note)
        {
            if (note.Equals(new BellNote(BellNote.Keys.Note1, BellNote.Octaves.High)) && CurrentOctave == BellNote.Octaves.Middle)
            {
                note = new BellNote(BellNote.Keys.Note8, BellNote.Octaves.Middle);
            }
            else if (note.Equals(new BellNote(BellNote.Keys.Note8, BellNote.Octaves.Middle)) && CurrentOctave == BellNote.Octaves.High)
            {
                note = new BellNote(BellNote.Keys.Note1, BellNote.Octaves.High);
            }
            else if (note.Equals(new BellNote(BellNote.Keys.Note1, BellNote.Octaves.Middle)) && CurrentOctave == BellNote.Octaves.Low)
            {
                note = new BellNote(BellNote.Keys.Note8, BellNote.Octaves.Low);
            }
            else if (note.Equals(new BellNote(BellNote.Keys.Note8, BellNote.Octaves.Low)) && CurrentOctave == BellNote.Octaves.Middle)
            {
                note = new BellNote(BellNote.Keys.Note1, BellNote.Octaves.Middle);
            }
            return note;
        }


        private void IncreaseOctave()
        {
            switch (CurrentOctave)
            {
                case BellNote.Octaves.Low:
                    CurrentOctave = BellNote.Octaves.Middle;
                    break;
                case BellNote.Octaves.Middle:
                    CurrentOctave = BellNote.Octaves.High;
                    break;
                case BellNote.Octaves.High:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            PressKey(GuildWarsControls.EliteSkill, CurrentOctave.ToString());

            Thread.Sleep(OctaveTimeout);
        }


        private void DecreaseOctave()
        {
            switch (CurrentOctave)
            {
                case BellNote.Octaves.Low:
                    break;
                case BellNote.Octaves.Middle:
                    CurrentOctave = BellNote.Octaves.Low;
                    break;
                case BellNote.Octaves.High:
                    CurrentOctave = BellNote.Octaves.Middle;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            PressKey(GuildWarsControls.UtilitySkill3, CurrentOctave.ToString());

            Thread.Sleep(OctaveTimeout);
        }


        private void PressNote(GuildWarsControls key)
        {
            PressKey(key, CurrentOctave.ToString());
            Thread.Sleep(NoteTimeout);
        }


        public override void Dispose() {
            Preview?.Dispose();
        }
    }
}