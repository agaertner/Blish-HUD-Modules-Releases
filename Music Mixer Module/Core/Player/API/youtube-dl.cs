using Blish_HUD.Content;
using Gapotchenko.FX.Diagnostics;
using Nekres.Music_Mixer.Core.Player.API.Models;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Nekres.Music_Mixer.Core.Player.API
{
    internal class youtube_dl
    {
        private static string ExecutablePath => Path.Combine(MusicMixer.Instance.ModuleDirectory, "bin/youtube-dl.exe");
        private static AudioBitrate AverageBitrate => MusicMixer.Instance.AverageBitrateSetting.Value;

        private static readonly Regex _youtubeVideoId = new (@"youtu(?:\.be|be\.com)/(?:.*v(?:/|=)|(?:.*/)?)(?<id>[a-zA-Z0-9-_]+)", RegexOptions.Compiled);
        private static readonly Regex _progressReport = new (@"^\[download\].*?(?<percentage>(.*?))% of (?<size>(.*?))MiB at (?<speed>(.*?)) ETA (?<eta>(.*?))$", RegexOptions.Compiled); //[download]   2.7% of 4.62MiB at 200.00KiB/s ETA 00:23
        private static readonly Regex _version = new (@"^Updating to version (?<toVersion>(.*?)) \.\.\.$|^youtube-dl is up-to-date \((?<isVersion>(.*?))\)$", RegexOptions.Compiled); //Updating to version 2015.01.16 ... | youtube-dl is up-to-date (2021.12.17)

        public static void Load()
        {
            var p = new Process
            {
                StartInfo =
                {
                    CreateNoWindow = true,
                    FileName = ExecutablePath,
                    Arguments = "-U",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                }
            };
            p.OutputDataReceived += (_, e) =>
            {
                if (string.IsNullOrEmpty(e.Data)) return;
                var data = e.Data;
                var match = _version.Match(data);
                if (!match.Success) return;
                var toVersion = match.Groups["toVersion"].Value;
                var isVersion = match.Groups["isVersion"].Value;
                var version = string.IsNullOrEmpty(isVersion) ? toVersion : isVersion;
                MusicMixer.Logger.Info($"Using youtube-dl version {version}");
            };
            p.ErrorDataReceived += (_, e) =>
            {
                if (string.IsNullOrEmpty(e.Data)) return;
                MusicMixer.Logger.Error($"Failed to load or update youtube-dl: \"{e.Data}\"");
            };
            p.Start();
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();
            
        }

        public static void GetThumbnail(AsyncTexture2D thumbnail, string id, string link, Action<AsyncTexture2D, string, string> callback)
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
            p.OutputDataReceived += (_, e) => callback.Invoke(thumbnail, id, e.Data);
            p.Start();
            p.BeginOutputReadLine();
        }

        public static async Task<bool> IsUrlSupported(string link)
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

        public static void GetAudioOnlyUrl<TModel>(string link, Func<string, TModel, Task> callback, TModel model = default)
        {
            using var p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    FileName = ExecutablePath,
                    Arguments = string.Format("-g {0} -f \"bestaudio[ext=m4a][abr<={1}]/bestaudio[ext=aac][abr<={1}]/bestaudio[abr<={1}]/bestaudio\"", link, AverageBitrate.ToString().Substring(1))
                }
            };
            p.OutputDataReceived += async (_, e) =>
            {
                if (string.IsNullOrEmpty(e.Data) || e.Data.ToLower().StartsWith("error")) return;
                await callback.Invoke(e.Data, model);
            };
            p.Start();
            p.BeginOutputReadLine();
        }

        public static void GetMetaData(string link, Action<MetaData> callback)
        {
            using var p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    FileName = ExecutablePath,
                    Arguments = $"--dump-json {link}"
                }
            };
            p.OutputDataReceived += async (_, e) =>
            {
                if (string.IsNullOrEmpty(e.Data)) return;
                callback.Invoke(JsonConvert.DeserializeObject<MetaData>(e.Data));
            };
            p.Start();
            p.BeginOutputReadLine();
        }

        public static void Download(string link, string outputFolder, AudioFormat format, IProgress<string> progress)
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
            p.BeginErrorReadLine();
        }
    }
}