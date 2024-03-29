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
using System.Reflection;
using System.Threading.Tasks;
using GenSync.EntityRelationManagement;
using GenSync.Logging;
using log4net;

namespace GenSync.Synchronization.States
{
    public abstract class UpdateFromNewerToOlder<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext>
        : UpdateBase<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext>
    {
        // ReSharper disable once StaticFieldInGenericType
        private static readonly ILog s_logger = LogManager.GetLogger(MethodInfo.GetCurrentMethod().DeclaringType);

        private readonly TAtypeEntityVersion _newA;
        private readonly TBtypeEntityVersion _newB;

        protected UpdateFromNewerToOlder(EntitySyncStateEnvironment<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> environment, IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> knownData, TAtypeEntityVersion newA, TBtypeEntityVersion newB)
            : base(environment, knownData)
        {
            _newA = newA;
            _newB = newB;
        }

        public override IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> Resolve()
        {
            var modificationTimeA = ModificationTimeA;
            var modificationTimeB = ModificationTimeB;

            // Assume that no modification of B (when modificationTimeB is NULL) means, that the item is never modified. Therefore it must be new. 
            // NOTE: THe nullcheck is just there to make it explicit. That would also work without the nullcheck, since compare operators of nullables evaluate to false, if there is no value. 
            if (modificationTimeB != null && modificationTimeA >= modificationTimeB)
            {
                s_logger.Info($"Considering '{KnownData.AtypeId}' (modified at '{modificationTimeA}') newer than '{KnownData.BtypeId}' (modified at '{modificationTimeB}').");
                return _environment.StateFactory.Create_UpdateAtoB(KnownData, _newA, _newB);
            }
            else
            {
                s_logger.Info($"Considering '{KnownData.BtypeId}' (modified at '{modificationTimeB}') newer than '{KnownData.AtypeId}' (modified at '{modificationTimeA}').");
                return _environment.StateFactory.Create_UpdateBtoA(KnownData, _newB, _newA);
            }
        }

        protected abstract DateTime ModificationTimeA { get; }
        protected abstract DateTime? ModificationTimeB { get; }

        public override void AddNewRelationNoThrow(Action<IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>> addAction)
        {
            s_logger.Error("This state should have been left via Resolve!");
        }

        public override void AddSyncronizationJob(
            IEntitySyncStateContext<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> stateContext,
            IJobList<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity> aJobs,
            IJobList<TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> bJobs,
            IEntitySynchronizationLoggerFactory<TAtypeEntityId, TAtypeEntity, TBtypeEntityId, TBtypeEntity> loggerFactory,
            TContext context)
        {
            s_logger.Error("This state should have been left via Resolve!");
        }

        public override IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> NotifyJobExecuted()
        {
            s_logger.Error("This state should have been left via Resolve!");
            return this;
        }

        public override void Accept(IEntitySyncStateContext<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> syncStateContext, ISynchronizationStateVisitor<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> visitor)
        {
            visitor.Visit(syncStateContext, this);
        }
    }
}