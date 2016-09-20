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
  public abstract class UpdateFromNewerToOlder<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity>
      : UpdateBase<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity>
  {
    // ReSharper disable once StaticFieldInGenericType
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private readonly TAtypeEntityVersion _newA;
    private readonly TBtypeEntityVersion _newB;

    protected UpdateFromNewerToOlder (EntitySyncStateEnvironment<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> environment, IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> knownData, TAtypeEntityVersion newA, TBtypeEntityVersion newB)
        : base (environment, knownData)
    {
      _newA = newA;
      _newB = newB;
    }

    public override IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> Resolve ()
    {
      if (AIsNewerThanB)
        return _environment.StateFactory.Create_UpdateAtoB (KnownData, _newA, _newB);
      else
        return _environment.StateFactory.Create_UpdateBtoA (KnownData, _newB, _newA);
    }

    protected abstract bool AIsNewerThanB { get; }

    public override void AddNewRelationNoThrow (Action<IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>> addAction)
    {
      s_logger.Error ("This state should have been left via Resolve!");
    }

    public override void AddSyncronizationJob (
       IJobList<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity> aJobs,
       IJobList<TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> bJobs,
       IEntitySynchronizationLogger logger)
    {
      s_logger.Error ("This state should have been left via Resolve!");
    }

    public override IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> NotifyJobExecuted ()
    {
      s_logger.Error ("This state should have been left via Resolve!");
      return this;
    }

    public override void Accept (ISynchronizationStateVisitor<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> visitor)
    {
      visitor.Visit(this);
    }
  }
}