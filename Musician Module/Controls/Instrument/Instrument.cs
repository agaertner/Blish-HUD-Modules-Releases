using Blish_HUD.Controls.Extern;
using Blish_HUD.Controls.Intern;
using Microsoft.Xna.Framework.Input;
using Nekres.Musician_Module.Domain.Values;
using System;
using System.Threading;
using Keyboard = Blish_HUD.Controls.Intern.Keyboard;

namespace Nekres.Musician_Module.Controls.Instrument
{
    public enum InstrumentSkillType
    {
        None,
        Note,
        IncreaseOctave,
        DecreaseOctave,
        StopPlaying
    }
    public enum InstrumentMode
    {
        None,
        Preview,
        Practice,
        Emulate
    }

    public abstract class Instrument : IDisposable
    {

        protected IInstrumentPreview Preview;
        public InstrumentMode Mode { get; set; }

        protected virtual void PressKey(GuildWarsControls key, string octave)
        {
            if (Mode == InstrumentMode.Practice)
            {
                InstrumentSkillType noteType;
                switch (key) {
                    case GuildWarsControls.EliteSkill:
                        noteType = InstrumentSkillType.IncreaseOctave;
                        break;
                    case GuildWarsControls.UtilitySkill3:
                        noteType = InstrumentSkillType.DecreaseOctave;
                        break;
                    default:
                        noteType = InstrumentSkillType.Note;
                        break;
                }
                MusicianModule.ModuleInstance.Conveyor.SpawnNoteBlock(key, noteType, Note.OctaveColors[octave]);

            } else if (Mode == InstrumentMode.Emulate) {

                Keyboard.Press((VirtualKeyShort)GetKeyBinding(key));
                Thread.Sleep(TimeSpan.FromMilliseconds(1));
                Keyboard.Release((VirtualKeyShort)GetKeyBinding(key));

            } else if (Mode == InstrumentMode.Preview) {

                Preview.PlaySoundByKey(key);

            }
        }
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

        public abstract void PlayNote(Note note);
        public abstract void GoToOctave(Note note);
        public abstract void Dispose();
    }
}