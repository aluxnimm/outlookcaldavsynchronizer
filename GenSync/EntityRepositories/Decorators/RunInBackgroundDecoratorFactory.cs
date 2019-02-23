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

namespace GenSync.EntityRepositories.Decorators
{
  public static class RunInBackgroundDecoratorFactory
  {
    public static IBatchWriteOnlyEntityRepository<TEntityId, TEntityVersion, TEntity, TContext>
      Create<TEntityId, TEntityVersion, TEntity, TContext>(IBatchWriteOnlyEntityRepository<TEntityId, TEntityVersion, TEntity, TContext> decorated)
      => new BatchWriteOnlyEntityRepositoryRunInBackgroundDecorator<TEntityId, TEntityVersion, TEntity, TContext>(decorated);

    public static IReadOnlyEntityRepository<TEntityId, TEntityVersion, TEntity, TContext>
      Create<TEntityId, TEntityVersion, TEntity, TContext>(IReadOnlyEntityRepository<TEntityId, TEntityVersion, TEntity, TContext> decorated)
      => new ReadOnlyEntityRepositoryRunInBackgroundDecorator<TEntityId, TEntityVersion, TEntity, TContext>(decorated);

    public static IStateAwareEntityRepository<TEntityId, TEntityVersion, TContext, TStateToken>
      Create<TEntityId, TEntityVersion, TContext, TStateToken>(IStateAwareEntityRepository<TEntityId, TEntityVersion, TContext, TStateToken> decorated)
      => new StateAwareEntityRepositoryRunInBackgroundDecorator<TEntityId, TEntityVersion, TContext, TStateToken>(decorated);
  }
}