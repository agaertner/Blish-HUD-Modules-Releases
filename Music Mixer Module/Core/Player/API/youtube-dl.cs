using Blish_HUD.Content;
using Microsoft.Xna.Framework.Graphics;
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

        public string ExecutablePath => Path.Combine(MusicMixerModule.ModuleInstance.ModuleDirectory, "bin/youtube-dl.exe");

        private readonly Regex _youtubeVideoId = new (@"youtu(?:\.be|be\.com)/(?:.*v(?:/|=)|(?:.*/)?)(?<id>[a-zA-Z0-9-_]+)", RegexOptions.Compiled);
        private readonly Regex _progressReport = new (@"^\[download\].*?(?<percentage>(.*?))% of (?<size>(.*?))MiB at (?<speed>(.*?)) ETA (?<eta>(.*?))$", RegexOptions.Compiled); //[download]   2.7% of 4.62MiB at 200.00KiB/s ETA 00:23
        private readonly Regex _version = new (@"Updating to version (?<version>(.*?)) \.\.\.", RegexOptions.Compiled); //Updating to version 2015.01.16 ...

        private static youtube_dl _instance;
        private bool _isLoaded;
        private AudioBitrate AverageBitrate => MusicMixerModule.ModuleInstance.AverageBitrateSetting.Value;

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

        public async void GetThumbnail(string link, AsyncTexture2D texture)
        {
            var youTubeId = GetYouTubeIdFromLink(link);
            var thumbnailUrl = $"https://img.youtube.com/vi/{youTubeId}/mqdefault.jpg";
            var textureDataResponse = await Blish_HUD.GameService.Gw2WebApi.AnonymousConnection.Client.Render.DownloadToByteArrayAsync(thumbnailUrl);
            using var textureStream = new MemoryStream(textureDataResponse);
            var loadedTexture = Texture2D.FromStream(Blish_HUD.GameService.Graphics.GraphicsDevice, textureStream);
            texture.SwapTexture(loadedTexture);
        }

        public string GetYouTubeIdFromLink(string youTubeLink)
        {
            var youtubeMatch = _youtubeVideoId.Match(youTubeLink);
            if (!youtubeMatch.Success) return string.Empty;
            return youtubeMatch.Groups["id"].Value;
        }

        public async Task<Uri> GetAudioOnlyUrl(string youTubeLink)
        {
            await Load();
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
            var url = await p.StandardOutput.ReadToEndAsync();
            p.WaitForExit();
            if (string.IsNullOrEmpty(url) || url.ToLower().StartsWith("error")) return null;
            return new Uri(url);
        }

        public async Task Download(string link, string outputFolder, AudioFormat format, IProgress<string> progress)
        {
            await Load();

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