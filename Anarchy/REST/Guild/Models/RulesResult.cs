using Newtonsoft.Json;

namespace Discord
{
    class RulesResult
    {
        [JsonProperty("application_status")]
        public string application_status { get; set; }
    }
}
