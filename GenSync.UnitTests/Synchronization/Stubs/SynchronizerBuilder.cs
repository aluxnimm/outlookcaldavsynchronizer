using System;
using System.Collections.Generic;
using GenSync.EntityMapping;
using GenSync.EntityRelationManagement;
using GenSync.EntityRepositories;
using GenSync.InitialEntityMatching;
using GenSync.ProgressReport;
using GenSync.Synchronization;
using GenSync.Synchronization.StateCreationStrategies;
using Rhino.Mocks;

namespace GenSync.UnitTests.Synchronization.Stubs
{
  internal class SynchronizerBuilder
  {
    public SynchronizerBuilder ()
    {
      AtypeRepository = MockRepository.GenerateMock<IEntityRepository<string, string, string>>();
      BtypeRepository = MockRepository.GenerateMock<IEntityRepository<string, string, string>>();
      EntityMapper = MockRepository.GenerateMock<IEntityMapper<string, string>>();
      EntityRelationDataAccess = MockRepository.GenerateMock<IEntityRelationDataAccess<string, string, string, string>>();
      EntityRelationDataFactory = MockRepository.GenerateMock<IEntityRelationDataFactory<string, string, string, string>>();
      InitialEntityMatcher = MockRepository.GenerateMock<IInitialEntityMatcher<string, string, string, string, string, string>>();
      InitialSyncStateCreationStrategy = MockRepository.GenerateMock<IInitialSyncStateCreationStrategy<string, string, string, string, string, string>>();
    }

    public Synchronizer<string, string, string, string, string, string> Build ()
    {
      return new Synchronizer<string, string, string, string, string, string> (
          AtypeRepository,
          BtypeRepository,
          InitialSyncStateCreationStrategy,
          EntityRelationDataAccess,
          EntityRelationDataFactory,
          InitialEntityMatcher,
          AtypeIdComparer,
          BtypeIdComparer,
          new NullTotalProgressFactory(),
          MockRepository.GenerateMock<IExceptionLogger>());
    }

    public IEntityRelationDataAccess<string, string, string, string> EntityRelationDataAccess { get; set; }

    public IEqualityComparer<string> BtypeIdComparer { get; set; }

    public IEqualityComparer<string> AtypeIdComparer { get; set; }

    public IInitialSyncStateCreationStrategy<string, string, string, string, string, string> InitialSyncStateCreationStrategy { get; set; }

    public IInitialEntityMatcher<string, string, string, string, string, string> InitialEntityMatcher { get; set; }

    public IEntityRelationDataFactory<string, string, string, string> EntityRelationDataFactory { get; set; }

    public IEntityMapper<string, string> EntityMapper { get; set; }

    public IEntityRepository<string, string, string> BtypeRepository { get; set; }

    public IEntityRepository<string, string, string> AtypeRepository { get; set; }
  }
}