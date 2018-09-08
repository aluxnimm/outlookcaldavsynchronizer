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
    public class GoogleTaskTest : GenericTwoWayTestBase<string, string, GoogleTaskTestSynchronizer>
    {
        private TestComponentContainer _testComponentContainer;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _testComponentContainer = new TestComponentContainer();
        }

        protected override Options GetOptions() => _testComponentContainer.TestOptionsFactory.CreateGoogleTasks();

        protected override GoogleTaskTestSynchronizer CreateSynchronizer(Options options)
        {
            return new GoogleTaskTestSynchronizer(options, _testComponentContainer);
        }

        protected override IEqualityComparer<string> AIdComparer { get; } = StringComparer.InvariantCulture;
        protected override IEqualityComparer<string> BIdComparer { get; } = StringComparer.InvariantCulture;

        protected override IReadOnlyList<(string AId, string BId)> GetRelations()
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

        protected override async Task<IReadOnlyList<string>> CreateInB(IEnumerable<string> contents)
        {
            return await Synchronizer.Server.CreateEntities(
                contents.Select<string, Action<Google.Apis.Tasks.v1.Data.Task>>(
                    c =>
                        a =>
                        {
                            a.Title = c;
                        }));
        }

        protected override async Task UpdateInA(IEnumerable<(string Id, string Content)> updates)
        {
            await Synchronizer.Outlook.UpdateEntities(updates, c => w => w.Inner.Subject = c);
        }

        protected override async Task UpdateInB(IEnumerable<(string Id, string Content)> updates)
        {
            await Synchronizer.Server.UpdateEntities(updates, c => w => w.Title = c);
        }

        protected override async Task<IReadOnlyList<(string Id, string Name)>> GetFromA(ICollection<string> ids)
        {
            return await Synchronizer.Outlook.GetEntities(ids, e => e.Inner.Subject, e => e.Dispose());
        }

        protected override async Task<IReadOnlyList<(string Id, string Name)>> GetFromB(ICollection<string> ids)
        {
            return await Synchronizer.Server.GetEntities(ids, e => e.Title);
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
