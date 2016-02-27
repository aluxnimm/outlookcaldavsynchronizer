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

namespace GenSync
{
  public static class IdWithHints
  {
    public static IIdWithHints<TEntityId, TVersion> Create<TEntityId, TVersion> (TEntityId id, TVersion versionHint, bool? wasDeletedHint) where TVersion : class
    {
      return new IdWithHintsClass<TEntityId, TVersion> (id, versionHint, wasDeletedHint);
    }

    public static IIdWithHints<TEntityId, TVersion> Create<TEntityId, TVersion> (TEntityId id, TVersion? versionHint, bool? wasDeletedHint) where TVersion : struct
    {
      return new IdWithHintsStruct<TEntityId, TVersion> (id, versionHint, wasDeletedHint);
    }

    private class IdWithHintsClass<TEntityId, TVersion> : IIdWithHints<TEntityId, TVersion>
     where TVersion : class
    {
      public TEntityId Id { get; private set; }
      public TVersion VersionHint { get; private set; }
      public bool IsVersionHintSpecified => VersionHint != null;
      public bool? WasDeletedHint { get; private set; }
    
      public IdWithHintsClass (TEntityId id, TVersion versionHint, bool? wasDeletedHint)
      {
        Id = id;
        VersionHint = versionHint;
        WasDeletedHint = wasDeletedHint;
      }
    }

    private class IdWithHintsStruct<TEntityId, TVersion> : IIdWithHints<TEntityId, TVersion>
        where TVersion : struct
    {
      private readonly TVersion? _version;

      public TEntityId Id { get; private set; }
      // ReSharper disable once PossibleInvalidOperationException
      public TVersion VersionHint => _version.Value;
      public bool IsVersionHintSpecified => _version.HasValue;
      public bool? WasDeletedHint { get; private set; }

      public IdWithHintsStruct (TEntityId id, TVersion? version, bool? wasDeletedHint)
      {
        Id = id;
        _version = version;
        WasDeletedHint = wasDeletedHint;
      }
    }
  }
}