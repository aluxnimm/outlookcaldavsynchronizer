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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using CalDavSynchronizer.EntityRelationManagement;
using CalDavSynchronizer.EntityVersionManagement;
using CalDavSynchronizer.InitialEntityMatching;

namespace CalDavSynchronizer.DataAccess
{
  public class StorageDataAccess<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> : IStorageDataAccess<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>
  {
    private const string s_atypeEntityStorageName = "atype.bin";
    private const string s_btypeEntityStorageName = "btype.bin";
    private const string s_relationStorageName = "relation.bin";

    private readonly string _dataDirectory;

    public StorageDataAccess (string dataDirectory)
    {
      _dataDirectory = dataDirectory;
    }

    public void DeleteCaches ()
    {
      if (!Directory.Exists (_dataDirectory))
        return;

      File.Delete (GetFullEntityPath (s_atypeEntityStorageName));
      File.Delete (GetFullEntityPath (s_btypeEntityStorageName));
      File.Delete (GetFullEntityPath (s_relationStorageName));

    }

    public void SaveChaches (EntityCaches<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> caches)
    {
      if (!Directory.Exists (_dataDirectory))
        Directory.CreateDirectory (_dataDirectory);

      SetVersionStorage (caches.AtypeStorage, s_atypeEntityStorageName);
      SetVersionStorage (caches.BtypeStorage, s_btypeEntityStorageName);
      SetEntityRelationStorage (caches.EntityRelationStorage, s_relationStorageName);
    }

    public EntityCaches<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> LoadOrCreateCaches (out bool cachesWereCreatedNew)
    {
      if (!(DoesEntityExist (s_atypeEntityStorageName) && DoesEntityExist (s_btypeEntityStorageName) && DoesEntityExist (s_relationStorageName)))
      {
        cachesWereCreatedNew = true;
        return new EntityCaches<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> (
            new VersionStorage<TAtypeEntityId, TAtypeEntityVersion>(),
            new VersionStorage<TBtypeEntityId, TBtypeEntityVersion> (),
            new EntityRelationStorage<TAtypeEntityId, TBtypeEntityId>()
            );
      }

      cachesWereCreatedNew = false;
      return new EntityCaches<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> (
          GetVersionStorage<TAtypeEntityId, TAtypeEntityVersion> (s_atypeEntityStorageName),
          GetVersionStorage<TBtypeEntityId, TBtypeEntityVersion> (s_btypeEntityStorageName),
          GetEntityRelationStorage<TAtypeEntityId, TBtypeEntityId> (s_relationStorageName)
          );
    }


    private IVersionStorage<TEntity, TVersion> GetVersionStorage<TEntity, TVersion> (String storageId)
    {
      using (var stream = CreateInputStream (storageId))
      {
        return (VersionStorage<TEntity, TVersion>) new BinaryFormatter().Deserialize (stream);
      }
    }

    private void SetVersionStorage<TEntity, TVersion> (IVersionStorage<TEntity, TVersion> storage, String storageId)
    {
      using (var stream = CreateOutputStream (storageId))
      {
        new BinaryFormatter().Serialize (stream, storage);
      }
    }


    private IEntityRelationStorage<TEntityOneId, TEntityTwoId> GetEntityRelationStorage<TEntityOneId, TEntityTwoId> (String storageId)
    {
      using (var stream = CreateInputStream (storageId))
      {
        return (EntityRelationStorage<TEntityOneId, TEntityTwoId>) new BinaryFormatter().Deserialize (stream);
      }
    }

    public void SetEntityRelationStorage<TEntityOneId, TEntityTwoId> (IEntityRelationStorage<TEntityOneId, TEntityTwoId> storage, String storageId)
    {
      using (var stream = CreateOutputStream (storageId))
      {
        new BinaryFormatter().Serialize (stream, storage);
      }
    }

    private bool DoesEntityExist (string entityId)
    {
      return File.Exists (GetFullEntityPath (entityId));
    }


    private Stream CreateOutputStream (string entityId)
    {
      return new FileStream (GetFullEntityPath (entityId), FileMode.Create, FileAccess.Write);
    }

    private string GetFullEntityPath (string entityId)
    {
      return Path.Combine (_dataDirectory, entityId);
    }

    private Stream CreateInputStream (string entityId)
    {
      return new FileStream (GetFullEntityPath (entityId), FileMode.Open, FileAccess.Read);
    }
  }
}