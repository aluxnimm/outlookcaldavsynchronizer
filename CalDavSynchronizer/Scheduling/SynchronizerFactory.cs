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
using System.Runtime.InteropServices;
using System.Security;
using System.Threading.Tasks;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation;
using CalDavSynchronizer.Implementation.Common;
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.Implementation.Contacts;
using CalDavSynchronizer.Implementation.Contacts.VCardTypeSwitch;
using CalDavSynchronizer.Implementation.DistributionLists;
using CalDavSynchronizer.Implementation.DistributionLists.Sogo;
using CalDavSynchronizer.Implementation.DistributionLists.VCard;
using CalDavSynchronizer.Implementation.Events;
using CalDavSynchronizer.Implementation.GoogleContacts;
using CalDavSynchronizer.Implementation.GoogleTasks;
using CalDavSynchronizer.Implementation.Tasks;
using CalDavSynchronizer.Implementation.TimeRangeFiltering;
using CalDavSynchronizer.Implementation.TimeZones;
using CalDavSynchronizer.Scheduling.ComponentCollectors;
using CalDavSynchronizer.Synchronization;
using CalDavSynchronizer.Ui.Options.ProfileTypes;
using CalDavSynchronizer.Utilities;
using DDay.iCal;
using DDay.iCal.Serialization.iCalendar;
using GenSync.EntityMapping;
using GenSync.EntityRelationManagement;
using GenSync.EntityRepositories;
using GenSync.InitialEntityMatching;
using GenSync.Logging;
using GenSync.ProgressReport;
using GenSync.Synchronization;
using GenSync.Synchronization.StateCreationStrategies.ConflictStrategies;
using GenSync.Synchronization.StateFactories;
using GenSync.Utilities;
using Google.Apis.Tasks.v1.Data;
using Google.Contacts;
using log4net;
using Microsoft.Office.Interop.Outlook;
using Thought.vCards;
using ContactEntityMapper = CalDavSynchronizer.Implementation.Contacts.ContactEntityMapper;
using Task = Google.Apis.Tasks.v1.Data.Task;

namespace CalDavSynchronizer.Scheduling
{
  public class SynchronizerFactory : ISynchronizerFactory
  {
    private static readonly ILog s_logger = LogManager.GetLogger (System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType);

    private readonly string _outlookEmailAddress;
    private readonly ITotalProgressFactory _totalProgressFactory;
    private readonly IOutlookSession _outlookSession;
    private readonly Func<Guid, string> _profileDataDirectoryFactory;
    private readonly IDaslFilterProvider _daslFilterProvider;
    private readonly IOutlookAccountPasswordProvider _outlookAccountPasswordProvider;
    private readonly GlobalTimeZoneCache _globalTimeZoneCache;
    private readonly IQueryOutlookFolderStrategy _queryFolderStrategy;
    private readonly IExceptionHandlingStrategy _exceptionHandlingStrategy;
    private readonly IComWrapperFactory _comWrapperFactory;

    public SynchronizerFactory (Func<Guid, string> profileDataDirectoryFactory, ITotalProgressFactory totalProgressFactory, IOutlookSession outlookSession, IDaslFilterProvider daslFilterProvider, IOutlookAccountPasswordProvider outlookAccountPasswordProvider, GlobalTimeZoneCache globalTimeZoneCache, IQueryOutlookFolderStrategy queryFolderStrategy, IExceptionHandlingStrategy exceptionHandlingStrategy, IComWrapperFactory comWrapperFactory)
    {
      if (outlookAccountPasswordProvider == null)
        throw new ArgumentNullException (nameof (outlookAccountPasswordProvider));
      if (queryFolderStrategy == null) throw new ArgumentNullException(nameof(queryFolderStrategy));
      if (exceptionHandlingStrategy == null) throw new ArgumentNullException(nameof(exceptionHandlingStrategy));
      if (comWrapperFactory == null) throw new ArgumentNullException(nameof(comWrapperFactory));

      _outlookEmailAddress = outlookSession.GetCurrentUserEmailAddressOrNull() ?? string.Empty;
     

      _totalProgressFactory = totalProgressFactory;
      _outlookSession = outlookSession;
      _daslFilterProvider = daslFilterProvider;
      _outlookAccountPasswordProvider = outlookAccountPasswordProvider;
      _profileDataDirectoryFactory = profileDataDirectoryFactory;
      _globalTimeZoneCache = globalTimeZoneCache;
      _queryFolderStrategy = queryFolderStrategy;
      _exceptionHandlingStrategy = exceptionHandlingStrategy;
      _comWrapperFactory = comWrapperFactory;
    }
    
    public async Task<IOutlookSynchronizer> CreateSynchronizer (Options options, GeneralOptions generalOptions)
    {
      if (options == null)
        throw new ArgumentNullException (nameof (options));
      if (generalOptions == null)
        throw new ArgumentNullException (nameof (generalOptions));

      return (await CreateSynchronizerWithComponents(options, generalOptions)).Item1;
    }

