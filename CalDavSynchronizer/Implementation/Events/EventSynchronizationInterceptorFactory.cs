using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation.ComWrappers;
using DDay.iCal;
using GenSync.Synchronization;
using GenSync.Synchronization.StateFactories;
using GenSync.Synchronization.States;

namespace CalDavSynchronizer.Implementation.Events
{
  class EventSynchronizationInterceptorFactory
    : ISynchronizationInterceptorFactory<AppointmentId, DateTime, AppointmentItemWrapper, WebResourceName, string, IICalendar>
     
  {
    public ISynchronizationInterceptor<AppointmentId, DateTime, AppointmentItemWrapper, WebResourceName, string, IICalendar> Create()
    {
      return new EventSynchronizationInterceptor();
    }
  }
}
