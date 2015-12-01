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
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation;
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.Implementation.Contacts;
using CalDavSynchronizer.Implementation.Events;
using CalDavSynchronizer.Implementation.Tasks;
using CalDavSynchronizer.Implementation.TimeRangeFiltering;
using CalDavSynchronizer.Synchronization;
using CalDavSynchronizer.Utilities;
using DDay.iCal;
using DDay.iCal.Serialization.iCalendar;
using GenSync.EntityRelationManagement;
using GenSync.EntityRepositories;
using GenSync.ProgressReport;
using GenSync.Synchronization;
using GenSync.Synchronization.StateFactories;
using log4net;
using Microsoft.Office.Interop.Outlook;
using Thought.vCards;

namespace CalDavSynchronizer.Scheduling
{
  public class SynchronizerFactory : ISynchronizerFactory
  {
    private static readonly ILog s_logger = LogManager.GetLogger (System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType);

    private readonly string _outlookEmailAddress;
    private readonly ITotalProgressFactory _totalProgressFactory;
    private readonly NameSpace _outlookSession;
    private readonly TimeSpan _calDavConnectTimeout;
    private readonly TimeSpan _calDavReadWriteTimeout;
    private readonly Func<Guid, string> _profileDataDirectoryFactory;

    public SynchronizerFactory (
        Func<Guid, string> profileDataDirectoryFactory,
        ITotalProgressFactory totalProgressFactory,
        NameSpace outlookSession,
        TimeSpan calDavConnectTimeout,
        TimeSpan calDavReadWriteTimeout)
    {
      _outlookEmailAddress = outlookSession.CurrentUser.Address;
      _totalProgressFactory = totalProgressFactory;
      _outlookSession = outlookSession;
      _calDavConnectTimeout = calDavConnectTimeout;
      _calDavReadWriteTimeout = calDavReadWriteTimeout;
      _profileDataDirectoryFactory = profileDataDirectoryFactory;
    }

    public IOutlookSynchronizer CreateSynchronizer (Options options)
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

    private OutlookSynchronizer CreateEventSynchronizer (Options options)
    {
      var calDavDataAccess = new CalDavDataAccess (
          new Uri (options.CalenderUrl),
          CreateWebDavClient (options, _calDavConnectTimeout));

      var storageDataDirectory = _profileDataDirectoryFactory (options.Id);

      var entityRelationDataAccess = new EntityRelationDataAccess<string, DateTime, OutlookEventRelationData, Uri, string> (storageDataDirectory);

      return CreateEventSynchronizer (options, calDavDataAccess, entityRelationDataAccess);
    }

    public static IWebDavClient CreateWebDavClient (Options options, TimeSpan timeout)
    {
      return CreateWebDavClient (
          options.UserName,
          options.Password,
          timeout,
          options.ServerAdapterType,
          options.CloseAfterEachRequest,
          options.ProxyOptions);
    }

    public static IWebDavClient CreateWebDavClient (
        string username,
        string password,
        TimeSpan timeout,
        ServerAdapterType serverAdapterType,
        bool closeConnectionAfterEachRequest,
        ProxyOptions proxyOptions
        )
    {
      switch (serverAdapterType)
      {
        case ServerAdapterType.Default:
        case ServerAdapterType.GoogleOAuth:
          var productAndVersion = GetProductAndVersion();
          return new DataAccess.HttpClientBasedClient.WebDavClient (
              () => CreateHttpClient (username, password, timeout, serverAdapterType, proxyOptions),
              productAndVersion.Item1,
              productAndVersion.Item2,
              closeConnectionAfterEachRequest);

        case ServerAdapterType.SynchronousWebRequestBased:
          return new DataAccess.WebRequestBasedClient.WebDavClient (
              username, password, timeout, timeout);
        default:
          throw new ArgumentOutOfRangeException ("serverAdapterType");
      }
    }