    public async Task<Tuple<IOutlookSynchronizer, AvailableSynchronizerComponents>> CreateSynchronizerWithComponents (Options options, GeneralOptions generalOptions)
    {
      if (options == null)
        throw new ArgumentNullException (nameof (options));
      if (generalOptions == null)
        throw new ArgumentNullException (nameof (generalOptions));

      AvailableSynchronizerComponents synchronizerComponents;
      
      var folder = _outlookSession.GetFolderDescriptorFromId(options.OutlookFolderEntryId, options.OutlookFolderStoreId);
      
      IOutlookSynchronizer synchronizer;

      switch (folder.DefaultItemType)
      {
        case OlItemType.olAppointmentItem:
          var availableEventSynchronizerComponents = new AvailableEventSynchronizerComponents();
          synchronizerComponents = availableEventSynchronizerComponents;
          synchronizer = await CreateEventSynchronizer (options, generalOptions, availableEventSynchronizerComponents);
          break;
        case OlItemType.olTaskItem:
          if (options.ServerAdapterType == ServerAdapterType.GoogleTaskApi)
          {
            var availableGoogleTaskApiSynchronizerComponents = new AvailableGoogleTaskApiSynchronizerComponents ();
            synchronizerComponents = availableGoogleTaskApiSynchronizerComponents;
            synchronizer = await CreateGoogleTaskSynchronizer(options, availableGoogleTaskApiSynchronizerComponents, generalOptions);
          }
          else
          {
            var availableTaskSynchronizerComponents = new AvailableTaskSynchronizerComponents ();
            synchronizerComponents = availableTaskSynchronizerComponents;
            synchronizer = CreateTaskSynchronizer(options, generalOptions, availableTaskSynchronizerComponents);
          }
          break;
        case OlItemType.olContactItem:
          if (options.ServerAdapterType == ServerAdapterType.GoogleContactApi)
          {
            var availableGoogleContactSynchronizerSynchronizerComponents = new AvailableGoogleContactSynchronizerSynchronizerComponents ();
            synchronizerComponents = availableGoogleContactSynchronizerSynchronizerComponents;
            synchronizer = await CreateGoogleContactSynchronizer(options, availableGoogleContactSynchronizerSynchronizerComponents, generalOptions);
          }
          else
          {
            var availableContactSynchronizerComponents = new AvailableContactSynchronizerComponents ();
            synchronizerComponents = availableContactSynchronizerComponents;
            synchronizer = CreateContactSynchronizer(options, generalOptions, availableContactSynchronizerComponents);
          }
          break;
        default:
          throw new NotSupportedException (
              string.Format (
                  "The folder '{0}' contains an item type ('{1}'), whis is not supported for synchronization",
                  folder.Name,
                  folder.DefaultItemType));
      }

      return Tuple.Create(synchronizer, synchronizerComponents);
    }

    private async Task<IOutlookSynchronizer> CreateEventSynchronizer (Options options, GeneralOptions generalOptions ,AvailableEventSynchronizerComponents componentsToFill)
    {
      ICalDavDataAccess calDavDataAccess;

      var calendarUrl = new Uri (options.CalenderUrl);

      if (calendarUrl.Scheme == Uri.UriSchemeFile)
      {
        calDavDataAccess = new FileSystemDavDataAccess (calendarUrl, ".ics");
      }
      else
      {
        calDavDataAccess = new CalDavDataAccess(
          calendarUrl,
          CreateWebDavClient(options, _outlookAccountPasswordProvider, generalOptions));
      }

      componentsToFill.CalDavDataAccess = calDavDataAccess;

      var storageDataDirectory = _profileDataDirectoryFactory (options.Id);

      var entityRelationDataAccess = new EntityRelationDataAccess<AppointmentId, DateTime, OutlookEventRelationData, WebResourceName, string> (storageDataDirectory);

      componentsToFill.EntityRelationDataAccess = entityRelationDataAccess;

      return await CreateEventSynchronizer (options, calDavDataAccess, entityRelationDataAccess, componentsToFill, generalOptions);
    }

    public static IWebDavClient CreateWebDavClient (
      Options options, 
      IOutlookAccountPasswordProvider outlookAccountPasswordProvider,
      GeneralOptions generalOptions)
    {
      if (outlookAccountPasswordProvider == null)
        throw new ArgumentNullException (nameof (outlookAccountPasswordProvider));
      if (generalOptions == null)
        throw new ArgumentNullException (nameof (generalOptions));

      return CreateWebDavClient (
          options.UserName,
          options.GetEffectivePassword(outlookAccountPasswordProvider),
          options.CalenderUrl,
          generalOptions.CalDavConnectTimeout,
          options.ServerAdapterType,
          options.CloseAfterEachRequest,
          options.PreemptiveAuthentication,
          options.ForceBasicAuthentication,
          options.ProxyOptions,
          generalOptions.EnableClientCertificate,
          generalOptions.AcceptInvalidCharsInServerResponse);
    }

    public static IWebDavClient CreateWebDavClient (
        string username,
        SecureString password,
        string serverUrl,
        TimeSpan timeout,
        ServerAdapterType serverAdapterType,
        bool closeConnectionAfterEachRequest,
        bool preemptiveAuthentication,
        bool forceBasicAuthentication,
        ProxyOptions proxyOptions,
        bool enableClientCertificate,
        bool acceptInvalidChars
        )
    {
      switch (serverAdapterType)
      {
        case ServerAdapterType.WebDavHttpClientBased:
        case ServerAdapterType.WebDavHttpClientBasedWithGoogleOAuth:
          var productAndVersion = GetProductAndVersion();
          return new DataAccess.HttpClientBasedClient.WebDavClient (
              () => CreateHttpClient (username, password, serverUrl, timeout, serverAdapterType, proxyOptions, preemptiveAuthentication, forceBasicAuthentication, enableClientCertificate),
              productAndVersion.Item1,
              productAndVersion.Item2,
              closeConnectionAfterEachRequest,
              acceptInvalidChars,
              RequiresEtagsWithoutQuotes(serverUrl));

        default:
          throw new ArgumentOutOfRangeException ("serverAdapterType");
      }
    }

    private static bool RequiresEtagsWithoutQuotes (string serverUrl)
    {
      var authorityParts = new Uri (serverUrl).Authority.ToLower().Split('.');
      if (authorityParts.Length >= 2)
      {
        var length = authorityParts.Length;
        if (authorityParts[length - 1] == "ru" && authorityParts[length - 2] == "mail")
        {
          s_logger.Info ($"Detected url which requires etags without quotes ('{serverUrl}')");
          return true;
        }
      }

      return false;
    }

