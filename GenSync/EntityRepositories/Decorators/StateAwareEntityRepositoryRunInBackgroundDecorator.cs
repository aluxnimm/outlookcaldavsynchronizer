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

namespace GenSync.EntityRepositories.Decorators
{
    public class StateAwareEntityRepositoryRunInBackgroundDecorator<TEntityId, TEntityVersion, TContext, TStateToken> : IStateAwareEntityRepository<TEntityId, TEntityVersion, TContext, TStateToken>
    {
        private readonly IStateAwareEntityRepository<TEntityId, TEntityVersion, TContext, TStateToken> _stateAwareEntityRepositoryImplementation;

        public StateAwareEntityRepositoryRunInBackgroundDecorator(IStateAwareEntityRepository<TEntityId, TEntityVersion, TContext, TStateToken> stateAwareEntityRepositoryImplementation)
        {
            _stateAwareEntityRepositoryImplementation = stateAwareEntityRepositoryImplementation ?? throw new ArgumentNullException(nameof(stateAwareEntityRepositoryImplementation));
        }

        public async Task<(IEntityStateCollection<TEntityId, TEntityVersion> States, TStateToken NewToken)> GetFullRepositoryState(IEnumerable<TEntityId> idsOfknownEntities, TStateToken stateToken, TContext context, IGetVersionsLogger logger)
        {
            return await Task.Run(() => _stateAwareEntityRepositoryImplementation.GetFullRepositoryState(idsOfknownEntities, stateToken, context, logger));
        }
    }
}