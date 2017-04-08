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
using CalDavSynchronizer.ChangeWatching;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.Implementation;
using CalDavSynchronizer.Reports;
using CalDavSynchronizer.Scheduling;
using CalDavSynchronizer.Synchronization;
using GenSync;
using GenSync.EntityRepositories;
using GenSync.Logging;
using GenSync.Synchronization;
using log4net;
using log4net.Core;
using NUnit.Framework;
using Rhino.Mocks;

namespace CalDavSynchronizer.UnitTest.Scheduling
{
  [TestFixture]
  public class SynchronizationProfileRunnerFixture
  {
    private ISynchronizerFactory _synchronizerFactory;
    private SynchronizationProfileRunner _synchronizationProfileRunner;
    private TestSynchronizationReportSink _synchronizationReportSink;
    private StubDateTimeProvider _dateTimeProvider;
    private List<string> _loggedInfoMessages = new List<string>();

    [SetUp]
    public void SetUp ()
    {
      _synchronizerFactory = MockRepository.GenerateStub<ISynchronizerFactory>();
      _dateTimeProvider = new StubDateTimeProvider();
      _synchronizationReportSink = new TestSynchronizationReportSink();
      _synchronizationProfileRunner = new SynchronizationProfileRunner (
          _synchronizerFactory,
          _synchronizationReportSink,
          MockRepository.GenerateStub<IFolderChangeWatcherFactory> (),
          delegate { },
          MockRepository.GenerateStub<ISynchronizationRunLogger> (),
          _dateTimeProvider);

      var logger = MockRepository.GenerateStub<ILog>();
      logger
        .Expect(_ => _.Info(null))
        .IgnoreArguments()
        .WhenCalled(_ => _loggedInfoMessages.Add((string)_.Arguments[0]));
      logger
        .Expect(_ => _.Logger)
        .Return(MockRepository.GenerateStub<ILogger>());

      SetSynchronizationProfileRunnerLogger (logger);
    }

    async Task SetupSynchronizerFactory(IOutlookSynchronizer synchronizer)
    {
      var options = new Options {SynchronizationIntervalInMinutes = 1};
      var generalOptions = new GeneralOptions ();
      _synchronizerFactory.Expect (f => f.CreateSynchronizer (options, generalOptions)).Return (Task.FromResult<IOutlookSynchronizer> (synchronizer));
      await _synchronizationProfileRunner.UpdateOptions (options, generalOptions);
    }

    [Test]
    public async Task RunNoThrowAndRescheduleIfNotRunning ()
    {
      var synchronizer = new StubSynchronizer();
      await SetupSynchronizerFactory(synchronizer);
      var synchronizationTask1 = _synchronizationProfileRunner.RunAndRescheduleNoThrow (true);
      var synchronizationTask2 = _synchronizationProfileRunner.RunAndRescheduleNoThrow (true);
      var synchronizationTask3 = _synchronizationProfileRunner.RunAndRescheduleNoThrow (true);
      var synchronizationTask4 = _synchronizationProfileRunner.RunAndRescheduleNoThrow (true);

      Assert.That (synchronizationTask1.IsCompleted, Is.False);
      Assert.That (synchronizationTask2.IsCompleted, Is.True);
      Assert.That (synchronizationTask3.IsCompleted, Is.True);
      Assert.That (synchronizationTask4.IsCompleted, Is.True);

      synchronizer.FinishSynchronizationEvent.Set();

      synchronizationTask1.Wait();

      Assert.That (synchronizer.RunCount, Is.EqualTo (2));
    }


    [Test]
    public async Task RunAndRescheduleNoThrow_SynchronizerThrowsOverloadException_DoesntReportAnError()
    {
      var synchronizer = MockRepository.GenerateStrictMock<IOutlookSynchronizer>();
      await SetupSynchronizerFactory (synchronizer);

      _dateTimeProvider.Now = new DateTime (2000, 1, 1);

      synchronizer
        .Expect(_ => synchronizer.Synchronize(Arg<ISynchronizationLogger>.Is.NotNull))
        .Throw(new WebRepositoryOverloadException(null, new Exception()))
        .Repeat.Times(4);
      
      await _synchronizationProfileRunner.RunAndRescheduleNoThrow (false);
      _dateTimeProvider.Now += TimeSpan.FromMinutes (5);
      await _synchronizationProfileRunner.RunAndRescheduleNoThrow (false);
      _dateTimeProvider.Now += TimeSpan.FromMinutes (5);
      await _synchronizationProfileRunner.RunAndRescheduleNoThrow (false);
      _dateTimeProvider.Now += TimeSpan.FromMinutes (5);
      await _synchronizationProfileRunner.RunAndRescheduleNoThrow (false);

      Assert.That (
       _synchronizationReportSink.Reports.Count,
       Is.EqualTo(4));

      Assert.That(
        _synchronizationReportSink.Reports.Any(r => r.HasErrors),
        Is.False);

    }


