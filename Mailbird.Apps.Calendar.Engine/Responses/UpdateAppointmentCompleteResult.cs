using Mailbird.Apps.Calendar.Engine.Metadata;

namespace Mailbird.Apps.Calendar.Engine.Responses
{
    public class UpdateAppointmentCompleteResult : BaseResult
    {
        public UpdateAppointmentCompleteResult(Appointment appointment)
            : base()
        {
            Appointment = appointment;
        }
        public Appointment Appointment { get; set; }
    }
}
