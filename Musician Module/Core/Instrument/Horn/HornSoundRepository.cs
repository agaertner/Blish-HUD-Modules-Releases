using System;
using System.Collections.Generic;
using Blish_HUD.Controls.Intern;
using Nekres.Musician_Module;

namespace Nekres.Musician.Core.Instrument.Horn
{
    public class HornSoundRepository : IDisposable
    {
        private readonly Dictionary<string, string> Map = new Dictionary<string, string>
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


        private readonly Dictionary<string, OggSource> Sound = new Dictionary<string, OggSource>
        {
            {"E3", new OggSource(MusicianModule.ModuleInstance.ContentsManager.GetFileStream(@"instruments\Horn\E3.ogg"))},
            {"F3", new OggSource(MusicianModule.ModuleInstance.ContentsManager.GetFileStream(@"instruments\Horn\F3.ogg"))},
            {"G3", new OggSource(MusicianModule.ModuleInstance.ContentsManager.GetFileStream(@"instruments\Horn\G3.ogg"))},
            {"A3", new OggSource(MusicianModule.ModuleInstance.ContentsManager.GetFileStream(@"instruments\Horn\A3.ogg"))},
            {"B3", new OggSource(MusicianModule.ModuleInstance.ContentsManager.GetFileStream(@"instruments\Horn\B3.ogg"))},
            {"C4", new OggSource(MusicianModule.ModuleInstance.ContentsManager.GetFileStream(@"instruments\Horn\C4.ogg"))},
            {"D4", new OggSource(MusicianModule.ModuleInstance.ContentsManager.GetFileStream(@"instruments\Horn\D4.ogg"))},
            {"E4", new OggSource(MusicianModule.ModuleInstance.ContentsManager.GetFileStream(@"instruments\Horn\E4.ogg"))},
            {"F4", new OggSource(MusicianModule.ModuleInstance.ContentsManager.GetFileStream(@"instruments\Horn\F4.ogg"))},
            {"G4", new OggSource(MusicianModule.ModuleInstance.ContentsManager.GetFileStream(@"instruments\Horn\G4.ogg"))},
            {"A4", new OggSource(MusicianModule.ModuleInstance.ContentsManager.GetFileStream(@"instruments\Horn\A4.ogg"))},
            {"B4", new OggSource(MusicianModule.ModuleInstance.ContentsManager.GetFileStream(@"instruments\Horn\B4.ogg"))},
            {"C5", new OggSource(MusicianModule.ModuleInstance.ContentsManager.GetFileStream(@"instruments\Horn\C5.ogg"))},
            {"D5", new OggSource(MusicianModule.ModuleInstance.ContentsManager.GetFileStream(@"instruments\Horn\D5.ogg"))},
            {"E5", new OggSource(MusicianModule.ModuleInstance.ContentsManager.GetFileStream(@"instruments\Horn\E5.ogg"))},
            {"F5", new OggSource(MusicianModule.ModuleInstance.ContentsManager.GetFileStream(@"instruments\Horn\F5.ogg"))},
            {"G5", new OggSource(MusicianModule.ModuleInstance.ContentsManager.GetFileStream(@"instruments\Horn\G5.ogg"))},
            {"A5", new OggSource(MusicianModule.ModuleInstance.ContentsManager.GetFileStream(@"instruments\Horn\A5.ogg"))},
            {"B5", new OggSource(MusicianModule.ModuleInstance.ContentsManager.GetFileStream(@"instruments\Horn\B5.ogg"))},
            {"C6", new OggSource(MusicianModule.ModuleInstance.ContentsManager.GetFileStream(@"instruments\Horn\C6.ogg"))},
            {"D6", new OggSource(MusicianModule.ModuleInstance.ContentsManager.GetFileStream(@"instruments\Horn\D6.ogg"))},
            {"E6", new OggSource(MusicianModule.ModuleInstance.ContentsManager.GetFileStream(@"instruments\Horn\E6.ogg"))}
        };


        public OggSource Get(string id)
        {
            return Sound[id];
        }


        public OggSource Get(GuildWarsControls key, HornNote.Octaves octave)
        {
            return Sound[Map[$"{key}{octave}"]];
        }


        public void Dispose() {
            Map?.Clear();
            foreach (var snd in Sound)
                snd.Value?.Dispose();
        }
    }
}