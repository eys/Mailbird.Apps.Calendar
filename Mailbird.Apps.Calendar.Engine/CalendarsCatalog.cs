using System;
using System.Collections.Generic;
using System.Linq;
using Mailbird.Apps.Calendar.Engine.Interfaces;
using Mailbird.Apps.Calendar.Engine.Metadata;
using Mailbird.Apps.Calendar.Engine.Responses;

namespace Mailbird.Apps.Calendar.Engine
{
    /// <summary>
    /// CalendarCatalog create all instamces of ICalendarProvider and calendars workout
    /// go throught this clas
    /// </summary>
    public class CalendarsCatalog
    {
        private readonly Dictionary<string, ICalendarProvider> _calendarProviders;

        public Metadata.Calendar DefaultCalendar { get; set; }

        public CalendarsCatalog()
        {
            _calendarProviders = AppDomainAssemblyTypeScanner.TypesOf<ICalendarProvider>().Select(x => (ICalendarProvider)Activator.CreateInstance(x)).ToDictionary(x => x.Name, x => x);
        }

        public IEnumerable<ICalendarProvider> GetProviders
        {
            get
            {
                return _calendarProviders.Values;
            }
        }

        public void GetCalendarAppointments()
        {
            foreach (ICalendarProvider calendarProvider in _calendarProviders.Values)
            {
                calendarProvider.GetAppointments();
            }
        }

        public void InsertAppointment(Appointment appointment)
        {
            var provider = _calendarProviders[appointment.Calendar.Provider];

            provider.InsertAppointment(appointment);
        }

        public bool UpdateAppointment(Appointment appointment)
        {
            ICalendarProvider provider = _calendarProviders[appointment.Calendar.Provider];
            return provider.UpdateAppointment(appointment);
        }

        public bool RemoveAppointment(object appointmentId, Metadata.Calendar calendar)
        {
            var provider = _calendarProviders[calendar.Provider];
            return provider.RemoveAppointment(appointmentId, calendar.CalendarId);
        }

    }
}