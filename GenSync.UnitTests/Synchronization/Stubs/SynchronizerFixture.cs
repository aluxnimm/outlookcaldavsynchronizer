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
using GenSync.EntityRelationManagement;
using GenSync.Logging;
using GenSync.Synchronization.States;
using NUnit.Framework;
using Rhino.Mocks;

namespace GenSync.UnitTests.Synchronization.Stubs
{
  [TestFixture]
  public class SynchronizerFixture
  {
    [Test]
    public async Task Synchronize_GetVersionsAndGetReturnDuplicateEntries_RemovesDuplicates ()
    {
      var builder = new SynchronizerBuilder();

      builder.AtypeIdComparer = StringComparer.InvariantCultureIgnoreCase;

      builder.AtypeRepository
          .Expect (r => r.GetAllVersions (new string[] { }))
          .IgnoreArguments()
          .Return (
              Task.FromResult<IReadOnlyList<EntityVersion<string, string>>> (
                  new[] { EntityVersion.Create ("A1", "v1"), EntityVersion.Create ("a1", "v3") }));

      builder.BtypeRepository
          .Expect (r => r.GetAllVersions (new string[] { }))
          .IgnoreArguments()
          .Return (
              Task.FromResult<IReadOnlyList<EntityVersion<string, string>>> (
                  new[] { EntityVersion.Create ("b1", "v2") }));


      Task<IReadOnlyList<EntityWithId<string, string>>> aTypeLoadTask = new Task<IReadOnlyList<EntityWithId<string, string>>> (
          () => new List<EntityWithId<string, string>> { EntityWithId.Create ("A1", "AAAA"), EntityWithId.Create ("a1", "____") });
      aTypeLoadTask.RunSynchronously();
      builder.AtypeRepository
          .Expect (r => r.Get (Arg<ICollection<string>>.Matches (c => c.Count == 1 && c.First() == "A1"), Arg<ISynchronizationLogger>.Is.NotNull))
          .Return (aTypeLoadTask);

      Task<IReadOnlyList<EntityWithId<string, string>>> bTypeLoadTask = new Task<IReadOnlyList<EntityWithId<string, string>>> (
          () => new List<EntityWithId<string, string>> { EntityWithId.Create ("b1", "BBBB"), });
      bTypeLoadTask.RunSynchronously();
      builder.BtypeRepository
          .Expect (r => r.Get (Arg<ICollection<string>>.Matches (c => c.Count == 1 && c.First() == "b1"), Arg<ISynchronizationLogger>.Is.NotNull))
          .Return (bTypeLoadTask);


      var knownData = new EntityRelationData<string, string, string, string> ("A1", "v1", "b1", "v2");
      builder.InitialEntityMatcher
          .Expect (m => m.FindMatchingEntities (null, null, null, null, null))
          .IgnoreArguments()
          .Return (new List<IEntityRelationData<string, string, string, string>> { knownData });

      builder.InitialSyncStateCreationStrategy
          .Expect (s => s.CreateFor_Unchanged_Unchanged (knownData))
          .Return (new DoNothing<string, string, string, string, string, string> (knownData));

      var synchronizer = builder.Build();
      await synchronizer.SynchronizeNoThrow (NullSynchronizationLogger.Instance);

      builder.EntityRelationDataAccess.AssertWasCalled (
          c => c.SaveEntityRelationData (Arg<List<IEntityRelationData<string, string, string, string>>>.Matches (l => l.Count == 1 && l[0] == knownData)));
    }
  }
}