// This file is Part of CalDavSynchronizer (https://sourceforge.net/projects/outlookcaldavsynchronizer/)
// Copyright (c) 2015 Gerhard Zehetbauer 
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

namespace CalDavSynchronizer.ConflictManagement
{
  public class FixedConflictResolutionStrategy<TAtypeEntity, TBtypeEntity> : IConflictResolutionStrategy<TAtypeEntity, TBtypeEntity>
  {

    public static readonly IConflictResolutionStrategy<TAtypeEntity, TBtypeEntity> AWinsAlways = new FixedConflictResolutionStrategy<TAtypeEntity, TBtypeEntity> (GenericConflictResolution.AWins);
    public static readonly IConflictResolutionStrategy<TAtypeEntity, TBtypeEntity> BWinsAlways = new FixedConflictResolutionStrategy<TAtypeEntity, TBtypeEntity> (GenericConflictResolution.BWins);

    private readonly GenericConflictResolution _winner;

    public FixedConflictResolutionStrategy (GenericConflictResolution winner)
    {
      _winner = winner;
    }

    public GenericConflictResolution ResolveModifiedConflict (TAtypeEntity atypeEntity, TBtypeEntity btypeEntity)
    {
      return _winner;
    }

    public GenericConflictResolution ResolveDeletionInAConflict (TBtypeEntity btypeEntity)
    {
      return _winner;
    }

    public GenericConflictResolution ResolveDeletionInBConflict (TAtypeEntity btypeEntity)
    {
      return _winner;
    }
  }
}