    private static async Task<HttpClient> CreateHttpClient (string username, string password, TimeSpan calDavConnectTimeout, ServerAdapterType serverAdapterType, ProxyOptions proxyOptions)
    {
      IWebProxy proxy = (proxyOptions != null) ? CreateProxy (proxyOptions) : null;

      switch (serverAdapterType)
      {
        case ServerAdapterType.Default:
          var httpClientHandler = new HttpClientHandler();
          if (!string.IsNullOrEmpty (username))
          {
            httpClientHandler.Credentials = new NetworkCredential (username, password);
            httpClientHandler.AllowAutoRedirect = false;
          }
          httpClientHandler.Proxy = proxy;
          httpClientHandler.UseProxy = (proxy != null);

          var httpClient = new HttpClient (httpClientHandler);
          httpClient.Timeout = calDavConnectTimeout;
          return httpClient;
        case ServerAdapterType.GoogleOAuth:
          return await OAuth.Google.GoogleHttpClientFactory.CreateHttpClient (username, GetProductWithVersion(), proxy);
        default:
          throw new ArgumentOutOfRangeException ("serverAdapterType");
      }
    }

    private static IWebProxy CreateProxy (ProxyOptions proxyOptions)
    {
      IWebProxy proxy = null;

      if (proxyOptions.ProxyUseDefault)
      {
        proxy = WebRequest.DefaultWebProxy;
        proxy.Credentials = CredentialCache.DefaultCredentials;
      }
      else if (proxyOptions.ProxyUseManual)
      {
        proxy = new WebProxy (proxyOptions.ProxyUrl, false);
        if (!string.IsNullOrEmpty (proxyOptions.ProxyUserName))
        {
          proxy.Credentials = new NetworkCredential (proxyOptions.ProxyUserName, proxyOptions.ProxyPassword);
        }
        else
        {
          proxy.Credentials = CredentialCache.DefaultCredentials;
        }
      }
      return proxy;
    }

    private static Tuple<string, string> GetProductAndVersion ()
    {
      var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
      return Tuple.Create ("CalDavSynchronizer", string.Format ("{0}.{1}", version.Major, version.Minor));
    }

    private static string GetProductWithVersion ()
    {
      var productAndVersion = GetProductAndVersion();
      return string.Format ("{0}/{1}", productAndVersion.Item1, productAndVersion.Item2);
    }

    /// <remarks>
    /// Public because it is being used by integration tests
    /// </remarks>
    public OutlookSynchronizer CreateEventSynchronizer (
        Options options,
        ICalDavDataAccess calDavDataAccess,
        IEntityRelationDataAccess<string, DateTime, Uri, string> entityRelationDataAccess)
    {
      var dateTimeRangeProvider =
          options.IgnoreSynchronizationTimeRange ?
              NullDateTimeRangeProvider.Instance :
              new DateTimeRangeProvider (options.DaysToSynchronizeInThePast, options.DaysToSynchronizeInTheFuture);

      var mappingParameters = GetMappingParameters<EventMappingConfiguration> (options);

      var atypeRepository = new OutlookEventRepository (
          _outlookSession,
          options.OutlookFolderEntryId,
          options.OutlookFolderStoreId,
          dateTimeRangeProvider,
          mappingParameters);

      IEntityRepository<IICalendar, Uri, string> btypeRepository = new CalDavRepository (
          calDavDataAccess,
          new iCalendarSerializer(),
          CalDavRepository.EntityType.Event,
          dateTimeRangeProvider);

      var entityMapper = new EventEntityMapper (
          _outlookEmailAddress, new Uri ("mailto:" + options.EmailAddress),
          _outlookSession.Application.TimeZones.CurrentTimeZone.ID,
          _outlookSession.Application.Version,
          mappingParameters);

      var outlookEventRelationDataFactory = new OutlookEventRelationDataFactory();

      var syncStateFactory = new EntitySyncStateFactory<string, DateTime, AppointmentItemWrapper, Uri, string, IICalendar> (
          entityMapper,
          atypeRepository,
          btypeRepository,
          outlookEventRelationDataFactory,
          ExceptionHandler.Instance
          );

      var btypeIdEqualityComparer = EqualityComparer<Uri>.Default;
      var atypeIdEqualityComparer = EqualityComparer<string>.Default;

      var synchronizer = new Synchronizer<string, DateTime, AppointmentItemWrapper, Uri, string, IICalendar> (
          atypeRepository,
          btypeRepository,
          InitialEventSyncStateCreationStrategyFactory.Create (
              syncStateFactory,
              syncStateFactory.Environment,
              options.SynchronizationMode,
              options.ConflictResolution),
          entityRelationDataAccess,
          outlookEventRelationDataFactory,
          new InitialEventEntityMatcher (btypeIdEqualityComparer),
          atypeIdEqualityComparer,
          btypeIdEqualityComparer,
          _totalProgressFactory,
          ExceptionHandler.Instance);

      return new OutlookSynchronizer (synchronizer, atypeRepository);
    }

