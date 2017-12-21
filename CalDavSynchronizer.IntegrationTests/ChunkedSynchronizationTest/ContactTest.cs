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
using GenSync.EntityRelationManagement;
using NUnit.Framework;
using Thought.vCards;

namespace CalDavSynchronizer.IntegrationTests.ChunkedSynchronizationTest
{
  public class ContactTest : ChunkedSynchronizationTestBase<string,WebResourceName, ContactTestSynchronizer>
  {
    private TestComponentContainer _testComponentContainer;
 

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
      _testComponentContainer = new TestComponentContainer();
    }

    protected override int? OrdinalOfReportToCheck => 0;

    protected override Options GetOptions()
    {
      var options = _testComponentContainer.TestOptionsFactory.CreateSogoContacts();
      // Set MapDistributionLists to ensure distributionlists are deleted
      ((ContactMappingConfiguration)options.MappingConfiguration).MapDistributionLists = true;
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
      return Synchronizer.Components.EntityRelationDataAccess.LoadEntityRelationData()
        .Select(r => (r.AtypeId, r.BtypeId))
        .ToArray();
    }

    protected override async Task<IReadOnlyList<string>> CreateInA(IEnumerable<string> contents)
    {
      return await Synchronizer.Outlook.CreateEntities(
        contents.Select<string, Action<IContactItemWrapper>>(
          c =>
            a =>
            {
              a.Inner.LastName = c;
            }));
    }

    protected override async Task<IReadOnlyList<WebResourceName>> CreateInB(IEnumerable<string> contents)
    {
      return await Synchronizer.Server.CreateEntities(
        contents.Select<string, Action<vCard>>(
          c =>
            a =>
            {
              a.FamilyName = c;
            }));
    }
    
    protected override async Task UpdateInA(string id, string content)
    {
      await Synchronizer.Outlook.UpdateEntity(id, w => w.Inner.LastName = content);
    }

    protected override async Task UpdateInB(WebResourceName id, string content)
    {
      await Synchronizer.Server.UpdateEntity(id, w => w.FamilyName = content);
    }

    protected override async Task<string> GetFromA(string id)
    {
      using (var wrapper = await Synchronizer.Outlook.GetEntity(id))
      {
        return wrapper.Inner.LastName;
      }
    }

    protected override async Task<string> GetFromB(WebResourceName id)
    {
      return (await Synchronizer.Server.GetEntity(id)).FamilyName;
    }

    protected override async Task DeleteInA(string id)
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
