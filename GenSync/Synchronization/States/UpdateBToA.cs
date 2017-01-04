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
  public class UpdateBToA<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext>
      : UpdateBase<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext>
  {
    // ReSharper disable once StaticFieldInGenericType
    private static readonly ILog s_logger = LogManager.GetLogger (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    private readonly TBtypeEntityVersion _newBVersion;
    private readonly TAtypeEntityVersion _currentAVersion;
    private IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> _nextStateAfterJobExecution;

    public UpdateBToA (
        EntitySyncStateEnvironment<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> environment,
        IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> knownData,
        TBtypeEntityVersion newBVersion,
        TAtypeEntityVersion currentAVersion)
        : base (environment, knownData)
    {
      _newBVersion = newBVersion;
      _currentAVersion = currentAVersion;
    }

    public override void AddSyncronizationJob (
        IJobList<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity> aJobs,
        IJobList<TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> bJobs,
        IEntitySynchronizationLogger logger,
        TContext context)
    {
      logger.SetAId (KnownData.AtypeId);
      logger.SetBId (KnownData.BtypeId);
      aJobs.AddUpdateJob (new JobWrapper (this, logger, context));
    }

    async Task<TAtypeEntity> UpdateEntity (TAtypeEntity entity, IEntitySynchronizationLogger logger, TContext context)
    {
      return await _environment.Mapper.Map2To1 (_bEntity, entity, logger, context);
    }

    private void NotifyOperationSuceeded (
        EntityVersion<TAtypeEntityId, TAtypeEntityVersion> result,
        IEntitySynchronizationLogger logger)
    {
      logger.SetAId (result.Id);
      _nextStateAfterJobExecution = CreateDoNothing (result.Id, result.Version, KnownData.BtypeId, _newBVersion);
    }

    private void NotifyEntityNotFound ()
    {
      SetNextStateAsFailed ();
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

    public override IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> NotifyJobExecuted ()
    {
      if (_nextStateAfterJobExecution == null)
      {
        s_logger.Error ($"{nameof (_nextStateAfterJobExecution)} was not set. Defaulting to failed state.");
        SetNextStateAsFailed();
      }
      return _nextStateAfterJobExecution;
    }

    public override void Accept(ISynchronizationStateVisitor<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> visitor)
    {
      visitor.Visit(this);
    }

    private void SetNextStateAsFailed ()
    {
      _nextStateAfterJobExecution = CreateDoNothing();
    }

    struct JobWrapper : IUpdateJob<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity>
    {
      private readonly UpdateBToA<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> _state;
      readonly IEntitySynchronizationLogger _logger;
      private readonly TContext _context;

      public JobWrapper (
          UpdateBToA<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> state,
          IEntitySynchronizationLogger logger, 
          TContext context)
      {
        if (state == null)
          throw new ArgumentNullException (nameof (state));
        if (logger == null)
          throw new ArgumentNullException (nameof (logger));

        _state = state;
        _logger = logger;
        _context = context;
      }

      public TAtypeEntityId EntityId => _state.KnownData.AtypeId;
      public TAtypeEntityVersion Version => _state._currentAVersion;
      public TAtypeEntity EntityToUpdate => _state._aEntity;

      public async Task<TAtypeEntity> UpdateEntity (TAtypeEntity entity)
      {
        return await _state.UpdateEntity (entity, _logger, _context);
      }

      public void NotifyOperationSuceeded (EntityVersion<TAtypeEntityId, TAtypeEntityVersion> result)
      {
        _state.NotifyOperationSuceeded (result, _logger);
      }

      public void NotifyEntityNotFound ()
      {
        _state.NotifyEntityNotFound ();
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