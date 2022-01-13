using Newtonsoft.Json;

namespace Discord
{
    public class MessageComponent
    {
        [JsonProperty("type")]
        public MessageComponentType Type { get; protected set; }
        [JsonProperty("components")]
        public string CustomId { get; protected set; }
    }
}
