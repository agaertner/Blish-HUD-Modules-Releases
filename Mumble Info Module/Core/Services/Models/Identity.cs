using Newtonsoft.Json;

namespace Nekres.Mumble_Info.Core.Services.Models
{
    internal class Identity
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("profession")]
        public uint Profession { get; set; }

        [JsonProperty("spec")]
        public uint Spec { get; set; }

        [JsonProperty("race")]
        public uint Race { get; set; }

        [JsonProperty("map_id")]
        public uint MapId { get; set; }

        [JsonProperty("world_id")]
        public uint WorldId { get; set; }

        [JsonProperty("team_color_id")]
        public uint TeamColorId { get; set; }

        [JsonProperty("commander")]
        public bool Commander { get; set; }

        [JsonProperty("fov")]
        public float Fov { get; set; }

        [JsonProperty("uisz")]
        public uint Uisz { get; set; }

        public Identity()
        {
            this.Name = "Dummy";
            this.Profession = 4;
            this.Spec = 55;
            this.Race = 4;
            this.MapId = 50;
            this.WorldId = 268435505;
            this.TeamColorId = 0;
            this.Commander = false;
            this.Fov = 0.873f;
            this.Uisz = 1;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
