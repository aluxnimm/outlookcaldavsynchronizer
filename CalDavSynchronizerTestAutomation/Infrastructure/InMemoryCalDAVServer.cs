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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation.TimeRangeFiltering;
using GenSync;

namespace CalDavSynchronizerTestAutomation.Infrastructure
{
  internal class InMemoryCalDAVServer : ICalDavDataAccess
  {
    private readonly Dictionary<Uri, Tuple<string, string>> _entites = new Dictionary<Uri, Tuple<string, string>>();

    public Task<bool> IsResourceCalender ()
    {
      return Task.FromResult (true);
    }

    public Task<bool> DoesSupportCalendarQuery ()
    {
      return Task.FromResult (true);
    }

    public Task<bool> IsCalendarAccessSupported ()
    {
      return Task.FromResult (true);
    }

    public Task<bool> IsWriteable ()
    {
      return Task.FromResult (true);
    }

    public Task<IReadOnlyList<EntityVersion<Uri, string>>> GetEventVersions (DateTimeRange? range)
    {
      if (range != null)
        throw new NotSupportedException ("range not supported");

      return Task.FromResult<IReadOnlyList<EntityVersion<Uri, string>>> (
          _entites.Select (e => EntityVersion.Create (e.Key, e.Value.Item1)).ToList());
    }

    public Task<IReadOnlyList<EntityVersion<Uri, string>>> GetTodoVersions (DateTimeRange? range)
    {
      if (range != null)
        throw new NotSupportedException ("range not supported");

      return Task.FromResult<IReadOnlyList<EntityVersion<Uri, string>>> (
          _entites.Select (e => EntityVersion.Create (e.Key, e.Value.Item1)).ToList());
    }

    public Task<IReadOnlyList<EntityVersion<Uri, string>>> GetVersions (IEnumerable<Uri> eventUrls)
    {
      return Task.FromResult<IReadOnlyList<EntityVersion<Uri, string>>> (
          eventUrls.Select (id => EntityVersion.Create (id, _entites[id].Item1)).ToList());
    }

    public Task<IReadOnlyList<EntityWithId<Uri, string>>> GetEntities (IEnumerable<Uri> eventUrls)
    {
      return Task.FromResult<IReadOnlyList<EntityWithId<Uri, string>>> (
          eventUrls.Select (id => EntityWithId.Create (id, _entites[id].Item2)).ToList());
    }

    public Task<EntityVersion<Uri, string>> CreateEntity (string iCalData, string uid)
    {
      var id = new Uri ("http://bla.com/" + Guid.NewGuid());
      const int version = 1;
      _entites.Add (id, Tuple.Create (version.ToString(), iCalData));
      return Task.FromResult (EntityVersion.Create (id, version.ToString()));
    }

    public Task DeleteEntity (Uri uri, string etag)
    {
      if (!_entites.ContainsKey (uri))
        throw new Exception ("tried to delete non existing entity!");

      if (_entites[uri].Item1 != etag)
        throw new Exception ("tried to delete stale version!");

      _entites.Remove (uri);
      return Task.FromResult (0);
    }

    public Task<EntityVersion<Uri, string>> UpdateEntity (
        Uri url,
        string version,
        string iCalData)
    {
      var existingVersion = _entites[url].Item1;
      if (version != existingVersion)
        throw new Exception ("tried to update stale version!");

      var newVersion = (version + "x");
      _entites[url] = Tuple.Create (newVersion, iCalData);
      return Task.FromResult (EntityVersion.Create (url, newVersion));
    }
  }
}