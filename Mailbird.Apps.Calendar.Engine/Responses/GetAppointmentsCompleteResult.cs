using System.Collections.Generic;
using Mailbird.Apps.Calendar.Engine.Metadata;

namespace Mailbird.Apps.Calendar.Engine.Responses
{
    public class GetAppointmentsCompleteResult : BaseResult
    {
        public List<Appointment> Appointments { get; set; }

        public GetAppointmentsCompleteResult()
            : base()
        {
            
        }
    }
}
