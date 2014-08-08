using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using DevExpress.XtraScheduler;
using DevExpress.XtraScheduler.Xml;
using Mailbird.Apps.Calendar.Infrastructure;
using Appointment = DevExpress.XtraScheduler.Appointment;
using ViewModelBase = Mailbird.Apps.Calendar.Infrastructure.ViewModelBase;


namespace Mailbird.Apps.Calendar.ViewModels
{
    public class FlyoutViewModel : Infrastructure.ViewModelBase
    {
        private const string DefaultTimeValue = "01.01.0001 12:00:00 PM";

        #region Bindable members

        private DateTime _startDateTime;

        private bool _isOpen;

        private string _startTime;

        private string _subject;

        private string _location;

        private DateTime _endDate;

        private string _endTime;

        private bool _allDayEvent;

        private int _labelId;

        private int _statusId;

        private object _resourceId;

        private string _description;

        private Appointment _appointment;

        

        private TreeData _selectedTreeItem;

        private Engine.Metadata.Calendar _selectedCalendar;

        private KeyValuePair<int, string> _selectedReminder;

        #endregion

        public FlyoutViewModel()
        {
            SelectedAppointments = new ObservableCollection<Appointment>();
            CreateReminderCollecton();
        }

        private void CreateReminderCollecton()
        {
            var dictionart = new List<KeyValuePair<int, string>>
            {
                new KeyValuePair<int, string>(0, "None"),
                new KeyValuePair<int, string>(300, "5 Minutes"),
                new KeyValuePair<int, string>(600, "10 Minutes"),
                new KeyValuePair<int, string>(900, "15 Minutes"),
                new KeyValuePair<int, string>(1200, "20 Minutes"),
                new KeyValuePair<int, string>(1500, "25 Minutes"),
                new KeyValuePair<int, string>(1800, "30 Minutes"),
                new KeyValuePair<int, string>(3600, "1 Hours"),
                new KeyValuePair<int, string>(7200, "2 Hours"),
                new KeyValuePair<int, string>(10800, "3 Hours"),
                new KeyValuePair<int, string>(14400, "4 Hours"),
                new KeyValuePair<int, string>(18000, "5 Hours"),
                new KeyValuePair<int, string>(21600, "6 Hours"),
                new KeyValuePair<int, string>(43200, "0.5 Days"),
                new KeyValuePair<int, string>(86400, "1 Days"),
                new KeyValuePair<int, string>(172800, "2 Days")
            };
            SelectedReminder = dictionart[0];
            ReminderCollection = dictionart;
        }

        public void OkCommandeExecute()
        {
            IsOpen = false;
            if (CurrentAppointmentId != null)
            {
                var appointment = new Engine.Metadata.Appointment();
                appointment.Id = CurrentAppointmentId;
                appointment.Subject = Subject;
                appointment.Location = Location;
                if (AllDayEvent)
                {
                    appointment.StartTime = DateTime.Parse(StartDate.Date.ToShortDateString());
                    appointment.EndTime = DateTime.Parse(EndDate.Date.ToShortDateString());
                    //appointment.StartTime = StartDate.Date + (AllDayEvent ? DateTime.MinValue.TimeOfDay : StartDate.TimeOfDay);
                    //appointment.EndTime = EndDate.Date + (AllDayEvent ? DateTime.MinValue.TimeOfDay : EndDate.TimeOfDay);
                }
                else
                {
                    appointment.StartTime = StartDate;
                    appointment.EndTime = EndDate;
                }
                
                appointment.AllDayEvent = AllDayEvent;
                appointment.LabelId = LabelId;
                appointment.Description = Description;
                appointment.StatusId = StatusId;
                appointment.Calendar = SelectedCalendar;
                appointment.ReminderInfo = GetReminderInfo(appointment);
                UpdateAppointmentAction(CurrentAppointmentId, appointment);
            }
            else
            {
                var appointment = new Engine.Metadata.Appointment();
                appointment.Id = Guid.NewGuid();
                appointment.Subject = Subject;
                appointment.Location = Location;
                appointment.StartTime = StartDate.Date + (AllDayEvent ? DateTime.MinValue.TimeOfDay : StartDate.TimeOfDay);
                appointment.EndTime = EndDate.Date + (AllDayEvent ? DateTime.MinValue.TimeOfDay : EndDate.TimeOfDay);
                appointment.AllDayEvent = AllDayEvent;
                appointment.LabelId = LabelId;
                appointment.Description = Description;
                appointment.StatusId = StatusId;
                appointment.Calendar = SelectedCalendar;
                appointment.ReminderInfo = GetReminderInfo(appointment);
                AddAppointmentAction(appointment);
            }
        }

