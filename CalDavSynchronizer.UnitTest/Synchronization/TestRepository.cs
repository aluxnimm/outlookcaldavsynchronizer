// This file is Part of CalDavSynchronizer (https://sourceforge.net/projects/outlookcaldavsynchronizer/)
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
using System.Linq;
using CalDavSynchronizer.EntityRepositories;
using CalDavSynchronizer.EntityVersionManagement;

namespace CalDavSynchronizer.UnitTest.Synchronization
{
  internal class TestRepository : EntityRepositoryBase<string, string, int>
  {
    private readonly string _idPrefix;

    public readonly Dictionary<string, Tuple<int, string>> EntityVersionAndContentById = new Dictionary<string, Tuple<int, string>>();
    private int _nextId = 1;


    public int Count
    {
      get { return EntityVersionAndContentById.Count; }
    }

    public TestRepository (string idPrefix)
    {
      _idPrefix = idPrefix;
    }

    public override IEnumerable<EntityIdWithVersion<string, int>> GetEntityVersions (DateTime @from, DateTime to)
    {
      return EntityVersionAndContentById.Select (kv => new EntityIdWithVersion<string, int> (kv.Key, kv.Value.Item1)).ToArray();
    }

    public override IDictionary<string, string> GetEntities (IEnumerable<string> sourceEntityIds)
    {
      return sourceEntityIds.Select (id => new { id, EntityVersionAndContentById[id].Item2 }).ToDictionary (v => v.id, v => v.Item2);
    }

    public override bool Delete (string entityId)
    {
      EntityVersionAndContentById.Remove (entityId);
      return true;
    }

    public override EntityIdWithVersion<string, int> Update (string entityId, Func<string, string> entityModifier)
    {
      var kv = EntityVersionAndContentById[entityId];
      EntityVersionAndContentById.Remove (entityId);

      var newValue = entityModifier (kv.Item2);
      var newVersion = kv.Item1 + 1;

      var newEntityId = entityId + "u";

      EntityVersionAndContentById[newEntityId] = Tuple.Create (newVersion, newValue);
      return new EntityIdWithVersion<string, int> (newEntityId, newVersion);
    }

    public EntityIdWithVersion<string, int> UpdateWithoutIdChange (string entityId, Func<string, string> entityModifier)
    {
      var kv = EntityVersionAndContentById[entityId];

      var newValue = entityModifier (kv.Item2);
      var newVersion = kv.Item1 + 1;

      EntityVersionAndContentById[entityId] = Tuple.Create (newVersion, newValue);
      return new EntityIdWithVersion<string, int> (entityId, newVersion);
    }


    public override EntityIdWithVersion<string, int> Create (Func<string, string> entityInitializer)
    {
      var newValue = entityInitializer (string.Empty);
      var entityId = _idPrefix + _nextId++;
      EntityVersionAndContentById[entityId] = Tuple.Create (0, newValue);
      return new EntityIdWithVersion<string, int> (entityId, 0);
    }

    public Tuple<int, string> this [string id]
    {
      get { return EntityVersionAndContentById[id]; }
    }
  }
}