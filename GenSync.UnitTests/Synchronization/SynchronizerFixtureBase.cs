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
using GenSync.EntityRepositories;
using GenSync.InitialEntityMatching;
using GenSync.Logging;
using GenSync.ProgressReport;
using GenSync.Synchronization;
using GenSync.Synchronization.StateCreationStrategies;
using GenSync.Synchronization.StateCreationStrategies.ConflictStrategies;
using GenSync.Synchronization.StateFactories;
using GenSync.UnitTests.Synchronization.Stubs;
using NUnit.Framework;
using Rhino.Mocks;

namespace GenSync.UnitTests.Synchronization
{
  internal abstract class SynchronizerFixtureBase
  {
    protected TestRepository _localRepository;
    protected TestRepository _serverRepository;
    private IEntityRelationDataFactory<Identifier, int, Identifier, int> _entityRelationDataFactory;
    protected IEntitySyncStateFactory<Identifier, int, string, Identifier, int, string, int> _factory;
    private List<EntityRelationData> _entityRelationData;
    private IEntityRelationDataAccess<Identifier, int, Identifier, int> _entityRelationDataAccess;

    [SetUp]
    public void Setup ()
    {
      _entityRelationData = new List<EntityRelationData>();
      _localRepository = new TestRepository ("l");
      _serverRepository = new TestRepository ("s");
      _entityRelationDataFactory = new EntityRelationDataFactory();

      _factory = new EntitySyncStateFactory<Identifier, int, string, Identifier, int, string, int> (
          new Mapper(),
          _entityRelationDataFactory,
          MockRepository.GenerateMock<IExceptionLogger>()
          );

      _entityRelationDataAccess = MockRepository.GenerateStub<IEntityRelationDataAccess<Identifier, int, Identifier, int>>();
      _entityRelationDataAccess.Stub (d => d.LoadEntityRelationData()).WhenCalled (a => a.ReturnValue = _entityRelationData.ToArray());
      _entityRelationDataAccess
          .Stub (d => d.SaveEntityRelationData (null))
          .IgnoreArguments()
          .WhenCalled (a => { _entityRelationData = ((List<IEntityRelationData<Identifier, int, Identifier, int>>) a.Arguments[0]).Cast<EntityRelationData>().ToList(); });
    }

    protected Exception SynchronizeTwoWay (
      GenericConflictResolution winner,
      List<IEntityRelationData<Identifier, int, Identifier, int>> matchingEntities = null)
    {
      var strategy = CreateTwoWaySyncStrategy (winner);
      return SynchronizeInternal (strategy, matchingEntities);
    }

    protected void SynchronizePartialTwoWay (
        GenericConflictResolution winner,
        IIdWithHints<Identifier, int>[] aEntitesToSynchronize,
        IIdWithHints<Identifier, int>[] bEntitesToSynchronize)
    {
      var strategy = CreateTwoWaySyncStrategy (winner);
      PartialSynchronizeInternal (strategy, aEntitesToSynchronize, bEntitesToSynchronize);
    }

    private TwoWayInitialSyncStateCreationStrategy<Identifier, int, string, Identifier, int, string, int> CreateTwoWaySyncStrategy (GenericConflictResolution winner)
    {
      IConflictInitialSyncStateCreationStrategy<Identifier, int, string, Identifier, int, string, int> conflictInitialStrategy;
      if (winner == GenericConflictResolution.AWins)
        conflictInitialStrategy = new ConflictInitialSyncStateCreationStrategyAWins<Identifier, int, string, Identifier, int, string, int> (_factory);
      else
        conflictInitialStrategy = new ConflictInitialSyncStateCreationStrategyBWins<Identifier, int, string, Identifier, int, string, int> (_factory);

      var strategy = new TwoWayInitialSyncStateCreationStrategy<Identifier, int, string, Identifier, int, string, int> (_factory, conflictInitialStrategy);
      return strategy;
    }

    protected void SynchronizeOneWay ()
    {
      SynchronizeInternal (
          new OneWayInitialSyncStateCreationStrategy_AToB<Identifier, int, string, Identifier, int, string, int> (_factory, OneWaySyncMode.Replicate)
          );
    }

    private Exception SynchronizeInternal (
      IInitialSyncStateCreationStrategy<Identifier, int, string, Identifier, int, string, int> strategy,
      List<IEntityRelationData<Identifier, int, Identifier, int>> matchingEntities = null)
    {
      var synchronizer = CreateSynchronizer (strategy,matchingEntities);

      try
      {
        synchronizer.Synchronize(NullSynchronizationLogger.Instance, 0).Wait();
        return null;
      }
      catch (AggregateException x)
      {
        return x.InnerException;
      }

    }

    private async Task SynchronizeInternalAsync (
      IInitialSyncStateCreationStrategy<Identifier, int, string, Identifier, int, string, int> strategy,
      List<IEntityRelationData<Identifier, int, Identifier, int>> matchingEntities = null)
    {
      var synchronizer = CreateSynchronizer (strategy, matchingEntities);
      await synchronizer.Synchronize (NullSynchronizationLogger.Instance, 0);
    }


