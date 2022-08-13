using Gw2Sharp.ChatLinks;
using Newtonsoft.Json;
using System;
using System.IO;

namespace Nekres.Special_Forces.Core.Services.Persistance
{
    internal class RawTemplate
    {
        [JsonProperty("title")] 
        public string Title { get; set; }

        [JsonProperty("patch")] 
        public DateTime Patch { get; set; }

        [JsonProperty("template")] 
        public string Template { get; set; }

        [JsonProperty("rotation")] 
        public Rotation Rotation { get; set; }

        [JsonProperty("utilitykeys")] 
        public int[] Utilitykeys { get; set; }

        public BuildChatLink GetBuildChatLink()
        {
            if (this.Template == null) return null;
            try {
                return (BuildChatLink) Gw2ChatLink.Parse(Template);
            } catch (FormatException e) {
                SpecialForcesModule.Logger.Error(e.Message + e.StackTrace);
            }
            return null;
        }

        public void Save()
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            var title = Title;
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                title = title.Replace(c, '-');
            }
            var path = Path.Combine(SpecialForcesModule.Instance.DirectoriesManager.GetFullDirectoryPath("special_forces"), title);
            System.IO.File.WriteAllText(path + ".json", json);
        }
    }

    internal class Rotation
    {
        [JsonProperty("opener")] 
        public string Opener { get; set; }

        [JsonProperty("loop")] 
        public string Loop { get; set; }
    }
}