using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Discord
{
    public class ContextProperties
    {
        [JsonProperty("location")]
        public string location { get; set; } = "Join Guild";

        [JsonProperty("location_guild_id")]
        public string GuilldId { get; set; }

        [JsonProperty("location_channel_id")]
        public string ChannelId { get; set; }

        [JsonProperty("location_channel_type")]
        public string ChannelType { get; set; }

        public static ContextProperties FromBase64(string base64)
        {
            return JsonConvert.DeserializeObject<ContextProperties>(Encoding.UTF8.GetString(Convert.FromBase64String(base64)));
        }
        
        public string ToBase64()
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this)));
        }
        public static async Task<string> GetContextProperties(string inv_code)
        {
            string request_url = "https://discord.com/api/v9/invites/" + inv_code + "?inputValue=" + inv_code + "&with_counts=true&with_expiration=true";
            HttpClient client = new HttpClient();
            var response_context = await client.SendAsync(new HttpRequestMessage()
            {
                Method = new HttpMethod("GET"),
                RequestUri = new Uri(request_url)
            });
            var resp_context = new DiscordHttpResponse((int)response_context.StatusCode, response_context.Content.ReadAsStringAsync().Result);
            var json = JObject.Parse(resp_context.Body.ToString());
            var context = new ContextProperties();
            context.ChannelId = json["channel"].Value<string>("id");
            context.ChannelType = json["channel"].Value<string>("type");
            context.GuilldId = json["guild"].Value<string>("id");
            string return_value = JsonConvert.SerializeObject(context);
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(return_value));
        }
    }
}