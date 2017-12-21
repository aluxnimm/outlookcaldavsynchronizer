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
using CalDavSynchronizer.Implementation.GoogleContacts;
using CalDavSynchronizer.IntegrationTests.TestBase;
using GenSync.EntityRelationManagement;
using NUnit.Framework;

namespace CalDavSynchronizer.IntegrationTests.ChunkedSynchronizationTest
{
  public class GoogleContactTest : ChunkedSynchronizationTestBase<string,string, GoogleContactTestSynchronizer>
  {
    private TestComponentContainer _testComponentContainer;
 

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
      _testComponentContainer = new TestComponentContainer();
    }

    protected override int? OrdinalOfReportToCheck => 0;

    protected override Options GetOptions() => _testComponentContainer.TestOptionsFactory.CreateGoogleContacts();
    
    protected override GoogleContactTestSynchronizer CreateSynchronizer(Options options)
    {
      return new GoogleContactTestSynchronizer(options, _testComponentContainer);
    }
    
    protected override IEqualityComparer<string> AIdComparer { get; } = StringComparer.InvariantCulture;
    protected override IEqualityComparer<string> BIdComparer { get; } = StringComparer.InvariantCulture;

    protected override IReadOnlyList<(string AId, string BId)> GetRelations()
    {
      return Synchronizer.Components.GoogleContactsEntityRelationDataAccess.LoadEntityRelationData()
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

    protected override async Task<IReadOnlyList<string>> CreateInB(IEnumerable<string> contents)
    {
      return await Synchronizer.Server.CreateEntities(
        contents.Select<string, Action<GoogleContactWrapper>>(
          c =>
            a =>
            {
              a.Contact.Name.FamilyName = c;
            }));
    }
    
    protected override async Task UpdateInA(string id, string content)
    {
      await Synchronizer.Outlook.UpdateEntity(id, w => w.Inner.LastName = content);
    }

    protected override async Task UpdateInB(string id, string content)
    {
      await Synchronizer.Server.UpdateEntity(id, w => w.Contact.Name.FamilyName = content);
    }

    protected override async Task<string> GetFromA(string id)
    {
      using (var wrapper = await Synchronizer.Outlook.GetEntity(id))
      {
        return wrapper.Inner.LastName;
      }
    }

    protected override async Task<string> GetFromB(string id)
    {
      return (await Synchronizer.Server.GetEntity(id)).Contact.Name.FamilyName;
    }

    protected override async Task DeleteInA(string id)
    {
      await Synchronizer.Outlook.DeleteEntity(id);
    }

    protected override async Task DeleteInB(string id)
    {
      await Synchronizer.Server.DeleteEntity(id);
    }


    [TestCase(2, 7, false)]
    public override Task Test(int? chunkSize, int itemsPerOperation, bool useWebDavCollectionSync)
    {
      return base.Test(chunkSize, itemsPerOperation, useWebDavCollectionSync);
    }
  }
}
