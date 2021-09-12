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
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation.Contacts;
using CalDavSynchronizer.Implementation.Contacts.VCardTypeSwitch;
using GenSync;
using GenSync.EntityRepositories;
using GenSync.Logging;
using NUnit.Framework;
using Rhino.Mocks;
using Thought.vCards;

namespace CalDavSynchronizer.UnitTest.Implementation.Contacts.VCardTypeSwitch
{
    [TestFixture]
    public class VCardTypeDetectorFixture
    {
        private VCardTypeDetector _detector;
        private IReadOnlyEntityRepository<WebResourceName, string, vCard, ICardDavRepositoryLogger> _readOnlyEntityRepository;
        private DummyDataAccess _dummyDataAccess;

        [SetUp]
        public void SetUp()
        {
            _readOnlyEntityRepository = MockRepository.GenerateStrictMock<IReadOnlyEntityRepository<WebResourceName, string, vCard, ICardDavRepositoryLogger>>();
            _dummyDataAccess = new DummyDataAccess();
            _detector = new VCardTypeDetector(_readOnlyEntityRepository, new VCardTypeCache(_dummyDataAccess));
        }


        [Test]
        public async Task GetVCardTypesAndCleanupCache_AllIdsInCache_DoesntAccessRepository()
        {
            await GetVCardTypesAndCleanupCacheTest(
                initialCacheEntries: new[]
                {
                    Tuple.Create("id1", VCardType.Contact),
                    Tuple.Create("id2", VCardType.Group),
                    Tuple.Create("id3", VCardType.Contact)
                },
                idsToQuery: new[] {"id2", "id3", "id1"},
                expectedResult: new[]
                {
                    Tuple.Create("id1", VCardType.Contact),
                    Tuple.Create("id2", VCardType.Group),
                    Tuple.Create("id3", VCardType.Contact)
                },
                expectedFinalCacheEntries: new[]
                {
                    Tuple.Create("id1", VCardType.Contact),
                    Tuple.Create("id2", VCardType.Group),
                    Tuple.Create("id3", VCardType.Contact)
                });
        }

        [Test]
        public async Task GetVCardTypesAndCleanupCache_AllIdsInCache_DoesntAccessRepositoryAndRemovesUnusedEntries()
        {
            await GetVCardTypesAndCleanupCacheTest(
                initialCacheEntries: new[]
                {
                    Tuple.Create("id1", VCardType.Contact),
                    Tuple.Create("id2", VCardType.Group),
                    Tuple.Create("id66", VCardType.Group),
                    Tuple.Create("id77", VCardType.Contact),
                    Tuple.Create("id3", VCardType.Contact)
                },
                idsToQuery: new[] {"id2", "id3", "id1"},
                expectedResult: new[]
                {
                    Tuple.Create("id1", VCardType.Contact),
                    Tuple.Create("id2", VCardType.Group),
                    Tuple.Create("id3", VCardType.Contact)
                },
                expectedFinalCacheEntries: new[]
                {
                    Tuple.Create("id1", VCardType.Contact),
                    Tuple.Create("id2", VCardType.Group),
                    Tuple.Create("id3", VCardType.Contact)
                });
        }

        [Test]
        public async Task GetVCardTypesAndCleanupCache_AllIdsInCache_LoadsMissingFromRepositoryAndRemovesUnusedEntries()
        {
            _readOnlyEntityRepository
                .Expect(r => r.Get(
                    new[]
                    {
                        new WebResourceName("id5"),
                        new WebResourceName("id4")
                    },
                    NullLoadEntityLogger.Instance,
                    NullCardDavRepositoryLogger.Instance))
                .Return(Task.FromResult<IEnumerable<EntityWithId<WebResourceName, vCard>>>(new[]
                {
                    EntityWithId.Create(new WebResourceName("id5"), new vCard {Kind = vCardKindType.Group}),
                    EntityWithId.Create(new WebResourceName("id4"), new vCard {Kind = vCardKindType.Location})
                }));

            await GetVCardTypesAndCleanupCacheTest(
                initialCacheEntries: new[]
                {
                    Tuple.Create("id1", VCardType.Contact),
                    Tuple.Create("id2", VCardType.Group),
                    Tuple.Create("id66", VCardType.Group),
                    Tuple.Create("id77", VCardType.Contact),
                    Tuple.Create("id3", VCardType.Contact)
                },
                idsToQuery: new[] {"id5", "id2", "id3", "id4", "id1"},
                expectedResult: new[]
                {
                    Tuple.Create("id1", VCardType.Contact),
                    Tuple.Create("id2", VCardType.Group),
                    Tuple.Create("id3", VCardType.Contact),
                    Tuple.Create("id4", VCardType.Contact),
                    Tuple.Create("id5", VCardType.Group)
                },
                expectedFinalCacheEntries: new[]
                {
                    Tuple.Create("id1", VCardType.Contact),
                    Tuple.Create("id2", VCardType.Group),
                    Tuple.Create("id3", VCardType.Contact),
                    Tuple.Create("id4", VCardType.Contact),
                    Tuple.Create("id5", VCardType.Group)
                });

            _readOnlyEntityRepository.VerifyAllExpectations();
        }


        public async Task GetVCardTypesAndCleanupCacheTest(
            IReadOnlyCollection<Tuple<string, VCardType>> initialCacheEntries,
            IReadOnlyCollection<string> idsToQuery,
            IReadOnlyCollection<Tuple<string, VCardType>> expectedResult,
            IReadOnlyCollection<Tuple<string, VCardType>> expectedFinalCacheEntries
        )
        {
            _dummyDataAccess.Entries = initialCacheEntries.Select(v => new VCardEntry {Id = new WebResourceName(v.Item1), Type = v.Item2}).ToArray();

            var result = await _detector.GetVCardTypesAndCleanupCache(idsToQuery.Select(i => new Entity(i)));

            Assert.That(result.Count(), Is.EqualTo(expectedResult.Count));
            foreach (var expectation in expectedResult)
            {
                Assert.That(result.Single(r => r.Id.Id.ToString() == expectation.Item1).Type, Is.EqualTo(expectation.Item2));
            }

            Assert.That(_dummyDataAccess.Entries.Length, Is.EqualTo(expectedFinalCacheEntries.Count));
            foreach (var expectation in expectedFinalCacheEntries)
            {
                Assert.That(_dummyDataAccess.Entries.Single(r => r.Id.Id.ToString() == expectation.Item1).Type, Is.EqualTo(expectation.Item2));
            }
        }


        class Entity : IEntity<WebResourceName>
        {
            public Entity(string id)
            {
                if (id == null) throw new ArgumentNullException(nameof(id));
                Id = new WebResourceName(id);
            }

            public WebResourceName Id { get; }
        }

        class DummyDataAccess : IVCardTypeCacheDataAccess
        {
            public VCardEntry[] Load()
            {
                return Entries;
            }

            public VCardEntry[] Entries { get; set; }

            public void Save(VCardEntry[] value)
            {
                Entries = value;
            }
        }
    }
}