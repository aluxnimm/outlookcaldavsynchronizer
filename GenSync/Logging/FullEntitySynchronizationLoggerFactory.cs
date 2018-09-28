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

namespace GenSync.Logging
{
  public class FullEntitySynchronizationLoggerFactory<TAtypeEntityId, TAtypeEntity, TBtypeEntityId, TBtypeEntity> : IFullEntitySynchronizationLoggerFactory<TAtypeEntityId, TAtypeEntity, TBtypeEntityId, TBtypeEntity>
    {
    private readonly IEntityLogMessageFactory<TAtypeEntity, TBtypeEntity> _entityLogMessageFactory;

    public FullEntitySynchronizationLoggerFactory(IEntityLogMessageFactory<TAtypeEntity,TBtypeEntity> entityLogMessageFactory)
    {
      _entityLogMessageFactory = entityLogMessageFactory ?? throw new ArgumentNullException(nameof(entityLogMessageFactory));
    }

    public IFullEntitySynchronizationLogger<TAtypeEntityId, TAtypeEntity, TBtypeEntityId, TBtypeEntity> CreateEntitySynchronizationLogger(SynchronizationOperation operation)
    {
      return new EntitySynchronizationLogger<TAtypeEntityId, TAtypeEntity, TBtypeEntityId, TBtypeEntity>(operation, _entityLogMessageFactory);
    }
  }

  public static class FullEntitySynchronizationLoggerFactory
  {
    public static FullEntitySynchronizationLoggerFactory<TAtypeEntityId, TAtypeEntity, TBtypeEntityId, TBtypeEntity> Create<TAtypeEntityId, TAtypeEntity, TBtypeEntityId, TBtypeEntity>(IEntityLogMessageFactory<TAtypeEntity, TBtypeEntity> entityLogMessageFactory)
    {
      return new FullEntitySynchronizationLoggerFactory<TAtypeEntityId, TAtypeEntity, TBtypeEntityId, TBtypeEntity>(entityLogMessageFactory);
    }
  }
}