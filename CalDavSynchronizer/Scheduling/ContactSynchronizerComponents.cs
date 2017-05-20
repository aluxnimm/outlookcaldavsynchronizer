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
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.Implementation.Contacts;
using CalDavSynchronizer.Utilities;
using GenSync.EntityRelationManagement;
using GenSync.EntityRepositories;
using GenSync.Synchronization.StateFactories;
using Thought.vCards;

namespace CalDavSynchronizer.Scheduling
{
  public class ContactSynchronizerComponents
  {
    public ContactSynchronizerComponents(Options options, OutlookContactRepository<ICardDavRepositoryLogger> atypeRepository, IEntityRepository<WebResourceName, string, vCard, ICardDavRepositoryLogger> btypeRepository, EntitySyncStateFactory<string, DateTime, IContactItemWrapper, WebResourceName, string, vCard, ICardDavRepositoryLogger> syncStateFactory, EntityRelationDataAccess<string, DateTime, OutlookContactRelationData, WebResourceName, string> storageDataAccess, OutlookContactRelationDataFactory entityRelationDataFactory, IEqualityComparer<WebResourceName> btypeIdEqualityComparer, EqualityComparer<string> atypeIdEqulityComparer, IWebDavClient webDavClientOrNullIfFileAccess, IChunkedExecutor chunkedExecutor, LoggingCardDavRepositoryDecorator repository, ContactMappingConfiguration mappingParameters, string storageDataDirectory, Uri serverUrl, ICardDavDataAccess cardDavDataAccess)
    {
      Options = options;
      AtypeRepository = atypeRepository;
      BtypeRepository = btypeRepository;
      SyncStateFactory = syncStateFactory;
      StorageDataAccess = storageDataAccess;
      EntityRelationDataFactory = entityRelationDataFactory;
      BtypeIdEqualityComparer = btypeIdEqualityComparer;
      AtypeIdEqulityComparer = atypeIdEqulityComparer;
      WebDavClientOrNullIfFileAccess = webDavClientOrNullIfFileAccess;
      ChunkedExecutor = chunkedExecutor;
      Repository = repository;
      MappingParameters = mappingParameters;
      StorageDataDirectory = storageDataDirectory;
      ServerUrl = serverUrl;
      CardDavDataAccess = cardDavDataAccess;
    }

    public Options Options { get; private set; }
    public OutlookContactRepository<ICardDavRepositoryLogger> AtypeRepository { get; private set; }
    public IEntityRepository<WebResourceName, string, vCard, ICardDavRepositoryLogger> BtypeRepository { get; set; }
    public EntitySyncStateFactory<string, DateTime, IContactItemWrapper, WebResourceName, string, vCard, ICardDavRepositoryLogger> SyncStateFactory { get; private set; }
    public EntityRelationDataAccess<string, DateTime, OutlookContactRelationData, WebResourceName, string> StorageDataAccess { get; private set; }
    public OutlookContactRelationDataFactory EntityRelationDataFactory { get; private set; }
    public IEqualityComparer<WebResourceName> BtypeIdEqualityComparer { get; private set; }
    public EqualityComparer<string> AtypeIdEqulityComparer { get; private set; }
    public IWebDavClient WebDavClientOrNullIfFileAccess { get; }
    public IChunkedExecutor ChunkedExecutor { get; }
    public LoggingCardDavRepositoryDecorator Repository { get; }
    public ContactMappingConfiguration MappingParameters { get; }
    public string StorageDataDirectory { get; }
    public Uri ServerUrl { get; }
    public ICardDavDataAccess CardDavDataAccess { get; }
  }
}