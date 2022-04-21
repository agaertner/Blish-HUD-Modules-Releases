using Blish_HUD.Controls.Intern;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;

namespace Nekres.Musician_Module.Controls.Instrument
{
    public class LuteSoundRepository : IDisposable
    {
        private readonly Dictionary<string, string> _map = new()
        {
            // Low Octave
            {$"{GuildWarsControls.WeaponSkill1}{LuteNote.Octaves.Low}", "C3"},
            {$"{GuildWarsControls.WeaponSkill2}{LuteNote.Octaves.Low}", "D3"},
            {$"{GuildWarsControls.WeaponSkill3}{LuteNote.Octaves.Low}", "E3"},
            {$"{GuildWarsControls.WeaponSkill4}{LuteNote.Octaves.Low}", "F3"},
            {$"{GuildWarsControls.WeaponSkill5}{LuteNote.Octaves.Low}", "G3"},
            {$"{GuildWarsControls.HealingSkill}{LuteNote.Octaves.Low}", "A3"},
            {$"{GuildWarsControls.UtilitySkill1}{LuteNote.Octaves.Low}", "B3"},
            {$"{GuildWarsControls.UtilitySkill2}{LuteNote.Octaves.Low}", "C4"},
            // Middle Octave
            {$"{GuildWarsControls.WeaponSkill1}{LuteNote.Octaves.Middle}", "C4"},
            {$"{GuildWarsControls.WeaponSkill2}{LuteNote.Octaves.Middle}", "D4"},
            {$"{GuildWarsControls.WeaponSkill3}{LuteNote.Octaves.Middle}", "E4"},
            {$"{GuildWarsControls.WeaponSkill4}{LuteNote.Octaves.Middle}", "F4"},
            {$"{GuildWarsControls.WeaponSkill5}{LuteNote.Octaves.Middle}", "G4"},
            {$"{GuildWarsControls.HealingSkill}{LuteNote.Octaves.Middle}", "A4"},
            {$"{GuildWarsControls.UtilitySkill1}{LuteNote.Octaves.Middle}", "B4"},
            {$"{GuildWarsControls.UtilitySkill2}{LuteNote.Octaves.Middle}", "C5"},
            // High Octave
            {$"{GuildWarsControls.WeaponSkill1}{LuteNote.Octaves.High}", "C5"},
            {$"{GuildWarsControls.WeaponSkill2}{LuteNote.Octaves.High}", "D5"},
            {$"{GuildWarsControls.WeaponSkill3}{LuteNote.Octaves.High}", "E5"},
            {$"{GuildWarsControls.WeaponSkill4}{LuteNote.Octaves.High}", "F5"},
            {$"{GuildWarsControls.WeaponSkill5}{LuteNote.Octaves.High}", "G5"},
            {$"{GuildWarsControls.HealingSkill}{LuteNote.Octaves.High}", "A5"},
            {$"{GuildWarsControls.UtilitySkill1}{LuteNote.Octaves.High}", "B5"},
            {$"{GuildWarsControls.UtilitySkill2}{LuteNote.Octaves.High}", "C6"},
        };

        private readonly Dictionary<string, SoundEffectInstance> _sound = new()
        {
            {"C3", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Lute\C3.wav").CreateInstance()},
            {"D3", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Lute\D3.wav").CreateInstance()},
            {"E3", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Lute\E3.wav").CreateInstance()},
            {"F3", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Lute\F3.wav").CreateInstance()},
            {"G3", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Lute\G3.wav").CreateInstance()},
            {"A3", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Lute\A3.wav").CreateInstance()},
            {"B3", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Lute\B3.wav").CreateInstance()},
            {"C4", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Lute\C4.wav").CreateInstance()},
            {"D4", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Lute\D4.wav").CreateInstance()},
            {"E4", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Lute\E4.wav").CreateInstance()},
            {"F4", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Lute\F4.wav").CreateInstance()},
            {"G4", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Lute\G4.wav").CreateInstance()},
            {"A4", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Lute\A4.wav").CreateInstance()},
            {"B4", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Lute\B4.wav").CreateInstance()},
            {"C5", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Lute\C5.wav").CreateInstance()},
            {"D5", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Lute\D5.wav").CreateInstance()},
            {"E5", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Lute\E5.wav").CreateInstance()},
            {"F5", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Lute\F5.wav").CreateInstance()},
            {"G5", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Lute\G5.wav").CreateInstance()},
            {"A5", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Lute\A5.wav").CreateInstance()},
            {"B5", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Lute\B5.wav").CreateInstance()},
            {"C6", MusicianModule.ModuleInstance.ContentsManager.GetSound(@"instruments\Lute\C6.wav").CreateInstance()}

        };

        public SoundEffectInstance Get(string id)
        {
            return _sound[id];
        }

        public SoundEffectInstance Get(GuildWarsControls key, LuteNote.Octaves octave)
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
