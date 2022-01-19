using Anticaptcha_example.ApiResponse;
using Newtonsoft.Json.Linq;

namespace Anticaptcha_example
{
    public interface IAnticaptchaTaskProtocol
    {
        JObject GetPostData();
        TaskResultResponse.SolutionData GetTaskSolution();
    }
}