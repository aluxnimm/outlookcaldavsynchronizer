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
using CalDavSynchronizer.Generic.Synchronization;
using CalDavSynchronizer.Generic.Synchronization.StateCreationStrategies;
using CalDavSynchronizer.Generic.Synchronization.StateFactories;
using CalDavSynchronizer.Generic.Synchronization.States;

namespace CalDavSynchronizer.UnitTest.Synchronization
{
  internal class TwoWaySynchronizerFixtureBase : SynchronizerFixtureBase
  {
    public void Synchronize (GenericConflictResolution winner)
    {
      IEntityConflictSyncStateFactory<string, int, string, string, int, string> conflictStrategy;
      if (winner == GenericConflictResolution.AWins)
        conflictStrategy = new EntityConflictSyncStateFactory_AWins<string, int, string, string, int, string> (_factory);
      else
        conflictStrategy = new EntityConflictSyncStateFactory_BWins<string, int, string, string, int, string> (_factory);


      var strategy = new TwoWayInitialSyncStateCreationStrategy<string, int, string, string, int, string> (_factory, conflictStrategy);

      SynchronizeInternal (strategy);
    }
  }
}