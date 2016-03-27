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
using System.Reflection;
using System.Threading.Tasks;
using GenSync.EntityRelationManagement;
using GenSync.EntityRepositories;
using GenSync.Logging;
using log4net;

namespace GenSync.Synchronization.States
{
  internal class CreateInA<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> :
      StateBase<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity>
  {
    // ReSharper disable once StaticFieldInGenericType
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private readonly TBtypeEntityId _bId;
    private readonly TBtypeEntityVersion _bVersion;
    private TBtypeEntity _bEntity;
    private IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> _nextStateAfterJobExecution;

    public CreateInA (EntitySyncStateEnvironment<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> environment, TBtypeEntityId bId, TBtypeEntityVersion bVersion)
        : base (environment)
    {
      _bId = bId;
      _bVersion = bVersion;
    }

    public override void AddRequiredEntitiesToLoad (Func<TAtypeEntityId, bool> a, Func<TBtypeEntityId, bool> b)
    {
      b (_bId);
    }

    public override IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> FetchRequiredEntities (IReadOnlyDictionary<TAtypeEntityId, TAtypeEntity> aEntities, IReadOnlyDictionary<TBtypeEntityId, TBtypeEntity> bEntites)
    {
      if (!bEntites.TryGetValue (_bId, out _bEntity))
      {
        // Just an info, because an add will be retried on next synchronization
        s_logger.InfoFormat ("Could not fetch entity '{0}'. Discarding operation.", _bId);
        return Discard();
      }

      return this;
    }

    public override IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> Resolve ()
    {
      return this;
    }

    public override void AddNewRelationNoThrow (Action<IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>> addAction)
    {
      s_logger.Error ("This state should have been left via PerformSyncActionNoThrow!");
    }

    public override void Dispose ()
    {
      _bEntity = default(TBtypeEntity);
    }

    public override void AddSyncronizationJob (
        IJobList<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion> aJobs,
        IJobList<TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion> bJobs,
        IEntitySynchronizationLogger logger)
    {
      logger.SetBId (_bId);
      aJobs.AddCreateJob (new JobWrapper (this, logger));
    }

    private TAtypeEntity InitializeEntity (TAtypeEntity entity, IEntityMappingLogger logger)
    {
      return _environment.Mapper.Map2To1 (_bEntity, entity, logger);
    }

    private void NotifyOperationSuceeded (EntityVersion<TAtypeEntityId, TAtypeEntityVersion> newVersion, IEntitySynchronizationLogger logger)
    {
      logger.SetAId (newVersion.Id);
      _nextStateAfterJobExecution = CreateDoNothing (newVersion.Id, newVersion.Version, _bId, _bVersion);
    }

    private void NotifyOperationFailed (Exception exception, IEntitySynchronizationLogger logger)
    {
      logger.LogAbortedDueToError (exception);
      LogException (exception);
      SetNextStateAsFailed();
    }

    private void NotifyOperationFailed (string errorMessage, IEntitySynchronizationLogger logger)
    {
      logger.LogAbortedDueToError (errorMessage);
      SetNextStateAsFailed ();
    }

    public override IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> NotifyJobExecuted ()
    {
      if (_nextStateAfterJobExecution == null)
      {
        s_logger.Error ($"{nameof (_nextStateAfterJobExecution)} was not set. Defaulting to failed state.");
        SetNextStateAsFailed();
      }
      return _nextStateAfterJobExecution;
    }

    private void SetNextStateAsFailed ()
    {
      _nextStateAfterJobExecution = Discard();
    }

    struct JobWrapper : ICreateJob<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion>
    {
      private readonly CreateInA<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> _state;
      readonly IEntitySynchronizationLogger _logger;

      public JobWrapper (
          CreateInA<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> state,
          IEntitySynchronizationLogger logger)
      {
        if (state == null)
          throw new ArgumentNullException (nameof (state));
        if (logger == null)
          throw new ArgumentNullException (nameof (logger));

        _state = state;
        _logger = logger;
      }

      public TAtypeEntity InitializeEntity (TAtypeEntity entity)
      {
        return _state.InitializeEntity (entity, _logger);
      }

      public void NotifyOperationSuceeded (EntityVersion<TAtypeEntityId, TAtypeEntityVersion> result)
      {
        _state.NotifyOperationSuceeded (result, _logger);
      }

      public void NotifyOperationFailed (Exception exception)
      {
        _state.NotifyOperationFailed (exception, _logger);
      }

      public void NotifyOperationFailed (string errorMessage)
      {
        _state.NotifyOperationFailed (errorMessage, _logger);
      }
    }
  }
}