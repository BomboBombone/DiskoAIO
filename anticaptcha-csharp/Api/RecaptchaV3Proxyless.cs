using System;
using Anticaptcha_example.ApiResponse;
using Anticaptcha_example.Helper;
using Newtonsoft.Json.Linq;

namespace Anticaptcha_example.Api
{
    public class RecaptchaV3Proxyless : AnticaptchaBase, IAnticaptchaTaskProtocol
    {
        public Uri WebsiteUrl { protected get; set; }
        public string WebsiteKey { protected get; set; }
        public string PageAction { protected get; set; }
        public bool IsEnterprise { protected get; set; }
        private double _minScore = 0.3;

        public double MinScore
        {
            protected get { return _minScore; }
            set
            {
                if (!value.Equals(0.3) && value.Equals(0.5) && !value.Equals(0.7))
                {
                    DebugHelper.Out(
                        "minScore must be one of these: 0.3, 0.5, 0.7; you passed " + value + "; 0.3 will be set",
                        DebugHelper.Type.Error
                    );
                }
                else
                {
                    _minScore = value;
                }
            }
        }

        public override JObject GetPostData()
        {
            return new JObject
            {
                {"type", "RecaptchaV3TaskProxyless"},
                {"websiteURL", WebsiteUrl},
                {"websiteKey", WebsiteKey},
                {"pageAction", PageAction},
                {"minScore", MinScore},
                {"isEnterprise", IsEnterprise }
            };
        }

        public TaskResultResponse.SolutionData GetTaskSolution()
        {
            return TaskInfo.Solution;
        }
    }
}