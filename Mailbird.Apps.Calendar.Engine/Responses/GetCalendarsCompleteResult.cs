using System.Collections.Generic;

namespace Mailbird.Apps.Calendar.Engine.Responses
{
    public class GetCalendarsCompleteResult : BaseResult
    {
        public GetCalendarsCompleteResult()
            : base()
        {
            
        }

        public IEnumerable<Metadata.Calendar> Calendars { get; set; }
    }
}
