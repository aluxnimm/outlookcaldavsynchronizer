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
using System.Configuration;
using System.IO;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation;
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.Implementation.Contacts;
using CalDavSynchronizer.Implementation.Events;
using CalDavSynchronizer.Implementation.Tasks;
using CalDavSynchronizer.Implementation.TimeRangeFiltering;
using CalDavSynchronizer.Utilities;
using DDay.iCal;
using DDay.iCal.Serialization.iCalendar;
using GenSync.EntityRelationManagement;
using GenSync.EntityRepositories;
using GenSync.ProgressReport;
using GenSync.Synchronization;
using GenSync.Synchronization.StateFactories;
using Microsoft.Office.Interop.Outlook;
using Thought.vCards;

namespace CalDavSynchronizer.Scheduling
{
  internal class SynchronizerFactory : ISynchronizerFactory
  {
    private readonly string _outlookEmailAddress;
    private readonly string _applicationDataDirectory;
    private readonly ITotalProgressFactory _totalProgressFactory;
    private readonly NameSpace _outlookSession;

    public SynchronizerFactory (string applicationDataDirectory, ITotalProgressFactory totalProgressFactory, NameSpace outlookSession)
    {
      _outlookEmailAddress = outlookSession.CurrentUser.Address;
      _applicationDataDirectory = applicationDataDirectory;
      _totalProgressFactory = totalProgressFactory;
      _outlookSession = outlookSession;
    }

    public ISynchronizer CreateSynchronizer (Options options)
    {
      OlItemType defaultItemType;
      string folderName;

      using (var outlookFolderWrapper = GenericComObjectWrapper.Create ((Folder) _outlookSession.GetFolderFromID (options.OutlookFolderEntryId, options.OutlookFolderStoreId)))
      {
        defaultItemType = outlookFolderWrapper.Inner.DefaultItemType;
        folderName = outlookFolderWrapper.Inner.Name;
      }

      switch (defaultItemType)
      {
        case OlItemType.olAppointmentItem:
          return CreateEventSynchronizer (options);
        case OlItemType.olTaskItem:
          return CreateTaskSynchronizer (options);
        case OlItemType.olContactItem:
          return CreateContactSynchronizer (options);
        default:
          throw new NotSupportedException (
              string.Format (
                  "The folder '{0}' contains an item type ('{1}'), whis is not supported for synchronization",
                  folderName,
                  defaultItemType));
      }
    }

    private ISynchronizer CreateEventSynchronizer (Options options)
    {
      var dateTimeRangeProvider =
          options.IgnoreSynchronizationTimeRange ?
              NullDateTimeRangeProvider.Instance :
              new DateTimeRangeProvider (options.DaysToSynchronizeInThePast, options.DaysToSynchronizeInTheFuture);

      var atypeRepository = new OutlookEventRepository (
          _outlookSession,
          options.OutlookFolderEntryId,
          options.OutlookFolderStoreId,
          dateTimeRangeProvider);

      IEntityRepository<IICalendar, Uri, string> btypeRepository = new CalDavRepository (
          new CalDavDataAccess (
              new Uri (options.CalenderUrl),
              new CalDavClient (
                  options.UserName,
                  options.Password,
                  TimeSpan.Parse (ConfigurationManager.AppSettings["calDavConnectTimeout"]),
                  TimeSpan.Parse (ConfigurationManager.AppSettings["calDavReadWriteTimeout"]),
                  Boolean.Parse (ConfigurationManager.AppSettings["disableCertificateValidation"]),
                  Boolean.Parse (ConfigurationManager.AppSettings["enableSsl3"]),
                  Boolean.Parse (ConfigurationManager.AppSettings["enableTls12"]))),
          new iCalendarSerializer(),
          CalDavRepository.EntityType.Event,
          dateTimeRangeProvider);

      if (StringComparer.InvariantCultureIgnoreCase.Compare (new Uri (options.CalenderUrl).Host, "www.google.com") == 0)
      {
        btypeRepository = new EntityRepositoryDeleteCreateInstaedOfUpdateWrapper<IICalendar, Uri, string> (btypeRepository);
      }

      var entityMapper = new EventEntityMapper (
          _outlookEmailAddress, new Uri ("mailto:" + options.EmailAddress),
          _outlookSession.Application.TimeZones.CurrentTimeZone.ID,
          _outlookSession.Application.Version);

      var outlookEventRelationDataFactory = new OutlookEventRelationDataFactory();

      var syncStateFactory = new EntitySyncStateFactory<string, DateTime, AppointmentItemWrapper, Uri, string, IICalendar> (
          entityMapper,
          atypeRepository,
          btypeRepository,
          outlookEventRelationDataFactory,
          ExceptionHandler.Instance
          );

      var storageDataDirectory = Path.Combine (
          _applicationDataDirectory,
          options.Id.ToString()
          );

      var btypeIdEqualityComparer = EqualityComparer<Uri>.Default;
      var atypeIdEqualityComparer = EqualityComparer<string>.Default;

      return new Synchronizer<string, DateTime, AppointmentItemWrapper, Uri, string, IICalendar> (
          atypeRepository,
          btypeRepository,
          InitialEventSyncStateCreationStrategyFactory.Create (
              syncStateFactory,
              syncStateFactory.Environment,
              options.SynchronizationMode,
              options.ConflictResolution),
          new EntityRelationDataAccess<string, DateTime, OutlookEventRelationData, Uri, string> (storageDataDirectory),
          outlookEventRelationDataFactory,
          new InitialEventEntityMatcher (btypeIdEqualityComparer),
          atypeIdEqualityComparer,
          btypeIdEqualityComparer,
          _totalProgressFactory,
          ExceptionHandler.Instance);
    }

