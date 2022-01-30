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
        public static string endpoint = "http://164.68.112.164:9000/chat";
        private ulong chat_id = 0;
        //public HttpRequest request { get; set; } = null;
        public BobbyAPI(ulong server_id)
        {

            CreateChat(server_id);
        }
        public void Train(string statement, string prev_statement)
        {
            try
            {
                var request = new HttpRequest()
                {

                };
                request.AddHeader("X-Forwarded-For", App.localIP);
                string payload = '{' + $"\"chat_id\":{chat_id},\"text\":\"{statement}\",\"prev_text\":\"{prev_statement}\"" + '}';
                request.Post(endpoint, payload, "application/json");
            }
            catch (Exception ex)
            {
                Debug.Log("Error when training AI: " + ex.StackTrace);
                throw new Exception("404, conversation not found");
            }
        }
        public string GetResponse(string statement)
        {
            try
            {
                var request = new HttpRequest()
                {

                };
                request.AddHeader("X-Forwarded-For", App.localIP);
                string payload = '{' + $"\"chat_id\":{chat_id},\"text\":\"{statement}\"" + '}';
                var res = request.Post(endpoint, payload, "application/json");
                return res.ToString().ToLower();
            }
            catch (Exception ex)
            {
                Debug.Log("Error when getting response from AI: " + ex.StackTrace);
                return "Ayoo";
            }
        }
        public void CreateChat(ulong guild_id)
        {
            try
            {
                var request = new HttpRequest()
                {

                };
                request.AddHeader("X-Forwarded-For", App.localIP);
                string payload = '{' + $"\"server_id\":{guild_id}" + '}';
                var res = request.Post(endpoint, payload, "application/json");
                chat_id = guild_id;
            }
            catch (Exception ex)
            {
                Debug.Log("Error when getting response from diskoaio.com: " + ex.Message);
            }
        }
    }
}
