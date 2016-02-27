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
using GenSync.EntityRepositories;

namespace GenSync
{
  /// <summary>
  /// Represents an Id of an Entity with an optional hints.
  /// Specifying the hints can prevent the synchronizer from calling <see cref="IReadOnlyEntityRepository{TEntity,TEntityId,TEntityVersion}.GetVersions(System.Collections.Generic.IEnumerable{IdWithAwarenessLevel{TEntityId}})"/>
  /// Specifying wrong hints can cause items to be not synced, but it is guaranteed that specifying wrong hints has no other effects (e.g. duplicating events).
  /// </summary>
  public interface IIdWithHints<out TEntityId, out TVersion>
  {
    TEntityId Id { get; }
    /// <remarks>
    ///Is allowed to throw an exception, if IsVersionSpecified returns false!
    /// </remarks>
    TVersion VersionHint { get; }
    bool IsVersionHintSpecified { get; }
    bool? WasDeletedHint { get; }
  }
}