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
using System.Net;
using CalDavSynchronizer.DataAccess;
using GenSync;
using GenSync.EntityRepositories;

namespace CalDavSynchronizer.Implementation
{
  class WebDavCollectionSyncEntityStates : IEntityStateCollection<WebResourceName, string>
  {
    private readonly Dictionary<WebResourceName, string> _changedOrAddedById;
    private readonly HashSet<WebResourceName> _deleted;
    private readonly IEqualityComparer<string> _versionComparer;

    public WebDavCollectionSyncEntityStates(IReadOnlyList<(WebResourceName Id, string Version)> changedOrAddedItems, IReadOnlyList<WebResourceName> deletedItems, IEqualityComparer<string> versionComparer)
    {
      _versionComparer = versionComparer;

      // Add it manually. ToDictionary will fail, if the server returns duplicate items
      _changedOrAddedById = new Dictionary<WebResourceName, string>(WebResourceName.Comparer);
      foreach (var item in changedOrAddedItems)
        _changedOrAddedById[item.Id] = item.Version;
      
      _deleted = new HashSet<WebResourceName>(deletedItems, WebResourceName.Comparer);
    }

    public (EntityState State, string RepositoryVersion) RemoveState(WebResourceName id, string knownVersion)
    {
      if (_changedOrAddedById.TryGetValue(id, out var repositoryVersion))
      {
        _changedOrAddedById.Remove(id);
        if (_versionComparer.Equals(knownVersion, repositoryVersion))
          return (EntityState.Unchanged, repositoryVersion);
        else
          return (EntityState.ChangedOrAdded, repositoryVersion);
      }
      if (_deleted.Contains(id))
      {
        _deleted.Remove(id);
        return (EntityState.Deleted, null);
      }
      else
      {
        return (EntityState.Unchanged, null);
      }
    }

    public Dictionary<WebResourceName, string> DisposeAndGetLeftovers()
    {
      return _changedOrAddedById;
    }
  }
}