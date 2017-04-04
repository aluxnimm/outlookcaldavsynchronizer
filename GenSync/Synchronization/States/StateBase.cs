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
using GenSync.Logging;
using log4net;

namespace GenSync.Synchronization.States
{
  public abstract class StateBase<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> 
    : IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext>
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    protected readonly EntitySyncStateEnvironment<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> _environment;

    protected StateBase (EntitySyncStateEnvironment<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> environment)
    {
      _environment = environment;
    }

    public abstract void AddRequiredEntitiesToLoad (Func<TAtypeEntityId, bool> a, Func<TBtypeEntityId, bool> b);
    public abstract IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> FetchRequiredEntities (IReadOnlyDictionary<TAtypeEntityId, TAtypeEntity> aEntities, IReadOnlyDictionary<TBtypeEntityId, TBtypeEntity> bEntites);

    public abstract IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> Resolve ();

    public abstract void AddSyncronizationJob (
      IEntitySyncStateContext<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> stateContext,
      IJobList<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity> aJobs, 
      IJobList<TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> bJobs,
      IEntitySynchronizationLogger logger, 
      TContext context);

    public abstract IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> NotifyJobExecuted ();

    public abstract void AddNewRelationNoThrow (Action<IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>> addAction);
    public abstract void Accept(IEntitySyncStateContext<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> syncStateContext, ISynchronizationStateVisitor<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> visitor);

    protected DoNothing<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> CreateDoNothing (IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> knownData)
    {
      return new DoNothing<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> (knownData);
    }

    protected IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> Discard ()
    {
      return new Discard<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext>();
    }

    protected DoNothing<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> CreateDoNothing (TAtypeEntityId atypeId, TAtypeEntityVersion atypeVersion, TBtypeEntityId btypeId, TBtypeEntityVersion btypeVersion)
    {
      return new DoNothing<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> (
          _environment.DataFactory.Create (atypeId, atypeVersion, btypeId, btypeVersion)
          );
    }

    protected void LogException (Exception x)
    {
      _environment.ExceptionLogger.LogException (x, s_logger);
    }

    public virtual void Dispose ()
    {
    }
  }
}