namespace Mailbird.Apps.Calendar.Engine.Responses
{
    public class AppointmentDeleteCompleteResult : BaseResult
    {
        public object AppointmentId { get; set; }

        public AppointmentDeleteCompleteResult(object appointmentId)
            : base()
        {
            AppointmentId = appointmentId;
        }

    }
}
