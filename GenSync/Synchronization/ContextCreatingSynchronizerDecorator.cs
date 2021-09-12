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
using System.Threading.Tasks;
using GenSync.Logging;

namespace GenSync.Synchronization
{
    public class ContextCreatingSynchronizerDecorator<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext>
        : IPartialSynchronizer<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>
    {
        private readonly IPartialSynchronizer<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion, TContext> _inner;
        private readonly ISynchronizationContextFactory<TContext> _contextFactory;

        public ContextCreatingSynchronizerDecorator(
            IPartialSynchronizer<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion, TContext> inner,
            ISynchronizationContextFactory<TContext> contextFactory)
        {
            if (inner == null) throw new ArgumentNullException(nameof(inner));
            if (contextFactory == null) throw new ArgumentNullException(nameof(contextFactory));

            _inner = inner;
            _contextFactory = contextFactory;
        }

        public async Task Synchronize(ISynchronizationLogger logger)
        {
            var synchronizationContext = await _contextFactory.Create();
            await _inner.Synchronize(logger, synchronizationContext);
            await _contextFactory.SynchronizationFinished(synchronizationContext);
        }

        public async Task SynchronizePartial(
            IEnumerable<IIdWithHints<TAtypeEntityId, TAtypeEntityVersion>> aIds,
            IEnumerable<IIdWithHints<TBtypeEntityId, TBtypeEntityVersion>> bIds,
            ISynchronizationLogger logger)
        {
            await _inner.SynchronizePartial(aIds, bIds, logger, _contextFactory.Create, _contextFactory.SynchronizationFinished);
        }
    }
}