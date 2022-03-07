using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiskoAIO.Twitter
{
    public class Tweet
    {
        [JsonProperty("id")]
        public string id { get; private set; }
        [JsonProperty("text")]
        public string text { get; private set; }
        public Tweet(string _id, string _text)
        {
            id = _id;
            text = _text;
        }
    }
}
