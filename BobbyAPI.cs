using Leaf.xNet;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiskoAIO
{
    public class BobbyAPI
    {
        public static string endpoint = "https://diskoaio.com/bobby/v4";
        public ulong chat_id = 0;
        public HttpRequest request { get; set; } = null;
        public BobbyAPI(ulong server_id)
        {
            CreateChat(server_id);
            request = new HttpRequest()
            {

            };
            request.AddHeader("X-Forwarded-For", App.localIP);
        }
        public void Train(string statement, string prev_statement)
        {
            try
            {
                string payload = '{' + $"\"chat_id\":{chat_id},\"text\":\"{statement}\",\"prev_text\":\"{prev_statement}\"" + '}';
                request.Post(endpoint, payload, "application/json");
            }
            catch (Exception ex)
            {
                Debug.Log("Error when training AI: " + ex.Message);
                throw new Exception("404, conversation not found");
            }
        }
        public string GetResponse(string statement)
        {
            try
            {
                string payload = '{' + $"\"chat_id\":{chat_id},\"text\":\"{statement}\"" + '}';
                var res = request.Post(endpoint, payload, "application/json");
                return res.ToString().ToLower();
            }
            catch (Exception ex)
            {
                Debug.Log("Error when getting response from AI: " + ex.Message);
                return "Ayoo";
            }
        }
        public void CreateChat(ulong guild_id)
        {
            try
            {
                string payload = '{' + $"\"server_id\":{guild_id}" + '}';
                var res = request.Post(endpoint, payload, "application/json");
                var json = JObject.Parse(res.ToString());
                chat_id = json.Value<ulong>("chat_id");
            }
            catch (Exception ex)
            {
                Debug.Log("Error when getting response from diskoaio.com: " + ex.Message);
            }
        }
    }
}
