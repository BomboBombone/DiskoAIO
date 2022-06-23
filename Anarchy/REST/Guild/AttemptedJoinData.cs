using Newtonsoft.Json;

namespace Discord
{
    public class AttemptedJoinData
    {
        [JsonProperty("captcha_key")]
        public string[] captcha_info;
        [JsonProperty("captcha_rqdata")]
        public string captcha_rqdata;
        [JsonProperty("captcha_rqtoken")]
        public string captcha_rqtoken;
        [JsonProperty("captcha_service")]
        public string captcha_service;
        [JsonProperty("captcha_sitekey")]
        public string captcha_sitekey;
    }
}