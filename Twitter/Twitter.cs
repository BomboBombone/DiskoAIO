
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DiskoAIO.Twitter
{
    class Twitter
    {
        public const string bearer_token = "AAAAAAAAAAAAAAAAAAAAAKXTWwEAAAAAyYsnlLLgG0x1RNVGThSC%2BwJnja4%3DMKHfxu9EFMRglhuvHv1ECs4D7IROpzKntpzz2EXYNlpIdOKJjS";
        public static string getTweetsEndpoint = "https://api.twitter.com/2/users/:userid/tweets?max_results=5";
        public static string getUserEndpoint = "https://api.twitter.com/2/users/by?usernames=:username&tweet.fields=author_id";
        public string userID;
        public static string getUserID(string username)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {bearer_token}");
            var response_context = client.SendAsync(new HttpRequestMessage()
            {
                Method = new System.Net.Http.HttpMethod("GET"),
                RequestUri = new Uri(getUserEndpoint.Replace(":username", username))
            }).GetAwaiter().GetResult();
            var jt = JToken.Parse(response_context.Content.ReadAsStringAsync().Result);
            var json = JObject.Parse(jt.ToString());
            return json["data"][0].Value<string>("id");
        }
        public Twitter(string username)
        {
            userID = getUserID(username);
        }
        public Tweet getLatestTweet()
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {bearer_token}");
            var response_context = client.SendAsync(new HttpRequestMessage()
            {
                Method = new System.Net.Http.HttpMethod("GET"),
                RequestUri = new Uri(getTweetsEndpoint.Replace(":userid", userID))
            }).GetAwaiter().GetResult();
            var jt = JToken.Parse(response_context.Content.ReadAsStringAsync().Result);
            var json = JObject.Parse(jt.ToString());
            return new Tweet(json["data"][0].Value<string>("id"), json["data"][0].Value<string>("text"));
        }
    }
}
