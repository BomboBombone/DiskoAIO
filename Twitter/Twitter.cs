
using Discord;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DiskoAIO.Twitter
{
    class Twitter
    {
        public HttpClient client { get; set; }
        public DiscordClient discordClient { get; set; }
        public HttpClientHandler clientHandler { get; set; }
        public string flow_token = null;

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
            //InitializeHttpClient();
        }
        public Twitter(string username, string password, string phone = null)
        {
            InitializeHttpClient();
            Login(username, password, phone);
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
        public void Follow(string username)
        {
            var userId = getUserID(username);
            client.SendAsync(new HttpRequestMessage()
            {
                Method = new System.Net.Http.HttpMethod("POST"),
                RequestUri = new Uri("https://twitter.com/i/api/1.1/friendships/create.json"),
                Content = new System.Net.Http.StringContent("user_id=956823686", Encoding.UTF8, "x-www-form-urlencoded")
            }).GetAwaiter().GetResult();
        }
        public void PostTweet(string input)
        {
            var payload = "{\"variables\":\"{\\\"tweet_text\\\":\\\"" + input + "\\\",\\\"media\\\":{\\\"media_entities\\\":[],\\\"possibly_sensitive\\\":false},\\\"withDownvotePerspective\\\":false,\\\"withReactionsMetadata\\\":false,\\\"withReactionsPerspective\\\":false,\\\"withSuperFollowsTweetFields\\\":true,\\\"withSuperFollowsUserFields\\\":true,\\\"semantic_annotation_ids\\\":[],\\\"dark_request\\\":false,\\\"__fs_dont_mention_me_view_api_enabled\\\":false,\\\"__fs_interactive_text_enabled\\\":false,\\\"__fs_responsive_web_uc_gql_enabled\\\":false}\",\"queryId\":\"E7Zjy2bwXIths_dsqOVvxQ\"}";
            client.SendAsync(new HttpRequestMessage()
            {
                Method = new System.Net.Http.HttpMethod("POST"),
                RequestUri = new Uri("https://twitter.com/i/api/graphql/E7Zjy2bwXIths_dsqOVvxQ/CreateTweet"),
                Content = new System.Net.Http.StringContent(payload, Encoding.UTF8, "application/json")
            }).GetAwaiter().GetResult();
        }
        public void Login(string username, string password, string phone = null)
        {
            //Start flow
            var payload = "{\"flow_token\":\"" + flow_token + "\",\"subtask_inputs\":[{\"subtask_id\":\"LoginJsInstrumentationSubtask\",\"js_instrumentation\":{\"response\":\"{\\\"rf\\\":{\\\"afb81c033fd6f364a802ede2f04b3badebed7eab30c6a3dc24b53f5bc0c3fb40\\\":39,\\\"a9bcb53c0a384833dd391b82e08f20e8d585a7653c4b134839040c3b73ca77b5\\\":-152,\\\"ade0004470720de9e449ba7a7f027f2e2acde541424efa9e318fb9799b3f67eb\\\":-78,\\\"a1eec63129cf622d1181176fe614fa9e8f255b603a3c2794b39d3ffb0df1a0b9\\\":4},\\\"s\\\":\\\"LPDkb_E8SSN66-KmBshp-jvMRB16IwLVChOWGpknRG-uQQLiQFrq70VOK5DdKauAv2Mn36XlCFg3ekrst6xPMyWzD6gqpf9hlk1USXvimzH1VDi7Uo1xg4v01CkMhMGxArq7DDdMoyGSb_pHNBxR60sXgdT7hhjNU0ZDaZdTZ1FjYvxa_VME5GSVYGEWpcyHIWl0_Qq3G1gn7XLx7M447qQAYk59jbjpLP-TUqRq9oDvheDxBvcKM4t6AWPsijzoNHd5A3VjayDta1w2mE3eUK21MdLz17ZZ_O3kmaffWfZyM0nGW-w-8SR34gXEI--6S3pWqAfpVg8ZW2YlWknp3QAAAX9VNQUk\\\"}\",\"link\":\"next_link\"}}]}";
            var res = client.SendAsync(new HttpRequestMessage()
            {
                Method = new System.Net.Http.HttpMethod("POST"),
                RequestUri = new Uri("https://twitter.com/i/api/1.1/onboarding/task.json"),
                Content = new System.Net.Http.StringContent(payload, Encoding.UTF8, "application/json")
            }).GetAwaiter().GetResult();
            var jt = JToken.Parse(res.Content.ReadAsStringAsync().Result);
            var json = JObject.Parse(jt.ToString());
            flow_token = json.Value<string>("flow_token");
            //Start login with username
            payload = "{\"flow_token\":\"" + flow_token + "\",\"subtask_inputs\":[{\"subtask_id\":\"LoginEnterUserIdentifierSSOSubtask\",\"settings_list\":{\"setting_responses\":[{\"key\":\"user_identifier\",\"response_data\":{\"text_data\":{\"result\":\"" + username + "\"}}}],\"link\":\"next_link\"}}]}";
            res = client.SendAsync(new HttpRequestMessage()
            {
                Method = new System.Net.Http.HttpMethod("POST"),
                RequestUri = new Uri("https://twitter.com/i/api/1.1/onboarding/task.json"),
                Content = new System.Net.Http.StringContent(payload, Encoding.UTF8, "application/json")
            }).GetAwaiter().GetResult();
            jt = JToken.Parse(res.Content.ReadAsStringAsync().Result);
            json = JObject.Parse(jt.ToString());
            flow_token = json.Value<string>("flow_token");
            //Enter password
            payload = "{\"flow_token\":\"" + flow_token + "\",\"subtask_inputs\":[{\"subtask_id\":\"LoginEnterPassword\",\"enter_password\":{\"password\":\"" + password + "\",\"link\":\"next_link\"}}]}";
            res = client.SendAsync(new HttpRequestMessage()
            {
                Method = new System.Net.Http.HttpMethod("POST"),
                RequestUri = new Uri("https://twitter.com/i/api/1.1/onboarding/task.json"),
                Content = new System.Net.Http.StringContent(payload, Encoding.UTF8, "application/json")
            }).GetAwaiter().GetResult();
            jt = JToken.Parse(res.Content.ReadAsStringAsync().Result);
            json = JObject.Parse(jt.ToString());
            flow_token = json.Value<string>("flow_token");
            //Login final confirmation sets access token and csrf cookie
            payload = "{\"flow_token\":\"" + flow_token + "\",\"subtask_inputs\":[{\"subtask_id\":\"AccountDuplicationCheck\",\"check_logged_in_account\":{\"link\":\"AccountDuplicationCheck_false\"}}]}";
            res = client.SendAsync(new HttpRequestMessage()
            {
                Method = new System.Net.Http.HttpMethod("POST"),
                RequestUri = new Uri("https://twitter.com/i/api/1.1/onboarding/task.json"),
                Content = new System.Net.Http.StringContent(payload, Encoding.UTF8, "application/json")
            }).GetAwaiter().GetResult();
            jt = JToken.Parse(res.Content.ReadAsStringAsync().Result);
            json = JObject.Parse(jt.ToString());
            flow_token = json.Value<string>("flow_token");
            if(flow_token.EndsWith("7"))
            {
                if(phone != null)
                {
                    payload = "{\"flow_token\":\"" + flow_token + "\",\"subtask_inputs\":[{\"subtask_id\":\"LoginAcid\",\"enter_text\":{\"text\":\"" + phone + "\",\"link\":\"next_link\"}}]}";
                    res = client.SendAsync(new HttpRequestMessage()
                    {
                        Method = new System.Net.Http.HttpMethod("POST"),
                        RequestUri = new Uri("https://twitter.com/i/api/1.1/onboarding/task.json"),
                        Content = new System.Net.Http.StringContent(payload, Encoding.UTF8, "application/json")
                    }).GetAwaiter().GetResult();
                }
                else
                {
                    throw new Exception("Account needs phone number to complete login");
                }
            }
            //Set csrf header for future requests
            var cookies = res.Headers.GetValues("set-cookie");
            foreach(var cookie in cookies)
            {
                if (cookie.StartsWith("ct0"))
                {
                    client.DefaultRequestHeaders.Add("x-csrf-token", cookie.Split(';').First().Split('=').Last());
                    break;
                }
            }
        }
        public void InitializeHttpClient()
        {
            clientHandler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
                UseCookies = true,
                CookieContainer = new System.Net.CookieContainer()
            };
            client = new System.Net.Http.HttpClient(clientHandler);
            client.DefaultRequestHeaders.Add("Authorization", "Bearer AAAAAAAAAAAAAAAAAAAAANRILgAAAAAAnNwIzUejRCOuH5E6I8xnZz4puTs%3D1Zv7ttfk8LF81IUq16cHjhLTvJu4FA33AGWWjCpTnA");

            var res = client.SendAsync(new HttpRequestMessage()
            {
                Method = new System.Net.Http.HttpMethod("POST"),
                RequestUri = new Uri("https://api.twitter.com/1.1/guest/activate.json")
            }).GetAwaiter().GetResult();
            var jt = JToken.Parse(res.Content.ReadAsStringAsync().Result);
            var json = JObject.Parse(jt.ToString());
            client.DefaultRequestHeaders.Add("x-guest-token", json.Value<string>("guest_token"));

            var payload = "{\"input_flow_data\":{\"flow_context\":{\"debug_overrides\":{},\"start_location\":{\"location\":\"splash_screen\"}}},\"subtask_versions\":{\"contacts_live_sync_permission_prompt\":0,\"email_verification\":1,\"topics_selector\":1,\"wait_spinner\":1,\"cta\":4}}";
            res = client.SendAsync(new HttpRequestMessage()
            {
                Method = new System.Net.Http.HttpMethod("POST"),
                RequestUri = new Uri("https://api.twitter.com/1.1/guest/activate.json"),
                Content = new System.Net.Http.StringContent(payload, Encoding.UTF8, "application/json")
            }).GetAwaiter().GetResult();
            jt = JToken.Parse(res.Content.ReadAsStringAsync().Result);
            json = JObject.Parse(jt.ToString());
            flow_token = json.Value<string>("flow_token");
        }

        public CookieContainer Cookies
        {
            get { return clientHandler.CookieContainer; }
            set { clientHandler.CookieContainer = value; }
        }
    }
}