    private ISynchronizer CreateTaskSynchronizer (Options options)
    {
      // TODO: dispose folder like it is done for events
      var calendarFolder = (Folder) _outlookSession.GetFolderFromID (options.OutlookFolderEntryId, options.OutlookFolderStoreId);
      var atypeRepository = new OutlookTaskRepository (calendarFolder, _outlookSession);

      var btypeRepository = new CalDavRepository (
          new CalDavDataAccess (
              new Uri (options.CalenderUrl),
              new CalDavClient (
                  options.UserName,
                  options.Password,
                  TimeSpan.Parse (ConfigurationManager.AppSettings["calDavConnectTimeout"]),
                  TimeSpan.Parse (ConfigurationManager.AppSettings["calDavReadWriteTimeout"]),
                  Boolean.Parse (ConfigurationManager.AppSettings["disableCertificateValidation"]),
                  Boolean.Parse (ConfigurationManager.AppSettings["enableSsl3"]),
                  Boolean.Parse (ConfigurationManager.AppSettings["enableTls12"]))
              ),
          new iCalendarSerializer(),
          CalDavRepository.EntityType.Todo,
          NullDateTimeRangeProvider.Instance);

      var outlookEventRelationDataFactory = new OutlookEventRelationDataFactory();
      var syncStateFactory = new EntitySyncStateFactory<string, DateTime, TaskItemWrapper, Uri, string, IICalendar> (
          new TaskMapper (_outlookSession.Application.TimeZones.CurrentTimeZone.ID),
          atypeRepository,
          btypeRepository,
          outlookEventRelationDataFactory,
          ExceptionHandler.Instance);

      var storageDataDirectory = Path.Combine (
          _applicationDataDirectory,
          options.Id.ToString()
          );

      var btypeIdEqualityComparer = EqualityComparer<Uri>.Default;
      var atypeIdEqualityComparer = EqualityComparer<string>.Default;

      return new Synchronizer<string, DateTime, TaskItemWrapper, Uri, string, IICalendar> (
          atypeRepository,
          btypeRepository,
          InitialTaskSyncStateCreationStrategyFactory.Create (
              syncStateFactory,
              syncStateFactory.Environment,
              options.SynchronizationMode,
              options.ConflictResolution),
          new EntityRelationDataAccess<string, DateTime, OutlookEventRelationData, Uri, string> (storageDataDirectory),
          outlookEventRelationDataFactory,
          new InitialTaskEntityMatcher (btypeIdEqualityComparer),
          atypeIdEqualityComparer,
          btypeIdEqualityComparer,
          _totalProgressFactory,
          ExceptionHandler.Instance);
    }

    private ISynchronizer CreateContactSynchronizer (Options options)
    {
      var atypeRepository = new OutlookContactRepository (
          _outlookSession,
          options.OutlookFolderEntryId,
          options.OutlookFolderStoreId);

      IEntityRepository<vCard, Uri, string> btypeRepository = new CardDavRepository (
          new CardDavDataAccess (
              new Uri (options.CalenderUrl),
              new CardDavClient (
                  options.UserName,
                  options.Password,
                  TimeSpan.Parse (ConfigurationManager.AppSettings["calDavConnectTimeout"]),
                  TimeSpan.Parse (ConfigurationManager.AppSettings["calDavReadWriteTimeout"]),
                  Boolean.Parse (ConfigurationManager.AppSettings["disableCertificateValidation"]),
                  Boolean.Parse (ConfigurationManager.AppSettings["enableSsl3"]),
                  Boolean.Parse (ConfigurationManager.AppSettings["enableTls12"]))
              ));

      if (StringComparer.InvariantCultureIgnoreCase.Compare (new Uri (options.CalenderUrl).Host, "www.google.com") == 0)
      {
        btypeRepository = new EntityRepositoryDeleteCreateInstaedOfUpdateWrapper<vCard, Uri, string> (btypeRepository);
      }

      var entityRelationDataFactory = new OutlookContactRelationDataFactory();

      var syncStateFactory = new EntitySyncStateFactory<string, DateTime, GenericComObjectWrapper<ContactItem>, Uri, string, vCard> (
          new ContactEntityMapper(),
          atypeRepository,
          btypeRepository,
          entityRelationDataFactory,
          ExceptionHandler.Instance);

      var btypeIdEqualityComparer = EqualityComparer<Uri>.Default;
      var atypeIdEqulityComparer = EqualityComparer<string>.Default;

      var storageDataDirectory = Path.Combine (
          _applicationDataDirectory,
          options.Id.ToString()
          );

      var storageDataAccess = new EntityRelationDataAccess<string, DateTime, OutlookContactRelationData, Uri, string> (storageDataDirectory);

      return new Synchronizer<string, DateTime, GenericComObjectWrapper<ContactItem>, Uri, string, vCard> (
          atypeRepository,
          btypeRepository,
          InitialContactSyncStateCreationStrategyFactory.Create (
              syncStateFactory,
              syncStateFactory.Environment,
              options.SynchronizationMode,
              options.ConflictResolution),
          storageDataAccess,
          entityRelationDataFactory,
          new InitialContactEntityMatcher (btypeIdEqualityComparer),
          atypeIdEqulityComparer,
          btypeIdEqualityComparer,
          _totalProgressFactory,
          ExceptionHandler.Instance);
    }
  }
}