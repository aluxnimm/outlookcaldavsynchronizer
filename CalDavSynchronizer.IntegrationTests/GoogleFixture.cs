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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation;
using CalDavSynchronizer.Implementation.Contacts;
using CalDavSynchronizer.Implementation.GoogleContacts;
using CalDavSynchronizer.IntegrationTests.Infrastructure;
using CalDavSynchronizer.IntegrationTests.TestBase;
using CalDavSynchronizer.Scheduling;
using CalDavSynchronizer.Scheduling.ComponentCollectors;
using CalDavSynchronizer.Ui;
using GenSync;
using GenSync.EntityRepositories;
using GenSync.Logging;
using GenSync.ProgressReport;
using GenSync.Synchronization;
using Microsoft.Office.Interop.Outlook;
using NUnit.Framework;
using Thought.vCards;
using Exception = System.Exception;

namespace CalDavSynchronizer.IntegrationTests
{
  [TestFixture]
  public class GoogleFixture : SynchronizerFixtureBase
  {
    [Test]
    [Apartment(System.Threading.ApartmentState.STA)]
    public async Task Run()
    {
      var options = GetOptions("Automated Test - Google Contacts");
      options.SynchronizationMode = SynchronizationMode.ReplicateOutlookIntoServer;

      ClearCache(options);

      var c = await SynchronizerFactory.CreateSynchronizerWithComponents(options, GeneralOptions);
      var components = (AvailableGoogleContactSynchronizerSynchronizerComponents) c.Item2;
      var synchronizer = c.Item1;


      await DeleteOutlookContacts(components.OutlookContactRepository);
      await DeleteGoogleContacts(components.GoogleContactRepository, components.GoogleContactContextFactory);

      var groupNames = GetOrCreateGoogleGroups(components.GoogleApiOperationExecutor, 40);
      var contactDatasByOutlookId = await CreateContactsInOutlook(components.OutlookContactRepository, CreateTestContactData(groupNames, 500));

      var reportSink = new TestReportSink();

      using (var logger = new SynchronizationLogger(options.Id, options.Name, reportSink))
      {
        await synchronizer.Synchronize(logger);
      }

      Assert.That(reportSink.SynchronizationReport.ADelta, Is.EqualTo("Unchanged: 0 , Added: 500 , Deleted 0 ,  Changed 0"));
      Assert.That(reportSink.SynchronizationReport.BDelta, Is.EqualTo("Unchanged: 0 , Added: 0 , Deleted 0 ,  Changed 0"));
      Assert.That(reportSink.SynchronizationReport.AJobsInfo, Is.EqualTo("Create 0 , Update 0 , Delete 0"));
      Assert.That(reportSink.SynchronizationReport.BJobsInfo, Is.EqualTo("Create 500 , Update 0 , Delete 0"));

      var outlook1IdsByGoogleId = components.GoogleContactsEntityRelationDataAccess.LoadEntityRelationData().ToDictionary(r => r.BtypeId, r => r.AtypeId);
      Assert.That(outlook1IdsByGoogleId.Count, Is.EqualTo(500));

      await DeleteOutlookContacts(components.OutlookContactRepository);

      options.SynchronizationMode = SynchronizationMode.ReplicateServerIntoOutlook;

      c = await SynchronizerFactory.CreateSynchronizerWithComponents(options, GeneralOptions);
      components = (AvailableGoogleContactSynchronizerSynchronizerComponents) c.Item2;
      synchronizer = c.Item1;

      reportSink.SynchronizationReport = null;

      using (var logger = new SynchronizationLogger(options.Id, options.Name, reportSink))
      {
        await synchronizer.Synchronize(logger);
      }

      Assert.That(reportSink.SynchronizationReport.ADelta, Is.EqualTo("Unchanged: 0 , Added: 0 , Deleted 500 ,  Changed 0"));
      Assert.That(reportSink.SynchronizationReport.BDelta, Is.EqualTo("Unchanged: 500 , Added: 0 , Deleted 0 ,  Changed 0"));
      Assert.That(reportSink.SynchronizationReport.AJobsInfo, Is.EqualTo("Create 500 , Update 0 , Delete 0"));
      Assert.That(reportSink.SynchronizationReport.BJobsInfo, Is.EqualTo("Create 0 , Update 0 , Delete 0"));
    }

    
  }
}