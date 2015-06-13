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
using CalDavSynchronizer.Generic.EntityRepositories;
using CalDavSynchronizer.Generic.EntityVersionManagement;
using CalDavSynchronizer.Generic.ProgressReport;

namespace CalDavSynchronizer.UnitTest.InitialEntityMatching
{
  internal class TestPersonARepository : IEntityRepository<PersonA, int, int>
  {
    private readonly IEnumerable<PersonA> _persons;

    public TestPersonARepository (IEnumerable<PersonA> persons)
    {
      _persons = persons;
    }


    public Dictionary<int, int> GetVersions (DateTime @from, DateTime to)
    {
      return _persons.ToDictionary (kv => kv.Id, kv => kv.Version);
    }

    public IReadOnlyDictionary<int, PersonA> Get (ICollection<int> ids, ITotalProgressLogger progressLogger)
    {
      var personsById = _persons.ToDictionary (p => p.Id);
      return ids.Select (id => personsById[id]).ToDictionary (p => p.Id);
    }

    public void Cleanup (IReadOnlyDictionary<int, PersonA> entities)
    {
    }

    public bool Delete (int entityId)
    {
      throw new NotImplementedException();
    }

    public EntityIdWithVersion<int, int> Update (int entityId, PersonA entityToUpdate, Func<PersonA, PersonA> entityModifier)
    {
      throw new NotImplementedException();
    }

    public EntityIdWithVersion<int, int> Create (Func<PersonA, PersonA> entityInitializer)
    {
      throw new NotImplementedException();
    }
  }
}