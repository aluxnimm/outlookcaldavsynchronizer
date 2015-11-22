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
using System.Threading.Tasks;
using GenSync.EntityRelationManagement;

namespace GenSync.Synchronization.States
{
  internal class RestoreInA<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity>
      : UpdateBase<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity>
  {
    private readonly TAtypeEntityVersion _currentAVersion;

    public RestoreInA (
        EntitySyncStateEnvironment<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> environment,
        IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> knownData,
        TAtypeEntityVersion currentAVersion)
        : base (environment, knownData)
    {
      _currentAVersion = currentAVersion;
    }

    public override async Task<IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity>> PerformSyncActionNoThrow ()
    {
      try
      {
        var newA = await _environment.ARepository.Update (
            _knownData.AtypeId,
            _currentAVersion,
            _aEntity,
            a => _environment.Mapper.Map2To1 (_bEntity, a));
        return CreateDoNothing (newA.Id, newA.Version, _knownData.BtypeId, _knownData.BtypeVersion);
      }
      catch (Exception x)
      {
        LogException (x);
        return CreateDoNothing();
      }
    }
  }
}