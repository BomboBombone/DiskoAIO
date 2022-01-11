using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;

namespace Discord
{
    public class GuildVerificationForm
    {
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("form_fields")]
        public List<GuildVerificationFormField> Fields { get; set; }
    }
    public class GuildVerificationFormMinimal
    {
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("form_fields")]
        public List<GuildVerificationFormFieldMinimal> Fields { get; set; }
    }
}
