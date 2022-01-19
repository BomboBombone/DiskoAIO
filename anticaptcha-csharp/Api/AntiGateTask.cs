using System;
using Anticaptcha_example.ApiResponse;
using Newtonsoft.Json.Linq;

namespace Anticaptcha_example.Api
{
    public class AntiGateTask : AnticaptchaBase, IAnticaptchaTaskProtocol
    {
        public Uri WebsiteUrl { protected get; set; }
        public string TemplateName { protected get; set; }
        public JObject Variables { protected get; set; }
        public string ProxyAddress { protected get; set; }
        public int ProxyPort { protected get; set; }
        public string ProxyLogin { protected get; set; }
        public string ProxyPassword { protected get; set; }

        public override JObject GetPostData()
        {
            var postData = new JObject
            {
                {"type", "AntiGateTask"},
                {"websiteURL", WebsiteUrl.ToString()},
                {"templateName", TemplateName},
            };

            if (ProxyAddress != null && ProxyPort != 0)
            {
                postData["proxyAddress"] = ProxyAddress;
                postData["proxyPort"] = ProxyPort;
            }

            if (ProxyLogin != null && ProxyPassword != null)
            {
                postData["proxyLogin"] = ProxyLogin;
                postData["proxyPassword"] = ProxyPassword;
            }

            if (Variables != null)
            {
                postData["variables"] = Variables;
            }

            return postData;
        }

        public TaskResultResponse.SolutionData GetTaskSolution()
        {
            return TaskInfo.Solution; ;
        }
    }
}