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
using System.Linq;
using System.Net;
using System.Net.Http;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation;
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.Implementation.Contacts;
using CalDavSynchronizer.Implementation.Events;
using CalDavSynchronizer.Implementation.GoogleTasks;
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
using Google.Apis.Tasks.v1.Data;
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
    private readonly Func<Guid, string> _profileDataDirectoryFactory;
    private readonly IDaslFilterProvider _daslFilterProvider;

    public SynchronizerFactory (
        Func<Guid, string> profileDataDirectoryFactory,
        ITotalProgressFactory totalProgressFactory,
        NameSpace outlookSession,
        TimeSpan calDavConnectTimeout, 
        IDaslFilterProvider daslFilterProvider)
    {
      _outlookEmailAddress = outlookSession.CurrentUser.Address;
      _totalProgressFactory = totalProgressFactory;
      _outlookSession = outlookSession;
      _calDavConnectTimeout = calDavConnectTimeout;
      _daslFilterProvider = daslFilterProvider;
      _profileDataDirectoryFactory = profileDataDirectoryFactory;
    }

    /// <summary>
    /// Components of the created synchronizer 
    /// Fields may be null, if the synchronizer doesn't use the component specified by the field 
    /// </summary>
    public class AvailableSynchronizerComponents
    {
      public ICalDavDataAccess CalDavDataAccess;
      public ICardDavDataAccess CardDavDataAccess;
    }

    public IOutlookSynchronizer CreateSynchronizer (Options options)
    {
      AvailableSynchronizerComponents synchronizerComponents;
      return CreateSynchronizer (options, out synchronizerComponents);
    }

    public IOutlookSynchronizer CreateSynchronizer (Options options,out AvailableSynchronizerComponents synchronizerComponents)
    {
      synchronizerComponents = new AvailableSynchronizerComponents();

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
          return CreateEventSynchronizer (options, synchronizerComponents);
        case OlItemType.olTaskItem:
          if (options.ServerAdapterType == ServerAdapterType.GoogleTaskApi)
            return CreateGoogleTaskSynchronizer (options);
          else
            return CreateTaskSynchronizer (options, synchronizerComponents);
        case OlItemType.olContactItem:
          return CreateContactSynchronizer (options, synchronizerComponents);
        default:
          throw new NotSupportedException (
              string.Format (
                  "The folder '{0}' contains an item type ('{1}'), whis is not supported for synchronization",
                  folderName,
                  defaultItemType));
      }
    }
    
    private IOutlookSynchronizer CreateEventSynchronizer (Options options,AvailableSynchronizerComponents componentsToFill)
    {
      var calDavDataAccess = new CalDavDataAccess (
          new Uri (options.CalenderUrl),
          CreateWebDavClient (options, _calDavConnectTimeout));

      componentsToFill.CalDavDataAccess = calDavDataAccess;

      var storageDataDirectory = _profileDataDirectoryFactory (options.Id);

      var entityRelationDataAccess = new EntityRelationDataAccess<string, DateTime, OutlookEventRelationData, WebResourceName, string> (storageDataDirectory);

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
          options.PreemptiveAuthentication,
          options.ProxyOptions);
    }

    public static IWebDavClient CreateWebDavClient (
        string username,
        string password,
        TimeSpan timeout,
        ServerAdapterType serverAdapterType,
        bool closeConnectionAfterEachRequest,
        bool preemptiveAuthentication,
        ProxyOptions proxyOptions
        )
    {
      switch (serverAdapterType)
      {
        case ServerAdapterType.WebDavHttpClientBased:
        case ServerAdapterType.WebDavHttpClientBasedWithGoogleOAuth:
          var productAndVersion = GetProductAndVersion();
          return new DataAccess.HttpClientBasedClient.WebDavClient (
              () => CreateHttpClient (username, password, timeout, serverAdapterType, proxyOptions, preemptiveAuthentication),
              productAndVersion.Item1,
              productAndVersion.Item2,
              closeConnectionAfterEachRequest);

        case ServerAdapterType.WebDavSynchronousWebRequestBased:
          return new DataAccess.WebRequestBasedClient.WebDavClient (
              username, password, timeout, timeout, closeConnectionAfterEachRequest, preemptiveAuthentication);
        default:
          throw new ArgumentOutOfRangeException ("serverAdapterType");
      }
    }

    private static async System.Threading.Tasks.Task<HttpClient> CreateHttpClient (string username, string password, TimeSpan calDavConnectTimeout, ServerAdapterType serverAdapterType, ProxyOptions proxyOptions, bool preemptiveAuthentication)
    {
      IWebProxy proxy = (proxyOptions != null) ? CreateProxy (proxyOptions) : null;

      switch (serverAdapterType)
      {
        case ServerAdapterType.WebDavHttpClientBased:
          var httpClientHandler = new HttpClientHandler();
          if (!string.IsNullOrEmpty (username))
          {
            httpClientHandler.Credentials = new NetworkCredential (username, password);
            httpClientHandler.AllowAutoRedirect = false;
            httpClientHandler.PreAuthenticate = preemptiveAuthentication;
          }
          httpClientHandler.Proxy = proxy;
          httpClientHandler.UseProxy = (proxy != null);

          var httpClient = new HttpClient (httpClientHandler);
          httpClient.Timeout = calDavConnectTimeout;
          return httpClient;
        case ServerAdapterType.WebDavHttpClientBasedWithGoogleOAuth:
          return await OAuth.Google.GoogleHttpClientFactory.CreateHttpClient (username, GetProductWithVersion(), proxy);
        default:
          throw new ArgumentOutOfRangeException ("serverAdapterType");
      }
    }

    public static IWebProxy CreateProxy (ProxyOptions proxyOptions)
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
    public IOutlookSynchronizer CreateEventSynchronizer (
        Options options,
        ICalDavDataAccess calDavDataAccess,
        IEntityRelationDataAccess<string, DateTime, WebResourceName, string> entityRelationDataAccess)
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
          mappingParameters,
          _daslFilterProvider);

      IEntityRepository<IICalendar, WebResourceName, string> btypeRepository = new CalDavRepository (
          calDavDataAccess,
          new iCalendarSerializer(),
          CalDavRepository.EntityType.Event,
          dateTimeRangeProvider,
          options.ServerAdapterType == ServerAdapterType.WebDavHttpClientBasedWithGoogleOAuth);

      var entityMapper = new EventEntityMapper (
          _outlookEmailAddress, new Uri ("mailto:" + options.EmailAddress),
          _outlookSession.Application.TimeZones.CurrentTimeZone.ID,
          _outlookSession.Application.Version,
          mappingParameters);

      var outlookEventRelationDataFactory = new OutlookEventRelationDataFactory();

      var syncStateFactory = new EntitySyncStateFactory<string, DateTime, AppointmentItemWrapper, WebResourceName, string, IICalendar> (
          entityMapper,
          atypeRepository,
          btypeRepository,
          outlookEventRelationDataFactory,
          ExceptionHandler.Instance
          );

      var btypeIdEqualityComparer = WebResourceName.Comparer;
      var atypeIdEqualityComparer = EqualityComparer<string>.Default;

      var synchronizer = new Synchronizer<string, DateTime, AppointmentItemWrapper, WebResourceName, string, IICalendar> (
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

      return new OutlookSynchronizer<WebResourceName> (synchronizer, atypeRepository);
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

    private IOutlookSynchronizer CreateTaskSynchronizer (Options options, AvailableSynchronizerComponents componentsToFill)
    {
      var atypeRepository = new OutlookTaskRepository (_outlookSession, options.OutlookFolderEntryId, options.OutlookFolderStoreId, _daslFilterProvider);

      var calDavDataAccess = new CalDavDataAccess (
          new Uri (options.CalenderUrl),
          CreateWebDavClient (
              options.UserName,
              options.Password,
              _calDavConnectTimeout,
              options.ServerAdapterType,
              options.CloseAfterEachRequest,
              options.PreemptiveAuthentication,
              options.ProxyOptions));

      componentsToFill.CalDavDataAccess = calDavDataAccess;

      var btypeRepository = new CalDavRepository (
          calDavDataAccess,
          new iCalendarSerializer(),
          CalDavRepository.EntityType.Todo,
          NullDateTimeRangeProvider.Instance,
          false);

      var outlookEventRelationDataFactory = new OutlookEventRelationDataFactory();
      var syncStateFactory = new EntitySyncStateFactory<string, DateTime, TaskItemWrapper, WebResourceName, string, IICalendar> (
          new TaskMapper (_outlookSession.Application.TimeZones.CurrentTimeZone.ID),
          atypeRepository,
          btypeRepository,
          outlookEventRelationDataFactory,
          ExceptionHandler.Instance);

      var storageDataDirectory = _profileDataDirectoryFactory (options.Id);

      var btypeIdEqualityComparer = WebResourceName.Comparer;
      var atypeIdEqualityComparer = EqualityComparer<string>.Default;

      var synchronizer = new Synchronizer<string, DateTime, TaskItemWrapper, WebResourceName, string, IICalendar> (
          atypeRepository,
          btypeRepository,
          InitialTaskSyncStateCreationStrategyFactory.Create (
              syncStateFactory,
              syncStateFactory.Environment,
              options.SynchronizationMode,
              options.ConflictResolution),
          new EntityRelationDataAccess<string, DateTime, OutlookEventRelationData, WebResourceName, string> (storageDataDirectory),
          outlookEventRelationDataFactory,
          new InitialTaskEntityMatcher (btypeIdEqualityComparer),
          atypeIdEqualityComparer,
          btypeIdEqualityComparer,
          _totalProgressFactory,
          ExceptionHandler.Instance);

      return new OutlookSynchronizer<WebResourceName> (synchronizer, atypeRepository);
    }

    private IOutlookSynchronizer CreateGoogleTaskSynchronizer (Options options)
    {
      var atypeRepository = new OutlookTaskRepository (_outlookSession, options.OutlookFolderEntryId, options.OutlookFolderStoreId, _daslFilterProvider);

      var tasksService = System.Threading.Tasks.Task.Run (() => OAuth.Google.GoogleHttpClientFactory.LoginToGoogleTasksService (options.UserName).Result).Result;

      TaskList taskList;
      try
      {
        taskList = tasksService.Tasklists.Get (options.CalenderUrl).Execute();
      }
      catch (Google.GoogleApiException)
      {
        s_logger.ErrorFormat ($"Profile '{options.Name}' (Id: '{options.Id}'): task list '{options.CalenderUrl}' not found.");
        throw;
      }

      var btypeRepository = new GoogleTaskRepository (tasksService, taskList);

      
      var relationDataFactory = new GoogleTaskRelationDataFactory ();
      var syncStateFactory = new EntitySyncStateFactory<string, DateTime, TaskItemWrapper, string, string, Task> (
          new GoogleTaskMapper (),
          atypeRepository,
          btypeRepository,
          relationDataFactory,
          ExceptionHandler.Instance);

      var storageDataDirectory = _profileDataDirectoryFactory (options.Id);

      var btypeIdEqualityComparer = EqualityComparer<string>.Default;
      var atypeIdEqualityComparer = EqualityComparer<string>.Default;

      var synchronizer = new Synchronizer<string, DateTime, TaskItemWrapper, string, string, Task> (
          atypeRepository,
          btypeRepository,
          InitialGoogleTaskSyncStateCreationStrategyFactory.Create (
              syncStateFactory,
              syncStateFactory.Environment,
              options.SynchronizationMode,
              options.ConflictResolution),
          new EntityRelationDataAccess<string, DateTime, GoogleTaskRelationData, string, string> (storageDataDirectory),
          relationDataFactory,
          new InitialGoogleTastEntityMatcher (btypeIdEqualityComparer),
          atypeIdEqualityComparer,
          btypeIdEqualityComparer,
          _totalProgressFactory,
          ExceptionHandler.Instance);

      return new OutlookSynchronizer<string> (synchronizer, atypeRepository);
    }

    private IOutlookSynchronizer CreateContactSynchronizer (Options options, AvailableSynchronizerComponents componentsToFill)
    {
      var atypeRepository = new OutlookContactRepository (
          _outlookSession,
          options.OutlookFolderEntryId,
          options.OutlookFolderStoreId,
          _daslFilterProvider);


      var cardDavDataAccess = new CardDavDataAccess (
          new Uri (options.CalenderUrl),
          CreateWebDavClient (
              options.UserName,
              options.Password,
              _calDavConnectTimeout,
              options.ServerAdapterType,
              options.CloseAfterEachRequest,
              options.PreemptiveAuthentication,
              options.ProxyOptions));

      componentsToFill.CardDavDataAccess = cardDavDataAccess;

      IEntityRepository<vCard, WebResourceName, string> btypeRepository = new CardDavRepository (
          cardDavDataAccess);

      var mappingParameters = GetMappingParameters<ContactMappingConfiguration> (options);

      var entityMapper = new ContactEntityMapper (mappingParameters);

      var entityRelationDataFactory = new OutlookContactRelationDataFactory();

      var syncStateFactory = new EntitySyncStateFactory<string, DateTime, ContactItemWrapper, WebResourceName, string, vCard> (
          entityMapper,
          atypeRepository,
          btypeRepository,
          entityRelationDataFactory,
          ExceptionHandler.Instance);

      var btypeIdEqualityComparer = WebResourceName.Comparer;
      var atypeIdEqulityComparer = EqualityComparer<string>.Default;

      var storageDataDirectory = _profileDataDirectoryFactory (options.Id);

      var storageDataAccess = new EntityRelationDataAccess<string, DateTime, OutlookContactRelationData, WebResourceName, string> (storageDataDirectory);

      var synchronizer = new Synchronizer<string, DateTime, ContactItemWrapper, WebResourceName, string, vCard> (
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

      return new OutlookSynchronizer<WebResourceName> (synchronizer, atypeRepository);
    }
  }
}