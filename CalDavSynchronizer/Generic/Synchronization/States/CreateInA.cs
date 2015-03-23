// This file is Part of CalDavSynchronizer (http://outlookcaldavsynchronizer.sourceforge.net/)
// Copyright (c) 2015 Gerhard Zehetbauer 
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
using CalDavSynchronizer.Generic.EntityRelationManagement;

namespace CalDavSynchronizer.Generic.Synchronization.States
{
  internal class CreateInA<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> :
      StateBase<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity>
  {

    private readonly TBtypeEntityId _bId;
    private readonly TBtypeEntityVersion _bVersion;
    private TBtypeEntity _bEntity;


    public CreateInA (EntitySyncStateEnvironment<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> environment, TBtypeEntityId bId, TBtypeEntityVersion bVersion)
        : base(environment)
    {
      _bId = bId;
      _bVersion = bVersion;
    }

    public override void AddRequiredEntitiesToLoad (Func<TAtypeEntityId, bool> a, Func<TBtypeEntityId, bool> b)
    {
      b (_bId);
    }

    public override IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> FetchRequiredEntities (IDictionary<TAtypeEntityId, TAtypeEntity> aEntities, IDictionary<TBtypeEntityId, TBtypeEntity> bEntites)
    {
      if (!bEntites.TryGetValue (_bId, out _bEntity))
        return Discard ();

      return this;
    }

    public override IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> Resolve ()
    {
      return this;
    }

    public override IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> PerformSyncAction ()
    {
      try
      {
        var newA = _environment.ARepository.Create (a => _environment.Mapper.Map2To1 (_bEntity, a));
        return CreateDoNothing (newA.Id, newA.Version, _bId, _bVersion);
      }
      catch (Exception x)
      {
        LogException (x);
        return Discard ();
      }
    }

    public override void AddNewData (Action<IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>> addAction)
    {
      throw new InvalidOperationException ();
    }
  }
}