    private void PartialSynchronizeInternal(
      IInitialSyncStateCreationStrategy<Identifier, int, string, Identifier, int, string, int> strategy,
      IIdWithHints<Identifier, int>[] aEntitesToSynchronize = null,
      IIdWithHints<Identifier, int>[] bEntitesToSynchronize = null)
    {
      var synchronizer = CreateSynchronizer(strategy);

      synchronizer.SynchronizePartial(
        aEntitesToSynchronize ?? new IIdWithHints<Identifier, int>[] {},
        bEntitesToSynchronize ?? new IIdWithHints<Identifier, int>[] {},
        NullSynchronizationLogger.Instance,
        () => Task.FromResult(0),
        c => Task.FromResult(0)).Wait();
    }

    private Synchronizer<Identifier, int, string, Identifier, int, string, int> CreateSynchronizer (
      IInitialSyncStateCreationStrategy<Identifier, int, string, Identifier, int, string, int> strategy,
      List<IEntityRelationData<Identifier, int, Identifier, int>> matchingEntities = null)
    {
      var initialEntityMatcherStub = MockRepository.GenerateStub<IInitialEntityMatcher<Identifier, int, string, Identifier, int, string>>();
      initialEntityMatcherStub
          .Stub (_ => _.FindMatchingEntities (
              Arg<IEntityRelationDataFactory<Identifier, int, Identifier, int>>.Is.NotNull,
              Arg<IReadOnlyDictionary<Identifier, string>>.Is.NotNull,
              Arg<IReadOnlyDictionary<Identifier, string>>.Is.NotNull,
              Arg<IReadOnlyDictionary<Identifier, int>>.Is.NotNull,
              Arg<IReadOnlyDictionary<Identifier, int>>.Is.NotNull))
          .Return (matchingEntities ?? new List<IEntityRelationData<Identifier, int, Identifier, int>>());

      var atypeWriteRepository = BatchEntityRepositoryAdapter.Create (_localRepository);
      var btypeWriteRepository = BatchEntityRepositoryAdapter.Create (_serverRepository);

      return new Synchronizer<Identifier, int, string, Identifier, int, string, int> (
          _localRepository,
          _serverRepository,
          atypeWriteRepository,
          btypeWriteRepository,
          strategy,
          _entityRelationDataAccess,
          _entityRelationDataFactory,
          initialEntityMatcherStub,
          IdentifierEqualityComparer.Instance,
          IdentifierEqualityComparer.Instance,
          NullTotalProgressFactory.Instance,
          EqualityComparer<int>.Default,
          EqualityComparer<int>.Default,
          MockRepository.GenerateMock<IEntitySyncStateFactory<Identifier, int, string, Identifier, int, string, int>> ());
    }

    protected void ExecuteMultipleTimes (Action a)
    {
      a();
      a();
      a();
      a();
    }

    public void AssertServerCount (int i)
    {
      Assert.AreEqual (i, _serverRepository.Count);
    }

    public void AssertLocalCount (int i)
    {
      Assert.AreEqual (i, _localRepository.Count);
    }

    public void AssertServer (string id, int version, string content)
    {
      Assert.AreEqual (Tuple.Create (version, content), _serverRepository[id]);
    }

    public void AssertLocal (string id, int version, string content)
    {
      Assert.AreEqual (Tuple.Create (version, content), _localRepository[id]);
    }

    public void AssertLocal (int version, string content)
    {
      CollectionAssert.Contains (_localRepository.EntityVersionAndContentById.Values, Tuple.Create (version, content));
    }

    public void AssertServer (int version, string content)
    {
      CollectionAssert.Contains (_serverRepository.EntityVersionAndContentById.Values, Tuple.Create (version, content));
    }
    
    public async Task InitializeWithTwoEvents ()
    {
      await InitializeWithEvents (2);
    }

    public async Task InitializeWithEvents (int count)
    {

      for (var i = 1; i <= count; i++)
      {
        var local = await _localRepository.Create (v => Task.FromResult ($"Item {i}"), NullSynchronizationContextFactory.Instance.Create ().Result);
        var server = await _serverRepository.Create (v => Task.FromResult ($"Item {i}"), NullSynchronizationContextFactory.Instance.Create ().Result);

        _entityRelationData.Add (new EntityRelationData (
          local.Id,
          local.Version,
          server.Id,
          server.Version
        ));
      }

      AssertRelations (
        Enumerable.Range (1, count).Select (i => new EntityRelationData ($"l{i}", 0, $"s{i}", 0)).ToArray ());
    }

    public void AssertRelations (params EntityRelationData[] relations)
    {
      Assert.That (_entityRelationData.Count, Is.EqualTo (relations.Length));

      foreach (var relation in relations)
      {
        if (_entityRelationData.SingleOrDefault (r =>
           IdentifierEqualityComparer.Instance.Equals (r.AtypeId, relation.AtypeId) &&
           IdentifierEqualityComparer.Instance.Equals (r.BtypeId, relation.BtypeId) &&
           r.AtypeVersion == relation.AtypeVersion &&
           r.BtypeVersion == relation.BtypeVersion) == null)
        {
          Assert.Fail ("Relations mismatch");
        }
      }
    }
  }
}