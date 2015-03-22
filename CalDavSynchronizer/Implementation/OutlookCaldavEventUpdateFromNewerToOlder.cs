using System;
using CalDavSynchronizer.Generic.EntityRelationManagement;
using CalDavSynchronizer.Generic.Synchronization.States;
using DDay.iCal;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Implementation
{
  internal class OutlookCaldavEventUpdateFromNewerToOlder
      : UpdateFromNewerToOlder<string, DateTime, AppointmentItem, Uri, string, IEvent>
  {
    public OutlookCaldavEventUpdateFromNewerToOlder (EntitySyncStateEnvironment<string, DateTime, AppointmentItem, Uri, string, IEvent> environment, IEntityRelationData<string, DateTime, Uri, string> knownData, DateTime newA, string newB)
        : base(environment, knownData, newA, newB)
    {
    }

    protected override bool AIsNewerThanB
    {
      get { return _aEntity.LastModificationTime.ToUniversalTime() >= _bEntity.LastModified.UTC; }
    }
  }
}