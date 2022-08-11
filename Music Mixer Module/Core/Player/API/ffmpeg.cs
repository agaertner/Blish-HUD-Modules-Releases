using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Nekres.Music_Mixer.Core.Player.API
{
    internal static class ffmpeg
    {
        public static string ExecutablePath => Path.Combine(MusicMixer.Instance.ModuleDirectory, "bin/ffmpeg.exe");

        /// <summary>
        /// Converts the file
        /// </summary>
        /// <param name="fileName">The path to the file which should become converted</param>
        /// <param name="newFileName">The name of the new file WITHOUT extension</param>
        public static Task ConvertFile(string fileName, string newFileName)
        {
            return ConvertFile(fileName, newFileName, AudioBitrate.B256, AudioFormat.Best);
        }

        public static async Task<Stream> ConvertImage(string sourceFile, string outFile)
        {
            if (File.Exists(outFile)) return new FileStream(outFile, FileMode.Open, FileAccess.Read);

            var p = new Process
            {
                StartInfo =
                {
                    CreateNoWindow = true,
                    FileName = ExecutablePath,
                    Arguments = $"-i {sourceFile} {outFile}",
                    UseShellExecute = false,
                    WorkingDirectory = Path.GetDirectoryName(ExecutablePath)
                }
            };
            p.Start();
            p.WaitForExit();
            if (p.ExitCode != 0) return null;
            var path = Path.Combine(Path.GetDirectoryName(ExecutablePath), outFile);
            return await Task.Delay(200).ContinueWith(_ => new FileStream(path, FileMode.Open, FileAccess.Read));
        }

        /// <summary>
        /// Converts the file
        /// </summary>
        /// <param name="fileName">The path to the file which should become converted</param>
        /// <param name="newFileName">The name of the new file WITHOUT extension</param>
        /// <param name="bitrate">The audio bitrate</param>
        /// <param name="format"></param>
        public static async Task ConvertFile(string fileName, string newFileName, AudioBitrate bitrate, AudioFormat format)
        {
            var fileToConvert = new FileInfo(fileName);

            var p = new Process
            {
                StartInfo =
                {
                    CreateNoWindow = true,
                    FileName = ExecutablePath,
                    Arguments = GetParameter(fileName, newFileName, bitrate, format),
                    UseShellExecute = false
                }
            };

            p.Start();
            await Task.Run(() => p.WaitForExit());
            var newFile = new FileInfo(newFileName);

            if (!newFile.Exists || newFile.Length == 0)
            {
                if (newFile.Exists) newFile.Delete();
                fileToConvert.MoveTo(newFileName); //If the convert failed, we just use the "old" file
            }

            fileToConvert.Delete();
        }

        private static string GetParameter(string inputFile, string outputFile, AudioBitrate bitrate, AudioFormat format)
        {
            return
                $"-i \"{inputFile}\" -c:a {GetAudioLibraryFromFormat(format)} -vn -b:a {bitrate.ToString().Remove(0, 1)}k \"{outputFile}\"";
        }

        public static string GetAudioLibraryFromFormat(AudioFormat format)
        {
            switch (format)
            {
                case AudioFormat.Best:
                    return "copy";
                case AudioFormat.MP3:
                    return "libmp3lame";
                case AudioFormat.AAC:
                    return "libfdk_aac";
                case AudioFormat.WMA:
                    return "wmav2";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


    }

    public enum AudioFormat
    {
        Best,
        MP3,
        AAC,
        WMA,
        FLAC
    }

    public enum AudioBitrate
    {
        [Description("64 kbit/s")]
        B64,
        [Description("96 kbit/s")]
        B96,
        [Description("128 kbit/s")]
        B128,
        [Description("160 kbit/s")]
        B160,
        [Description("192 kbit/s")]
        B192,
        [Description("256 kbit/s")]
        B256,
        [Description("320 kbit/s")]
        B320
    }
}