        #region Bindable Properties

        public string Subject
        {
            get { return _subject; }
            set { SetValue(ref _subject, value, () => Subject); }
        }

        public string Location
        {
            get { return _location; }
            set { SetValue(ref _location, value, () => Location); }
        }

        public DateTime StartDate
        {
            get { return _startDateTime; }
            set { if (SetValue(ref _startDateTime, value, () => StartDate)) EndDateValidation(); }
        }

        public DateTime EndDate
        {
            get { return _endDate; }
            set
            {
                if (SetValue(ref _endDate, value, () => EndDate)) EndDateValidation();
            }
        }

        public bool AllDayEvent
        {
            get { return _allDayEvent; }
            set { if (SetValue(ref _allDayEvent, value, () => AllDayEvent) && value) AllDayEventValidation(); }
        }

        public int LabelId
        {
            get { return _labelId; }
            set { SetValue(ref _labelId, value, () => LabelId); }
        }

        public int StatusId
        {
            get { return _statusId; }
            set { SetValue(ref _statusId, value, () => StatusId); }
        }

        public object ResourceId
        {
            get { return _resourceId; }
            set { SetValue(ref _resourceId, value, () => ResourceId); }
        }

        public string Description
        {
            get { return _description; }
            set { SetValue(ref _description, value, () => Description); }
        }

        public bool IsOpen
        {
            get { return _isOpen; }
            set
            {
                if (SetValue(ref _isOpen, value, () => IsOpen) && value)
                    ShowSelectedAppointment();
            }
        }

        public Appointment Appointment
        {
            get { return _appointment; }
            set { SetValue(ref _appointment, value, () => Appointment); }
        }

        public KeyValuePair<int, string> SelectedReminder
        {
            get { return _selectedReminder; } 
            set { SetValue(ref _selectedReminder, value, () => SelectedReminder); }
        } 

        public List<KeyValuePair<int,string>> ReminderCollection { get; private set; }
        
        public TreeData SelectedTreeItem
        {
            get { return _selectedTreeItem; }
            set
            {
                if (SetValue(ref _selectedTreeItem, value, () => SelectedTreeItem))
                {
                    if (value != null)
                        _selectedCalendar = value.Data as Mailbird.Apps.Calendar.Engine.Metadata.Calendar;
                }
            }
        }

        public Mailbird.Apps.Calendar.Engine.Metadata.Calendar SelectedCalendar
        {
            get { return _selectedCalendar ?? new Mailbird.Apps.Calendar.Engine.Metadata.Calendar() { CalendarId = Guid.NewGuid().ToString(), Provider = "LocalCalendarsStorage", Name = "LocalCalendar" }; }
            set
            {
                if (value == null)
                {
                    return;
                }
                if (SetValue(ref _selectedCalendar, value, () => SelectedCalendar))
                    _selectedTreeItem = new TreeData
                    {
                        Data = value,
                        DataType = TreeDataType.Calendar,
                        Name = value.Name,
                        ParentID = value.Provider
                    };
                RaisePropertyChanged(() => SelectedTreeItem);
            }
        }

        public ObservableCollection<Appointment> SelectedAppointments { get; private set; }

        #endregion

        #region Public Properties

        public Action<Engine.Metadata.Appointment> AddAppointmentAction { get; set; }

        public Action<object, Engine.Metadata.Appointment> UpdateAppointmentAction { get; set; }

        public Action<object, Engine.Metadata.Calendar> RemoveAppointmentAction { get; set; }

