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
using System.Text;
using System.Threading.Tasks;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation.TimeRangeFiltering;
using GenSync;

namespace CalDavDataAccessIntegrationTests
{
  internal class CalDavDataAccessCreateDeleteInsteadOfUpdateWrappper : ICalDavDataAccess
  {
    private readonly ICalDavDataAccess _inner;

    public CalDavDataAccessCreateDeleteInsteadOfUpdateWrappper (ICalDavDataAccess inner)
    {
      _inner = inner;
    }

    public bool IsResourceCalender ()
    {
      return _inner.IsResourceCalender();
    }

    public bool DoesSupportCalendarQuery ()
    {
      return _inner.DoesSupportCalendarQuery();
    }

    public bool IsCalendarAccessSupported ()
    {
      return _inner.IsCalendarAccessSupported();
    }

    public bool IsWriteable ()
    {
      return _inner.IsWriteable();
    }

    public IReadOnlyList<EntityIdWithVersion<Uri, string>> GetEvents (DateTimeRange? range)
    {
      return _inner.GetEvents (range);
    }

    public IReadOnlyList<EntityIdWithVersion<Uri, string>> GetTodos (DateTimeRange? range)
    {
      return _inner.GetTodos (range);
    }

    public IReadOnlyList<EntityWithVersion<Uri, string>> GetEntities (IEnumerable<Uri> eventUrls)
    {
      return _inner.GetEntities (eventUrls);
    }

    public EntityIdWithVersion<Uri, string> CreateEntity (string iCalData)
    {
      return _inner.CreateEntity (iCalData);
    }

    public bool DeleteEntity (Uri uri)
    {
      return _inner.DeleteEntity (uri);
    }

    public bool DeleteEntity (EntityIdWithVersion<Uri, string> entity)
    {
      return _inner.DeleteEntity (entity);
    }

    public EntityIdWithVersion<Uri, string> UpdateEntity (Uri url, string iCalData)
    {
      if (!_inner.DeleteEntity (url))
        throw new Exception();
      return _inner.CreateEntity (iCalData);
    }

    public EntityIdWithVersion<Uri, string> UpdateEntity (EntityIdWithVersion<Uri, string> evt, string contents)
    {
      if (!_inner.DeleteEntity (evt))
        throw new Exception ("(412) Precondition Failed");
      return _inner.CreateEntity (contents);
    }
  }
}