using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenSync.EntityRelationManagement;
using GenSync.Synchronization.States;
using NUnit.Framework;
using Rhino.Mocks;

namespace GenSync.UnitTests.Synchronization
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
          .Expect (r => r.GetVersions())
          .IgnoreArguments()
          .Return (
              Task.FromResult<IReadOnlyList<EntityIdWithVersion<string, string>>> (
                  new[] { EntityIdWithVersion.Create ("A1", "v1"), EntityIdWithVersion.Create ("a1", "v3") }));

      builder.BtypeRepository
          .Expect (r => r.GetVersions())
          .IgnoreArguments()
          .Return (
              Task.FromResult<IReadOnlyList<EntityIdWithVersion<string, string>>> (
                  new[] { EntityIdWithVersion.Create ("b1", "v2") }));


      Task<IReadOnlyList<EntityWithVersion<string, string>>> aTypeLoadTask = new Task<IReadOnlyList<EntityWithVersion<string, string>>> (
          () => new List<EntityWithVersion<string, string>> { EntityWithVersion.Create ("A1", "AAAA"), EntityWithVersion.Create ("a1", "____") });
      aTypeLoadTask.RunSynchronously();
      builder.AtypeRepository
          .Expect (r => r.Get (Arg<ICollection<string>>.Matches (c => c.Count == 1 && c.First() == "A1")))
          .Return (aTypeLoadTask);

      Task<IReadOnlyList<EntityWithVersion<string, string>>> bTypeLoadTask = new Task<IReadOnlyList<EntityWithVersion<string, string>>> (
          () => new List<EntityWithVersion<string, string>> { EntityWithVersion.Create ("b1", "BBBB"), });
      bTypeLoadTask.RunSynchronously();
      builder.BtypeRepository
          .Expect (r => r.Get (Arg<ICollection<string>>.Matches (c => c.Count == 1 && c.First() == "b1")))
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
      await synchronizer.Synchronize();

      builder.EntityRelationDataAccess.AssertWasCalled (
          c => c.SaveEntityRelationData (Arg<List<IEntityRelationData<string, string, string, string>>>.Matches (l => l.Count == 1 && l[0] == knownData)));
    }
  }
}