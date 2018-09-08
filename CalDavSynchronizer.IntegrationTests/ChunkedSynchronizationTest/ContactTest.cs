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
    public class ContactTest : GenericTwoWayTestBase<string, WebResourceName, ContactTestSynchronizer>
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

        protected override async Task UpdateInA(IEnumerable<(string Id, string Content)> updates)
        {
            await Synchronizer.Outlook.UpdateEntities(updates, u => w => w.Inner.LastName = u);
        }

        protected override async Task UpdateInB(IEnumerable<(WebResourceName Id, string Content)> updates)
        {
            await Synchronizer.Server.UpdateEntities(updates, u => w => w.FamilyName = u);
        }

        protected override async Task<IReadOnlyList<(string Id, string Name)>> GetFromA(ICollection<string> ids)
        {
            return await Synchronizer.Outlook.GetEntities(ids, e => e.Inner.LastName, e => e.Dispose());
        }

        protected override async Task<IReadOnlyList<(WebResourceName Id, string Name)>> GetFromB(ICollection<WebResourceName> ids)
        {
            return await Synchronizer.Server.GetEntities(ids, e => e.FamilyName);
        }

        protected override async Task DeleteInA(IEnumerable<string> ids)
        {
            await Synchronizer.Outlook.DeleteEntities(ids);
        }

        protected override async Task DeleteInB(IEnumerable<WebResourceName> ids)
        {
            await Synchronizer.Server.DeleteEntities(ids);
        }
        

        [Test]
        [TestCase(null, 7, false, Category = TestCategories.BasicCrud)]
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
