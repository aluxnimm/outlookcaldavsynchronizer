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
using System.Threading.Tasks;
using CalDavSynchronizer.Contracts;
using NUnit.Framework;

namespace CalDavSynchronizer.IntegrationTests.TestBase
{
    [TestFixture]
    public abstract class GenericTwoWayTestBase<TAId, TBId, TSynchronizer>
        where TSynchronizer : TestSynchronizerBase
    {
        protected abstract IEqualityComparer<TAId> AIdComparer { get; }
        protected abstract IEqualityComparer<TBId> BIdComparer { get; }

        protected abstract IReadOnlyList<(TAId AId, TBId BId)> GetRelations();

        protected abstract Task<IReadOnlyList<TAId>> CreateInA(IEnumerable<string> contents);
        protected abstract Task<IReadOnlyList<TBId>> CreateInB(IEnumerable<string> contents);

        protected abstract Task UpdateInA(IEnumerable<(TAId Id, string Content)> updates);
        protected abstract Task UpdateInB(IEnumerable<(TBId Id, string Content)> updates);

        protected abstract Task<IReadOnlyList<(TAId Id, string Name)>> GetFromA(ICollection<TAId> ids);
        protected abstract Task<IReadOnlyList<(TBId Id, string Name)>> GetFromB(ICollection<TBId> ids);

        protected abstract Task DeleteInA(IEnumerable<TAId> ids);
        protected abstract Task DeleteInB(IEnumerable<TBId> ids);

        protected TSynchronizer Synchronizer { get; private set; }
        protected abstract Options GetOptions();
        protected abstract TSynchronizer CreateSynchronizer(Options options);

        protected async Task InitializeSynchronizer(int? chunkSize, bool useWebDavCollectionSync)
        {
            var options = GetOptions();
            options.ChunkSize = chunkSize ?? 1000; // Some profiles use chunks even if it disabled, because the server forces that
            options.IsChunkedSynchronizationEnabled = chunkSize.HasValue;
            options.UseWebDavCollectionSync = useWebDavCollectionSync;
            Synchronizer = CreateSynchronizer(options);
            await Synchronizer.Initialize();
        }



        public virtual async Task Test(int? chunkSize, int itemsPerOperation, bool useWebDavCollectionSync)
        {
            await InitializeSynchronizer(null, false);
            Synchronizer.ClearCache();
            await Synchronizer.ClearEventRepositoriesAndCache();

            await InitializeSynchronizer(chunkSize, useWebDavCollectionSync);

            // Set maximum alloed open items to chunksize+1, since the synchronizer builds chunks with operations that load an entity and keep it open
            // A delete operation in a Outlook repository will open an entity and release it immediately
            var maximumOpenItemsPerType = chunkSize + 1;

            var matchingItems = Enumerable.Range(1, itemsPerOperation * 3).Select(i => $"Item {i:000}").ToArray();
            var nextName = matchingItems.Length + 1;
            var itemsJustInA = Enumerable.Range(0, itemsPerOperation).Select(i => $"Item {nextName++:000}").ToArray();
            var itemsJustInB = Enumerable.Range(0, itemsPerOperation).Select(i => $"Item {nextName++:000}").ToArray();

            Assert.That(
                (await CreateInA(matchingItems.Union(itemsJustInA))).Count,
                Is.EqualTo(matchingItems.Length + itemsJustInA.Length));

            Assert.That(
                (await CreateInB(matchingItems.Union(itemsJustInB))).Count,
                Is.EqualTo(matchingItems.Length + itemsJustInB.Length));

            await Synchronizer.SynchronizeAndCheck(
                unchangedA: itemsPerOperation * 3, addedA: itemsPerOperation, changedA: 0, deletedA: 0,
                unchangedB: itemsPerOperation * 3, addedB: itemsPerOperation, changedB: 0, deletedB: 0,
                createA: itemsPerOperation, updateA: 0, deleteA: 0,
                createB: itemsPerOperation, updateB: 0, deleteB: 0,
                ordinalOfReportToCheck: OrdinalOfReportToCheck,
                maximumOpenItemsPerType: maximumOpenItemsPerType);

            var relations = GetRelations();
            Assert.That(relations.Count, Is.EqualTo(itemsPerOperation * 5));
            
            var aUpdates = new HashSet<TAId>(AIdComparer);
            var bUpdates = new HashSet<TBId>(BIdComparer);
            var aDeletes = new HashSet<TAId>(AIdComparer);
            var bDeletes = new HashSet<TBId>(BIdComparer);
            var notUpdated = new HashSet<(TAId AId, TBId BId)>(new RelationEqualityComparer(AIdComparer, BIdComparer));

            for (var i = 0; i < relations.Count; i++)
            {
                var relation = relations[i];

                if (i % 5 == 0)
                {
                    aUpdates.Add(relation.AId);
                }
                else if (i % 5 == 1)
                {
                    bUpdates.Add(relation.BId);
                }
                else if (i % 5 == 2)
                {
                    aDeletes.Add((relation.AId));
                }
                else if (i % 5 == 3)
                {
                    bDeletes.Add(relation.BId);
                }
                else
                {
                    notUpdated.Add(relation);
                }
            }
            
            await UpdateInA((await GetFromA(aUpdates)).Select(u => (u.Item1,"Upd" + u.Name)));
            await UpdateInB((await GetFromB(bUpdates)).Select(u => (u.Item1, "Upd" + u.Name)));
            await DeleteInA(aDeletes);
            await DeleteInB(bDeletes);
            
            Assert.That(aUpdates.Count, Is.EqualTo(itemsPerOperation));
            Assert.That(bUpdates.Count, Is.EqualTo(itemsPerOperation));
            Assert.That(aDeletes.Count, Is.EqualTo(itemsPerOperation));
            Assert.That(bDeletes.Count, Is.EqualTo(itemsPerOperation));
            Assert.That(notUpdated.Count, Is.EqualTo(itemsPerOperation));

            await Synchronizer.SynchronizeAndCheck(
                unchangedA: itemsPerOperation * 3, addedA: 0, changedA: itemsPerOperation, deletedA: itemsPerOperation,
                unchangedB: itemsPerOperation * 3, addedB: 0, changedB: itemsPerOperation, deletedB: itemsPerOperation,
                createA: 0, updateA: itemsPerOperation, deleteA: itemsPerOperation,
                createB: 0, updateB: itemsPerOperation, deleteB: itemsPerOperation,
                ordinalOfReportToCheck: OrdinalOfReportToCheck,
                maximumOpenItemsPerType: maximumOpenItemsPerType);

            var relations2 = GetRelations();

            Assert.That(relations2.Count, Is.EqualTo(itemsPerOperation * 3));

            var aContentsById = (await GetFromA(relations2.Select(r => r.AId).ToArray())).ToDictionary(e => e.Id, e => e.Name, AIdComparer);
            var bContentsById = (await GetFromB(relations2.Select(r => r.BId).ToArray())).ToDictionary(e => e.Id, e => e.Name, BIdComparer);

            foreach (var relation in relations2)
            {
                Assert.That(aDeletes.Contains(relation.AId), Is.False);
                Assert.That(bDeletes.Contains(relation.BId), Is.False);

                var aContent = aContentsById[relation.AId];
                var bContent = bContentsById[relation.BId];
                
                if (aUpdates.Remove(relation.AId))
                {
                    Assert.That(aContent, Does.StartWith("Upd"));
                    Assert.That(bContent, Does.StartWith("Upd"));
                }
                else if (bUpdates.Remove(relation.BId))
                {
                    Assert.That(aContent, Does.StartWith("Upd"));
                    Assert.That(bContent, Does.StartWith("Upd"));
                }
                else if (notUpdated.Remove(relation))
                {
                    Assert.That(aContent, Does.Not.StartWith("Upd"));
                    Assert.That(bContent, Does.Not.StartWith("Upd"));
                }
            }

            Assert.That(aUpdates.Count, Is.EqualTo(0));
            Assert.That(bUpdates.Count, Is.EqualTo(0));
            Assert.That(notUpdated.Count, Is.EqualTo(0));
        }

        protected virtual int? OrdinalOfReportToCheck => null;

        class RelationEqualityComparer : IEqualityComparer<(TAId AId, TBId BId)>
        {
            private readonly IEqualityComparer<TAId> _aIdComparer;
            private readonly IEqualityComparer<TBId> _bIdComparer;

            public RelationEqualityComparer(IEqualityComparer<TAId> aIdComparer, IEqualityComparer<TBId> bIdComparer)
            {
                _aIdComparer = aIdComparer ?? throw new ArgumentNullException(nameof(aIdComparer));
                _bIdComparer = bIdComparer ?? throw new ArgumentNullException(nameof(bIdComparer));
            }

            public bool Equals((TAId AId, TBId BId) x, (TAId AId, TBId BId) y)
            {
                return _aIdComparer.Equals(x.AId, y.AId) && _bIdComparer.Equals(x.BId, y.BId);
            }

            public int GetHashCode((TAId AId, TBId BId) obj)
            {
                return _aIdComparer.GetHashCode(obj.AId) * 397 ^ _bIdComparer.GetHashCode(obj.BId);
            }
        }
    }
}
