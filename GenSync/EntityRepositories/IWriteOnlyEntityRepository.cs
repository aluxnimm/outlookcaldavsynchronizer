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

namespace GenSync.EntityRepositories
{
  /// <summary>
  /// All writeoperations that a repository has to support
  /// </summary>
  public interface IWriteOnlyEntityRepository<TEntityId, TEntityVersion, TEntity, TContext>
  {
    /// <returns>
    /// true: Entity was found an deleted.
    /// false: Entity was not found
    /// </returns>
    Task<bool> TryDelete (TEntityId entityId, TEntityVersion version, TContext context, IEntitySynchronizationLogger logger);
    /// <returns>
    /// Id and Version of the updated entity
    /// or
    /// Null if entity was not found
    /// </returns>
    Task<EntityVersion<TEntityId, TEntityVersion>> TryUpdate (TEntityId entityId, TEntityVersion version, TEntity entityToUpdate, Func<TEntity, Task<TEntity>> entityModifier, TContext context, IEntitySynchronizationLogger logger);
    Task<EntityVersion<TEntityId, TEntityVersion>> Create (Func<TEntity, Task<TEntity>> entityInitializer, TContext context);
  }
}