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
using GenSync.EntityRelationManagement;

namespace GenSync.Synchronization.States
{
  public abstract class StateWithKnownData<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> :
      StateBase<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext>
  {
    public IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> KnownData { get; }


    protected StateWithKnownData (EntitySyncStateEnvironment<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> environment, IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> knownData)
        : base (environment)
    {
      KnownData = knownData;
    }

    protected DoNothing<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> CreateDoNothing ()
    {
      return new DoNothing<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> (KnownData);
    }

    public override IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> Abort ()
    {
      return CreateDoNothing ();
    }
  }
}