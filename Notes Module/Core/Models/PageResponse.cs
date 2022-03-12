using Newtonsoft.Json;

namespace Nekres.Notes.Core.Models
{
    internal sealed class PageResponse
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("index")]
        public int Index { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }
    }
}
