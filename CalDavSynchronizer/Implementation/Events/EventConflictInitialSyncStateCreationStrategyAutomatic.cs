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
using CalDavSynchronizer.Implementation.ComWrappers;
using DDay.iCal;
using GenSync.EntityRelationManagement;
using GenSync.Synchronization;
using GenSync.Synchronization.StateCreationStrategies.ConflictStrategies;
using GenSync.Synchronization.States;

namespace CalDavSynchronizer.Implementation.Events
{
  internal class EventConflictInitialSyncStateCreationStrategyAutomatic
      : ConflictInitialSyncStateCreationStrategyAutomatic<string, DateTime, AppointmentItemWrapper, Uri, string, IICalendar>
  {
    public EventConflictInitialSyncStateCreationStrategyAutomatic (EntitySyncStateEnvironment<string, DateTime, AppointmentItemWrapper, Uri, string, IICalendar> environment)
        : base (environment)
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