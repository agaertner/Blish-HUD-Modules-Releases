using Nekres.Notes.Core.Models;
using Newtonsoft.Json;
namespace Nekres.Notes.UI.Models
{
    internal sealed class PageModel
    {
        [JsonProperty("title")] public string Title { get; set; }
        [JsonProperty("content")] public string Content { get; set; }

        public PageModel() {}

        public PageModel((string, string) page)
        {
            Title = page.Item1;
            Content = page.Item2;
        }

        public static PageModel FromResponse(PageResponse page)
        {
            return new PageModel
            {
                Title = page.Title, 
                Content = page.Content
            };
        }
    }
}
