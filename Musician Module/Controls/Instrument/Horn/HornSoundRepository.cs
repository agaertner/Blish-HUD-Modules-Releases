using Blish_HUD.Controls.Intern;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;

namespace Nekres.Musician_Module.Controls.Instrument
{
    public class HornSoundRepository : IDisposable
    {
        private readonly Dictionary<string, string> _map = new()
        {
            // Low Octave
            {$"{GuildWarsControls.WeaponSkill1}{HornNote.Octaves.Low}", "E3"},
            {$"{GuildWarsControls.WeaponSkill2}{HornNote.Octaves.Low}", "F3"},
            {$"{GuildWarsControls.WeaponSkill3}{HornNote.Octaves.Low}", "G3"},
            {$"{GuildWarsControls.WeaponSkill4}{HornNote.Octaves.Low}", "A3"},
            {$"{GuildWarsControls.WeaponSkill5}{HornNote.Octaves.Low}", "B3"},
            {$"{GuildWarsControls.HealingSkill}{HornNote.Octaves.Low}", "C4"},
            {$"{GuildWarsControls.UtilitySkill1}{HornNote.Octaves.Low}", "D4"},
            {$"{GuildWarsControls.UtilitySkill2}{HornNote.Octaves.Low}", "E4"},
            // Middle Octave
            {$"{GuildWarsControls.WeaponSkill1}{HornNote.Octaves.Middle}", "E4"},
            {$"{GuildWarsControls.WeaponSkill2}{HornNote.Octaves.Middle}", "F4"},
            {$"{GuildWarsControls.WeaponSkill3}{HornNote.Octaves.Middle}", "G4"},
            {$"{GuildWarsControls.WeaponSkill4}{HornNote.Octaves.Middle}", "A4"},
            {$"{GuildWarsControls.WeaponSkill5}{HornNote.Octaves.Middle}", "B4"},
            {$"{GuildWarsControls.HealingSkill}{HornNote.Octaves.Middle}", "C5"},
            {$"{GuildWarsControls.UtilitySkill1}{HornNote.Octaves.Middle}", "D5"},
            {$"{GuildWarsControls.UtilitySkill2}{HornNote.Octaves.Middle}", "E5"},
            // High Octave
            {$"{GuildWarsControls.WeaponSkill1}{HornNote.Octaves.High}", "E5"},
            {$"{GuildWarsControls.WeaponSkill2}{HornNote.Octaves.High}", "F5"},
            {$"{GuildWarsControls.WeaponSkill3}{HornNote.Octaves.High}", "G5"},
            {$"{GuildWarsControls.WeaponSkill4}{HornNote.Octaves.High}", "A5"},
            {$"{GuildWarsControls.WeaponSkill5}{HornNote.Octaves.High}", "B5"},
            {$"{GuildWarsControls.HealingSkill}{HornNote.Octaves.High}", "C6"},
            {$"{GuildWarsControls.UtilitySkill1}{HornNote.Octaves.High}", "D6"},
            {$"{GuildWarsControls.UtilitySkill2}{HornNote.Octaves.High}", "E6"}
        };

        private readonly Dictionary<string, SoundEffectInstance> _sound = new()
        {
            {"E3", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Horn\E3.wav").CreateInstance()},
            {"F3", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Horn\F3.wav").CreateInstance()},
            {"G3", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Horn\G3.wav").CreateInstance()},
            {"A3", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Horn\A3.wav").CreateInstance()},
            {"B3", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Horn\B3.wav").CreateInstance()},
            {"C4", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Horn\C4.wav").CreateInstance()},
            {"D4", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Horn\D4.wav").CreateInstance()},
            {"E4", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Horn\E4.wav").CreateInstance()},
            {"F4", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Horn\F4.wav").CreateInstance()},
            {"G4", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Horn\G4.wav").CreateInstance()},
            {"A4", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Horn\A4.wav").CreateInstance()},
            {"B4", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Horn\B4.wav").CreateInstance()},
            {"C5", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Horn\C5.wav").CreateInstance()},
            {"D5", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Horn\D5.wav").CreateInstance()},
            {"E5", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Horn\E5.wav").CreateInstance()},
            {"F5", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Horn\F5.wav").CreateInstance()},
            {"G5", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Horn\G5.wav").CreateInstance()},
            {"A5", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Horn\A5.wav").CreateInstance()},
            {"B5", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Horn\B5.wav").CreateInstance()},
            {"C6", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Horn\C6.wav").CreateInstance()},
            {"D6", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Horn\D6.wav").CreateInstance()},
            {"E6", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Horn\E6.wav").CreateInstance()}
        };

        public SoundEffectInstance Get(string id)
        {
            return _sound[id];
        }

        public SoundEffectInstance Get(GuildWarsControls key, HornNote.Octaves octave)
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