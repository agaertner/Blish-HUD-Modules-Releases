﻿using System;
using System.Collections.Generic;
using System.Threading;
using Blish_HUD.Controls.Intern;
using Nekres.Musician.Core.Domain;

namespace Nekres.Musician.Core.Instrument.Horn
{
    public class Horn : Musician.Core.Instrument.Instrument
    {
        private readonly TimeSpan NoteTimeout = TimeSpan.FromMilliseconds(5);
        private readonly TimeSpan OctaveTimeout = TimeSpan.FromTicks(500);

        private readonly Dictionary<HornNote.Keys, GuildWarsControls> NoteMap = new Dictionary<HornNote.Keys, GuildWarsControls>
        {
            {HornNote.Keys.Note1, GuildWarsControls.WeaponSkill1},
            {HornNote.Keys.Note2, GuildWarsControls.WeaponSkill2},
            {HornNote.Keys.Note3, GuildWarsControls.WeaponSkill3},
            {HornNote.Keys.Note4, GuildWarsControls.WeaponSkill4},
            {HornNote.Keys.Note5, GuildWarsControls.WeaponSkill5},
            {HornNote.Keys.Note6, GuildWarsControls.HealingSkill},
            {HornNote.Keys.Note7, GuildWarsControls.UtilitySkill1},
            {HornNote.Keys.Note8, GuildWarsControls.UtilitySkill2}
        };

        private HornNote.Octaves CurrentOctave = HornNote.Octaves.Low;

        public Horn() { 
            Preview = new HornPreview(); 
        }


        public override void PlayNote(Note note)
        {
            var hornNote = HornNote.From(note);

            if (RequiresAction(hornNote))
            {
                if (hornNote.Key == HornNote.Keys.None)
                {
                    PressNote(GuildWarsControls.EliteSkill);
                }
                else
                {
                    hornNote = OptimizeNote(hornNote);
                    PressNote(NoteMap[hornNote.Key]);
                }
            }
        }


        public override void GoToOctave(Note note)
        {
            var hornNote = HornNote.From(note);

            if (RequiresAction(hornNote))
            {
                hornNote = OptimizeNote(hornNote);

                while (CurrentOctave != hornNote.Octave)
                {
                    if (CurrentOctave < hornNote.Octave)
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


        private static bool RequiresAction(HornNote hornNote)
        {
            return hornNote.Key != HornNote.Keys.None;
        }


        private HornNote OptimizeNote(HornNote note)
        {
            if (note.Equals(new HornNote(HornNote.Keys.Note1, HornNote.Octaves.High)) && CurrentOctave == HornNote.Octaves.Middle)
            {
                note = new HornNote(HornNote.Keys.Note8, HornNote.Octaves.Middle);
            }
            else if (note.Equals(new HornNote(HornNote.Keys.Note8, HornNote.Octaves.Middle)) && CurrentOctave == HornNote.Octaves.High)
            {
                note = new HornNote(HornNote.Keys.Note1, HornNote.Octaves.High);
            }
            else if (note.Equals(new HornNote(HornNote.Keys.Note1, HornNote.Octaves.Middle)) && CurrentOctave == HornNote.Octaves.Low)
            {
                note = new HornNote(HornNote.Keys.Note8, HornNote.Octaves.Low);
            }
            else if (note.Equals(new HornNote(HornNote.Keys.Note8, HornNote.Octaves.Low)) && CurrentOctave == HornNote.Octaves.Middle)
            {
                note = new HornNote(HornNote.Keys.Note1, HornNote.Octaves.Middle);
            }
            return note;
        }


        private void IncreaseOctave()
        {
            switch (CurrentOctave)
            {
                case HornNote.Octaves.Low:
                    CurrentOctave = HornNote.Octaves.Middle;
                    break;
                case HornNote.Octaves.Middle:
                    CurrentOctave = HornNote.Octaves.High;
                    break;
                case HornNote.Octaves.High:
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
                case HornNote.Octaves.Low:
                    break;
                case HornNote.Octaves.Middle:
                    CurrentOctave = HornNote.Octaves.Low;
                    break;
                case HornNote.Octaves.High:
                    CurrentOctave = HornNote.Octaves.Middle;
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