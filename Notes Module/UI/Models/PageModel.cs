using Newtonsoft.Json;
namespace Nekres.Notes.UI.Models
{
    public sealed class PageModel
    {
        public PageModel() {}

        public PageModel((string, string) page)
        {
            Title = page.Item1;
            Content = page.Item2;
        }
        [JsonProperty("title")] public string Title { get; set; }
        [JsonProperty("content")] public string Content { get; set; }
    }
}