        public DateTime SelectedStartDateTime { get; set; }

        public DateTime SelectedEndDateTime { get; set; }

        public bool IsEdited { get { return CheckOnEdit(); } }

        #endregion

        #region Private members

        private object CurrentAppointmentId { get; set; }

        #endregion

        #region Private methods & helpers

        private void ShowSelectedAppointment()
        {
            if (SelectedAppointments != null && SelectedAppointments.Any())
            {
                SetAppointment(SelectedAppointments.First());
            }
            else
            {
                Subject = null;
                Location = null;
                LabelId = 0;
                StatusId = 0;
                AllDayEvent = false;
                Description = null;
                //Resolve date time to format that is used on view
                StartDate = SelectedStartDateTime;
                EndDate = SelectedStartDateTime;
                SelectedReminder= ReminderCollection[0];
                CurrentAppointmentId = null;
            }
        }

        private void EndDateValidation()
        {
            if (StartDate.Date > EndDate.Date)
            {
                _endDate = StartDate;
                RaisePropertyChanged(()=>EndDate);
            }

            AllDayEventValidation();
        }

        private void AllDayEventValidation()
        {
            if (!AllDayEvent) return;

            _startTime = DefaultTimeValue;
            
            _endTime = DefaultTimeValue;
            
            if (Math.Abs((EndDate - StartDate).TotalDays) < 1)
            {
                _endDate = EndDate.AddDays(1);
                RaisePropertyChanged(() => EndDate);
            }
        }

        private string GetReminderInfo(Engine.Metadata.Appointment appointment)
        {
            if (SelectedReminder.Key != 0)
            {
                var apt = new Appointment(AppointmentType.Normal, appointment.StartTime, appointment.EndTime);
                var reminder = apt.CreateNewReminder();
                reminder.TimeBeforeStart = TimeSpan.FromSeconds(SelectedReminder.Key);
                apt.Reminders.Add(reminder);
                var helper =
                    ReminderCollectionXmlPersistenceHelper.CreateSaveInstance(apt, DateSavingType.LocalTime);
                return helper.ToXml();
            }
            return null;
        }

        private void SetAppointment(Appointment appointment)
        {
            CurrentAppointmentId = appointment.Id;
            Subject = appointment.Subject;
            Location = appointment.Location;
            StartDate = appointment.Start;
            EndDate = appointment.End;
            LabelId = appointment.LabelId;
            ResourceId = appointment.ResourceId;
            StatusId = appointment.StatusId;
            AllDayEvent = appointment.AllDay;
            Description = appointment.Description;
            //if appointment has reminder we look for the closest time to our defoult remanders
            if (appointment.Reminder != null)
            {
               var closestKeyPair =
                    ReminderCollection.First(
                        n => Math.Abs(appointment.Reminder.TimeBeforeStart.TotalSeconds - n.Key) < double.Epsilon);
                SelectedReminder = closestKeyPair;
            }
            else
            {
                SelectedReminder = ReminderCollection[0];
            }
            Appointment = appointment;
            SelectedCalendar = appointment.CustomFields["cfCalendar"] as Mailbird.Apps.Calendar.Engine.Metadata.Calendar;
        }

        private bool CheckOnEdit()
        {
            if (CurrentAppointmentId == null)
                return !string.IsNullOrEmpty(Description)
                       || !string.IsNullOrEmpty(Subject)
                       || !string.IsNullOrEmpty(Location);

            return Subject != _appointment.Subject
                       || Location != _appointment.Location
                       || StartDate != _appointment.Start
                       || EndDate != _appointment.End
                       || AllDayEvent != _appointment.AllDay
                       || LabelId != _appointment.LabelId
                       || StatusId != _appointment.StatusId
                       || ResourceId != _appointment.ResourceId
                       || (_appointment.Reminder != null && Math.Abs(_appointment.Reminder.TimeBeforeStart.TotalSeconds - SelectedReminder.Key) > double.Epsilon)
                       || (_appointment.Reminder == null && SelectedReminder.Key != 0)
                       || Description != _appointment.Description;
        }
        #endregion
    }
}
