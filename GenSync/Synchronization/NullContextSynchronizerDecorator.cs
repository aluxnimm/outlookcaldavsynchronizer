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
using GenSync.Logging;

namespace GenSync.Synchronization
{
  public class NullContextSynchronizerDecorator<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity>
    : IPartialSynchronizer<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>
  {
    private readonly IPartialSynchronizer<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion, int> _inner;

    public NullContextSynchronizerDecorator(IPartialSynchronizer<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion, int> inner)
    {
      _inner = inner;
    }

    public async Task Synchronize(ISynchronizationLogger logger)
    {
      await _inner.Synchronize(logger, 0);
    }

    public async Task SynchronizePartial(
      IEnumerable<IIdWithHints<TAtypeEntityId, TAtypeEntityVersion>> aIds,
      IEnumerable<IIdWithHints<TBtypeEntityId, TBtypeEntityVersion>> bIds,
      ISynchronizationLogger logger)
    {
      await _inner.SynchronizePartial(aIds, bIds, logger, () => Task.FromResult(0), c => Task.FromResult(0));
    }
  }
}