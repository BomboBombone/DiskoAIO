using Newtonsoft.Json;
using System.Collections.Generic;

namespace Discord
{
    public class GuildVerificationFormField
    {
        [JsonProperty("field_type")]
        public string FieldType { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }
        
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("automations")]
        public string Automations { get; set; }

        [JsonProperty("values")]
        public IReadOnlyList<string> Values { get; set; }
        [JsonProperty("required")]
        public bool Required { get; set; } = true;
        [JsonProperty("response")]
        public object Response { get; set; }
    }
    public class GuildVerificationFormFieldMinimal
    {
        [JsonProperty("field_type")]
        public string FieldType { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("values")]
        public IReadOnlyList<string> Values { get; set; }
        [JsonProperty("required")]
        public bool Required { get; set; } = true;
        [JsonProperty("response")]
        public object Response { get; set; }
    }
}
