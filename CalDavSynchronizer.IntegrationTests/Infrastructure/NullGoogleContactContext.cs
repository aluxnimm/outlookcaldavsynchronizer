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
using CalDavSynchronizer.Implementation.GoogleContacts;
using GenSync;
using Google.Contacts;

namespace CalDavSynchronizer.IntegrationTests.Infrastructure
{
  class NullGoogleContactContext : IGoogleContactContext
  {
    public static readonly IGoogleContactContext Instance = new NullGoogleContactContext();

    private NullGoogleContactContext()
    {
    }

    public IGoogleGroupCache GroupCache { get; } = new NullGoogleGroupCache();
    public IGoogleContactCache ContactCache { get; } = new NullGoogleContactCache();

    class NullGoogleGroupCache : IGoogleGroupCache
    {
      public IEnumerable<Group> Groups
      {
        get { throw new NotImplementedException(); }
      }

      public Group GetOrCreateGroup(string groupName)
      {
        throw new NotImplementedException();
      }

      public bool IsDefaultGroupId(string id)
      {
        throw new NotImplementedException();
      }

      public bool IsDefaultGroup(Group @group)
      {
        throw new NotImplementedException();
      }

      public void AddDefaultGroupToContact(Contact contact)
      {
        throw new NotImplementedException();
      }
    }

    class NullGoogleContactCache : IGoogleContactCache
    {
      public bool TryGetValue(string key, out Contact value)
      {
        throw new NotImplementedException();
      }

      public Task<IEnumerable<EntityVersion<string, GoogleContactVersion>>> GetAllVersions()
      {
        throw new NotImplementedException();
      }
    }
  }
}
