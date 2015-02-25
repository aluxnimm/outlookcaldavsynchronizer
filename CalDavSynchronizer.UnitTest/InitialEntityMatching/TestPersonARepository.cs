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
using System.Linq;
using CalDavSynchronizer.EntityRepositories;
using CalDavSynchronizer.EntityVersionManagement;

namespace CalDavSynchronizer.UnitTest.InitialEntityMatching
{
  internal class TestPersonARepository : EntityRepositoryBase<PersonA, int, int>
  {
    private readonly IEnumerable<PersonA> _persons;

    public TestPersonARepository (IEnumerable<PersonA> persons)
    {
      _persons = persons;
    }


    public override IEnumerable<EntityIdWithVersion<int, int>> GetEntityVersions (DateTime @from, DateTime to)
    {
      return _persons.Select (p => new EntityIdWithVersion<int, int> (p.Id, p.Version)).ToArray ();
    }

    public override IDictionary<int, PersonA> GetEntities (IEnumerable<int> sourceEntityIds)
    {
      var personsById = _persons.ToDictionary (p => p.Id);
      return sourceEntityIds.Select (id => personsById[id]).ToDictionary (p => p.Id);
    }

    public override bool Delete (int entityId)
    {
      throw new NotImplementedException();
    }

    public override EntityIdWithVersion<int, int> Update (int entityId, Func<PersonA, PersonA> entityModifier, PersonA cachedCurrentTargetEntityIfAvailable)
    {
      throw new NotImplementedException();
    }

    public override EntityIdWithVersion<int, int> Create (Func<PersonA, PersonA> entityInitializer)
    {
      throw new NotImplementedException();
    }
  }
}