using System;
using Newtonsoft.Json;

namespace Nekres.Music_Mixer.Core.Player.API.Models
{
    internal class MetaData
    {
        /// <summary>
        /// Video identifier
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Video title
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// Video URL
        /// </summary>
        [JsonProperty("webpage_url")]
        public string Url { get; set; }

        /// <summary>
        /// Full name of the video uploader
        /// </summary>
        [JsonProperty("uploader")]
        public string Uploader { get; set; }

        private string _artist;
        /// <summary>
        /// Artist(s) of the track. Returns <see cref="Uploader"/> if none exist.
        /// </summary>
        [JsonProperty("artist")]
        public string Artist
        {
            get => string.IsNullOrEmpty(_artist) ? this.Uploader : _artist;
            set => _artist = value;
        }

        /// <summary>
        /// Length of the video
        /// </summary>
        [JsonProperty("duration"), JsonConverter(typeof(TimeSpanFromSecondsConverter))]
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Average audio bitrate in KBit/s
        /// </summary>
        [JsonProperty("abr")]
        public string AudioBitRate { get; set; }

        /// <summary>
        /// Name of the audio codec in use
        /// </summary>
        [JsonProperty("acodec")]
        public string AudioCodec { get; set; }
    }
}
