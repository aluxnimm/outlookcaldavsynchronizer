// This file is Part of CalDavSynchronizer (http://outlookcaldavsynchronizer.sourceforge.net/)
// Copyright (c) 2015 Gerhard Zehetbauer 
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using CalDavSynchronizer.Generic.EntityRelationManagement;
using CalDavSynchronizer.Generic.Synchronization;
using CalDavSynchronizer.Generic.Synchronization.States;
using DDay.iCal;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Implementation
{
  public static class InitialSyncStateCreationStrategyFactory
  {

    private static IEntityConflictSyncStateFactory<string, DateTime, AppointmentItemWrapper, Uri, string, IICalendar> Create (IEntitySyncStateFactory<string, DateTime, AppointmentItemWrapper, Uri, string, IICalendar> syncStateFactory, EntitySyncStateEnvironment<string, DateTime, AppointmentItemWrapper, Uri, string, IICalendar> environment, ConflictResolution conflictResolution)
    {
      switch (conflictResolution)
      {
        case ConflictResolution.OutlookWins:
          return new EntityConflictSyncStateFactory_AWins<string, DateTime, AppointmentItemWrapper, Uri, string, IICalendar> (syncStateFactory);
        case ConflictResolution.ServerWins:
          return new EntityConflictSyncStateFactory_BWins<string, DateTime, AppointmentItemWrapper, Uri, string, IICalendar> (syncStateFactory);
        case ConflictResolution.Automatic:
          return new OutlookCaldavEventEntityConflictSyncStateFactory_Automatic (environment);
      }

      throw new NotImplementedException ();
    }

    public static IInitialSyncStateCreationStrategy<string, DateTime, AppointmentItemWrapper, Uri, string, IICalendar> Create (IEntitySyncStateFactory<string, DateTime, AppointmentItemWrapper, Uri, string, IICalendar> syncStateFactory, EntitySyncStateEnvironment<string, DateTime, AppointmentItemWrapper, Uri, string, IICalendar> environment, SynchronizationMode synchronizationMode, ConflictResolution conflictResolution)
    {
      switch (synchronizationMode)
      {
        case SynchronizationMode.MergeInBothDirections:
          var conflictResolutionStrategy = Create (syncStateFactory,environment, conflictResolution);
          return new TwoWayInitialSyncStateCreationStrategy<string, DateTime, AppointmentItemWrapper, Uri, string, IICalendar> (
              syncStateFactory,
              conflictResolutionStrategy
              );
        case SynchronizationMode.ReplicateOutlookIntoServer:
        case SynchronizationMode.MergeOutlookIntoServer:
          return new OneWayInitialSyncStateCreationStrategy_AToB<string, DateTime, AppointmentItemWrapper, Uri, string, IICalendar> (
              syncStateFactory,
              synchronizationMode == SynchronizationMode.ReplicateOutlookIntoServer ? OneWaySyncMode.Replicate : OneWaySyncMode.Merge
           );
        case SynchronizationMode.ReplicateServerIntoOutlook:
        case SynchronizationMode.MergeServerIntoOutlook:
          return new OneWayInitialSyncStateCreationStrategy_BToA<string, DateTime, AppointmentItemWrapper, Uri, string, IICalendar> (
             syncStateFactory,
             synchronizationMode == SynchronizationMode.ReplicateServerIntoOutlook ? OneWaySyncMode.Replicate : OneWaySyncMode.Merge
           );
       
      }
      throw new NotImplementedException ();
    }

  }

  internal class OutlookCaldavEventEntityConflictSyncStateFactory_Automatic
      : EntityConflictSyncStateFactory_Automatic<string, DateTime, AppointmentItemWrapper, Uri, string, IICalendar>
  {
    public OutlookCaldavEventEntityConflictSyncStateFactory_Automatic (EntitySyncStateEnvironment<string, DateTime, AppointmentItemWrapper, Uri, string, IICalendar> environment)
        : base(environment)
    {
    }

    protected override IEntitySyncState<string, DateTime, AppointmentItemWrapper, Uri, string, IICalendar> Create_FromNewerToOlder (IEntityRelationData<string, DateTime, Uri, string> knownData, DateTime newA, string newB)
    {
      return new OutlookCaldavEventUpdateFromNewerToOlder (
          _environment,
          knownData,
          newA,
          newB
          );
    }
  }
}