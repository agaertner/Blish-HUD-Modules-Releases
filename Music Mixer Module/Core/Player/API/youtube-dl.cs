﻿using Blish_HUD.Content;
using Gapotchenko.FX.Diagnostics;
using Nekres.Music_Mixer.Core.Player.API.Models;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Blish_HUD;

namespace Nekres.Music_Mixer.Core.Player.API
{
    internal class youtube_dl
    {

        public string ExecutablePath => Path.Combine(MusicMixer.Instance.ModuleDirectory, "bin/youtube-dl.exe");

        private readonly Regex _youtubeVideoId = new (@"youtu(?:\.be|be\.com)/(?:.*v(?:/|=)|(?:.*/)?)(?<id>[a-zA-Z0-9-_]+)", RegexOptions.Compiled);
        private readonly Regex _progressReport = new (@"^\[download\].*?(?<percentage>(.*?))% of (?<size>(.*?))MiB at (?<speed>(.*?)) ETA (?<eta>(.*?))$", RegexOptions.Compiled); //[download]   2.7% of 4.62MiB at 200.00KiB/s ETA 00:23
        private readonly Regex _version = new (@"Updating to version (?<version>(.*?)) \.\.\.", RegexOptions.Compiled); //Updating to version 2015.01.16 ...

        private static youtube_dl _instance;
        private bool _isLoaded;
        private AudioBitrate AverageBitrate => MusicMixer.Instance.AverageBitrateSetting.Value;

        public static youtube_dl Instance
        {
            get { return _instance ??= new youtube_dl(); }
        }

        private youtube_dl()
        {
        }

        public async Task Load()
        {
            if (_isLoaded) return;

            var p = new Process
            {
                StartInfo =
                {
                    CreateNoWindow = true,
                    FileName = ExecutablePath,
                    Arguments = "-U",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                }
            };

            p.Start();
            var info = await p.StandardOutput.ReadLineAsync();
            var match = _version.Match(info);
            if (match.Success)
            {
                await Task.Run(() => p.WaitForExit());
            }
            _isLoaded = true;
        }

        public void GetThumbnail(AsyncTexture2D thumbnail, string id, string link, Action<AsyncTexture2D, string, string> callback)
        {
            using var p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    FileName = ExecutablePath,
                    Arguments = $"--get-thumbnail {link}"
                }
            };
            p.OutputDataReceived += (_, e) => callback(thumbnail, id, e.Data);
            p.Start();
            p.BeginOutputReadLine();
        }

        public async Task<bool> IsUrlSupported(string link)
        {
            using var p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    FileName = ExecutablePath,
                    Arguments = $"--dump-json {link}" // Url is supported if we get any results.
                }
            };
            p.Start();
            return await p.WaitForExitAsync().ContinueWith(t => !t.IsFaulted && p.ExitCode == 0);
        }

        public async Task<string> GetAudioOnlyUrl(string youTubeLink)
        {
            using var p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    FileName = ExecutablePath,
                    Arguments = string.Format("-g {0} -f \"bestaudio[ext=m4a][abr<={1}]/bestaudio[ext=aac][abr<={1}]/bestaudio[abr<={1}]/bestaudio\"", youTubeLink, AverageBitrate.ToString().Substring(1))
                }
            };
            p.Start();
            return await p.WaitForExitAsync().ContinueWith(t =>
            {
                if (t.IsFaulted || p.ExitCode != 0) return string.Empty;
                var data = p.StandardOutput.ReadToEnd();
                if (string.IsNullOrEmpty(data) || data.ToLower().StartsWith("error")) return string.Empty;
                return data;
            });
        }

        public void GetMetaData(string youTubeLink, Func<MetaData, Task> callback)
        {
            using var p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    FileName = ExecutablePath,
                    Arguments = $"--dump-json {youTubeLink}"
                }
            };
            p.OutputDataReceived += async (_, e) =>
            {
                if (string.IsNullOrEmpty(e.Data)) return;
                await callback(JsonConvert.DeserializeObject<MetaData>(e.Data));
            };
            p.Start();
            p.BeginOutputReadLine();
        }

        public void Download(string link, string outputFolder, AudioFormat format, IProgress<string> progress)
        {
            Directory.CreateDirectory(outputFolder);

            using var p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    FileName = ExecutablePath,
                    Arguments = $"{link} -o \"{outputFolder}/%(title)s.%(ext)s\" --restrict-filenames --extract-audio --audio-format {format.ToString().ToLower()} --ffmpeg-location \"{ffmpeg.ExecutablePath}\""
                }
            };
            p.ErrorDataReceived += (_, e) =>
            {
                if (string.IsNullOrEmpty(e.Data)) return;
                var match = _progressReport.Match(e.Data);
                if (!match.Success) return;
                var percent = double.Parse(match.Groups["percentage"].Value, CultureInfo.InvariantCulture) / 100;
                var totalSize = double.Parse(match.Groups["size"].Value, CultureInfo.InvariantCulture);
                var size = percent * totalSize;
                var speed = match.Groups["speed"].Value;
                var eta = match.Groups["eta"].Value;
                var message = $"{size}/{totalSize}MB ({percent}%), {eta}, {speed}";
                progress.Report(message);
                Debug.WriteLine(message);
            };
            p.Start();
        }
    }
}