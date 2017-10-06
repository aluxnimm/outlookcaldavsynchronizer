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
using CalDavSynchronizer.DataAccess;
using GenSync;
using GenSync.EntityRepositories;

namespace CalDavSynchronizer.Implementation.Contacts
{
  class LoggingEntityStateCollectionDecorator : IEntityStateCollection<WebResourceName, string>
  {
    private readonly IEntityStateCollection<WebResourceName, string> _inner;
    private readonly ICardDavRepositoryLogger _logger;

    public LoggingEntityStateCollectionDecorator(IEntityStateCollection<WebResourceName, string> inner, ICardDavRepositoryLogger logger)
    {
      _inner = inner ?? throw new ArgumentNullException(nameof(inner));
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Dictionary<WebResourceName, string> DisposeAndGetLeftovers()
    {
      var result = _inner.DisposeAndGetLeftovers();
      _logger.LogEntitiesExists(result.Keys);
      return result;
    }

    public (EntityState State, string RepositoryVersion) RemoveState(WebResourceName id, string knownVersion)
    {
      var result = _inner.RemoveState(id, knownVersion);
      switch (result.State)
      {
        case EntityState.ChangedOrAdded:
        case EntityState.Unchanged:
          _logger.LogEntityExists(id);
          break;
      }
      return result;
    }
  }
}