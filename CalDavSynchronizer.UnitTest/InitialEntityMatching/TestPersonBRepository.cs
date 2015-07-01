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
  internal class TestPersonBRepository : IEntityRepository<PersonB, Identifier<string>, string>
  {
    private readonly IEnumerable<PersonB> _persons;

    public TestPersonBRepository (IEnumerable<PersonB> persons)
    {
      _persons = persons;
    }


    public Dictionary<Identifier<string>, string> GetVersions (DateTime @from, DateTime to)
    {
      return _persons.ToDictionary (kv => kv.Id, kv => kv.Version, new IdentifierEqualityComparer<string>());
    }

    public async Task<IReadOnlyDictionary<Identifier<string>, PersonB>> Get (ICollection<Identifier<string>> ids)
    {
      var personsById = _persons.ToDictionary (p => p.Id, new IdentifierEqualityComparer<string>());
      return ids.Select (id => personsById[id]).ToDictionary (p => p.Id, new IdentifierEqualityComparer<string>());
    }

    public void Cleanup (IReadOnlyDictionary<Identifier<string>, PersonB> entities)
    {
    }

    public bool Delete (Identifier<string> entityId)
    {
      throw new NotImplementedException();
    }

    public EntityIdWithVersion<Identifier<string>, string> Update (Identifier<string> entityId, PersonB entityToUpdate, Func<PersonB, PersonB> entityModifier)
    {
      throw new NotImplementedException();
    }

    public EntityIdWithVersion<Identifier<string>, string> Create (Func<PersonB, PersonB> entityInitializer)
    {
      throw new NotImplementedException();
    }
  }
}