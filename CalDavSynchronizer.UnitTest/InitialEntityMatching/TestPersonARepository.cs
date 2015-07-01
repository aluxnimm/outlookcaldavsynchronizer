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
using System.Threading.Tasks;
using CalDavSynchronizer.Generic.EntityRepositories;
using CalDavSynchronizer.Generic.EntityVersionManagement;
using CalDavSynchronizer.Generic.ProgressReport;

namespace CalDavSynchronizer.UnitTest.InitialEntityMatching
{
  internal class TestPersonARepository : IEntityRepository<PersonA, Identifier<int>, int>
  {
    private readonly IEnumerable<PersonA> _persons;

    public TestPersonARepository (IEnumerable<PersonA> persons)
    {
      _persons = persons;
    }


    public Dictionary<Identifier<int>, int> GetVersions (DateTime @from, DateTime to)
    {
      return _persons.ToDictionary (kv => kv.Id, kv => kv.Version, new IdentifierEqualityComparer<int>());
    }

    public async Task<IReadOnlyDictionary<Identifier<int>, PersonA>> Get (ICollection<Identifier<int>> ids)
    {
      var personsById = _persons.ToDictionary (p => p.Id, new IdentifierEqualityComparer<int>());
      return ids.Select (id => personsById[id]).ToDictionary (p => p.Id, new IdentifierEqualityComparer<int>());
    }

    public void Cleanup (IReadOnlyDictionary<Identifier<int>, PersonA> entities)
    {
    }

    public bool Delete (Identifier<int> entityId)
    {
      throw new NotImplementedException();
    }

    public EntityIdWithVersion<Identifier<int>, int> Update (Identifier<int> entityId, PersonA entityToUpdate, Func<PersonA, PersonA> entityModifier)
    {
      throw new NotImplementedException();
    }

    public EntityIdWithVersion<Identifier<int>, int> Create (Func<PersonA, PersonA> entityInitializer)
    {
      throw new NotImplementedException();
    }
  }
}