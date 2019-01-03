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
  public class DeleteInA<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext>
      : StateWithKnownData<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext>
  {
    // ReSharper disable once StaticFieldInGenericType
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private readonly TAtypeEntityVersion _currentAVersion;

    public DeleteInA (
        EntitySyncStateEnvironment<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> environment,
        IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> knownData,
        TAtypeEntityVersion currentAVersion)
        : base (environment, knownData)
    {
      _currentAVersion = currentAVersion;
    }

    public override void AddRequiredEntitiesToLoad (Func<TAtypeEntityId, bool> a, Func<TBtypeEntityId, bool> b)
    {
    }

    public override IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> FetchRequiredEntities (IReadOnlyDictionary<TAtypeEntityId, TAtypeEntity> aEntities, IReadOnlyDictionary<TBtypeEntityId, TBtypeEntity> bEntites)
    {
      return this;
    }

    public override IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> Resolve ()
    {
      return this;
    }

    public override void AddNewRelationNoThrow (Action<IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>> addAction)
    {
      s_logger.Error ("This state should have been left via PerformSyncActionNoThrow!");
    }

    public override void Accept(IEntitySyncStateContext<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> syncStateContext, ISynchronizationStateVisitor<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> visitor)
    {
      visitor.Visit (syncStateContext, this);
    }

    public override void AddSyncronizationJob (
        IEntitySyncStateContext<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> stateContext,
        IJobList<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity> aJobs,
        IJobList<TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> bJobs,
        IEntitySynchronizationLoggerFactory<TAtypeEntityId, TAtypeEntity,TBtypeEntityId, TBtypeEntity> loggerFactory,
        TContext context)
    {
      var logger = loggerFactory.CreateEntitySynchronizationLogger(SynchronizationOperation.DeleteInA);
      logger.SetAId (KnownData.AtypeId);
      aJobs.AddDeleteJob (new JobWrapper (stateContext, this, logger));
    }

    private void NotifyOperationSuceeded (IEntitySyncStateContext<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> stateContext)
    {
      stateContext.SetState (Discard ());
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

    private DoNothing<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> CreateFailed()
    {
      return CreateDoNothing ();
    }

    struct JobWrapper : IDeleteJob<TAtypeEntityId, TAtypeEntityVersion>
    {
      private readonly IEntitySyncStateContext<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> _stateContext;
      private readonly DeleteInA<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> _state;
      readonly IEntitySynchronizationLogger<TAtypeEntityId, TAtypeEntity, TBtypeEntityId, TBtypeEntity> _logger;

      public JobWrapper (
          IEntitySyncStateContext<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> stateContext,
          DeleteInA<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> state,
          IEntitySynchronizationLogger<TAtypeEntityId, TAtypeEntity, TBtypeEntityId, TBtypeEntity> logger)
      {
        if (state == null)
          throw new ArgumentNullException (nameof (state));
        if (logger == null)
          throw new ArgumentNullException (nameof (logger));

        _stateContext = stateContext;
        _state = state;
        _logger = logger;
      }

      public IEntitySynchronizationLogger Logger => _logger;
      public TAtypeEntityId EntityId => _state.KnownData.AtypeId;
      public TAtypeEntityVersion Version => _state._currentAVersion;

      public void NotifyOperationSuceeded ()
      {
        _state.NotifyOperationSuceeded(_stateContext);
      }

      public void NotifyEntityNotFound ()
      {
        _state.NotifyEntityNotFound(_stateContext);
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