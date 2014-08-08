using Mailbird.Apps.Calendar.Engine.Metadata;

namespace Mailbird.Apps.Calendar.Engine
{
    public class BaseResult
    {
        public BaseResult()
        {
            Success = true;
        }

        public bool Success { get; set; }

        public string Error { get; set; }
    }
}
