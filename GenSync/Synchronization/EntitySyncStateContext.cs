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
using System.Text;
using System.Threading.Tasks;
using GenSync.EntityRelationManagement;
using GenSync.Logging;
using GenSync.Synchronization.States;

namespace GenSync.Synchronization
{
    class EntitySyncStateContext<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> : IEntitySyncStateContext<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext>
    {
        private IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> _state;

        public EntitySyncStateContext(IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            _state = state;
        }

        public void AddRequiredEntitiesToLoad(Func<TAtypeEntityId, bool> a, Func<TBtypeEntityId, bool> b)
        {
            _state.AddRequiredEntitiesToLoad(a, b);
        }

        public void SetState(IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> newState)
        {
            if (!ReferenceEquals(newState, _state))
            {
                _state.Dispose();
                _state = newState;
            }
        }

        public void FetchRequiredEntities(IReadOnlyDictionary<TAtypeEntityId, TAtypeEntity> aEntities, IReadOnlyDictionary<TBtypeEntityId, TBtypeEntity> bEntites)
        {
            SetState(_state.FetchRequiredEntities(aEntities, bEntites));
        }

        public void Resolve()
        {
            SetState(_state.Resolve());
        }

        public void AddSyncronizationJob(IJobList<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity> aJobs, IJobList<TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> bJobs, IEntitySynchronizationLoggerFactory<TAtypeEntityId, TAtypeEntity, TBtypeEntityId, TBtypeEntity> loggerFactory, TContext context)
        {
            _state.AddSyncronizationJob(this, aJobs, bJobs, loggerFactory, context);
        }

        public void NotifyJobExecuted()
        {
            SetState(_state.NotifyJobExecuted());
        }

        public void AddNewRelationNoThrow(Action<IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>> addAction)
        {
            _state.AddNewRelationNoThrow(addAction);
        }

        public void Dispose()
        {
            _state.Dispose();
        }

        public void Accept(ISynchronizationStateVisitor<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> visitor)
        {
            _state.Accept(this, visitor);
        }

        public void Abort()
        {
            SetState(_state.Abort());
        }
    }
}