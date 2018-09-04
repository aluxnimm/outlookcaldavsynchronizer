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
  public class TaskTest : GenericTwoWayTestBase<string,WebResourceName, TaskTestSynchronizer>
  {
    private TestComponentContainer _testComponentContainer;
  
    [OneTimeSetUp]
    public void OneTimeSetup()
    {
      _testComponentContainer = new TestComponentContainer();
    }

    protected override Options GetOptions() => _testComponentContainer.TestOptionsFactory.CreateSogoTasks();

    protected override TaskTestSynchronizer CreateSynchronizer(Options options)
    {
      return new TaskTestSynchronizer(options, _testComponentContainer);
    }
    
    protected override IEqualityComparer<string> AIdComparer { get; } =StringComparer.InvariantCulture;
    protected override IEqualityComparer<WebResourceName> BIdComparer { get; } = WebResourceName.Comparer;
   
    protected override IReadOnlyList<(string AId, WebResourceName BId)> GetRelations()
    {
      return Synchronizer.Components.EntityRelationDataAccess.LoadEntityRelationData()
        .Select(r => (r.AtypeId, r.BtypeId))
        .ToArray();
    }

    protected override async Task<IReadOnlyList<string>> CreateInA(IEnumerable<string> contents)
    {
      return await Synchronizer.Outlook.CreateEntities(
        contents.Select<string, Action<ITaskItemWrapper>>(
          c =>
            a =>
            {
              a.Inner.Subject = c;
            }));
    }

    protected override async Task<IReadOnlyList<WebResourceName>> CreateInB(IEnumerable<string> contents)
    {
      return await Synchronizer.Server.CreateEntities(
        contents.Select<string, Action<IICalendar>>(
          c =>
            a =>
            {
              var todo = new Todo();
              todo.Summary = c;
              a.Todos.Add(todo);
            }));
    }

    protected override async Task UpdateInA(string id, string content)
    {
      await Synchronizer.Outlook.UpdateEntity(id, w => w.Inner.Subject = content);
    }

    protected override async Task UpdateInB(WebResourceName id, string content)
    {
      await Synchronizer.Server.UpdateEntity(id, w => w.Todos[0].Summary = content);
    }

    protected override async Task<string> GetFromA(string id)
    {
      using (var wrapper = await Synchronizer.Outlook.GetEntity(id))
      {
        return wrapper.Inner.Subject;
      }
    }

    protected override async Task<string> GetFromB(WebResourceName id)
    {
      return (await Synchronizer.Server.GetEntity(id)).Todos[0].Summary;
    }

    protected override async Task DeleteInA(string id)
    {
      await Synchronizer.Outlook.DeleteEntity(id);
    }

    protected override async Task DeleteInB(WebResourceName id)
    {
      await Synchronizer.Server.DeleteEntity(id);
    }

    [Test]
    [TestCase(null, 7, false, Category = TestCategories.BasicCrud)]
    [TestCase(2, 7, false)]
    [TestCase(7, 7, false)]
    [TestCase(29, 7, false)]
    [TestCase(1, 7, false)]
    public override Task Test(int? chunkSize, int itemsPerOperation, bool useWebDavCollectionSync)
    {
      return base.Test(chunkSize, itemsPerOperation, useWebDavCollectionSync);
    }
  }
}
