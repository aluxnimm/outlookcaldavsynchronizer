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
using CalDavSynchronizer.IntegrationTests.TestBase;
using NUnit.Framework;

namespace CalDavSynchronizer.IntegrationTests.ChunkedSynchronizationTest
{
  [TestFixture]
  public abstract class ChunkedSynchronizationTestBase<TAId,TBId, TSynchronizer> 
    where TSynchronizer : TestSynchronizerBase
  {
    protected abstract IEqualityComparer<TAId> AIdComparer { get; }
    protected abstract IEqualityComparer<TBId> BIdComparer { get; }
    
    protected abstract IReadOnlyList<(TAId AId, TBId BId)> GetRelations();

    protected abstract Task<TAId> CreateInA(string content);
    protected abstract Task<TBId> CreateInB(string content);

    protected abstract Task UpdateInA(TAId id, string content);
    protected abstract Task UpdateInB(TBId id, string content);

    protected abstract Task<string> GetFromA(TAId id);
    protected abstract Task<string> GetFromB(TBId id);

    protected abstract Task DeleteInA(TAId id);
    protected abstract Task DeleteInB(TBId id);

    protected TSynchronizer Synchronizer { get; private set; }
    protected abstract string ProfileName { get; }
    protected abstract TSynchronizer CreateSynchronizer(Options options);

    protected async Task InitializeSynchronizer(int? chunkSize)
    {
      var options = TestComponentContainer.GetOptions(ProfileName);
      options.ChunkSize = chunkSize ?? 0;
      options.IsChunkedSynchronizationEnabled = chunkSize.HasValue;
      Synchronizer = CreateSynchronizer(options);
      await Synchronizer.Initialize();
    }


    [TestCase(null, 25)]
    [TestCase(3, 25)]
    [TestCase(25, 25)]
    [TestCase(101, 25)]
    [TestCase(1,5)]
    public async Task Test(int? chunkSize, int itemsPerOperation)
    {
      await InitializeSynchronizer(chunkSize);

      // Set maximum alloed open items to chunksize+1, since the synchronizer builds chunks with operations that load an entity and keep it open
      // A delete operation in a Outlook repository will open an entity and release it immediately
      var maximumOpenItemsPerType = chunkSize +1;


      Synchronizer.ClearCache();
      await Synchronizer.ClearEventRepositoriesAndCache();

      await Task.WhenAll(Enumerable.Range(0, itemsPerOperation*3).Select(i => CreateInA($"Item {i}")));
      await Task.WhenAll(Enumerable.Range(0, itemsPerOperation*3).Select(i => CreateInB($"Item {i}")));

      int nextName = itemsPerOperation*3;

      var newAids = new List<TAId>();
      var newBids = new List<TBId>();

      for (var i = 0; i < itemsPerOperation; i++)
      {
        newAids.Add(await CreateInA($"Item {nextName++}"));
        newBids.Add(await CreateInB($"Item {nextName++}"));
      }

      await Synchronizer.SynchronizeAndCheck(
        unchangedA: itemsPerOperation * 3, addedA: itemsPerOperation, changedA: 0, deletedA: 0,
        unchangedB: itemsPerOperation * 3, addedB: itemsPerOperation, changedB: 0, deletedB: 0,
        createA: itemsPerOperation, updateA: 0, deleteA: 0,
        createB: itemsPerOperation, updateB: 0, deleteB: 0,
        ordinalOfReportToCheck:  OrdinalOfReportToCheck,
        maximumOpenItemsPerType: maximumOpenItemsPerType);
      
      var relations = GetRelations();
      Assert.That(relations.Count, Is.EqualTo(itemsPerOperation * 5));

      var updatedA = new HashSet<TAId>(AIdComparer);
      var updatedB = new HashSet<TBId>(BIdComparer);
      var notUpdated = new HashSet<(TAId AId, TBId BId)>(new RelationEqualityComparer(AIdComparer, BIdComparer));
      var deletedA = new HashSet<TAId>(AIdComparer);
      var deletedB = new HashSet<TBId>(BIdComparer);

      for (var i=0; i < relations.Count; i++)
      {
        var relation = relations[i];

        if (i % 5 == 0)
        {
          await UpdateInA(relation.AId, "Upd" + await GetFromA(relation.AId));
          updatedA.Add(relation.AId);
        }
        else if (i % 5 == 1)
        {
          await UpdateInB(relation.BId, "Upd" + await GetFromB(relation.BId));
          updatedB.Add(relation.BId);
        }
        else if (i % 5 == 2)
        {
          await DeleteInA(relation.AId);
          deletedA.Add(relation.AId);
        }
        else if (i % 5 == 3)
        {
          await DeleteInB(relation.BId);
          deletedB.Add(relation.BId);
        }
        else
        {
          notUpdated.Add(relation);
        }
      }

      Assert.That(updatedA.Count, Is.EqualTo(itemsPerOperation));
      Assert.That(updatedB.Count, Is.EqualTo(itemsPerOperation));
      Assert.That(deletedA.Count, Is.EqualTo(itemsPerOperation));
      Assert.That(deletedB.Count, Is.EqualTo(itemsPerOperation));
      Assert.That(notUpdated.Count, Is.EqualTo(itemsPerOperation));

      await Synchronizer.SynchronizeAndCheck(
        unchangedA: itemsPerOperation*3, addedA: 0, changedA: itemsPerOperation, deletedA: itemsPerOperation,
        unchangedB: itemsPerOperation*3, addedB: 0, changedB: itemsPerOperation, deletedB: itemsPerOperation,
        createA: 0, updateA: itemsPerOperation, deleteA: itemsPerOperation,
        createB: 0, updateB: itemsPerOperation, deleteB: itemsPerOperation,
        ordinalOfReportToCheck: OrdinalOfReportToCheck,
        maximumOpenItemsPerType: maximumOpenItemsPerType);

      var relations2 = GetRelations();

      Assert.That(relations.Count, Is.EqualTo(itemsPerOperation * 5));

      foreach (var relation in relations2)
      {
        Assert.That(deletedA.Contains(relation.AId), Is.False);
        Assert.That(deletedB.Contains(relation.BId), Is.False);

        var aContent = await GetFromA(relation.AId);
        var bContent = await GetFromB(relation.BId);


        if (updatedA.Remove(relation.AId))
        {
          Assert.That(aContent, Does.StartWith("Upd"));
          Assert.That(bContent, Does.StartWith("Upd"));
        }
        else if (updatedB.Remove(relation.BId))
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

      Assert.That(updatedA.Count, Is.EqualTo(0));
      Assert.That(updatedB.Count, Is.EqualTo(0));
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
