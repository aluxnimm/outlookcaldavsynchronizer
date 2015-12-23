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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Scheduling;
using CalDavSynchronizer.Synchronization;
using GenSync.Logging;
using GenSync.Synchronization;
using NUnit.Framework;
using Rhino.Mocks;

namespace CalDavSynchronizer.UnitTest.Scheduling
{
  [TestFixture]
  public class SynchronizationWorkerFixture
  {
    private ISynchronizerFactory _synchronizerFactory;
    private SynchronizationProfileRunner _synchronizationProfileRunner;
    private StubSynchronizer _stubSynchronizer;

    [SetUp]
    public void SetUp ()
    {
      _synchronizerFactory = MockRepository.GenerateStub<ISynchronizerFactory>();
      _synchronizationProfileRunner = new SynchronizationProfileRunner (
          _synchronizerFactory,
          MockRepository.GenerateStub<ISynchronizationReportRepository>());

      var options = new Options();
      _stubSynchronizer = new StubSynchronizer();
      _synchronizerFactory.Expect (f => f.CreateSynchronizer (options)).Return (_stubSynchronizer);
      _synchronizationProfileRunner.UpdateOptions (options);
    }

    [Test]
    public void RunNoThrowAndRescheduleIfNotRunning ()
    {
      var synchronizationTask1 = _synchronizationProfileRunner.RunAndRescheduleNoThrow (true);
      var synchronizationTask2 = _synchronizationProfileRunner.RunAndRescheduleNoThrow (true);
      var synchronizationTask3 = _synchronizationProfileRunner.RunAndRescheduleNoThrow (true);
      var synchronizationTask4 = _synchronizationProfileRunner.RunAndRescheduleNoThrow (true);

      Assert.That (synchronizationTask1.IsCompleted, Is.False);
      Assert.That (synchronizationTask2.IsCompleted, Is.True);
      Assert.That (synchronizationTask3.IsCompleted, Is.True);
      Assert.That (synchronizationTask4.IsCompleted, Is.True);

      _stubSynchronizer.FinishSynchronizationEvent.Set();

      synchronizationTask1.Wait();

      Assert.That (_stubSynchronizer.RunCount, Is.EqualTo (2));
    }

    private class StubSynchronizer : IOutlookSynchronizer
    {
      private int _runCount;
      private ManualResetEventSlim _finishSynchronizationEvent = new ManualResetEventSlim();

      public ManualResetEventSlim FinishSynchronizationEvent
      {
        get { return _finishSynchronizationEvent; }
      }

      public int RunCount
      {
        get { return _runCount; }
      }

      public Task SynchronizeNoThrow (ISynchronizationLogger logger)
      {
        _runCount++;
        return Task.Run (() => FinishSynchronizationEvent.Wait());
      }

      public Task SnychronizePartialNoThrow (IEnumerable<string> outlookIds, ISynchronizationLogger logger)
      {
        throw new NotImplementedException();
      }

      public bool IsResponsible (string folderEntryId, string folderStoreId)
      {
        throw new NotImplementedException();
      }
    }
  }
}