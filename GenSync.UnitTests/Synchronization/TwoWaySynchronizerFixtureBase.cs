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
using GenSync.Synchronization.StateCreationStrategies;
using GenSync.Synchronization.StateCreationStrategies.ConflictStrategies;

namespace GenSync.UnitTests.Synchronization
{
  internal class TwoWaySynchronizerFixtureBase : SynchronizerFixtureBase
  {
    public void Synchronize (GenericConflictResolution winner)
    {
      IConflictInitialSyncStateCreationStrategy<Identifier, int, string, Identifier, int, string> conflictInitialStrategy;
      if (winner == GenericConflictResolution.AWins)
        conflictInitialStrategy = new ConflictInitialSyncStateCreationStrategyAWins<Identifier, int, string, Identifier, int, string> (_factory);
      else
        conflictInitialStrategy = new ConflictInitialSyncStateCreationStrategyBWins<Identifier, int, string, Identifier, int, string> (_factory);


      var strategy = new TwoWayInitialSyncStateCreationStrategy<Identifier, int, string, Identifier, int, string> (_factory, conflictInitialStrategy);

      SynchronizeInternal (strategy);
    }
  }
}