using Mailbird.Apps.Calendar.Engine.Metadata;

namespace Mailbird.Apps.Calendar.Engine.Responses
{
    public class AppointmentCompleteResult : BaseResult
    {
        public AppointmentCompleteResult(Appointment appointment)
            : base()
        {
            Appointment = appointment;
        }
        public Appointment Appointment { get; set; }
    }
}
