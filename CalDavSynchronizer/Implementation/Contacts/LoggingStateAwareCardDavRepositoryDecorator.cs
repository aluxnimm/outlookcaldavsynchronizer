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
using System.Collections.Generic;
using System.Threading.Tasks;
using CalDavSynchronizer.DataAccess;
using GenSync.EntityRepositories;
using GenSync.Logging;

namespace CalDavSynchronizer.Implementation.Contacts
{
  public class LoggingStateAwareCardDavRepositoryDecorator : IStateAwareEntityRepository<WebResourceName, string, ICardDavRepositoryLogger, string>
  {
    private readonly IStateAwareEntityRepository<WebResourceName, string, int, string> _inner;

    public LoggingStateAwareCardDavRepositoryDecorator(IStateAwareEntityRepository<WebResourceName, string, int, string> inner)
    {
      _inner = inner;
    }

    public async Task<(IEntityStateCollection<WebResourceName, string> States, string NewToken)> GetFullRepositoryState(IEnumerable<WebResourceName> idsOfknownEntities, string stateToken, ICardDavRepositoryLogger context, IGetVersionsLogger logger)
    {
      var result = await _inner.GetFullRepositoryState(idsOfknownEntities, stateToken, 0, logger);
      return (new LoggingEntityStateCollectionDecorator(result.States, context), result.NewToken);
    }
  }
}