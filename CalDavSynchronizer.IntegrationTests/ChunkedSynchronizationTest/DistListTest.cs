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
    public class DistListTest : GenericTwoWayTestBase<string, WebResourceName, ContactTestSynchronizer>
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
            ((ContactMappingConfiguration) options.MappingConfiguration).DistributionListType = DistributionListType.Sogo;
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

        protected override async Task UpdateInA(IEnumerable<(string Id, string Content)> updates)
        {
            await Synchronizer.OutlookDistListsOrNull.UpdateEntities(updates, c => w => w.Inner.DLName = c);
        }

        protected override async Task UpdateInB(IEnumerable<(WebResourceName Id, string Content)> updates)
        {
            await Synchronizer.ServerSogoDistListsOrNull.UpdateEntities(updates, c => w => w.Name = c);
        }

        protected override async Task<IReadOnlyList<(string Id, string Name)>> GetFromA(ICollection<string> ids)
        {
            return await Synchronizer.OutlookDistListsOrNull.GetEntities(ids, e => e.Inner.DLName, e => e.Dispose());
        }

        protected override async Task<IReadOnlyList<(WebResourceName Id, string Name)>> GetFromB(ICollection<WebResourceName> ids)
        {
            return await Synchronizer.ServerSogoDistListsOrNull.GetEntities(ids, e => e.Name);
        }

        protected override async Task DeleteInA(IEnumerable<string> ids)
        {
            await Synchronizer.OutlookDistListsOrNull.DeleteEntities(ids);
        }

        protected override async Task DeleteInB(IEnumerable<WebResourceName> ids)
        {
            await Synchronizer.ServerSogoDistListsOrNull.DeleteEntities(ids);
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
