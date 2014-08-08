using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using DevExpress.Mvvm;
using Google.Apis.Requests;
using Mailbird.Apps.Calendar.Engine.Interfaces;
using Mailbird.Apps.Calendar.Engine.Metadata;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Mailbird.Apps.Calendar.Engine.Responses;

namespace Mailbird.Apps.Calendar.Engine.CalendarProviders
{
    public class GoogleCalendarProvider : ICalendarProvider
    {
       
        private CalendarService _calendarService;

        public GoogleCalendarProvider()
        {
            Name = "GoogleCalendarsStorage";
            Authorize();
        }

        public IEnumerable<Metadata.Calendar> Calendars { get; private set; }

        public string Name { get; private set; }

        private void Authorize()
        {
            UserCredential credential;
            using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    new[] { CalendarService.Scope.Calendar },
                    "MailbirdCalendar",
                    CancellationToken.None).Result;
            }

            // Create the service.
            _calendarService = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "MailbirdCalendar",
            });

            //Calendars = GetCalendars();
        }

        public void GetCalendars()
        {
            var getCalendarsCompleteResult = new GetCalendarsCompleteResult();

            var getRequest = _calendarService.CalendarList.List();

            var batchRequest1 = new BatchRequest(_calendarService);
            batchRequest1.Queue<CalendarList>(getRequest, (result, error, i, message) =>
            {
                if (error != null && !string.IsNullOrEmpty(error.Message))
                {
                    getCalendarsCompleteResult.Success = false;
                    getCalendarsCompleteResult.Error = error.Message;
                }
                else
                {
                    var list = result.Items.Select(c => new Metadata.Calendar
                    {
                        CalendarId = c.Id,
                        Name = c.Summary,
                        Description = c.Description,
                        AccessRights = c.AccessRole == "reader" ? Metadata.Calendar.Access.Read : Metadata.Calendar.Access.Write,
                        CalendarColor = (Color)ColorConverter.ConvertFromString(c.BackgroundColor),
                        Provider = "GoogleCalendarsStorage"
                    });

                    getCalendarsCompleteResult.Calendars = list;
                    Calendars = list;//stored locally
                    GetAppointments();
                }

                Messenger.Default.Send(getCalendarsCompleteResult);//early send info 

            });
            batchRequest1.ExecuteAsync();
            
        }

        public void GetAppointments()
        {
            var batchRequest1 = new BatchRequest(_calendarService);

            foreach (var calendar in Calendars)
            {
                var getRequest = _calendarService.Events.List(calendar.CalendarId);
    
                batchRequest1.Queue<Events>(getRequest, (result1, error1, i1, message1) =>
                {
                    var getAppointmentsCompleteResult = new GetAppointmentsCompleteResult();
                    if (error1 != null && !string.IsNullOrEmpty(error1.Message))
                    {
                        getAppointmentsCompleteResult.Success = false;
                        getAppointmentsCompleteResult.Error = error1.Message;
                        //early send info 
                    }
                    else
                    {
                        IEnumerable<Appointment> list = result1.Items.Select(a => new Appointment
                        {
                            Id = a.Id,
                            Calendar = calendar,
                            StartTime = (a.Start != null && a.Start.DateTime.HasValue) ? a.Start.DateTime.Value : DateTime.Now,
                            EndTime = (a.End != null && a.End.DateTime.HasValue) ? a.End.DateTime.Value : DateTime.Now,
                            Subject = a.Summary,
                            Description = a.Description,
                            Location = a.Location,
                            LabelId = !string.IsNullOrEmpty(a.ColorId) ? int.Parse(a.ColorId) : 0
                        });


                        getAppointmentsCompleteResult.Appointments = list.ToList();
                    }

                    

                    Messenger.Default.Send(getAppointmentsCompleteResult);
                });
            }
            batchRequest1.ExecuteAsync();


        //var appointments = new List<Appointment>();
            //foreach (var calendar in Calendars)
            //{
            //    appointments.AddRange(GetAppointments(calendar));
            //}
            //return appointments;
        }


        public void InsertAppointment(Appointment appointment)
        {
            var baseResult = new AppointmentCompleteResult(appointment);

            var googleEvent = new Event();
            googleEvent.Summary = appointment.Subject;
            googleEvent.Location = appointment.Location;
            googleEvent.Start = new EventDateTime
            {
                DateTime = appointment.StartTime
            };
            googleEvent.End = new EventDateTime
            {
                DateTime = appointment.EndTime
            };
            googleEvent.EndTimeUnspecified = appointment.AllDayEvent;
            googleEvent.Description = appointment.Description;
            //googleEvent.ColorId = appointment.LabelId.ToString();
            EventsResource.InsertRequest request = _calendarService.Events.Insert(googleEvent, appointment.Calendar.CalendarId);

            var batchRequest1 = new BatchRequest(_calendarService);
            batchRequest1.Queue<Event>(request, (result1, error1, i1, message1) =>
            {
                if (error1 != null && !string.IsNullOrEmpty(error1.Message))
                {
                    baseResult.Success = false;
                    baseResult.Error = error1.Message;
                }
                baseResult.Appointment.Id = result1.Id;
                baseResult.Appointment.StartTime = (result1.Start != null && result1.Start.DateTime.HasValue)
                    ? result1.Start.DateTime.Value
                    : DateTime.Now;
                baseResult.Appointment.EndTime = (result1.End != null && result1.End.DateTime.HasValue)
                    ? result1.End.DateTime.Value
                    : DateTime.Now;

                Messenger.Default.Send(baseResult);
                
            });
            
            batchRequest1.ExecuteAsync();
        }

        public bool UpdateAppointment(Appointment appointment)
        {
            var baseResult = new AppointmentCompleteResult(appointment);

            var getRequest = _calendarService.Events.Get(appointment.Calendar.CalendarId, appointment.Id.ToString());

            var batchRequest1 = new BatchRequest(_calendarService);
            batchRequest1.Queue<Event>(getRequest, (result1, error1, i1, message1) =>
            {
                if (error1 != null && !string.IsNullOrEmpty(error1.Message))
                {
                    baseResult.Success = false;
                    baseResult.Error = error1.Message;
                    Messenger.Default.Send(baseResult);//early send info 
                }
                else
                {
                    Event googleEvent = result1;
                    googleEvent.Summary = appointment.Subject;
                    googleEvent.Location = appointment.Location;
                    googleEvent.Start.DateTime = appointment.StartTime;
                    googleEvent.End.DateTime = appointment.EndTime;
                    googleEvent.EndTimeUnspecified = appointment.AllDayEvent;
                    googleEvent.Description = appointment.Description;
                    googleEvent.ColorId = appointment.LabelId.ToString();

                    EventsResource.UpdateRequest request = _calendarService.Events.Update(googleEvent, appointment.Calendar.CalendarId, googleEvent.Id);

                    var batchRequest2 = new BatchRequest(_calendarService);
                    batchRequest2.Queue<Event>(request, (result2, error2, i2, message2) =>
                    {
                        if (error2 != null && !string.IsNullOrEmpty(error2.Message))
                        {   
                            baseResult.Success = false;
                            baseResult.Error = error2.Message;
                        }
                        Messenger.Default.Send(baseResult);
                    });
                    batchRequest2.ExecuteAsync();
                }
            });
            batchRequest1.ExecuteAsync();

            return true;
        }



        public bool RemoveAppointment(object appointmentId, object calendarId)
        {
            var baseResult = new AppointmentDeleteCompleteResult(appointmentId);

            EventsResource.DeleteRequest request = _calendarService.Events.Delete(calendarId.ToString(), appointmentId.ToString());
            var batchRequest1 = new BatchRequest(_calendarService);
            batchRequest1.Queue<Event>(request, (result1, error1, i1, message1) =>
            {
                if (error1 != null && !string.IsNullOrEmpty(error1.Message))
                {
                    baseResult.Success = false;
                    baseResult.Error = error1.Message;
                }
                Messenger.Default.Send(baseResult);
            });

            batchRequest1.ExecuteAsync();
            return true;
        }

        
    }

    
}