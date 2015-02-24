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
using CalDavSynchronizer.ConflictManagement;
using CalDavSynchronizer.Synchronization;
using NUnit.Framework;

namespace CalDavSynchronizer.UnitTest.Synchronization
{
  internal class SynchronizerFixtureBase
  {
    protected TestSynchronizerSetup _synchronizerSetup;

    protected ISynchronizer _synchronizer;


    [SetUp ()]
    public virtual void Setup ()
    {
      _synchronizerSetup = new TestSynchronizerSetup();
    }

    protected void SynchronizeInternal (SynchronizationMode synchronizationMode, GenericConflictResolution conflictResolution)
    {
      var conflictResolutionStrategy = new FixedConflictResolutionStrategy<string, string> (conflictResolution);

      _synchronizer = SynchronizerFactory<string, string, int, string, string, int>.Create (synchronizationMode, conflictResolutionStrategy, _synchronizerSetup);
      _synchronizer.Synchronize();
    }


    protected void ExecuteTwice (Action a)
    {
      a();
      a();
    }
  }
}