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
using CalDavSynchronizer.ChangeWatching;
using CalDavSynchronizer.Synchronization;
using GenSync;
using GenSync.Logging;
using GenSync.Synchronization;
using NUnit.Framework;

namespace CalDavSynchronizer.UnitTest.Synchronization
{
    [TestFixture]
    public class OutlookSynchronizerFixture
    {
        [Test]
        public async Task SynchronizePartial_AppliesIdNormalizer()
        {
            var innerSynchronizer = new CapturingPartialSynchronizer();
            IGetVersionsLogger normalizerLogger = null;

            var synchronizer =
                new OutlookSynchronizer<string, string>(
                    innerSynchronizer,
                    (ids, logger) =>
                    {
                        normalizerLogger = logger;
                        return ids
                            .Select(_ =>
                                IdWithHints.Create<string, DateTime>(
                                    "master-entry-id",
                                    (DateTime?) null,
                                    null))
                            .ToList();
                    });

            await synchronizer.SynchronizePartial(
                new IOutlookId[]
                {
                    new GenericId(
                        "dead-occurrence-entry-id",
                        new DateTime(2026, 8, 25, 12, 0, 0, DateTimeKind.Utc),
                        false)
                },
                NullSynchronizationLogger.Instance);

            var normalizedId = innerSynchronizer.AIds.Single();
            Assert.That(normalizedId.Id, Is.EqualTo("master-entry-id"));
            Assert.That(normalizedId.IsVersionHintSpecified, Is.False);
            Assert.That(normalizedId.WasDeletedHint, Is.Null);
            Assert.That(normalizerLogger, Is.SameAs(NullGetVersionsLogger.Instance));
        }

        [Test]
        public async Task SynchronizePartial_WithoutNormalizerPassesOriginalId()
        {
            var innerSynchronizer = new CapturingPartialSynchronizer();
            var synchronizer = new OutlookSynchronizer<string, string>(innerSynchronizer);
            var version = new DateTime(2026, 8, 25, 12, 0, 0, DateTimeKind.Utc);

            await synchronizer.SynchronizePartial(
                new IOutlookId[] {new GenericId("entry-id", version, false)},
                NullSynchronizationLogger.Instance);

            var id = innerSynchronizer.AIds.Single();
            Assert.That(id.Id, Is.EqualTo("entry-id"));
            Assert.That(id.IsVersionHintSpecified, Is.True);
            Assert.That(id.VersionHint, Is.EqualTo(version));
            Assert.That(id.WasDeletedHint, Is.False);
        }

        private sealed class CapturingPartialSynchronizer
            : IPartialSynchronizer<string, DateTime, string, string>
        {
            public IReadOnlyList<IIdWithHints<string, DateTime>> AIds { get; private set; }

            public Task Synchronize(ISynchronizationLogger logger)
            {
                return Task.FromResult(0);
            }

            public Task SynchronizePartial(
                IEnumerable<IIdWithHints<string, DateTime>> aIds,
                IEnumerable<IIdWithHints<string, string>> bIds,
                ISynchronizationLogger logger)
            {
                AIds = aIds.ToList();
                return Task.FromResult(0);
            }
        }
    }
}
