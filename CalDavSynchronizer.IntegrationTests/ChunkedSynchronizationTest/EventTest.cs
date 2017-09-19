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
using System.Threading.Tasks;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation;
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.Implementation.Events;
using CalDavSynchronizer.IntegrationTests.TestBase;
using DDay.iCal;
using GenSync.EntityRelationManagement;
using NUnit.Framework;

namespace CalDavSynchronizer.IntegrationTests.ChunkedSynchronizationTest
{
  public class EventTest : ChunkedSynchronizationTestBase<AppointmentId,WebResourceName, EventTestSynchronizer>
  {
    private TestComponentContainer _testComponentContainer;
    private DateTime _startDateTime;
    private DateTime _endDateTime;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
      _startDateTime = DateTime.Now.Date.AddDays(10).AddHours(15);
      _endDateTime = _startDateTime.AddHours(1);
      _testComponentContainer = new TestComponentContainer();
    }

    protected override string ProfileName { get; } = "IntegrationTest/Events/Sogo";

    protected override EventTestSynchronizer CreateSynchronizer(Options options)
    {
      return new EventTestSynchronizer(options, _testComponentContainer);
    }
    
    protected override IEqualityComparer<AppointmentId> AIdComparer { get; } = AppointmentId.Comparer;
    protected override IEqualityComparer<WebResourceName> BIdComparer { get; } = WebResourceName.Comparer;
   
    protected override IReadOnlyList<(AppointmentId AId, WebResourceName BId)> GetRelations()
    {
      return Synchronizer.Components.EntityRelationDataAccess.LoadEntityRelationData()
        .Select(r => (r.AtypeId, r.BtypeId))
        .ToArray();
    }

    protected override async Task<IReadOnlyList<AppointmentId>> CreateInA(IEnumerable<string> contents)
    {
      return await Synchronizer.Outlook.CreateEntities(
        contents.Select<string, Action<IAppointmentItemWrapper>>(
          c =>
            a =>
            {
              a.Inner.Subject = c;
              a.Inner.Start = _startDateTime;
              a.Inner.End = _endDateTime;
            }));
    }

    protected override async Task<IReadOnlyList<WebResourceName>> CreateInB(IEnumerable<string> contents)
    {
      return await Synchronizer.Server.CreateEntities(
        contents.Select<string, Action<IICalendar>>(
          c =>
            a =>
            {
              var evt = new Event();
              evt.Start = new iCalDateTime(_startDateTime);
              evt.End = new iCalDateTime(_endDateTime);
              evt.Summary = c;
              a.Events.Add(evt);
            }));
    }

    protected override async Task UpdateInA(AppointmentId id, string content)
    {
      await Synchronizer.Outlook.UpdateEntity(id, w => w.Inner.Subject = content);
    }

    protected override async Task UpdateInB(WebResourceName id, string content)
    {
      await Synchronizer.Server.UpdateEntity(id, w => w.Events[0].Summary = content);
    }

    protected override async Task<string> GetFromA(AppointmentId id)
    {
      using (var wrapper = await Synchronizer.Outlook.GetEntity(id))
      {
        return wrapper.Inner.Subject;
      }
    }

    protected override async Task<string> GetFromB(WebResourceName id)
    {
      return (await Synchronizer.Server.GetEntity(id)).Events[0].Summary;
    }

    protected override async Task DeleteInA(AppointmentId id)
    {
      await Synchronizer.Outlook.DeleteEntity(id);
    }

    protected override async Task DeleteInB(WebResourceName id)
    {
      await Synchronizer.Server.DeleteEntity(id);
    }

    [TestCase(null, 7, false)]
    [TestCase(2, 7, false)]
    [TestCase(7, 7, false)]
    [TestCase(29, 7, false)]
    [TestCase(1, 7, false)]
    [TestCase(null, 7, true)]
    [TestCase(2, 7, true)]
    [TestCase(7, 7, true)]
    [TestCase(29, 7, true)]
    [TestCase(1, 7, true)]
    public override Task Test(int? chunkSize, int itemsPerOperation, bool useWebDavCollectionSync)
    {
      return base.Test(chunkSize, itemsPerOperation, useWebDavCollectionSync);
    }
  }
}
