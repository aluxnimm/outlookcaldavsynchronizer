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
    protected IEntitySyncStateFactory<Identifier, int, string, Identifier, int, string> _factory;
    private List<EntityRelationData> _entityRelationData;
    private IEntityRelationDataAccess<Identifier, int, Identifier, int> _entityRelationDataAccess;

    [SetUp]
    public void Setup ()
    {
      _entityRelationData = new List<EntityRelationData>();
      _localRepository = new TestRepository ("l");
      _serverRepository = new TestRepository ("s");
      _entityRelationDataFactory = new EntityRelationDataFactory();

      _factory = new EntitySyncStateFactory<Identifier, int, string, Identifier, int, string> (
          new Mapper(),
          _localRepository,
          _serverRepository,
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

    protected void SynchronizeTwoWay (GenericConflictResolution winner)
    {
      var strategy = CreateTwoWaySyncStrategy (winner);
      SynchronizeInternal (strategy);
    }

    protected void SynchronizePartialTwoWay (
        GenericConflictResolution winner,
        Identifier[] aEntitesToSynchronize,
        Identifier[] bEntitesToSynchronize)
    {
      var strategy = CreateTwoWaySyncStrategy (winner);
      PartialSynchronizeInternal (strategy, aEntitesToSynchronize, bEntitesToSynchronize);
    }

    private TwoWayInitialSyncStateCreationStrategy<Identifier, int, string, Identifier, int, string> CreateTwoWaySyncStrategy (GenericConflictResolution winner)
    {
      IConflictInitialSyncStateCreationStrategy<Identifier, int, string, Identifier, int, string> conflictInitialStrategy;
      if (winner == GenericConflictResolution.AWins)
        conflictInitialStrategy = new ConflictInitialSyncStateCreationStrategyAWins<Identifier, int, string, Identifier, int, string> (_factory);
      else
        conflictInitialStrategy = new ConflictInitialSyncStateCreationStrategyBWins<Identifier, int, string, Identifier, int, string> (_factory);

      var strategy = new TwoWayInitialSyncStateCreationStrategy<Identifier, int, string, Identifier, int, string> (_factory, conflictInitialStrategy);
      return strategy;
    }

    protected void SynchronizeOneWay ()
    {
      SynchronizeInternal (
          new OneWayInitialSyncStateCreationStrategy_AToB<Identifier, int, string, Identifier, int, string> (_factory, OneWaySyncMode.Replicate)
          );
    }

    private void SynchronizeInternal (IInitialSyncStateCreationStrategy<Identifier, int, string, Identifier, int, string> strategy)
    {
      var synchronizer = CreateSynchronizer (strategy);

      synchronizer.Synchronize (NullSynchronizationLogger.Instance).Wait();
    }

    private void PartialSynchronizeInternal (
        IInitialSyncStateCreationStrategy<Identifier, int, string, Identifier, int, string> strategy,
        Identifier[] aEntitesToSynchronize = null,
        Identifier[] bEntitesToSynchronize = null)
    {
      var synchronizer = CreateSynchronizer (strategy);

      synchronizer.SynchronizePartial (
          aEntitesToSynchronize ?? new Identifier[] { },
          bEntitesToSynchronize ?? new Identifier[] { },
          NullSynchronizationLogger.Instance).Wait();
    }

    private Synchronizer<Identifier, int, string, Identifier, int, string> CreateSynchronizer (IInitialSyncStateCreationStrategy<Identifier, int, string, Identifier, int, string> strategy)
    {
      return new Synchronizer<Identifier, int, string, Identifier, int, string> (
          _localRepository,
          _serverRepository,
          strategy,
          _entityRelationDataAccess,
          _entityRelationDataFactory,
          MockRepository.GenerateStub<IInitialEntityMatcher<string, Identifier, int, string, Identifier, int>>(),
          IdentifierEqualityComparer.Instance,
          IdentifierEqualityComparer.Instance,
          NullTotalProgressFactory.Instance,
          MockRepository.GenerateMock<IExceptionLogger>());
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
      var v1 = await _localRepository.Create (v => "Item 1");
      var v2 = await _localRepository.Create (v => "Item 2");

      var v3 = await _serverRepository.Create (v => "Item 1");
      var v4 = await _serverRepository.Create (v => "Item 2");

      _entityRelationData.Add (new EntityRelationData (
          v1.Id,
          v1.Version,
          v3.Id,
          v3.Version
          ));

      _entityRelationData.Add (new EntityRelationData (
          v2.Id,
          v2.Version,
          v4.Id,
          v4.Version
          ));
    }
  }
}