    [Test]
    public async Task RunAndRescheduleNoThrow_SynchronizerThrowsOverloadExceptionWithPostponeDate_DoesntReportAnErrorAndWaitsTilDatePassed ()
    {
      var synchronizer = MockRepository.GenerateStrictMock<IOutlookSynchronizer> ();
      await SetupSynchronizerFactory (synchronizer);

      _dateTimeProvider.Now = new DateTime(2000,1,1);

      synchronizer
        .Expect (_ => synchronizer.Synchronize (Arg<ISynchronizationLogger>.Is.NotNull))
        .Throw (new WebRepositoryOverloadException (new DateTime(2000, 1, 5), new Exception ()))
        .Repeat.Once();

      await _synchronizationProfileRunner.RunAndRescheduleNoThrow (false);
      _dateTimeProvider.Now += TimeSpan.FromMinutes(5);
      await _synchronizationProfileRunner.RunAndRescheduleNoThrow (false);
      _dateTimeProvider.Now += TimeSpan.FromMinutes (5);
      await _synchronizationProfileRunner.RunAndRescheduleNoThrow (false);
      _dateTimeProvider.Now += TimeSpan.FromMinutes (5);
      await _synchronizationProfileRunner.RunAndRescheduleNoThrow (false);

      synchronizer.VerifyAllExpectations();

      Assert.That(
        _loggedInfoMessages.Count,
        Is.EqualTo(3));

      Assert.That(
        _loggedInfoMessages.All(m => m == "This profile will not run, since it is postponed until '05.01.2000 00:00:00'"),
        Is.True);

      Assert.That (
       _synchronizationReportSink.Reports.Count,
       Is.EqualTo (1));

      Assert.That (
        _synchronizationReportSink.Reports.Any (r => r.HasErrors),
        Is.False);

      _synchronizationReportSink.Reports.Clear();

      _dateTimeProvider.Now += TimeSpan.FromDays (5);

      synchronizer.BackToRecord();
      synchronizer
       .Expect (_ => synchronizer.Synchronize (Arg<ISynchronizationLogger>.Is.NotNull))
       .Return(Task.FromResult(0))
       .Repeat.Times (4);
      synchronizer.Replay();

      await _synchronizationProfileRunner.RunAndRescheduleNoThrow (false);
      _dateTimeProvider.Now += TimeSpan.FromMinutes (5);
      await _synchronizationProfileRunner.RunAndRescheduleNoThrow (false);
      _dateTimeProvider.Now += TimeSpan.FromMinutes (5);
      await _synchronizationProfileRunner.RunAndRescheduleNoThrow (false);
      _dateTimeProvider.Now += TimeSpan.FromMinutes (5);
      await _synchronizationProfileRunner.RunAndRescheduleNoThrow (false);

      synchronizer.VerifyAllExpectations ();

      Assert.That (
       _synchronizationReportSink.Reports.Count,
       Is.EqualTo (4));

      Assert.That (
        _synchronizationReportSink.Reports.Any (r => r.HasErrors),
        Is.False);
    }


    void SetSynchronizationProfileRunnerLogger(ILog logger)
    {
      var field = typeof(SynchronizationProfileRunner).GetField("s_logger", System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
      field.SetValue(null, logger);
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

      public Task Synchronize (ISynchronizationLogger logger)
      {
        _runCount++;
        return Task.Run (() => FinishSynchronizationEvent.Wait());
      }

      public Task SynchronizePartial (IEnumerable<IOutlookId> outlookIds, ISynchronizationLogger logger)
      {
        throw new NotImplementedException();
      }
    }


    class TestSynchronizationReportSink : ISynchronizationReportSink
    {
      public void PostReport(SynchronizationReport report)
      {
        Reports.Add(report);
      }

      public List<SynchronizationReport> Reports { get;  } = new List<SynchronizationReport>();
    }

    class StubDateTimeProvider : IDateTimeProvider
    {
      public DateTime Now { get; set; }
    }
  }
}