    private static async System.Threading.Tasks.Task<HttpClient> CreateHttpClient ( string username,
                                                                                    SecureString password,
                                                                                    string serverUrl, 
                                                                                    TimeSpan calDavConnectTimeout, 
                                                                                    ServerAdapterType serverAdapterType, 
                                                                                    ProxyOptions proxyOptions, 
                                                                                    bool preemptiveAuthentication,
                                                                                    bool forceBasicAuthentication,
                                                                                    bool enableClientCertificate )
    {
      IWebProxy proxy = (proxyOptions != null) ? CreateProxy (proxyOptions) : null;

      switch (serverAdapterType)
      {
        case ServerAdapterType.WebDavHttpClientBased:
          var httpClientHandler = new HttpClientHandler();
          if (!string.IsNullOrEmpty (username))
          {
            var credentials = new NetworkCredential (username, password);
            if (forceBasicAuthentication)
            {
              var cache = new CredentialCache();
              cache.Add (new Uri (new Uri (serverUrl).GetLeftPart (UriPartial.Authority)), "Basic", credentials);
              httpClientHandler.Credentials = cache;
            }
            else
            {
              httpClientHandler.Credentials = credentials;
            }

            httpClientHandler.AllowAutoRedirect = false;
            httpClientHandler.PreAuthenticate = preemptiveAuthentication;
          }
          httpClientHandler.Proxy = proxy;
          httpClientHandler.UseProxy = (proxy != null);
          if (enableClientCertificate)
            httpClientHandler.ClientCertificateOptions = ClientCertificateOption.Automatic;

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
    public async Task<IOutlookSynchronizer> CreateEventSynchronizer (
        Options options,
        ICalDavDataAccess calDavDataAccess,
        IEntityRelationDataAccess<AppointmentId, DateTime, WebResourceName, string> entityRelationDataAccess,
        AvailableEventSynchronizerComponents componentsToFill,
        GeneralOptions generalOptions)
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
          _daslFilterProvider,
          _queryFolderStrategy,
          _comWrapperFactory);

      componentsToFill.OutlookEventRepository = atypeRepository;

      IEntityRepository<WebResourceName, string, IICalendar, IEventSynchronizationContext> btypeRepository = new CalDavRepository<IEventSynchronizationContext> (
          calDavDataAccess,
          new iCalendarSerializer(),
          CalDavRepository.EntityType.Event,
          dateTimeRangeProvider,
          options.ServerAdapterType == ServerAdapterType.WebDavHttpClientBasedWithGoogleOAuth);

      componentsToFill.CalDavRepository = btypeRepository;

      var timeZoneCache = new TimeZoneCache (CreateHttpClient(options.ProxyOptions), mappingParameters.IncludeHistoricalData, _globalTimeZoneCache);

      ITimeZone configuredEventTimeZoneOrNull;

      if (mappingParameters.UseIanaTz)
        configuredEventTimeZoneOrNull = await timeZoneCache.GetByTzIdOrNull(mappingParameters.EventTz);
      else
        configuredEventTimeZoneOrNull = null;

      var entityMapper = new EventEntityMapper (
          _outlookEmailAddress, new Uri ("mailto:" + options.EmailAddress),
          _outlookSession.TimeZones.CurrentTimeZone.ID,
          _outlookSession.ApplicationVersion,
          timeZoneCache,
          mappingParameters,
          configuredEventTimeZoneOrNull,
          _outlookSession.TimeZones);

      var outlookEventRelationDataFactory = new OutlookEventRelationDataFactory();

      var syncStateFactory = new EntitySyncStateFactory<AppointmentId, DateTime, IAppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext> (
          entityMapper,
          outlookEventRelationDataFactory,
          ExceptionHandler.Instance
          );

      var btypeIdEqualityComparer = WebResourceName.Comparer;
      var atypeIdEqualityComparer = AppointmentId.Comparer;

      var aTypeWriteRepository = BatchEntityRepositoryAdapter.Create (atypeRepository, _exceptionHandlingStrategy);
      var bTypeWriteRepository = BatchEntityRepositoryAdapter.Create (btypeRepository, _exceptionHandlingStrategy);

      var synchronizer = new Synchronizer<AppointmentId, DateTime, IAppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext, EventEntityMatchData, EventServerEntityMatchData>(
        atypeRepository,
        btypeRepository,
        aTypeWriteRepository,
        bTypeWriteRepository,
        InitialSyncStateCreationStrategyFactory<AppointmentId, DateTime, IAppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext>.Create(
          syncStateFactory,
          syncStateFactory.Environment,
          options.SynchronizationMode,
          options.ConflictResolution,
          e => new EventConflictInitialSyncStateCreationStrategyAutomatic(e)),
        entityRelationDataAccess,
        outlookEventRelationDataFactory,
        new InitialEventEntityMatcher(btypeIdEqualityComparer),
        atypeIdEqualityComparer,
        btypeIdEqualityComparer,
        _totalProgressFactory,
        EqualityComparer<DateTime>.Default,
        EqualityComparer<string>.Default,
        syncStateFactory,
        _exceptionHandlingStrategy,
        new EventEntityMatchDataFactory(),
        new EventServerEntityMatchDataFactory(),
        options.EffectiveChunkSize,
        CreateChunkedExecutor(options),
        FullEntitySynchronizationLoggerFactory.Create(generalOptions.LogEntityNames ? EntityLogMessageFactory.Instance : NullEntityLogMessageFactory<IAppointmentItemWrapper, IICalendar>.Instance),
        new EventSynchronizationInterceptorFactory());

      return new OutlookEventSynchronizer<WebResourceName, string> (
        new ContextCreatingSynchronizerDecorator<AppointmentId, DateTime, IAppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext>(
          synchronizer,
          new EventSynchronizationContextFactory(atypeRepository, btypeRepository, entityRelationDataAccess, mappingParameters.CleanupDuplicateEvents, atypeIdEqualityComparer, _outlookSession)));
    }

