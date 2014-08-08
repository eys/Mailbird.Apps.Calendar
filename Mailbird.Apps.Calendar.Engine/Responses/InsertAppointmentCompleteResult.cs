using Mailbird.Apps.Calendar.Engine.Metadata;

namespace Mailbird.Apps.Calendar.Engine.Responses
{
    public class InsertAppointmentCompleteResult : BaseResult
    {
        public InsertAppointmentCompleteResult(Appointment appointment)
            : base()
        {
            Appointment = appointment;
        }
        public Appointment Appointment { get; set; }
    }
}
