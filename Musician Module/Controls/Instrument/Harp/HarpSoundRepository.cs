using Blish_HUD.Controls.Intern;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;

namespace Nekres.Musician_Module.Controls.Instrument
{
    public class HarpSoundRepository : IDisposable
    {
        private readonly Dictionary<string, string> _map = new()
        {
            {$"{GuildWarsControls.WeaponSkill1}{HarpNote.Octaves.Low}", "C3"},
            {$"{GuildWarsControls.WeaponSkill2}{HarpNote.Octaves.Low}", "D3"},
            {$"{GuildWarsControls.WeaponSkill3}{HarpNote.Octaves.Low}", "E3"},
            {$"{GuildWarsControls.WeaponSkill4}{HarpNote.Octaves.Low}", "F3"},
            {$"{GuildWarsControls.WeaponSkill5}{HarpNote.Octaves.Low}", "G3"},
            {$"{GuildWarsControls.HealingSkill}{HarpNote.Octaves.Low}", "A3"},
            {$"{GuildWarsControls.UtilitySkill1}{HarpNote.Octaves.Low}", "B3"},
            {$"{GuildWarsControls.UtilitySkill2}{HarpNote.Octaves.Low}", "C4"},
            {$"{GuildWarsControls.WeaponSkill1}{HarpNote.Octaves.Middle}", "C4"},
            {$"{GuildWarsControls.WeaponSkill2}{HarpNote.Octaves.Middle}", "D4"},
            {$"{GuildWarsControls.WeaponSkill3}{HarpNote.Octaves.Middle}", "E4"},
            {$"{GuildWarsControls.WeaponSkill4}{HarpNote.Octaves.Middle}", "F4"},
            {$"{GuildWarsControls.WeaponSkill5}{HarpNote.Octaves.Middle}", "G4"},
            {$"{GuildWarsControls.HealingSkill}{HarpNote.Octaves.Middle}", "A4"},
            {$"{GuildWarsControls.UtilitySkill1}{HarpNote.Octaves.Middle}", "B4"},
            {$"{GuildWarsControls.UtilitySkill2}{HarpNote.Octaves.Middle}", "C5"},
            {$"{GuildWarsControls.WeaponSkill1}{HarpNote.Octaves.High}", "C5"},
            {$"{GuildWarsControls.WeaponSkill2}{HarpNote.Octaves.High}", "D5"},
            {$"{GuildWarsControls.WeaponSkill3}{HarpNote.Octaves.High}", "E5"},
            {$"{GuildWarsControls.WeaponSkill4}{HarpNote.Octaves.High}", "F5"},
            {$"{GuildWarsControls.WeaponSkill5}{HarpNote.Octaves.High}", "G5"},
            {$"{GuildWarsControls.HealingSkill}{HarpNote.Octaves.High}", "A5"},
            {$"{GuildWarsControls.UtilitySkill1}{HarpNote.Octaves.High}", "B5"},
            {$"{GuildWarsControls.UtilitySkill2}{HarpNote.Octaves.High}", "C6"}
        };

        private readonly Dictionary<string, SoundEffectInstance> _sound = new()
        {
            {"C3", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Harp\C3.wav").CreateInstance()},
            {"D3", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Harp\D3.wav").CreateInstance()},
            {"E3", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Harp\E3.wav").CreateInstance()},
            {"F3", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Harp\F3.wav").CreateInstance()},
            {"G3", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Harp\G3.wav").CreateInstance()},
            {"A3", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Harp\A3.wav").CreateInstance()},
            {"B3", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Harp\B3.wav").CreateInstance()},
            {"C4", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Harp\C4.wav").CreateInstance()},
            {"D4", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Harp\D4.wav").CreateInstance()},
            {"E4", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Harp\E4.wav").CreateInstance()},
            {"F4", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Harp\F4.wav").CreateInstance()},
            {"G4", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Harp\G4.wav").CreateInstance()},
            {"A4", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Harp\A4.wav").CreateInstance()},
            {"B4", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Harp\B4.wav").CreateInstance()},
            {"C5", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Harp\C5.wav").CreateInstance()},
            {"D5", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Harp\D5.wav").CreateInstance()},
            {"E5", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Harp\E5.wav").CreateInstance()},
            {"F5", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Harp\F5.wav").CreateInstance()},
            {"G5", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Harp\G5.wav").CreateInstance()},
            {"A5", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Harp\A5.wav").CreateInstance()},
            {"B5", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Harp\B5.wav").CreateInstance()},
            {"C6", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Harp\C6.wav").CreateInstance()}
        };

        public SoundEffectInstance Get(string id)
        {
            return _sound[id];
        }

        public SoundEffectInstance Get(GuildWarsControls key, HarpNote.Octaves octave)
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