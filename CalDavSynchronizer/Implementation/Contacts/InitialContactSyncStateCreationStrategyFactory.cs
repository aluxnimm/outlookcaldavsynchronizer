// This file is Part of CalDavSynchronizer (http://outlookcaldavsynchronizer.sourceforge.net/)
// Copyright (c) 2015 Gerhard Zehetbauer
// Copyright (c) 2015 Alexander Nimmervoll
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
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation.ComWrappers;
using GenSync.Synchronization;
using GenSync.Synchronization.StateCreationStrategies;
using GenSync.Synchronization.StateCreationStrategies.ConflictStrategies;
using GenSync.Synchronization.StateFactories;
using Microsoft.Office.Interop.Outlook;
using Thought.vCards;

namespace CalDavSynchronizer.Implementation.Contacts
{
  public static class InitialContactSyncStateCreationStrategyFactory
  {
    private static IConflictInitialSyncStateCreationStrategy<string, DateTime, ContactItemWrapper, WebResourceName, string, vCard> Create (IEntitySyncStateFactory<string, DateTime, ContactItemWrapper, WebResourceName, string, vCard> syncStateFactory, EntitySyncStateEnvironment<string, DateTime, ContactItemWrapper, WebResourceName, string, vCard> environment, ConflictResolution conflictResolution)
    {
      switch (conflictResolution)
      {
        case ConflictResolution.OutlookWins:
          return new ConflictInitialSyncStateCreationStrategyAWins<string, DateTime, ContactItemWrapper, WebResourceName, string, vCard> (syncStateFactory);
        case ConflictResolution.ServerWins:
          return new ConflictInitialSyncStateCreationStrategyBWins<string, DateTime, ContactItemWrapper, WebResourceName, string, vCard> (syncStateFactory);
        case ConflictResolution.Automatic:
          return new ContactConflictInitialSyncStateCreationStrategyAutomatic (environment);
      }

      throw new NotImplementedException();
    }

    public static IInitialSyncStateCreationStrategy<string, DateTime, ContactItemWrapper, WebResourceName, string, vCard> Create (IEntitySyncStateFactory<string, DateTime, ContactItemWrapper, WebResourceName, string, vCard> syncStateFactory, EntitySyncStateEnvironment<string, DateTime, ContactItemWrapper, WebResourceName, string, vCard> environment, SynchronizationMode synchronizationMode, ConflictResolution conflictResolution)
    {
      switch (synchronizationMode)
      {
        case SynchronizationMode.MergeInBothDirections:
          var conflictResolutionStrategy = Create (syncStateFactory, environment, conflictResolution);
          return new TwoWayInitialSyncStateCreationStrategy<string, DateTime, ContactItemWrapper, WebResourceName, string, vCard> (
              syncStateFactory,
              conflictResolutionStrategy
              );
        case SynchronizationMode.ReplicateOutlookIntoServer:
        case SynchronizationMode.MergeOutlookIntoServer:
          return new OneWayInitialSyncStateCreationStrategy_AToB<string, DateTime, ContactItemWrapper, WebResourceName, string, vCard> (
              syncStateFactory,
              synchronizationMode == SynchronizationMode.ReplicateOutlookIntoServer ? OneWaySyncMode.Replicate : OneWaySyncMode.Merge
              );
        case SynchronizationMode.ReplicateServerIntoOutlook:
        case SynchronizationMode.MergeServerIntoOutlook:
          return new OneWayInitialSyncStateCreationStrategy_BToA<string, DateTime, ContactItemWrapper, WebResourceName, string, vCard> (
              syncStateFactory,
              synchronizationMode == SynchronizationMode.ReplicateServerIntoOutlook ? OneWaySyncMode.Replicate : OneWaySyncMode.Merge
              );
      }
      throw new NotImplementedException();
    }
  }
}