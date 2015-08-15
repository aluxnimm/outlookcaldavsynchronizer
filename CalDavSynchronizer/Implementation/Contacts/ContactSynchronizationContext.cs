// This file is Part of CalDavSynchronizer (http://outlookcaldavsynchronizer.sourceforge.net/)
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
using System.Collections.Generic;
using System.Reflection;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.Implementation.Events;
using CalDavSynchronizer.Implementation.TimeRangeFiltering;
using DDay.iCal;
using DDay.iCal.Serialization.iCalendar;
using GenSync.EntityMapping;
using GenSync.EntityRelationManagement;
using GenSync.EntityRepositories;
using GenSync.InitialEntityMatching;
using GenSync.Synchronization;
using log4net;
using Microsoft.Office.Interop.Outlook;
using Thought.vCards;

namespace CalDavSynchronizer.Implementation.Contacts
{
  public class ContactSynchronizationContext : ISynchronizerContext<string, DateTime, GenericComObjectWrapper<ContactItem>, Uri, string, vCard>
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private readonly IEntityRelationDataAccess<string, DateTime, Uri, string> _storageDataAccess;
    private readonly ContactEntityMapper _entityMapper;
    private readonly OutlookContactRepository _atypeRepository;
    private readonly IEntityRepository<vCard, Uri, string> _btypeRepository;
    private readonly IEntityRelationDataFactory<string, DateTime, Uri, string> _entityRelationDataFactory;

    public ContactSynchronizationContext (NameSpace outlookSession, IEntityRelationDataAccess<string, DateTime, Uri, string> storageDataAccess, Options options, string outlookEmailAddress, TimeSpan connectTimeout, TimeSpan readWriteTimeout, bool disableCertValidation, bool useSsl3, bool useTls12, IEqualityComparer<Uri> btypeIdEqualityComparer)
    {
      if (outlookSession == null)
        throw new ArgumentNullException ("outlookSession");

      _entityRelationDataFactory = new OutlookContactRelationDataFactory();

      _entityMapper = new ContactEntityMapper();

      _atypeRepository = new OutlookContactRepository (
          outlookSession,
          options.OutlookFolderEntryId,
          options.OutlookFolderStoreId);

      _btypeRepository = new CardDavRepository (
          new CardDavDataAccess (
              new Uri (options.CalenderUrl),
              new CardDavClient (
                  options.UserName,
                  options.Password,
                  connectTimeout,
                  readWriteTimeout,
                  disableCertValidation,
                  useSsl3,
                  useTls12)
              ));

      if (StringComparer.InvariantCultureIgnoreCase.Compare (new Uri (options.CalenderUrl).Host, "www.google.com") == 0)
      {
        _btypeRepository = new EntityRepositoryDeleteCreateInstaedOfUpdateWrapper<vCard, Uri, string> (_btypeRepository);
      }

      _storageDataAccess = storageDataAccess;

      InitialEntityMatcher = new InitialContactEntityMatcher (btypeIdEqualityComparer);
    }

    public IEntityMapper<GenericComObjectWrapper<ContactItem>, vCard> EntityMapper
    {
      get { return _entityMapper; }
    }

    public IEntityRepository<GenericComObjectWrapper<ContactItem>, string, DateTime> AtypeRepository
    {
      get { return _atypeRepository; }
    }

    public IEntityRepository<vCard, Uri, string> BtypeRepository
    {
      get { return _btypeRepository; }
    }

    public IInitialEntityMatcher<GenericComObjectWrapper<ContactItem>, string, DateTime, vCard, Uri, string> InitialEntityMatcher { get; private set; }

    public IEntityRelationDataFactory<string, DateTime, Uri, string> EntityRelationDataFactory
    {
      get { return _entityRelationDataFactory; }
    }

    public IEnumerable<IEntityRelationData<string, DateTime, Uri, string>> LoadEntityRelationData ()
    {
      return _storageDataAccess.LoadEntityRelationData();
    }

    public void SaveEntityRelationData (List<IEntityRelationData<string, DateTime, Uri, string>> data)
    {
      _storageDataAccess.SaveEntityRelationData (data);
    }
  }
}