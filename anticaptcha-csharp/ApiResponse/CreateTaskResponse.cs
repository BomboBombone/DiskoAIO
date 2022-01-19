using Anticaptcha_example.Helper;

namespace Anticaptcha_example.ApiResponse
{
    public class CreateTaskResponse
    {
        public CreateTaskResponse(dynamic json)
        {
            ErrorId = JsonHelper.ExtractInt(json, "errorId");

            if (ErrorId != null)
            {
                if (ErrorId.Equals(0))
                {
                    TaskId = JsonHelper.ExtractInt(json, "taskId");
                }
                else
                {
                    ErrorCode = JsonHelper.ExtractStr(json, "errorCode");
                    ErrorDescription = JsonHelper.ExtractStr(json, "errorDescription") ?? "(no error description)";
                }
            }
            else
            {
                DebugHelper.Out("Unknown error", DebugHelper.Type.Error);
            }
        }

        public int? ErrorId { get; private set; }
        public string ErrorCode { get; private set; }
        public string ErrorDescription { get; private set; }
        public int? TaskId { get; private set; }
    }
}