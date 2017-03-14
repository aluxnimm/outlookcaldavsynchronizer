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
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.Implementation.Contacts;
using GenSync.EntityRelationManagement;
using GenSync.EntityRepositories;
using GenSync.Logging;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Implementation.DistributionLists
{
  public class DistributionListSychronizationContext
  {
    private readonly Dictionary<string, string> _serverFileNamesByEmailAddress = new Dictionary<string, string>();

    public DistributionListSychronizationContext(CacheItem[] emailAddressCacheItems, IOutlookSession outlookSession)
    {
      if (emailAddressCacheItems == null) throw new ArgumentNullException(nameof(emailAddressCacheItems));
      if (outlookSession == null) throw new ArgumentNullException(nameof(outlookSession));

      OutlookSession = outlookSession;
      foreach (var cacheItem in emailAddressCacheItems)
      {
        foreach (var emailAddress in cacheItem.EmailAddresses)
        {
          var serverFileName = cacheItem.Id.GetServerFileName();
          _serverFileNamesByEmailAddress[emailAddress] = serverFileName;
        }
      }
    }

    public IOutlookSession OutlookSession { get; }
    
    public string GetServerFileNameByEmailAddress(string emailAddress)
    {
      string serverFileName;
      if (_serverFileNamesByEmailAddress.TryGetValue(emailAddress, out serverFileName))
        return serverFileName;
      else
        return null;
    }
  }
}
