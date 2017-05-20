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
    : ISynchronizationInterceptor<AppointmentId, DateTime, IAppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext>,
     ISynchronizationStateVisitor<AppointmentId, DateTime, IAppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext>
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod ().DeclaringType);

    private Dictionary<string, ContextWithDelete> _deletesInByGlobalAppointmentId;
    private Dictionary<string, ContextWithCreate> _createsInByGlobalAppointmentId;

    public void TransformInitialCreatedStates(
      IReadOnlyList<IEntitySyncStateContext<AppointmentId, DateTime, IAppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext>> syncStateContexts,
      IEntitySyncStateFactory<AppointmentId, DateTime, IAppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext> stateFactory)
    {
      _deletesInByGlobalAppointmentId = new Dictionary<string, ContextWithDelete>();
      _createsInByGlobalAppointmentId = new Dictionary<string, ContextWithCreate>();

      foreach (var state in syncStateContexts)
        state.Accept(this);

      foreach (var kvpDelete in _deletesInByGlobalAppointmentId)
      {
        ContextWithCreate create;
        if (_createsInByGlobalAppointmentId.TryGetValue(kvpDelete.Key, out create))
        {
          s_logger.Info($"Converting deletion of '{kvpDelete.Value.State.KnownData.BtypeId.OriginalAbsolutePath}' and creation of new from '{create.State.AId}' into an update.");

          kvpDelete.Value.Context.SetState(stateFactory.Create_Discard());

          create.Context.SetState(stateFactory.Create_UpdateAtoB(
            new OutlookEventRelationData
            {
              AtypeId = create.State.AId,
              AtypeVersion = create.State.AVersion,
              BtypeId = kvpDelete.Value.State.KnownData.BtypeId,
              BtypeVersion = kvpDelete.Value.State.KnownData.BtypeVersion
            },
            create.State.AVersion,
            kvpDelete.Value.State.KnownData.BtypeVersion));
        }
      }

      _deletesInByGlobalAppointmentId = null;
      _createsInByGlobalAppointmentId = null;

    }

    public void Dispose()
    {
      _deletesInByGlobalAppointmentId = null;
      _createsInByGlobalAppointmentId = null;
    }

  
    public void Visit(IEntitySyncStateContext<AppointmentId, DateTime, IAppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext> syncStateContext, RestoreInA<AppointmentId, DateTime, IAppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext> state)
    {
      
    }

    public void Visit(IEntitySyncStateContext<AppointmentId, DateTime, IAppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext> syncStateContext, UpdateBToA<AppointmentId, DateTime, IAppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext> state)
    {
      
    }

    public void Visit(IEntitySyncStateContext<AppointmentId, DateTime, IAppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext> syncStateContext, UpdateAToB<AppointmentId, DateTime, IAppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext> state)
    {
      
    }

    public void Visit(IEntitySyncStateContext<AppointmentId, DateTime, IAppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext> syncStateContext, RestoreInB<AppointmentId, DateTime, IAppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext> state)
    {
      
    }

    public void Visit(IEntitySyncStateContext<AppointmentId, DateTime, IAppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext> syncStateContext, DeleteInBWithNoRetry<AppointmentId, DateTime, IAppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext> state)
    {
      
    }

    public void Visit(IEntitySyncStateContext<AppointmentId, DateTime, IAppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext> syncStateContext, DeleteInB<AppointmentId, DateTime, IAppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext> state)
    {
      if (!string.IsNullOrEmpty (state.KnownData.AtypeId.GlobalAppointmentId))
        _deletesInByGlobalAppointmentId[state.KnownData.AtypeId.GlobalAppointmentId] = new ContextWithDelete(syncStateContext, state);
    }

    public void Visit(IEntitySyncStateContext<AppointmentId, DateTime, IAppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext> syncStateContext, DeleteInAWithNoRetry<AppointmentId, DateTime, IAppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext> state)
    {
      
    }

    public void Visit(IEntitySyncStateContext<AppointmentId, DateTime, IAppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext> syncStateContext, DeleteInA<AppointmentId, DateTime, IAppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext> state)
    {
      
    }

    public void Visit(IEntitySyncStateContext<AppointmentId, DateTime, IAppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext> syncStateContext, CreateInB<AppointmentId, DateTime, IAppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext> state)
    {
      if (!string.IsNullOrEmpty (state.AId.GlobalAppointmentId))
        _createsInByGlobalAppointmentId[state.AId.GlobalAppointmentId] = new ContextWithCreate(syncStateContext,state);
    }

    public void Visit(IEntitySyncStateContext<AppointmentId, DateTime, IAppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext> syncStateContext, CreateInA<AppointmentId, DateTime, IAppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext> state)
    {
      
    }

    public void Visit(IEntitySyncStateContext<AppointmentId, DateTime, IAppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext> syncStateContext, DoNothing<AppointmentId, DateTime, IAppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext> doNothing)
    {
      
    }

    public void Visit(IEntitySyncStateContext<AppointmentId, DateTime, IAppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext> syncStateContext, Discard<AppointmentId, DateTime, IAppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext> discard)
    {
      
    }

    public void Visit(IEntitySyncStateContext<AppointmentId, DateTime, IAppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext> syncStateContext, UpdateFromNewerToOlder<AppointmentId, DateTime, IAppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext> updateFromNewerToOlder)
    {
      
    }
    
    struct ContextWithDelete
    {
      public readonly IEntitySyncStateContext<AppointmentId, DateTime, IAppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext> Context;
      public readonly DeleteInB<AppointmentId, DateTime, IAppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext> State;

      public ContextWithDelete (IEntitySyncStateContext<AppointmentId, DateTime, IAppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext> context, DeleteInB<AppointmentId, DateTime, IAppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext> state)
      {
        if (context == null)
          throw new ArgumentNullException (nameof (context));
        if (state == null)
          throw new ArgumentNullException (nameof (state));
        Context = context;
        State = state;
      }
    }

    struct ContextWithCreate
    {
      public readonly IEntitySyncStateContext<AppointmentId, DateTime, IAppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext> Context;
      public readonly CreateInB<AppointmentId, DateTime, IAppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext> State;

      public ContextWithCreate (IEntitySyncStateContext<AppointmentId, DateTime, IAppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext> context, CreateInB<AppointmentId, DateTime, IAppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext> state)
      {
        if (context == null)
          throw new ArgumentNullException (nameof (context));
        if (state == null)
          throw new ArgumentNullException (nameof (state));
        Context = context;
        State = state;
      }
    }
  }
}
