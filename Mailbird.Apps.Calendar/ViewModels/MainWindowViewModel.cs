#region References
using System.Collections.Generic;
﻿using System.Collections.ObjectModel;
﻿using System.Runtime.InteropServices;
﻿using System.Windows;
using System.Windows.Threading;
using DevExpress.Mvvm;
﻿using DevExpress.Xpf.Scheduler;
﻿using Mailbird.Apps.Calendar.Engine;
using System;
﻿using System.Linq;
﻿using DevExpress.Xpf.Core.Native;
﻿using Mailbird.Apps.Calendar.Engine.CalendarProviders;
﻿using Mailbird.Apps.Calendar.Engine.Interfaces;
﻿using Mailbird.Apps.Calendar.Engine.Metadata;
﻿using Mailbird.Apps.Calendar.Engine.Responses;
﻿using Mailbird.Apps.Calendar.Infrastructure;
﻿using ViewModelBase = Mailbird.Apps.Calendar.Infrastructure.ViewModelBase;
#endregion
namespace Mailbird.Apps.Calendar.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, IDisposable
    {
        #region PrivateProps

        private readonly CalendarsCatalog _calendarsCatalog = new CalendarsCatalog();

        private readonly ObservableCollection<TreeData> _treeData = new ObservableCollection<TreeData>();

        private readonly DispatcherTimer timer;

        #endregion

        #region PublicProps

        public FlyoutViewModel FlyoutViewModel { get; private set; }

        public ObservableCollection<Appointment> AppointmentCollection { get; private set; }

        public ObservableCollection<TreeData> TreeData
        {
            get { return _treeData; }
        }        

        #endregion PublicProps

        public MainWindowViewModel()
        {
            Messenger.Default.Register<GetAppointmentsCompleteResult>(this, OnGetAppointmentsComplete);
            Messenger.Default.Register<GetCalendarsCompleteResult>(this, OnGetCalendarsComplete);
            Messenger.Default.Register<InsertAppointmentCompleteResult>(this, OnInsertAppointmentComplete);
            Messenger.Default.Register<UpdateAppointmentCompleteResult>(this, OnUpdateAppointmentComplete);
            Messenger.Default.Register<AppointmentDeleteCompleteResult>(this, OnAppointmentDeleteComplete);

            foreach (var provider in _calendarsCatalog.GetProviders)
            {
                AddElementToTree(provider);
                provider.GetCalendars();
            }

            AppointmentCollection = new ObservableCollection<Appointment>();

            FlyoutViewModel = new FlyoutViewModel
            {
                AddAppointmentAction = AddAppointment,
                UpdateAppointmentAction = UpdateAppointment,
                RemoveAppointmentAction = (appointmentId, calendarId) => RemoveAppointment(appointmentId, calendarId)
            };

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMinutes(1);
            timer.Tick += OnTimerTick;

            timer.Start();
        }

        private void OnAppointmentDeleteComplete(AppointmentDeleteCompleteResult msg)
        {
            if (msg == null)
            {
                return;
            }
            if (msg.Success)
            {
                var temp = new Appointment[AppointmentCollection.Count];

                AppointmentCollection.CopyTo(temp, 0);
                foreach (
                    Appointment appointmentToRemove in temp.Where(t => t.Id.ToString() == msg.AppointmentId.ToString()))
                {
                    Application.Current.Dispatcher.BeginInvoke(
                        (Action)(() => AppointmentCollection.Remove(appointmentToRemove)));
                }
            }
            else
            {
                MessageBoxHelper.ShowError("Error remove appointment. Details : " + msg.Error, "Error",
                    MessageBoxButton.OK);
            }
        }

        private void OnUpdateAppointmentComplete(UpdateAppointmentCompleteResult msg)
        {
            if (msg == null)
            {
                return;
            }
            if (msg.Success)
            {
                Application.Current.Dispatcher.BeginInvoke(
                    (Action)(() =>
                    {
                        for (var i = 0; i < AppointmentCollection.Count; i++)
                        {
                            if (AppointmentCollection[i].Id == msg.Appointment.Id)
                            {
                                AppointmentCollection[i] = msg.Appointment;
                                break;
                            }
                        }
                    }));
            }
            else
            {
                MessageBoxHelper.ShowError("Error updating appointment. Details : " + msg.Error, "Error",
                    MessageBoxButton.OK);
            }
        }

        private void OnInsertAppointmentComplete(InsertAppointmentCompleteResult msg)
        {
            if (msg == null)
            {
                return;
            }
            if (msg.Success)
            {
                Application.Current.Dispatcher.BeginInvoke(
                    (Action)(() => AppointmentCollection.Add(msg.Appointment)));
            }
            else
            {
                MessageBoxHelper.ShowError("Error creating appointment. Details : " + msg.Error, "Error",
                    MessageBoxButton.OK);
            }
        }

        private void OnGetCalendarsComplete(GetCalendarsCompleteResult msg)
        {
            if (msg == null)
            {
                return;
            }


            if (msg.Success)
            {
                foreach (var calendar in msg.Calendars)
                {
                    Application.Current.Dispatcher.BeginInvoke(
                        (Action)(() =>
                        {
                            AddElementToTree(calendar);
                        }));
                }
            }
            else
            {
                MessageBoxHelper.ShowError("Error cGetCalendars. Details : " + msg.Error, "Error",
                    MessageBoxButton.OK);
            }
        }

        private void OnGetAppointmentsComplete(GetAppointmentsCompleteResult msg)
        {
            if (msg.Appointments != null)
            {
                foreach (var appointment in msg.Appointments)
                {
                    if (!AppointmentCollection.Contains(appointment))
                    {
                        Appointment item = appointment;
                        Application.Current.Dispatcher.BeginInvoke(
                        (Action)(() =>
                        {
                            AppointmentCollection.Add(item);
                        }));
                    }
                    else
                    {

                        for (int i = 0; i < AppointmentCollection.Count; i++)
                        {
                            var temp = AppointmentCollection[i];
                            if (temp.Id == appointment.Id)
                            {
                                temp = appointment;
                                break;
                            }
                        }
                    }
                }
            }
        }

        public void AddAppointment(Appointment appointment)
        {
            if (appointment.Id == null )
                appointment.Id = Guid.NewGuid();
            if (appointment.Calendar == null)
                appointment.Calendar = _calendarsCatalog.DefaultCalendar;
            
            _calendarsCatalog.InsertAppointment(appointment);
        }



        public void UpdateAppointment(object appointmentId, Appointment appointment)
        {
            _calendarsCatalog.UpdateAppointment(appointment);
        }

        
        public void RemoveAppointment(object appointmentId, Engine.Metadata.Calendar calendar)
        {
            _calendarsCatalog.RemoveAppointment(appointmentId, calendar);
        }


        public void AppointmentOnViewChanged(Appointment appointment)
        {
            var app = AppointmentCollection.First(f => f.Id.ToString() == appointment.Id.ToString());
            //Set missed fields
            appointment.ReminderInfo = app.ReminderInfo;
            appointment.Calendar = app.Calendar;
            UpdateAppointment(appointment.Id, appointment);
        }

        private void AddElementToTree(object element)
        {
            if (element is ICalendarProvider)
            {
                TreeData.Add(new TreeData
                {
                    DataType = TreeDataType.Provider,
                    Data = element,
                    Name = (element as ICalendarProvider).Name,
                    ParentID = "0"
                });
            }
            if (element is Engine.Metadata.Calendar)
            {
                TreeData.Add(new TreeData
                {
                    DataType = TreeDataType.Calendar,
                    Data = element,
                    Name = (element as Engine.Metadata.Calendar).Name,
                    ParentID = (element as Engine.Metadata.Calendar).Provider
                });
            }
        }

        public void OpenInnerFlyout(SchedulerControl scheduler)
        {
            FlyoutViewModel.SelectedStartDateTime = scheduler.SelectedInterval.Start;
            FlyoutViewModel.SelectedEndDateTime = scheduler.SelectedInterval.End;
            FlyoutViewModel.IsOpen = true;
        }

        public void CloseInnerFlyout()
        {
            if (FlyoutViewModel.IsEdited)
            {
                FlyoutViewModel.OkCommandeExecute();
            }
            else
            {
                FlyoutViewModel.IsOpen = false;
            }
        }

        public void Dispose()
        {
            timer.Tick -= OnTimerTick;
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            AppointmentCollection.Clear();
            foreach (var provider in _calendarsCatalog.GetProviders)
            {
                AddElementToTree(provider);
                provider.GetAppointments();
            }
            Console.WriteLine("Reload!");
        }
    }
}
