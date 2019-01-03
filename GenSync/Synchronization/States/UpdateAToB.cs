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
  public class UpdateAToB<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext>
      : UpdateBase<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext>
  {
    private static readonly ILog s_logger = LogManager.GetLogger (System.Reflection.MethodBase.GetCurrentMethod ().DeclaringType);
    private readonly TAtypeEntityVersion _newAVersion;
    private readonly TBtypeEntityVersion _currentBVersion;

    public UpdateAToB (
        EntitySyncStateEnvironment<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> environment,
        IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> knownData,
        TAtypeEntityVersion newAVersion,
        TBtypeEntityVersion currentBVersion)
        : base (environment, knownData)
    {
      _newAVersion = newAVersion;
      _currentBVersion = currentBVersion;
    }

    public override void AddSyncronizationJob(
      IEntitySyncStateContext<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> stateContext,
      IJobList<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity> aJobs,
      IJobList<TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> bJobs,
      IEntitySynchronizationLoggerFactory<TAtypeEntityId, TAtypeEntity,TBtypeEntityId, TBtypeEntity> loggerFactory,
      TContext context)
    {
      var logger = loggerFactory.CreateEntitySynchronizationLogger(SynchronizationOperation.UpdateInB);
      logger.SetAId(KnownData.AtypeId);
      logger.SetBId(KnownData.BtypeId);
      logger.LogA(_aEntity);
      logger.LogB(_bEntity);

      bJobs.AddUpdateJob(new JobWrapper(stateContext, this, logger, context));
    }

    async Task<TBtypeEntity> UpdateEntity (TBtypeEntity entity, IEntitySynchronizationLogger<TAtypeEntityId, TAtypeEntity, TBtypeEntityId, TBtypeEntity> logger, TContext context)
    {
      return await _environment.Mapper.Map1To2 (_aEntity, entity, logger, context);
    }

    private void NotifyOperationSuceeded (
      IEntitySyncStateContext<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> stateContext,
        EntityVersion<TBtypeEntityId, TBtypeEntityVersion> result,
      IEntitySynchronizationLogger<TAtypeEntityId, TAtypeEntity, TBtypeEntityId, TBtypeEntity> logger)
    {
      logger.SetBId (result.Id);
      stateContext.SetState(CreateDoNothing (KnownData.AtypeId, _newAVersion, result.Id, result.Version));
    }

    private void NotifyEntityNotFound (IEntitySyncStateContext<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> stateContext)
    {
      stateContext.SetState (CreateFailed());
    }

    private void NotifyOperationFailed (IEntitySyncStateContext<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> stateContext, Exception exception, IEntitySynchronizationLogger<TAtypeEntityId, TAtypeEntity, TBtypeEntityId, TBtypeEntity> logger)
    {
      logger.LogAbortedDueToError (exception);
      LogException (exception);
      stateContext.SetState (CreateFailed());
    }

    private void NotifyOperationFailed (IEntitySyncStateContext<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> stateContext, string errorMessage, IEntitySynchronizationLogger<TAtypeEntityId, TAtypeEntity, TBtypeEntityId, TBtypeEntity> logger)
    {
      logger.LogAbortedDueToError (errorMessage);
      stateContext.SetState (CreateFailed());
    }

    public override IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> NotifyJobExecuted ()
    {
      s_logger.Error ("State was not left. Defaulting to failed state.");
      return CreateFailed ();
    }

    public override void Accept(IEntitySyncStateContext<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> syncStateContext, ISynchronizationStateVisitor<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> visitor)
    {
      visitor.Visit (syncStateContext, this);
    }

    private DoNothing<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> CreateFailed()
    {
      return CreateDoNothing ();
    }

    struct JobWrapper : IUpdateJob<TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity>
    {
      private readonly IEntitySyncStateContext<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> _stateContext;
      private readonly UpdateAToB<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> _state;
      readonly IEntitySynchronizationLogger<TAtypeEntityId, TAtypeEntity, TBtypeEntityId, TBtypeEntity> _logger;
      private readonly TContext _context;

      public JobWrapper (
          IEntitySyncStateContext<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> stateContext,
          UpdateAToB<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> state,
          IEntitySynchronizationLogger<TAtypeEntityId, TAtypeEntity, TBtypeEntityId, TBtypeEntity> logger, 
          TContext context)
      {
        if (state == null)
          throw new ArgumentNullException (nameof (state));
        if (logger == null)
          throw new ArgumentNullException (nameof (logger));

        _stateContext = stateContext;
        _state = state;
        _logger = logger;
        _context = context;
      }

      public IEntitySynchronizationLogger Logger => _logger;
      public TBtypeEntityId EntityId => _state.KnownData.BtypeId;
      public TBtypeEntityVersion Version => _state._currentBVersion;
      public TBtypeEntity EntityToUpdate => _state._bEntity;

      public async Task<TBtypeEntity> UpdateEntity (TBtypeEntity entity)
      {
        return await _state.UpdateEntity (entity, _logger, _context);
      }

      public void NotifyOperationSuceeded (EntityVersion<TBtypeEntityId, TBtypeEntityVersion> result)
      {
        _state.NotifyOperationSuceeded (_stateContext, result, _logger);
      }

      public void NotifyEntityNotFound ()
      {
        _state.NotifyEntityNotFound (_stateContext);
      }

      public void NotifyOperationFailed (Exception exception)
      {
        _state.NotifyOperationFailed (_stateContext, exception, _logger);
      }

      public void NotifyOperationFailed (string errorMessage)
      {
        _state.NotifyOperationFailed (_stateContext, errorMessage, _logger);
      }
    }
  }
}