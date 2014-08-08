using System.Collections.Generic;
using Mailbird.Apps.Calendar.Engine.CalendarProviders;
using Mailbird.Apps.Calendar.Engine.Metadata;

namespace Mailbird.Apps.Calendar.Engine.Interfaces
{
    /// <summary>
    /// Implemenration of this interface provide all base functionality to work with calendar and appointments
    /// </summary>
    public interface ICalendarProvider
    {
        string Name { get; }
        

        /// <summary>
        /// return all calendars of the provider
        /// </summary>
        /// <returns></returns>
        void GetCalendars();

        /// <summary>
        /// return all apppointments of the provider
        /// </summary>
        /// <returns></returns>
        void GetAppointments();


        /// <summary>
        /// insert appointment into storage
        /// </summary>
        /// <param name="appointment">appointment to insert</param>
        /// <returns></returns>
        void InsertAppointment(Appointment appointment);

        /// <summary>
        /// upadte existed appointment
        /// </summary>
        /// <param name="appointment">appointmnt to update</param>
        /// <returns></returns>
        bool UpdateAppointment(Appointment appointment);

        /// <summary>
        /// remove appointment
        /// </summary>
        /// <param name="appointmentId">appointment to remove</param>
        /// <param name="calendarId"></param>
        /// <returns></returns>
        bool RemoveAppointment(object appointmentId, object calendarId);
    }
}