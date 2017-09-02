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
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.Implementation.Contacts;
using CalDavSynchronizer.Implementation.DistributionLists;
using GenSync;
using GenSync.EntityRelationManagement;
using GenSync.EntityRepositories;
using GenSync.Logging;
using GenSync.Synchronization;
using Microsoft.Office.Interop.Outlook;
using Thought.vCards;

namespace CalDavSynchronizer.Implementation
{
  public class ContactAndDistListSynchronizer
    : IPartialSynchronizer<string, DateTime, WebResourceName, string>
  {
    private readonly IPartialSynchronizer<string, DateTime, WebResourceName, string, ICardDavRepositoryLogger> _contactSynchronizer;
    private readonly ISynchronizer<DistributionListSychronizationContext> _distributionListSynchronizer;
    private readonly EmailAddressCacheDataAccess _emailAddressCacheDataAccess;
    private readonly IEntityRepository<WebResourceName, string, vCard, ICardDavRepositoryLogger> _loggingCardDavRepositoryDecorator;
    private readonly IOutlookSession _outlookSession;

    public ContactAndDistListSynchronizer(
      IPartialSynchronizer<string, DateTime, WebResourceName, string, ICardDavRepositoryLogger> contactSynchronizer, 
      ISynchronizer<DistributionListSychronizationContext> distributionListSynchronizer,
      EmailAddressCacheDataAccess emailAddressCacheDataAccess,
      IEntityRepository<WebResourceName, string, vCard, ICardDavRepositoryLogger> loggingCardDavRepositoryDecorator, 
      IOutlookSession outlookSession)
    {
      if (contactSynchronizer == null) throw new ArgumentNullException(nameof(contactSynchronizer));
      if (distributionListSynchronizer == null) throw new ArgumentNullException(nameof(distributionListSynchronizer));
      if (loggingCardDavRepositoryDecorator == null) throw new ArgumentNullException(nameof(loggingCardDavRepositoryDecorator));
      if (outlookSession == null) throw new ArgumentNullException(nameof(outlookSession));

      _contactSynchronizer = contactSynchronizer;
      _distributionListSynchronizer = distributionListSynchronizer;
      _emailAddressCacheDataAccess = emailAddressCacheDataAccess;
      _loggingCardDavRepositoryDecorator = loggingCardDavRepositoryDecorator;
      _outlookSession = outlookSession;
    }

    public async Task Synchronize (ISynchronizationLogger logger)
    {
      var emailAddressCache = new EmailAddressCache();
      emailAddressCache.Items = _emailAddressCacheDataAccess.Load();

      using (var subLogger = logger.CreateSubLogger("Contacts"))
      {
        await _contactSynchronizer.Synchronize(subLogger, emailAddressCache);
      }

      var idsToQuery = emailAddressCache.GetEmptyCacheItems();
      if (idsToQuery.Length > 0)
        await _loggingCardDavRepositoryDecorator.Get(idsToQuery, NullLoadEntityLogger.Instance, emailAddressCache);
      var cacheItems = emailAddressCache.Items;
      _emailAddressCacheDataAccess.Save(cacheItems);

      var distListContext = new DistributionListSychronizationContext(cacheItems, _outlookSession);

      using (var subLogger = logger.CreateSubLogger("DistLists"))
      {
        await _distributionListSynchronizer.Synchronize(subLogger, distListContext);
      }
    }

    public async Task SynchronizePartial(IEnumerable<IIdWithHints<string, DateTime>> aIds, IEnumerable<IIdWithHints<WebResourceName, string>> bIds, ISynchronizationLogger logger)
    {
      var emailAddressCache = new EmailAddressCache();
      emailAddressCache.Items = _emailAddressCacheDataAccess.Load();

      using (var subLogger = logger.CreateSubLogger("Contacts"))
      {
        await _contactSynchronizer.Synchronize(subLogger, emailAddressCache);
      }

      _emailAddressCacheDataAccess.Save(emailAddressCache.Items);
    }
  }
}