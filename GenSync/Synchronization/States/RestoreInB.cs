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
using System.Threading.Tasks;
using GenSync.EntityRelationManagement;
using GenSync.EntityRepositories;
using GenSync.Logging;
using log4net;

namespace GenSync.Synchronization.States
{
  internal class RestoreInB<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity>
      : UpdateBase<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity>
  {
    private static readonly ILog s_logger = LogManager.GetLogger (System.Reflection.MethodBase.GetCurrentMethod ().DeclaringType);
    private readonly TBtypeEntityVersion _currentBVersion;
    private IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> _nextStateAfterJobExecution;

    public RestoreInB (
        EntitySyncStateEnvironment<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> environment,
        IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> knownData,
        TBtypeEntityVersion currentBVersion)
        : base (environment, knownData)
    {
      _currentBVersion = currentBVersion;
    }

    public override void AddSyncronizationJob (IJobList<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion> aJobs, IJobList<TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion> bJobs, IEntitySynchronizationLogger logger)
    {
      logger.SetAId (_knownData.AtypeId);
      logger.SetBId (_knownData.BtypeId);
      bJobs.AddUpdateJob (new JobWrapper (this, logger));
    }

    TBtypeEntity UpdateEntity (TBtypeEntity entity, IEntitySynchronizationLogger logger)
    {
      return _environment.Mapper.Map1To2 (_aEntity, entity, logger);
    }

    private void NotifyOperationSuceeded (
        EntityVersion<TBtypeEntityId, TBtypeEntityVersion> result,
        IEntitySynchronizationLogger logger)
    {
      logger.SetBId (result.Id);
      _nextStateAfterJobExecution = CreateDoNothing (_knownData.AtypeId, _knownData.AtypeVersion, result.Id, result.Version);
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
      _nextStateAfterJobExecution = CreateDoNothing();
    }

    struct JobWrapper : IUpdateJob<TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion>
    {
      private readonly RestoreInB<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> _state;
      readonly IEntitySynchronizationLogger _logger;

      public JobWrapper (
          RestoreInB<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> state,
          IEntitySynchronizationLogger logger)
      {
        if (state == null)
          throw new ArgumentNullException (nameof (state));
        if (logger == null)
          throw new ArgumentNullException (nameof (logger));

        _state = state;
        _logger = logger;
      }

      public TBtypeEntityId EntityId => _state._knownData.BtypeId;
      public TBtypeEntityVersion Version => _state._currentBVersion;
      public TBtypeEntity EntityToUpdate => _state._bEntity;

      public TBtypeEntity UpdateEntity (TBtypeEntity entity)
      {
        return _state.UpdateEntity (entity, _logger);
      }

      public void NotifyOperationSuceeded (EntityVersion<TBtypeEntityId, TBtypeEntityVersion> result)
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