using Blish_HUD.Controls.Intern;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;

namespace Nekres.Musician_Module.Controls.Instrument
{
    public class BellSoundRepository : IDisposable
    {
        private readonly Dictionary<string, string> _map = new()
        {
            // Low Octave
            {$"{GuildWarsControls.WeaponSkill1}{BellNote.Octaves.Low}", "D4"},
            {$"{GuildWarsControls.WeaponSkill2}{BellNote.Octaves.Low}", "E4"},
            {$"{GuildWarsControls.WeaponSkill3}{BellNote.Octaves.Low}", "F4"},
            {$"{GuildWarsControls.WeaponSkill4}{BellNote.Octaves.Low}", "G4"},
            {$"{GuildWarsControls.WeaponSkill5}{BellNote.Octaves.Low}", "A4"},
            {$"{GuildWarsControls.HealingSkill}{BellNote.Octaves.Low}", "B4"},
            {$"{GuildWarsControls.UtilitySkill1}{BellNote.Octaves.Low}", "C5"},
            {$"{GuildWarsControls.UtilitySkill2}{BellNote.Octaves.Low}", "D5"},
            // Middle Octave
            {$"{GuildWarsControls.WeaponSkill1}{BellNote.Octaves.Middle}", "D5"},
            {$"{GuildWarsControls.WeaponSkill2}{BellNote.Octaves.Middle}", "E5"},
            {$"{GuildWarsControls.WeaponSkill3}{BellNote.Octaves.Middle}", "F5"},
            {$"{GuildWarsControls.WeaponSkill4}{BellNote.Octaves.Middle}", "G5"},
            {$"{GuildWarsControls.WeaponSkill5}{BellNote.Octaves.Middle}", "A5"},
            {$"{GuildWarsControls.HealingSkill}{BellNote.Octaves.Middle}", "B5"},
            {$"{GuildWarsControls.UtilitySkill1}{BellNote.Octaves.Middle}", "C6"},
            {$"{GuildWarsControls.UtilitySkill2}{BellNote.Octaves.Middle}", "D6"},
            // High Octave
            {$"{GuildWarsControls.WeaponSkill1}{BellNote.Octaves.High}", "D6"},
            {$"{GuildWarsControls.WeaponSkill2}{BellNote.Octaves.High}", "E6"},
            {$"{GuildWarsControls.WeaponSkill3}{BellNote.Octaves.High}", "F6"},
            {$"{GuildWarsControls.WeaponSkill4}{BellNote.Octaves.High}", "G6"},
            {$"{GuildWarsControls.WeaponSkill5}{BellNote.Octaves.High}", "A6"},
            {$"{GuildWarsControls.HealingSkill}{BellNote.Octaves.High}", "B6"},
            {$"{GuildWarsControls.UtilitySkill1}{BellNote.Octaves.High}", "C7"},
            {$"{GuildWarsControls.UtilitySkill2}{BellNote.Octaves.High}", "D7"}
        };


        private readonly Dictionary<string, SoundEffectInstance> _sound = new()
        {
            {"D4", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Bell\D4.wav").CreateInstance()},
            {"E4", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Bell\E4.wav").CreateInstance()},
            {"F4", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Bell\F4.wav").CreateInstance()},
            {"G4", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Bell\G4.wav").CreateInstance()},
            {"A4", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Bell\A4.wav").CreateInstance()},
            {"B4", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Bell\B4.wav").CreateInstance()},
            {"C5", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Bell\C5.wav").CreateInstance()},
            {"D5", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Bell\D5.wav").CreateInstance()},
            {"E5", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Bell\E5.wav").CreateInstance()},
            {"F5", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Bell\F5.wav").CreateInstance()},
            {"G5", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Bell\G5.wav").CreateInstance()},
            {"A5", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Bell\A5.wav").CreateInstance()},
            {"B5", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Bell\B5.wav").CreateInstance()},
            {"C6", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Bell\C6.wav").CreateInstance()},
            {"D6", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Bell\D6.wav").CreateInstance()},
            {"E6", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Bell\E6.wav").CreateInstance()},
            {"F6", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Bell\F6.wav").CreateInstance()},
            {"G6", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Bell\G6.wav").CreateInstance()},
            {"A6", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Bell\A6.wav").CreateInstance()},
            {"B6", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Bell\B6.wav").CreateInstance()},
            {"C7", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Bell\C7.wav").CreateInstance()},
            {"D7", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Bell\D7.wav").CreateInstance()}

        };

        public SoundEffectInstance Get(string id)
        {
            return _sound[id];
        }

        public SoundEffectInstance Get(GuildWarsControls key, BellNote.Octaves octave)
        {
            return _sound[_map[$"{key}{octave}"]];
        }

        public void Dispose() {
            _map?.Clear();
            foreach (var snd in _sound)
                snd.Value?.Dispose();
        }
    }
}