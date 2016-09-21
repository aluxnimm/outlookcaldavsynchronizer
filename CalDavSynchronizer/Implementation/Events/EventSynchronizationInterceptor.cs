using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation.ComWrappers;
using DDay.iCal;
using GenSync.Synchronization;
using GenSync.Synchronization.StateFactories;
using GenSync.Synchronization.States;
using log4net;

namespace CalDavSynchronizer.Implementation.Events
{
  class EventSynchronizationInterceptor 
    : ISynchronizationInterceptor<AppointmentId, DateTime, AppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext>,
     ISynchronizationStateVisitor<AppointmentId, DateTime, AppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext>
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod ().DeclaringType);

    private Dictionary<string, DeleteInB<AppointmentId, DateTime, AppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext>> _deletesInByGlobalAppointmentId;
    private Dictionary<string, CreateInB<AppointmentId, DateTime, AppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext>> _createsInByGlobalAppointmentId;

    public List<IEntitySyncState<AppointmentId, DateTime, AppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext>> TransformInitialCreatedStates (
      List<IEntitySyncState<AppointmentId, DateTime, AppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext>> states, 
      IEntitySyncStateFactory<AppointmentId, DateTime, AppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext> stateFactory)
    {
      _deletesInByGlobalAppointmentId = new Dictionary<string, DeleteInB<AppointmentId, DateTime, AppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext>>();
      _createsInByGlobalAppointmentId = new Dictionary<string, CreateInB<AppointmentId, DateTime, AppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext>>();

      foreach (var state in states)
        state.Accept(this);

      foreach (var kvpDelete in _deletesInByGlobalAppointmentId)
      {
        CreateInB<AppointmentId, DateTime, AppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext> create;
        if (_createsInByGlobalAppointmentId.TryGetValue(kvpDelete.Key, out create))
        {
          s_logger.Info($"Converting deletion of '{kvpDelete.Value.KnownData.BtypeId.OriginalAbsolutePath}' and creation of new from '{create.AId}' into an update.");

          // TODO: removes are inefficient O(n) ops!!
          states.Remove(kvpDelete.Value);
          states.Remove(create);
          states.Add(
            stateFactory.Create_UpdateAtoB(
              new OutlookEventRelationData
              {
                AtypeId = create.AId,
                AtypeVersion = create.AVersion,
                BtypeId = kvpDelete.Value.KnownData.BtypeId,
                BtypeVersion = kvpDelete.Value.KnownData.BtypeVersion
              },
              create.AVersion,
              kvpDelete.Value.KnownData.BtypeVersion));
        }
      }

      _deletesInByGlobalAppointmentId = null;
      _createsInByGlobalAppointmentId = null;

      return states;
    }

    public void Dispose()
    {
      _deletesInByGlobalAppointmentId = null;
      _createsInByGlobalAppointmentId = null;
    }

    public void Visit (DeleteInBWithNoRetry<AppointmentId, DateTime, AppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext> state)
    {
   
    }

    public void Visit (DeleteInAWithNoRetry<AppointmentId, DateTime, AppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext> state)
    {
    
    }

    public void Visit(CreateInB<AppointmentId, DateTime, AppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext> state)
    {
      _createsInByGlobalAppointmentId[state.AId.GlobalAppointmentId] = state;
    }

    public void Visit (DoNothing<AppointmentId, DateTime, AppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext> doNothing)
    {
  
    }

    public void Visit (UpdateFromNewerToOlder<AppointmentId, DateTime, AppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext> updateFromNewerToOlder)
    {
   
    }

    public void Visit (Discard<AppointmentId, DateTime, AppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext> discard)
    {
     
    }

    public void Visit (CreateInA<AppointmentId, DateTime, AppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext> state)
    {
  
    }

    public void Visit (DeleteInA<AppointmentId, DateTime, AppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext> state)
    {
  
    }

    public void Visit(DeleteInB<AppointmentId, DateTime, AppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext> state)
    {
      _deletesInByGlobalAppointmentId[state.KnownData.AtypeId.GlobalAppointmentId] = state;
    }

    public void Visit (RestoreInB<AppointmentId, DateTime, AppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext> state)
    {
   
    }

    public void Visit (UpdateAToB<AppointmentId, DateTime, AppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext> state)
    {
     
    }

    public void Visit (UpdateBToA<AppointmentId, DateTime, AppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext> state)
    {
   
    }

    public void Visit (RestoreInA<AppointmentId, DateTime, AppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext> state)
    {
   
    }
  }
}
