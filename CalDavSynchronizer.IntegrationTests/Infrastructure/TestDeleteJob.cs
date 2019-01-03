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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenSync;
using GenSync.EntityRepositories;
using GenSync.Logging;

namespace CalDavSynchronizer.IntegrationTests.Infrastructure
{
  class TestDeleteJob<TEntityId, TEntityVersion> : IDeleteJob<TEntityId, TEntityVersion>
  {
    private readonly EntityVersion<TEntityId, TEntityVersion> _version;
    public IEntitySynchronizationLogger Logger { get; } = NullEntitySynchronizationLogger.Instance;
    public TEntityId EntityId => _version.Id;
    public TEntityVersion Version => _version.Version;

    public TestDeleteJob (EntityVersion<TEntityId, TEntityVersion> version)
    {
      if (version == null)
        throw new ArgumentNullException (nameof (version));
      _version = version;
    }

    public void NotifyOperationSuceeded ()
    {
    }

    public void NotifyEntityNotFound ()
    {
      throw new Exception ("Test Failed");
    }

    public void NotifyOperationFailed (Exception exception)
    {
      throw new Exception ("Test Failed");
    }

    public void NotifyOperationFailed (string errorMessage)
    {
      throw new Exception ("Test Failed");
    }
  }
}
