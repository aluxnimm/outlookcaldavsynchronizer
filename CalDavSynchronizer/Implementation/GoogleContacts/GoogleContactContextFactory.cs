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
using GenSync.Synchronization;
using Google.Contacts;

namespace CalDavSynchronizer.Implementation.GoogleContacts
{
  class GoogleContactContextFactory : ISynchronizationContextFactory<IGoogleContactContext>
  {
    private readonly IGoogleApiOperationExecutor _apiOperationExecutor;
    private readonly IEqualityComparer<string> _contactIdComparer;
    private readonly string _userName;
    private readonly int _readChunkSize;

    public GoogleContactContextFactory (IGoogleApiOperationExecutor apiOperationExecutor, IEqualityComparer<string> contactIdComparer, string userName, int readChunkSize)
    {
      if (apiOperationExecutor == null)
        throw new ArgumentNullException (nameof (apiOperationExecutor));
      if (contactIdComparer == null)
        throw new ArgumentNullException (nameof (contactIdComparer));
      if (String.IsNullOrEmpty (userName))
        throw new ArgumentException ("Argument is null or empty", nameof (userName));

      _apiOperationExecutor = apiOperationExecutor;
      _contactIdComparer = contactIdComparer;
      _userName = userName;
      _readChunkSize = readChunkSize;
    }

    public async Task<IGoogleContactContext> Create ()
    {
      return await Task.Run (() =>
      {
        var googleGroupCache = new GoogleGroupCache (_apiOperationExecutor);
        googleGroupCache.Fill();
        
        var googleContactCache = new GoogleContactCache(_apiOperationExecutor, _contactIdComparer, _userName, _readChunkSize);
        googleContactCache.Fill(googleGroupCache.DefaultGroupIdOrNull);

        var context = new GoogleContactContext (
          googleGroupCache,
          googleContactCache);
       
        return context;
      });
    }

    public Task SynchronizationFinished (IGoogleContactContext context)
    {
      return Task.FromResult(0);
    }
  }
}