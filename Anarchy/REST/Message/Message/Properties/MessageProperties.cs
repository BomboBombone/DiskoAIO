using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Discord
{
    public class MessageProperties
    {
        public MessageProperties()
        {
            var rnd = new Random();
            _nonce = "9391714539374" + rnd.Next(10000, 99999).ToString();
        }


        [JsonProperty("content")]
        public string Content { get; set; }


        [JsonProperty("nonce")]
#pragma warning disable IDE0052
        private readonly string _nonce;
#pragma warning restore


        [JsonProperty("tts")]
        public bool Tts { get; set; }


        [JsonProperty("message_reference")]
        public MessageReference ReplyTo { get; set; }


        [JsonProperty("embed")]
        public DiscordEmbed Embed { get; set; }


        [JsonProperty("components")]
        public List<MessageComponent> Components { get; set; }


        public bool ShouldSerializeReplyTo()
        {
            return ReplyTo != null;
        }

        public bool ShouldSerializeEmbed()
        {
            return Embed != null;
        }

        public bool ShouldSerializeComponents()
        {
            return Components != null;
        }
    }
}
