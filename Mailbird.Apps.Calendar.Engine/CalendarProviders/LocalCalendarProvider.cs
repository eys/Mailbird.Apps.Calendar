using System;
using System.Collections.Generic;
using DevExpress.Mvvm;
using Mailbird.Apps.Calendar.Engine.Interfaces;
using Mailbird.Apps.Calendar.Engine.Metadata;
using Mailbird.Apps.Calendar.Engine.Responses;
using Mailbird.Apps.Calendar.Engine.Utility;

namespace Mailbird.Apps.Calendar.Engine.CalendarProviders
{
    public class LocalCalendarProvider : ICalendarProvider
    {


        private const string LocalStoragePath = @"C:\LocalStorage2.txt";

        private readonly List<Metadata.Calendar> _calendars = new List<Metadata.Calendar>();

        private readonly List<Appointment> _calendarsEvents = new List<Appointment>();

        private readonly JsonWorker _worker;

        public string Name { get; private set; }

        public LocalCalendarProvider()
        {
            Name = "LocalCalendarsStorage";
            _worker = new JsonWorker(LocalStoragePath);
            LoadCalendars();
        }

        private void LoadCalendars()
        {
            var calendar = new Metadata.Calendar { CalendarId = Guid.NewGuid().ToString(), Provider = "LocalCalendarsStorage", Name = "LocalCalendar" };
            _calendars.Add(calendar);
            _worker.GetData<Appointment>().ForEach(f => _calendarsEvents.Add(f));
        }

        public void GetCalendars()
        {
            var getCalendarsCompleteResult = new GetCalendarsCompleteResult();
            getCalendarsCompleteResult.Calendars = _calendars;
            Messenger.Default.Send(getCalendarsCompleteResult);
        }

        public void GetAppointments()
        {
            var getAppointmentsCompleteResult = new GetAppointmentsCompleteResult();
            getAppointmentsCompleteResult.Appointments = _calendarsEvents;
            Messenger.Default.Send(getAppointmentsCompleteResult);
        }


        public void InsertAppointment(Appointment appointment)
        {
            var baseResult = new AppointmentCompleteResult(appointment);

            _calendarsEvents.Add(appointment);
            _worker.SaveData(appointment);

            Messenger.Default.Send(baseResult);
        }

        public bool UpdateAppointment(Appointment appointment)
        {
            return true;
        }

        public bool RemoveAppointment(object appointmentId, object calendarId)
        {
            return true;
        }

    }
}