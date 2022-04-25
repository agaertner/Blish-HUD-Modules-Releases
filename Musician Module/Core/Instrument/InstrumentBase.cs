using Blish_HUD.Controls.Extern;
using Blish_HUD.Controls.Intern;
using Microsoft.Xna.Framework.Input;
using Nekres.Musician.Core.Domain;
using System;
using System.Threading;
using Keyboard = Blish_HUD.Controls.Intern.Keyboard;

namespace Nekres.Musician.Core.Instrument
{
    public abstract class InstrumentBase : IDisposable
    {
        protected readonly TimeSpan NoteTimeout = TimeSpan.FromMilliseconds(5);
        protected readonly TimeSpan OctaveTimeout = TimeSpan.FromTicks(500);

        protected Octave CurrentOctave { get; set; }

        public readonly bool Walkable;

        protected InstrumentBase(bool walkable = true)
        {
            this.Walkable = walkable;
        }

        protected virtual void PressKey(GuildWarsControls key)
        {
            Keyboard.Press((VirtualKeyShort)GetKeyBinding(key));
            Thread.Sleep(TimeSpan.FromMilliseconds(1));
            Keyboard.Release((VirtualKeyShort)GetKeyBinding(key));
            Thread.Sleep(this.NoteTimeout);
        }

        public void PlayNote(RealNote realNote)
        {
            var note = ConvertNote(realNote);

            if (!RequiresAction(note)) return;

            if (note.Key == GuildWarsControls.None)
                PressKey(GuildWarsControls.EliteSkill);
            else
            {
                note = OptimizeNote(note);
                PressKey(note.Key);
            }
        }

        public void GoToOctave(RealNote realNote)
        {
            var note = ConvertNote(realNote);

            if (!RequiresAction(note)) return;

            note = OptimizeNote(note);

            while (CurrentOctave != note.Octave)
            {
                if (CurrentOctave < note.Octave)
                    IncreaseOctave();
                else
                    DecreaseOctave();
            }
        }

        protected abstract NoteBase ConvertNote(RealNote note);

        private bool RequiresAction(NoteBase note) => note.Key != GuildWarsControls.None;

        protected abstract NoteBase OptimizeNote(NoteBase note);

        protected abstract void IncreaseOctave();

        protected abstract void DecreaseOctave();

        public abstract void Dispose();

        private Keys GetKeyBinding(GuildWarsControls key)
        {
            switch (key)
            {
                case GuildWarsControls.SwapWeapons:
                    return MusicianModule.ModuleInstance.keySwapWeapons.Value.PrimaryKey;
                case GuildWarsControls.WeaponSkill1:
                    return MusicianModule.ModuleInstance.keyWeaponSkill1.Value.PrimaryKey;
                case GuildWarsControls.WeaponSkill2:
                    return MusicianModule.ModuleInstance.keyWeaponSkill2.Value.PrimaryKey;
                case GuildWarsControls.WeaponSkill3:
                    return MusicianModule.ModuleInstance.keyWeaponSkill3.Value.PrimaryKey;
                case GuildWarsControls.WeaponSkill4:
                    return MusicianModule.ModuleInstance.keyWeaponSkill4.Value.PrimaryKey;
                case GuildWarsControls.WeaponSkill5:
                    return MusicianModule.ModuleInstance.keyWeaponSkill5.Value.PrimaryKey;
                case GuildWarsControls.HealingSkill:
                    return MusicianModule.ModuleInstance.keyHealingSkill.Value.PrimaryKey;
                case GuildWarsControls.UtilitySkill1:
                    return MusicianModule.ModuleInstance.keyUtilitySkill1.Value.PrimaryKey;
                case GuildWarsControls.UtilitySkill2:
                    return MusicianModule.ModuleInstance.keyUtilitySkill2.Value.PrimaryKey;
                case GuildWarsControls.UtilitySkill3:
                    return MusicianModule.ModuleInstance.keyUtilitySkill3.Value.PrimaryKey;
                case GuildWarsControls.EliteSkill:
                    return MusicianModule.ModuleInstance.keyEliteSkill.Value.PrimaryKey;
                default: return Keys.None;
            }
        }
    }
}