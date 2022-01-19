using System;
using Anticaptcha_example.ApiResponse;
using Anticaptcha_example.Helper;
using Newtonsoft.Json.Linq;

namespace Anticaptcha_example.Api
{
    internal class FunCaptcha : AnticaptchaBase, IAnticaptchaTaskProtocol
    {
        public Uri WebsiteUrl { protected get; set; }
        public string WebsitePublicKey { protected get; set; }
        public string ProxyLogin { protected get; set; }
        public string ProxyPassword { protected get; set; }
        public int? ProxyPort { protected get; set; }
        public ProxyTypeOption? ProxyType { protected get; set; }
        public string ProxyAddress { protected get; set; }
        public string UserAgent { protected get; set; }

        public override JObject GetPostData()
        {
            if (ProxyType == null || ProxyPort == null || ProxyPort < 1 || ProxyPort > 65535 ||
                string.IsNullOrEmpty(ProxyAddress))
            {
                DebugHelper.Out("Proxy data is incorrect!", DebugHelper.Type.Error);

                return null;
            }

            return new JObject
            {
                {"type", "FunCaptchaTask"},
                {"websiteURL", WebsiteUrl},
                {"websitePublicKey", WebsitePublicKey},
                {"proxyType", ProxyType.ToString().ToLower()},
                {"proxyAddress", ProxyAddress},
                {"proxyPort", ProxyPort},
                {"proxyLogin", ProxyLogin},
                {"proxyPassword", ProxyPassword},
                {"userAgent", UserAgent}
            };
        }

        public TaskResultResponse.SolutionData GetTaskSolution()
        {
            return TaskInfo.Solution;
        }
    }
}