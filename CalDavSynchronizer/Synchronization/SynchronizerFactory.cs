// This file is Part of CalDavSynchronizer (https://sourceforge.net/projects/outlookcaldavsynchronizer/)
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
using CalDavSynchronizer.ConflictManagement;

namespace CalDavSynchronizer.Synchronization
{
  public static class SynchronizerFactory<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion, TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion>
  {
    public static ISynchronizer Create (SynchronizationMode synchronizationMode, IConflictResolutionStrategy<TAtypeEntity, TBtypeEntity> conflictResolutionStrategy, ISynchronizerContext<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion, TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion> context)
    {
      switch (synchronizationMode)
      {
        case SynchronizationMode.MergeInBothDirections:
          return new TwoWaySynchronizer<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion, TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion> (context,conflictResolutionStrategy);
        case SynchronizationMode.ReplicateOutlookIntoServer:
          return new OneWayReplicator<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion, TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion> (context);
        case SynchronizationMode.ReplicateServerIntoOutlook:

          var switchedContext = new SwitchSynchronizerContextDirectionWrapper<TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion> (context);
          var replicator = new OneWayReplicator<TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion> (switchedContext);
          return replicator;
      }

      throw new NotImplementedException();
    }

   
  }
}