    private T GetMappingParameters<T> (Options options)
        where T : class, new()
    {
      if (options.MappingConfiguration == null)
        return new T();

      var parameters = options.MappingConfiguration as T;

      if (parameters != null)
        return parameters;

      s_logger.ErrorFormat (
          "Expected mapping parameters of type '{0}', but found type of '{1}'. Falling back to default",
          typeof (T).Name,
          options.MappingConfiguration.GetType().Name);
      return new T();
    }

    private OutlookSynchronizer CreateTaskSynchronizer (Options options)
    {
      // TODO: dispose folder like it is done for events
      var calendarFolder = (Folder) _outlookSession.GetFolderFromID (options.OutlookFolderEntryId, options.OutlookFolderStoreId);
      var atypeRepository = new OutlookTaskRepository (calendarFolder, _outlookSession);

      var btypeRepository = new CalDavRepository (
          new CalDavDataAccess (
              new Uri (options.CalenderUrl),
              CreateWebDavClient (
                  options.UserName,
                  options.Password,
                  _calDavConnectTimeout,
                  options.ServerAdapterType,
                  options.CloseAfterEachRequest,
                  options.ProxyOptions)),
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

      var storageDataDirectory = _profileDataDirectoryFactory (options.Id);

      var btypeIdEqualityComparer = EqualityComparer<Uri>.Default;
      var atypeIdEqualityComparer = EqualityComparer<string>.Default;

      var synchronizer = new Synchronizer<string, DateTime, TaskItemWrapper, Uri, string, IICalendar> (
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

      return new OutlookSynchronizer (synchronizer, atypeRepository);
    }

    private OutlookSynchronizer CreateContactSynchronizer (Options options)
    {
      var atypeRepository = new OutlookContactRepository (
          _outlookSession,
          options.OutlookFolderEntryId,
          options.OutlookFolderStoreId);


      IEntityRepository<vCard, Uri, string> btypeRepository = new CardDavRepository (
          new CardDavDataAccess (
              new Uri (options.CalenderUrl),
              CreateWebDavClient (
                  options.UserName,
                  options.Password,
                  _calDavConnectTimeout,
                  options.ServerAdapterType,
                  options.CloseAfterEachRequest,
                  options.ProxyOptions)));

      var mappingParameters = GetMappingParameters<ContactMappingConfiguration>(options);

      var entityMapper = new ContactEntityMapper (mappingParameters);

      var entityRelationDataFactory = new OutlookContactRelationDataFactory();

      var syncStateFactory = new EntitySyncStateFactory<string, DateTime, ContactItemWrapper, Uri, string, vCard> (
          entityMapper,
          atypeRepository,
          btypeRepository,
          entityRelationDataFactory,
          ExceptionHandler.Instance);

      var btypeIdEqualityComparer = EqualityComparer<Uri>.Default;
      var atypeIdEqulityComparer = EqualityComparer<string>.Default;

      var storageDataDirectory = _profileDataDirectoryFactory (options.Id);

      var storageDataAccess = new EntityRelationDataAccess<string, DateTime, OutlookContactRelationData, Uri, string> (storageDataDirectory);

      var synchronizer = new Synchronizer<string, DateTime, ContactItemWrapper, Uri, string, vCard> (
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

      return new OutlookSynchronizer (synchronizer, atypeRepository);
    }
  }
}