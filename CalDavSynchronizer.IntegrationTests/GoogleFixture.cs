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

using System.Linq;
using System.Threading.Tasks;
using CalDavSynchronizer.Implementation;
using CalDavSynchronizer.IntegrationTests.Infrastructure;
using CalDavSynchronizer.IntegrationTests.TestBase;
using GenSync.Logging;
using NUnit.Framework;

namespace CalDavSynchronizer.IntegrationTests
{
  [TestFixture]
  public class GoogleFixture : GoogleContactsSynchronizerFixtureBase
  {
    [Test]
    [Apartment(System.Threading.ApartmentState.STA)]
    public async Task Run()
    {
      var options = GetOptions("Automated Test - Google Contacts");
      options.SynchronizationMode = SynchronizationMode.ReplicateOutlookIntoServer;

      await InitializeFor(options);
      await ClearEventRepositoriesAndCache();

      var groupNames = GetOrCreateGoogleGroups(40);
      await CreateContactsInOutlook(CreateTestContactData(groupNames, 500));

      var reportSink = new TestReportSink();

      using (var logger = new SynchronizationLogger(options.Id, options.Name, reportSink))
      {
        await Synchronizer.Synchronize(logger);
      }

      Assert.That(reportSink.SynchronizationReport.ADelta, Is.EqualTo("Unchanged: 0 , Added: 500 , Deleted 0 ,  Changed 0"));
      Assert.That(reportSink.SynchronizationReport.BDelta, Is.EqualTo("Unchanged: 0 , Added: 0 , Deleted 0 ,  Changed 0"));
      Assert.That(reportSink.SynchronizationReport.AJobsInfo, Is.EqualTo("Create 0 , Update 0 , Delete 0"));
      Assert.That(reportSink.SynchronizationReport.BJobsInfo, Is.EqualTo("Create 500 , Update 0 , Delete 0"));

      var outlook1IdsByGoogleId = Components.GoogleContactsEntityRelationDataAccess.LoadEntityRelationData().ToDictionary(r => r.BtypeId, r => r.AtypeId);
      Assert.That(outlook1IdsByGoogleId.Count, Is.EqualTo(500));

      await Outlook.DeleteAllEntities();

      options.SynchronizationMode = SynchronizationMode.ReplicateServerIntoOutlook;
      await InitializeFor(options);

      reportSink.SynchronizationReport = null;

      using (var logger = new SynchronizationLogger(options.Id, options.Name, reportSink))
      {
        await Synchronizer.Synchronize(logger);
      }

      Assert.That(reportSink.SynchronizationReport.ADelta, Is.EqualTo("Unchanged: 0 , Added: 0 , Deleted 500 ,  Changed 0"));
      Assert.That(reportSink.SynchronizationReport.BDelta, Is.EqualTo("Unchanged: 500 , Added: 0 , Deleted 0 ,  Changed 0"));
      Assert.That(reportSink.SynchronizationReport.AJobsInfo, Is.EqualTo("Create 500 , Update 0 , Delete 0"));
      Assert.That(reportSink.SynchronizationReport.BJobsInfo, Is.EqualTo("Create 0 , Update 0 , Delete 0"));
    }

    
  }
}