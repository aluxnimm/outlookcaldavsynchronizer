using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation;
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.Implementation.DistributionLists;
using CalDavSynchronizer.Implementation.DistributionLists.Sogo;
using CalDavSynchronizer.Implementation.Events;
using CalDavSynchronizer.IntegrationTests.TestBase;
using GenSync.EntityRelationManagement;
using NUnit.Framework;

namespace CalDavSynchronizer.IntegrationTests.ChunkedSynchronizationTest
{
  public class DistListTest : GenericTwoWayTestBase<string,WebResourceName, ContactTestSynchronizer>
  {
    private TestComponentContainer _testComponentContainer;
 

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
      _testComponentContainer = new TestComponentContainer();
    }

    protected override int? OrdinalOfReportToCheck => 1;

    protected override Options GetOptions()
    {
      var options = _testComponentContainer.TestOptionsFactory.CreateSogoContacts();
      ((ContactMappingConfiguration) options.MappingConfiguration).MapDistributionLists = true;
      ((ContactMappingConfiguration)options.MappingConfiguration).DistributionListType = DistributionListType.Sogo;
      return options;
    }

    protected override ContactTestSynchronizer CreateSynchronizer(Options options)
    {
      return new ContactTestSynchronizer(options, _testComponentContainer);
    }
    
    protected override IEqualityComparer<string> AIdComparer { get; } = StringComparer.InvariantCulture;
    protected override IEqualityComparer<WebResourceName> BIdComparer { get; } = WebResourceName.Comparer;
   
    protected override IReadOnlyList<(string AId, WebResourceName BId)> GetRelations()
    {
      return Synchronizer.Components.DistListEntityRelationDataAccess.LoadEntityRelationData()
        .Select(r => (r.AtypeId, r.BtypeId))
        .ToArray();
    }

    protected override async Task<IReadOnlyList<string>> CreateInA(IEnumerable<string> contents)
    {
      return await Synchronizer.OutlookDistListsOrNull.CreateEntities(
        contents.Select<string, Action<IDistListItemWrapper>>(
          c =>
            a =>
            {
              a.Inner.DLName = c;
            }));
    }

    protected override async Task<IReadOnlyList<WebResourceName>> CreateInB(IEnumerable<string> contents)
    {
      return await Synchronizer.ServerSogoDistListsOrNull.CreateEntities(
        contents.Select<string, Action<DistributionList>>(
          c =>
            a =>
            {
              a.Name = c;
            }));
    }

    protected override async Task UpdateInA(string id, string content)
    {
      await Synchronizer.OutlookDistListsOrNull.UpdateEntity(id, w => w.Inner.DLName = content);
    }

    protected override async Task UpdateInB(WebResourceName id, string content)
    {
      await Synchronizer.ServerSogoDistListsOrNull.UpdateEntity(id, w => w.Name = content);
    }

    protected override async Task<string> GetFromA(string id)
    {
      using (var wrapper = await Synchronizer.OutlookDistListsOrNull.GetEntity(id))
      {
        return wrapper.Inner.DLName;
      }
    }

    protected override async Task<string> GetFromB(WebResourceName id)
    {
      return (await Synchronizer.ServerSogoDistListsOrNull.GetEntity(id)).Name;
    }

    protected override async Task DeleteInA(string id)
    {
      await Synchronizer.OutlookDistListsOrNull.DeleteEntity(id);
    }

    protected override async Task DeleteInB(WebResourceName id)
    {
      await Synchronizer.ServerSogoDistListsOrNull.DeleteEntity(id);
    }

    [Test]
    [TestCase(null, 7, false)]
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