    private HttpClient CreateHttpClient(ProxyOptions proxyOptionsOrNull)
    {
      var proxy = proxyOptionsOrNull != null ? CreateProxy (proxyOptionsOrNull) : null;
      var httpClientHandler = new HttpClientHandler
      {
        Proxy = proxy,
        UseProxy = proxy != null
      };

      return new HttpClient (httpClientHandler);
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

    private IOutlookSynchronizer CreateTaskSynchronizer (Options options, GeneralOptions generalOptions, AvailableTaskSynchronizerComponents componentsToFill)
    {
      var mappingParameters = GetMappingParameters<TaskMappingConfiguration> (options);

      var atypeRepository = new OutlookTaskRepository (_outlookSession, options.OutlookFolderEntryId, options.OutlookFolderStoreId, _daslFilterProvider, mappingParameters, _queryFolderStrategy, _comWrapperFactory);



      ICalDavDataAccess calDavDataAccess;
      var calendarUrl = new Uri (options.CalenderUrl);

      if (calendarUrl.Scheme == Uri.UriSchemeFile)
      {
        calDavDataAccess = new FileSystemDavDataAccess (calendarUrl, ".ics");
      }
      else
      {
        calDavDataAccess = new CalDavDataAccess (
         calendarUrl,
         CreateWebDavClient (
           options.UserName,
           options.GetEffectivePassword (_outlookAccountPasswordProvider),
           options.CalenderUrl,
           generalOptions.CalDavConnectTimeout,
           options.ServerAdapterType,
           options.CloseAfterEachRequest,
           options.PreemptiveAuthentication,
           options.ForceBasicAuthentication,
           options.ProxyOptions,
           generalOptions.EnableClientCertificate,
           generalOptions.AcceptInvalidCharsInServerResponse));
      }

      componentsToFill.CalDavDataAccess = calDavDataAccess;

      var btypeRepository = new CalDavRepository<int> (
          calDavDataAccess,
          new iCalendarSerializer(),
          CalDavRepository.EntityType.Todo,
          NullDateTimeRangeProvider.Instance,
          false);

      componentsToFill.CalDavRepository = btypeRepository;
      componentsToFill.OutlookRepository = atypeRepository;

      var relationDataFactory = new TaskRelationDataFactory ();
      var syncStateFactory = new EntitySyncStateFactory<string, DateTime, ITaskItemWrapper, WebResourceName, string, IICalendar, int> (
          new TaskMapper (_outlookSession.TimeZones.CurrentTimeZone.ID, mappingParameters),
          relationDataFactory,
          ExceptionHandler.Instance);

      var storageDataDirectory = _profileDataDirectoryFactory (options.Id);

      var btypeIdEqualityComparer = WebResourceName.Comparer;
      var atypeIdEqualityComparer = EqualityComparer<string>.Default;

      var atypeWriteRepository = BatchEntityRepositoryAdapter.Create (atypeRepository, _exceptionHandlingStrategy);
      var btypeWriteRepository = BatchEntityRepositoryAdapter.Create (btypeRepository, _exceptionHandlingStrategy);

      var entityRelationDataAccess = new EntityRelationDataAccess<string, DateTime, TaskRelationData, WebResourceName, string> (storageDataDirectory);
      componentsToFill.EntityRelationDataAccess = entityRelationDataAccess;

      var synchronizer = new Synchronizer<string, DateTime, ITaskItemWrapper, WebResourceName, string, IICalendar, int, TaskEntityMatchData, IICalendar>(
        atypeRepository,
        btypeRepository,
        atypeWriteRepository,
        btypeWriteRepository,
        InitialSyncStateCreationStrategyFactory<string, DateTime, ITaskItemWrapper, WebResourceName, string, IICalendar, int>.Create(
          syncStateFactory,
          syncStateFactory.Environment,
          options.SynchronizationMode,
          options.ConflictResolution,
          e => new TaskConflictInitialSyncStateCreationStrategyAutomatic(e)),
        entityRelationDataAccess,
        relationDataFactory,
        new InitialTaskEntityMatcher(btypeIdEqualityComparer),
        atypeIdEqualityComparer,
        btypeIdEqualityComparer,
        _totalProgressFactory,
        EqualityComparer<DateTime>.Default,
        EqualityComparer<string>.Default,
        syncStateFactory,
        _exceptionHandlingStrategy,
        new TaskEntityMatchDataFactory(),
        IdentityMatchDataFactory<IICalendar>.Instance,
        options.EffectiveChunkSize,
        CreateChunkedExecutor(options),
        FullEntitySynchronizationLoggerFactory.Create(generalOptions.LogEntityNames ? EntityLogMessageFactory.Instance : NullEntityLogMessageFactory<ITaskItemWrapper, IICalendar>.Instance));

      return new OutlookSynchronizer<WebResourceName, string> (
        new NullContextSynchronizerDecorator<string, DateTime, ITaskItemWrapper, WebResourceName, string, IICalendar> (synchronizer));
    }

    private async Task<IOutlookSynchronizer> CreateGoogleTaskSynchronizer (Options options, AvailableGoogleTaskApiSynchronizerComponents componentsToFill, GeneralOptions generalOptions)
    {
      var mappingParameters = GetMappingParameters<TaskMappingConfiguration> (options);

      var atypeRepository = new OutlookTaskRepository (_outlookSession, options.OutlookFolderEntryId, options.OutlookFolderStoreId, _daslFilterProvider, mappingParameters, _queryFolderStrategy, _comWrapperFactory);

      componentsToFill.OutlookRepository = atypeRepository;

      IWebProxy proxy = options.ProxyOptions != null ? CreateProxy (options.ProxyOptions) : null;

      var tasksService = await OAuth.Google.GoogleHttpClientFactory.LoginToGoogleTasksService (options.UserName,proxy);

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

      componentsToFill.ServerRepository = btypeRepository;

      var relationDataFactory = new GoogleTaskRelationDataFactory ();
      var syncStateFactory = new EntitySyncStateFactory<string, DateTime, ITaskItemWrapper, string, string, Task, int> (
          new GoogleTaskMapper (),
          relationDataFactory,
          ExceptionHandler.Instance);

      var storageDataDirectory = _profileDataDirectoryFactory (options.Id);

      var btypeIdEqualityComparer = EqualityComparer<string>.Default;
      var atypeIdEqualityComparer = EqualityComparer<string>.Default;

      var atypeWriteRepository = BatchEntityRepositoryAdapter.Create (atypeRepository, _exceptionHandlingStrategy);
      var btypeWriteRepository = BatchEntityRepositoryAdapter.Create (btypeRepository, _exceptionHandlingStrategy);

      var entityRelationDataAccess = new EntityRelationDataAccess<string, DateTime, GoogleTaskRelationData, string, string> (storageDataDirectory);
      componentsToFill.EntityRelationDataAccess = entityRelationDataAccess;
      var synchronizer = new Synchronizer<string, DateTime, ITaskItemWrapper, string, string, Task, int, TaskEntityMatchData, Task> (
          atypeRepository,
          btypeRepository,
          atypeWriteRepository,
          btypeWriteRepository,
          InitialSyncStateCreationStrategyFactory<string, DateTime, ITaskItemWrapper, string, string, Task, int>.Create (
              syncStateFactory,
              syncStateFactory.Environment,
              options.SynchronizationMode,
              options.ConflictResolution,
              e => new GoogleTaskConflictInitialSyncStateCreationStrategyAutomatic (e)),
          entityRelationDataAccess,
          relationDataFactory,
          new InitialGoogleTastEntityMatcher (btypeIdEqualityComparer),
          atypeIdEqualityComparer,
          btypeIdEqualityComparer,
          _totalProgressFactory,
          EqualityComparer<DateTime>.Default,
          EqualityComparer<string>.Default,
          syncStateFactory,
          _exceptionHandlingStrategy,
        new TaskEntityMatchDataFactory(),
        IdentityMatchDataFactory<Task>.Instance,
        options.EffectiveChunkSize,
        CreateChunkedExecutor(options),
        FullEntitySynchronizationLoggerFactory.Create(generalOptions.LogEntityNames ? EntityLogMessageFactory.Instance : NullEntityLogMessageFactory<ITaskItemWrapper, Task>.Instance));

      return new OutlookSynchronizer<string, string> (
        new NullContextSynchronizerDecorator<string, DateTime, ITaskItemWrapper, string, string, Task>( synchronizer));
    }

    private IOutlookSynchronizer CreateContactSynchronizer (Options options, GeneralOptions generalOptions, AvailableContactSynchronizerComponents componentsToFill)
    {
      var synchronizerComponents = CreateContactSynchronizerComponents(options, generalOptions, componentsToFill);

      if (synchronizerComponents.MappingParameters.MapDistributionLists)
      {
        var btypeIdEqualityComparer = WebResourceName.Comparer;

        switch (synchronizerComponents.MappingParameters.DistributionListType)
        {
          case DistributionListType.Sogo:

            var synchronizer = CreateContactSynchronizer (synchronizerComponents, componentsToFill, options, generalOptions);

            ICardDavDataAccess distListDataAccess;
            if (synchronizerComponents.WebDavClientOrNullIfFileAccess == null)
            {
              distListDataAccess = new FileSystemDavDataAccess(synchronizerComponents.ServerUrl, ".vlist");
            }
            else
            {
              distListDataAccess = new CardDavDataAccess(
                synchronizerComponents.ServerUrl,
                synchronizerComponents.WebDavClientOrNullIfFileAccess,
                contentType => contentType == "text/x-vlist");
            }

            componentsToFill.SogoDistListDataAccessOrNull = distListDataAccess;

            var bDistListRepository = new SogoDistributionListRepository(distListDataAccess);

            componentsToFill.SogoDistListRepositoryOrNull = bDistListRepository;

            var distributionListSynchronizer = CreateDistListSynchronizer(
              options,
              generalOptions,
              bDistListRepository,
              new SogoDistListEntityMapper(),
              new InitialSogoDistListEntityMatcher(btypeIdEqualityComparer),
              e => new SogoDistListConflictInitialSyncStateCreationStrategyAutomatic(e),
              btypeIdEqualityComparer,
              componentsToFill,
              FullEntitySynchronizationLoggerFactory.Create(generalOptions.LogEntityNames ? EntityLogMessageFactory.Instance : NullEntityLogMessageFactory<IDistListItemWrapper, DistributionList>.Instance));

            return new OutlookSynchronizer<WebResourceName, string>(
              new ContactAndDistListSynchronizer(
                synchronizer,
                distributionListSynchronizer,
                new EmailAddressCacheDataAccess(Path.Combine(synchronizerComponents.StorageDataDirectory, "emailAddressCache.xml")),
                synchronizerComponents.BtypeRepository,
                _outlookSession,
                synchronizerComponents.StorageDataAccess,
                (options.OutlookFolderEntryId, options.OutlookFolderStoreId)));

          case DistributionListType.VCardGroup:
          case DistributionListType.VCardGroupWithUid:
            var vCardTypeDetector = new VCardTypeDetector (synchronizerComponents.BtypeRepository, new VCardTypeCache (new VCardTypeCacheDataAccess (Path.Combine (synchronizerComponents.StorageDataDirectory, "vcardTypeCache.xml"))));

            var contactRepository = new TypeFilteringVCardRepositoryDecorator<ICardDavRepositoryLogger> (synchronizerComponents.BtypeRepository, VCardType.Contact, vCardTypeDetector);
            synchronizerComponents.BtypeRepository = contactRepository;
            var contactSynchronizer = CreateContactSynchronizer(synchronizerComponents, componentsToFill,options, generalOptions);

            var contactGroupCardDavRepository = new CardDavRepository<DistributionListSychronizationContext> (synchronizerComponents.CardDavDataAccess, false);

            var contactGroupRepository = new TypeFilteringVCardRepositoryDecorator<DistributionListSychronizationContext> (contactGroupCardDavRepository, VCardType.Group, vCardTypeDetector);

            componentsToFill.VCardGroupRepositoryOrNull = contactGroupRepository;

            var distListSynchronizer = CreateDistListSynchronizer<vCard>(
              options,
              generalOptions,
              contactGroupRepository,
              synchronizerComponents.MappingParameters.DistributionListType ==  DistributionListType.VCardGroup ? (DistListEntityMapperBase) new DistListEntityMapper() : new UidDistListEntityMapper(),
              new InitialDistListEntityMatcher(btypeIdEqualityComparer),
              e => new DistListConflictInitialSyncStateCreationStrategyAutomatic(e),
              btypeIdEqualityComparer,
              componentsToFill,
              FullEntitySynchronizationLoggerFactory.Create(generalOptions.LogEntityNames ? EntityLogMessageFactory.Instance : NullEntityLogMessageFactory<IDistListItemWrapper, vCard>.Instance));

          
        

            return new OutlookSynchronizer<WebResourceName, string> (
              new ContactAndDistListSynchronizer (
                contactSynchronizer,
                distListSynchronizer,
                new EmailAddressCacheDataAccess (Path.Combine (synchronizerComponents.StorageDataDirectory, "emailAddressCache.xml")),
                synchronizerComponents.BtypeRepository,
                _outlookSession,
                synchronizerComponents.StorageDataAccess,
                (options.OutlookFolderEntryId, options.OutlookFolderStoreId)));

          default:
            throw new NotImplementedException($"{nameof(DistributionListType)} '{synchronizerComponents.MappingParameters.DistributionListType}' not implemented.");
        }

      
      }
      else
      {
        return new OutlookSynchronizer<WebResourceName, string>(
          new ContextCreatingSynchronizerDecorator<string, DateTime, IContactItemWrapper, WebResourceName, string, vCard, ICardDavRepositoryLogger>(
            CreateContactSynchronizer(synchronizerComponents, componentsToFill, options, generalOptions),
            new SynchronizationContextFactory<ICardDavRepositoryLogger>(() => NullCardDavRepositoryLogger.Instance)));
      }
    }

    private ContactSynchronizerComponents CreateContactSynchronizerComponents (
      Options options, GeneralOptions generalOptions, AvailableContactSynchronizerComponents componentsToFill)
    {
      var atypeRepository = new OutlookContactRepository<ICardDavRepositoryLogger>(
        _outlookSession,
        options.OutlookFolderEntryId,
        options.OutlookFolderStoreId,
        _daslFilterProvider,
        _queryFolderStrategy,
        _comWrapperFactory);

      ICardDavDataAccess cardDavDataAccess;
      var serverUrl = new Uri(options.CalenderUrl);

      IWebDavClient webDavClientOrNullIfFileAccess = null;

      if (serverUrl.Scheme == Uri.UriSchemeFile)
      {
        cardDavDataAccess = new FileSystemDavDataAccess(serverUrl, ".vcard");
      }
      else
      {
        webDavClientOrNullIfFileAccess = CreateWebDavClient(
          options.UserName,
          options.GetEffectivePassword(_outlookAccountPasswordProvider),
          options.CalenderUrl,
          generalOptions.CalDavConnectTimeout,
          options.ServerAdapterType,
          options.CloseAfterEachRequest,
          options.PreemptiveAuthentication,
          options.ForceBasicAuthentication,
          options.ProxyOptions,
          generalOptions.EnableClientCertificate,
          generalOptions.AcceptInvalidCharsInServerResponse);

        cardDavDataAccess = new CardDavDataAccess(
          serverUrl,
          webDavClientOrNullIfFileAccess,
          contentType => contentType != "text/x-vlist");
      }
      componentsToFill.CardDavDataAccess = cardDavDataAccess;

      var mappingParameters = GetMappingParameters<ContactMappingConfiguration>(options);

      var cardDavRepository = new CardDavRepository<int>(cardDavDataAccess, mappingParameters.WriteImAsImpp);
      var btypeRepository = new LoggingCardDavRepositoryDecorator(
        cardDavRepository);

      var credentials = new NetworkCredential(options.UserName, options.GetEffectivePassword(_outlookAccountPasswordProvider));
      var entityMapper = new ContactEntityMapper(mappingParameters, () => HttpUtility.CreateWebClientWithCredentialsAndProxy( credentials, options.ProxyOptions));

      var entityRelationDataFactory = new OutlookContactRelationDataFactory();

      var syncStateFactory = new EntitySyncStateFactory<string, DateTime, IContactItemWrapper, WebResourceName, string, vCard, ICardDavRepositoryLogger>(
        entityMapper,
        entityRelationDataFactory,
        ExceptionHandler.Instance);

      var btypeIdEqualityComparer = WebResourceName.Comparer;
      var atypeIdEqulityComparer = EqualityComparer<string>.Default;

      var storageDataDirectory = _profileDataDirectoryFactory(options.Id);

      var storageDataAccess = new EntityRelationDataAccess<string, DateTime, OutlookContactRelationData, WebResourceName, string>(storageDataDirectory);

      componentsToFill.EntityRelationDataAccess = storageDataAccess;


      return new ContactSynchronizerComponents(options, atypeRepository, btypeRepository, syncStateFactory, storageDataAccess, entityRelationDataFactory, btypeIdEqualityComparer, atypeIdEqulityComparer, webDavClientOrNullIfFileAccess, btypeRepository, mappingParameters, storageDataDirectory, serverUrl, cardDavDataAccess);
    }

    private Synchronizer<string, DateTime, IContactItemWrapper, WebResourceName, string, vCard, ICardDavRepositoryLogger, ContactMatchData, vCard> CreateContactSynchronizer(
      ContactSynchronizerComponents contactSynchronizerComponents,
      AvailableContactSynchronizerComponents componentsToFill,
      Options options, 
      GeneralOptions generalOptions)
    {
      componentsToFill.CardDavEntityRepository = contactSynchronizerComponents.BtypeRepository;
      componentsToFill.OutlookContactRepository = contactSynchronizerComponents.AtypeRepository;

      return new Synchronizer<string, DateTime, IContactItemWrapper, WebResourceName, string, vCard, ICardDavRepositoryLogger, ContactMatchData, vCard> (
        contactSynchronizerComponents.AtypeRepository,
        contactSynchronizerComponents.BtypeRepository,
        BatchEntityRepositoryAdapter.Create (contactSynchronizerComponents.AtypeRepository, _exceptionHandlingStrategy),
        BatchEntityRepositoryAdapter.Create (contactSynchronizerComponents.BtypeRepository, _exceptionHandlingStrategy),
        InitialSyncStateCreationStrategyFactory<string, DateTime, IContactItemWrapper, WebResourceName, string, vCard, ICardDavRepositoryLogger>.Create (
          contactSynchronizerComponents.SyncStateFactory,
          contactSynchronizerComponents.SyncStateFactory.Environment,
          contactSynchronizerComponents.Options.SynchronizationMode,
          contactSynchronizerComponents.Options.ConflictResolution,
          e => new ContactConflictInitialSyncStateCreationStrategyAutomatic (e)),
        contactSynchronizerComponents.StorageDataAccess,
        contactSynchronizerComponents.EntityRelationDataFactory,
        new InitialContactEntityMatcher (contactSynchronizerComponents.BtypeIdEqualityComparer),
        contactSynchronizerComponents.AtypeIdEqulityComparer,
        contactSynchronizerComponents.BtypeIdEqualityComparer,
        _totalProgressFactory,
        EqualityComparer<DateTime>.Default,
        EqualityComparer<string>.Default,
        contactSynchronizerComponents.SyncStateFactory, 
        _exceptionHandlingStrategy,
        new ContactMatchDataFactory(),
        IdentityMatchDataFactory<vCard>.Instance,
        options.EffectiveChunkSize,
        CreateChunkedExecutor(options),
        FullEntitySynchronizationLoggerFactory.Create(generalOptions.LogEntityNames ? EntityLogMessageFactory.Instance : NullEntityLogMessageFactory<IContactItemWrapper, vCard>.Instance));
    }
    
    private ISynchronizer<DistributionListSychronizationContext> CreateDistListSynchronizer<TBtypeEntity> (
      Options options, 
      GeneralOptions generalOptions,
      IEntityRepository<WebResourceName, string, TBtypeEntity, DistributionListSychronizationContext>  btypeRepository,
      IEntityMapper<IDistListItemWrapper, TBtypeEntity, DistributionListSychronizationContext> entityMapper,
      InitialEntityMatcherByPropertyGrouping<string, DateTime, DistListMatchData, string, WebResourceName, string, TBtypeEntity, string> initialDistListEntityMatcher,
      Func<
       EntitySyncStateEnvironment<string, DateTime, IDistListItemWrapper,  WebResourceName, string, TBtypeEntity, DistributionListSychronizationContext>,
       ConflictInitialSyncStateCreationStrategyAutomatic<string, DateTime, IDistListItemWrapper, WebResourceName, string, TBtypeEntity, DistributionListSychronizationContext>> automaticConflictResolutionStrategyFactory,
      IEqualityComparer<WebResourceName> btypeIdEqualityComparer,
      AvailableContactSynchronizerComponents componentsToFill,
      IFullEntitySynchronizationLoggerFactory<IDistListItemWrapper,TBtypeEntity> entitySynchronizationLoggerFactory) 
      where TBtypeEntity : new()
    {
      var atypeRepository = new OutlookDistListRepository<DistributionListSychronizationContext> (
          _outlookSession,
          options.OutlookFolderEntryId,
          options.OutlookFolderStoreId,
          _daslFilterProvider,
          _queryFolderStrategy,
          _comWrapperFactory);

      componentsToFill.OutlookDistListRepositoryOrNull = atypeRepository;

      var entityRelationDataFactory = new DistListRelationDataFactory ();

      var syncStateFactory = new EntitySyncStateFactory<string, DateTime, IDistListItemWrapper, WebResourceName, string, TBtypeEntity, DistributionListSychronizationContext> (
          entityMapper,
          entityRelationDataFactory,
          ExceptionHandler.Instance);

      var atypeIdEqulityComparer = EqualityComparer<string>.Default;

      var storageDataDirectory = _profileDataDirectoryFactory (options.Id);

      var storageDataAccess = new EntityRelationDataAccess<string, DateTime, DistListRelationData, WebResourceName, string> (storageDataDirectory, "distList");

      componentsToFill.DistListEntityRelationDataAccess = storageDataAccess;

      var atypeWriteRepository = BatchEntityRepositoryAdapter.Create (atypeRepository, _exceptionHandlingStrategy);
      var btypeWriteRepository = BatchEntityRepositoryAdapter.Create (btypeRepository, _exceptionHandlingStrategy);

      var synchronizer = new Synchronizer<string, DateTime, IDistListItemWrapper, WebResourceName, string, TBtypeEntity, DistributionListSychronizationContext, DistListMatchData, TBtypeEntity> (
          atypeRepository,
          btypeRepository,
          atypeWriteRepository,
          btypeWriteRepository,
          InitialSyncStateCreationStrategyFactory<string, DateTime, IDistListItemWrapper, WebResourceName, string, TBtypeEntity, DistributionListSychronizationContext>.Create (
              syncStateFactory,
              syncStateFactory.Environment,
              options.SynchronizationMode,
              options.ConflictResolution,
              automaticConflictResolutionStrategyFactory),
          storageDataAccess,
          entityRelationDataFactory,
          initialDistListEntityMatcher,
          atypeIdEqulityComparer,
          btypeIdEqualityComparer,
          _totalProgressFactory,
          EqualityComparer<DateTime>.Default,
          EqualityComparer<string>.Default,
          syncStateFactory,
          _exceptionHandlingStrategy,
        new DistListEntityMatchDataFactory(),
        IdentityMatchDataFactory<TBtypeEntity>.Instance,
        options.EffectiveChunkSize,
        CreateChunkedExecutor(options),
        entitySynchronizationLoggerFactory);

      return synchronizer;
    }

    private async Task<IOutlookSynchronizer> CreateGoogleContactSynchronizer (Options options, AvailableGoogleContactSynchronizerSynchronizerComponents componentsToFill, GeneralOptions generalOptions)
    {
      var atypeRepository = new OutlookContactRepository<IGoogleContactContext> (
          _outlookSession,
          options.OutlookFolderEntryId,
          options.OutlookFolderStoreId,
          _daslFilterProvider,
          _queryFolderStrategy,
          _comWrapperFactory);

      componentsToFill.OutlookContactRepository = atypeRepository;

      IWebProxy proxy = options.ProxyOptions != null ? CreateProxy (options.ProxyOptions) : null;

      var googleApiExecutor = new GoogleApiOperationExecutor (await OAuth.Google.GoogleHttpClientFactory.LoginToContactsService (options.UserName, proxy));

      var mappingParameters = GetMappingParameters<ContactMappingConfiguration> (options);

      var atypeIdEqulityComparer = EqualityComparer<string>.Default;
      var btypeIdEqualityComparer = EqualityComparer<string>.Default;

      var btypeRepository = new GoogleContactRepository (
        googleApiExecutor, 
        options.UserName,
        mappingParameters,
        btypeIdEqualityComparer,
        new ChunkedExecutor (Math.Min(options.ChunkSize, GoogleProfile.MaximumWriteBatchSize)),
        new ChunkedExecutor(options.ChunkSize));

      componentsToFill.GoogleContactRepository = btypeRepository;
      componentsToFill.GoogleApiOperationExecutor = googleApiExecutor;

      var entityMapper = new GoogleContactEntityMapper (mappingParameters);

      var entityRelationDataFactory = new GoogleContactRelationDataFactory ();

      var syncStateFactory = new EntitySyncStateFactory<string, DateTime, IContactItemWrapper, string, GoogleContactVersion, GoogleContactWrapper, IGoogleContactContext> (
          entityMapper,
          entityRelationDataFactory,
          ExceptionHandler.Instance);

      var storageDataDirectory = _profileDataDirectoryFactory (options.Id);

      var storageDataAccess = new EntityRelationDataAccess<string, DateTime, GoogleContactRelationData, string, GoogleContactVersion> (storageDataDirectory);

      componentsToFill.GoogleContactsEntityRelationDataAccess = storageDataAccess;

      var atypeWriteRepository = BatchEntityRepositoryAdapter.Create (atypeRepository, _exceptionHandlingStrategy);

      var synchronizer = new Synchronizer<string, DateTime, IContactItemWrapper, string, GoogleContactVersion, GoogleContactWrapper, IGoogleContactContext, ContactMatchData, GoogleContactWrapper>(
        atypeRepository,
        btypeRepository,
        atypeWriteRepository,
        btypeRepository,
        InitialSyncStateCreationStrategyFactory<string, DateTime, IContactItemWrapper, string, GoogleContactVersion, GoogleContactWrapper, IGoogleContactContext>.Create(
          syncStateFactory,
          syncStateFactory.Environment,
          options.SynchronizationMode,
          options.ConflictResolution,
          e => new GoogleContactConflictInitialSyncStateCreationStrategyAutomatic(e)),
        storageDataAccess,
        entityRelationDataFactory,
        new InitialGoogleContactEntityMatcher(btypeIdEqualityComparer),
        atypeIdEqulityComparer,
        btypeIdEqualityComparer,
        _totalProgressFactory,
        EqualityComparer<DateTime>.Default,
        new GoogleContactVersionComparer(),
        syncStateFactory,
        _exceptionHandlingStrategy,
        new ContactMatchDataFactory(),
        IdentityMatchDataFactory<GoogleContactWrapper>.Instance,
        options.EffectiveChunkSize,
        CreateChunkedExecutor(options),
        FullEntitySynchronizationLoggerFactory.Create(generalOptions.LogEntityNames ? EntityLogMessageFactory.Instance : NullEntityLogMessageFactory<IContactItemWrapper, GoogleContactWrapper>.Instance));

      var googleContactContextFactory = new GoogleContactContextFactory(googleApiExecutor, btypeIdEqualityComparer, options.UserName, options.ChunkSize);
      componentsToFill.GoogleContactContextFactory = googleContactContextFactory;
      return new OutlookSynchronizer<string, GoogleContactVersion> (
        new ContextCreatingSynchronizerDecorator<string, DateTime, IContactItemWrapper, string, GoogleContactVersion, GoogleContactWrapper, IGoogleContactContext> (
          synchronizer,
          googleContactContextFactory));
    }

    private static IChunkedExecutor CreateChunkedExecutor(Options options)
    {
      return options.IsChunkedSynchronizationEnabled ? new ChunkedExecutor(options.ChunkSize) : NullChunkedExecutor.Instance;
    }
  }
}