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
    public class GoogleContactTest : GenericTwoWayTestBase<string, string, GoogleContactTestSynchronizer>
    {
        private TestComponentContainer _testComponentContainer;


        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _testComponentContainer = new TestComponentContainer();
        }

        protected override TimeSpan PreSyncSleepTime => TimeSpan.FromSeconds(5);
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

        protected override async Task UpdateInA(IEnumerable<(string Id, string Content)> updates)
        {
            await Synchronizer.Outlook.UpdateEntities(updates, c => w => w.Inner.LastName = c);
        }

        protected override async Task UpdateInB(IEnumerable<(string Id, string Content)> updates)
        {
            await Synchronizer.Server.UpdateEntities(updates, c => w => w.Contact.Name.FamilyName = c);
        }

        protected override async Task<IReadOnlyList<(string Id, string Name)>> GetFromA(ICollection<string> ids)
        {
            return await Synchronizer.Outlook.GetEntities(ids, e => e.Inner.LastName, e => e.Dispose());
        }

        protected override async Task<IReadOnlyList<(string Id, string Name)>> GetFromB(ICollection<string> ids)
        {
            return await Synchronizer.Server.GetEntities(ids, e => e.Contact.Name.FamilyName);
        }

        protected override async Task DeleteInA(IEnumerable<string> ids)
        {
            await Synchronizer.Outlook.DeleteEntities(ids);
        }

        protected override async Task DeleteInB(IEnumerable<string> ids)
        {
            await Synchronizer.Server.DeleteEntities(ids);
        }
        
        [Test]
        [TestCase(2, 7, false, Category = TestCategories.BasicCrud)]
        public override Task Test(int? chunkSize, int itemsPerOperation, bool useWebDavCollectionSync)
        {
            return base.Test(chunkSize, itemsPerOperation, useWebDavCollectionSync);
        }
    }
}
