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
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Markup;
using CalDavSynchronizer.AutomaticUpdates;
using CalDavSynchronizer.ChangeWatching;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.Implementation.Events;
using CalDavSynchronizer.Reports;
using CalDavSynchronizer.Scheduling;
using CalDavSynchronizer.Ui;
using CalDavSynchronizer.Ui.Reports.ViewModels;
using CalDavSynchronizer.Utilities;
using GenSync;
using GenSync.ProgressReport;
using log4net;
using log4net.Repository.Hierarchy;
using log4net.Core;
using Microsoft.Office.Interop.Outlook;
using Application = Microsoft.Office.Interop.Outlook.Application;
using Exception = System.Exception;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Media;
using CalDavSynchronizer.Implementation;
using CalDavSynchronizer.Implementation.Common;
using CalDavSynchronizer.Implementation.Tasks;
using CalDavSynchronizer.Implementation.TimeZones;
using CalDavSynchronizer.ProfileTypes;
using CalDavSynchronizer.Scheduling.ComponentCollectors;
using CalDavSynchronizer.Ui.Options;
using CalDavSynchronizer.Ui.Options.BulkOptions.ViewModels;
using CalDavSynchronizer.Ui.Options.Models;
using CalDavSynchronizer.Ui.Options.ViewModels;
using CalDavSynchronizer.Ui.SystrayNotification;
using CalDavSynchronizer.Ui.SystrayNotification.ViewModels;
using GenSync.EntityRelationManagement;
using GenSync.Logging;
using GenSync.Synchronization;
using AppointmentId = CalDavSynchronizer.Implementation.Events.AppointmentId;
using MessageBox = System.Windows.Forms.MessageBox;

namespace CalDavSynchronizer
{
  public class ComponentContainer : IComponentContainer, IReportsViewModelParent, ISynchronizationReportSink
  {
    public const string MessageBoxTitle = "CalDav Synchronizer";
    private static readonly ILog s_logger = LogManager.GetLogger (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    // ReSharper disable once ConvertToConstant.Local
    private static readonly int c_requiredEntityCacheVersion = 3;

    private static readonly object _synchronizationContextLock = new object();
    private readonly Scheduler _scheduler;
    private readonly IOptionsDataAccess _optionsDataAccess;
    private readonly IGeneralOptionsDataAccess _generalOptionsDataAccess;
    private readonly UpdateChecker _updateChecker;
    private readonly NameSpace _session;
    private readonly SyncObject _syncObject;
    private readonly string _applicationDataDirectory;
    private readonly ISynchronizationReportRepository _synchronizationReportRepository;
    private readonly IUiService _uiService;
    private ReportsViewModel _currentReportsViewModel;
    private bool _showReportsWithWarningsImmediately;
    private bool _showReportsWithErrorsImmediately;
    private bool _logReportsWithJustWarnings;
    private bool _logReportsWithoutWarningsOrErrors;
    private readonly ReportGarbageCollection _reportGarbageCollection;
    private readonly SynchronizerFactory _synchronizerFactory;
    private readonly DaslFilterProvider _daslFilterProvider;
    private readonly GlobalTimeZoneCache _globalTimeZoneCache;
    private readonly IAvailableVersionService _availableVersionService;
    private readonly IPermanentStatusesViewModel _permanentStatusesViewModel;
    private ITrayNotifier _trayNotifier;
    private ISynchronizationProfilesViewModel _currentVisibleOptionsFormOrNull;
    private readonly IOutlookAccountPasswordProvider _outlookAccountPasswordProvider;
    private readonly SynchronizationStatus _synchronizationStatus;
    private readonly OutlookFolderStrategyWrapper _queryFolderStrategyWrapper;
    private readonly IOneTimeTaskRunner _oneTimeTaskRunner;
    private readonly TotalProgressFactory _totalProgressFactory;
    private readonly IOutlookSession _outlookSession;
    private readonly IProfileTypeRegistry _profileTypeRegistry;

    public event EventHandler SynchronizationFailedWhileReportsFormWasNotVisible;
    public event EventHandler<SchedulerStatusEventArgs> StatusChanged
    {
      add { _synchronizationStatus.StatusChanged += value; }
      remove { _synchronizationStatus.StatusChanged -= value; }
    }

    public ComponentContainer (Application application, IGeneralOptionsDataAccess generalOptionsDataAccess, IComWrapperFactory comWrapperFactory, IExceptionHandlingStrategy exceptionHandlingStrategy)
    {
      if (application == null) throw new ArgumentNullException(nameof(application));
      if (generalOptionsDataAccess == null) throw new ArgumentNullException(nameof(generalOptionsDataAccess));
      if (comWrapperFactory == null) throw new ArgumentNullException(nameof(comWrapperFactory));

      s_logger.Info ("Startup...");

      _profileTypeRegistry = ProfileTypeRegistry.Instance;

      if (GeneralOptionsDataAccess.WpfRenderModeSoftwareOnly)
        RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;

      _generalOptionsDataAccess = generalOptionsDataAccess;

      _synchronizationStatus = new SynchronizationStatus();

      var generalOptions = _generalOptionsDataAccess.LoadOptions();

      _daslFilterProvider = new DaslFilterProvider (generalOptions.IncludeCustomMessageClasses);

      SetWpfLocale();

      ConfigureServicePointManager (generalOptions);
      ConfigureLogLevel (generalOptions.EnableDebugLog);

      _session = application.Session;

      _outlookAccountPasswordProvider =
          string.IsNullOrEmpty (_session.CurrentProfileName)
              ? NullOutlookAccountPasswordProvider.Instance
              : new OutlookAccountPasswordProvider (_session.CurrentProfileName, application.Version);

      _globalTimeZoneCache = new GlobalTimeZoneCache();


      var applicationDataDirectoryBase = Path.Combine (
          Environment.GetFolderPath (
              generalOptions.StoreAppDataInRoamingFolder ? Environment.SpecialFolder.ApplicationData : Environment.SpecialFolder.LocalApplicationData),
          "CalDavSynchronizer");
      
      string optionsFilePath;

      (_applicationDataDirectory, optionsFilePath) = GetOrCreateDataDirectory(applicationDataDirectoryBase, _session.CurrentProfileName);
      
      _optionsDataAccess = new OptionsDataAccess (optionsFilePath);

      _uiService = new UiService();
      var options = _optionsDataAccess.Load();

      _permanentStatusesViewModel = new PermanentStatusesViewModel(_uiService, this, options);
      _permanentStatusesViewModel.OptionsRequesting += PermanentStatusesViewModel_OptionsRequesting;

      _queryFolderStrategyWrapper = new OutlookFolderStrategyWrapper(QueryOutlookFolderByRequestingItemStrategy.Instance);

      _totalProgressFactory = new TotalProgressFactory(
        _uiService,
        generalOptions.ShowProgressBar,
        generalOptions.ThresholdForProgressDisplay,
        ExceptionHandler.Instance);


      _outlookSession = new OutlookSession(_session);
      _synchronizerFactory = new SynchronizerFactory (
          GetProfileDataDirectory,
          _totalProgressFactory,
          _outlookSession,
          _daslFilterProvider,
          _outlookAccountPasswordProvider,
          _globalTimeZoneCache,
          _queryFolderStrategyWrapper,
          exceptionHandlingStrategy,
          comWrapperFactory,
          _optionsDataAccess,
          _profileTypeRegistry);

      _synchronizationReportRepository = CreateSynchronizationReportRepository();

      UpdateGeneralOptionDependencies (generalOptions);

      _scheduler = new Scheduler (
          _synchronizerFactory,
          this,
          EnsureSynchronizationContext,
          new FolderChangeWatcherFactory (
              _session),
          _synchronizationStatus);

      EnsureCacheCompatibility (options);

      _availableVersionService = new AvailableVersionService();
      _updateChecker = new UpdateChecker (_availableVersionService, () => _generalOptionsDataAccess.IgnoreUpdatesTilVersion);
      _updateChecker.NewerVersionFound += UpdateChecker_NewerVersionFound;
      _updateChecker.IsEnabled = generalOptions.ShouldCheckForNewerVersions;

      _reportGarbageCollection = new ReportGarbageCollection (_synchronizationReportRepository, TimeSpan.FromDays (generalOptions.MaxReportAgeInDays));

      _trayNotifier = generalOptions.EnableTrayIcon ? new TrayNotifier (this) : NullTrayNotifer.Instance;

      try
      {
        using (var syncObjects = GenericComObjectWrapper.Create (_session.SyncObjects))
        {
          if (syncObjects.Inner != null && syncObjects.Inner.Count > 0)
          {
            _syncObject = syncObjects.Inner[1];
            if (generalOptions.TriggerSyncAfterSendReceive)
              _syncObject.SyncEnd += SyncObject_SyncEnd;
          }
        }
      }
      catch (COMException ex)
      {
        s_logger.Error ("Can't access SyncObjects", ex);
      }

      _oneTimeTaskRunner = new OneTimeTaskRunner(_outlookSession);
    }

    private void PermanentStatusesViewModel_OptionsRequesting(object sender, OptionsEventArgs e)
    {
      e.Options = _optionsDataAccess.Load();
    }

    private static bool _wpfLocaleSet;
    private static void SetWpfLocale()
    {
      if (!_wpfLocaleSet)
      {
        FrameworkElement.LanguageProperty.OverrideMetadata(
          typeof(FrameworkElement),
          new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
        _wpfLocaleSet = true;
      }
    }
    
    public async Task InitializeSchedulerAndStartAsync()
    {
      _scheduler.Start();

      var options = _optionsDataAccess.Load ();
      var generalOptions = _generalOptionsDataAccess.LoadOptions ();

      await _scheduler.SetOptions (options, generalOptions);
      if (generalOptions.TriggerSyncAfterSendReceive)
      {
        s_logger.Info ("Triggering sync after startup");
        EnsureSynchronizationContext ();
        SynchronizeInitial ();
      }
    }

    async void SynchronizeInitial()
    {
      try
      {
        await Task.Delay(TimeSpan.FromSeconds(10));
        SynchronizeNowAsync();
      }
      catch (Exception x)
      {
        s_logger.Error("Error during initial sync", x);
      }
    }


    private void SyncObject_SyncEnd()
    {
      s_logger.Info ("Snyc triggered after Outlook Send/Receive finished");
      EnsureSynchronizationContext();
      SynchronizeNowAsync();
    }

    private void EnsureCacheCompatibility (Options[] options)
    {
      var currentEntityCacheVersion = _generalOptionsDataAccess.EntityCacheVersion;

      if (currentEntityCacheVersion == 0 && c_requiredEntityCacheVersion == 1)
      {
        try
        {
          s_logger.InfoFormat ("Converting caches from 0 to 1");
          EntityCacheVersionConversion.Version0To1.Convert (
              options.Select (o => EntityRelationDataAccess.GetRelationStoragePath (GetProfileDataDirectory (o.Id))).ToArray());
          _generalOptionsDataAccess.EntityCacheVersion = c_requiredEntityCacheVersion;
        }
        catch (Exception x)
        {
          s_logger.Error ("Error during conversion. Deleting caches", x);
          if (DeleteCachesForProfiles (options.Select (p => Tuple.Create (p.Id, p.Name))))
            _generalOptionsDataAccess.EntityCacheVersion = c_requiredEntityCacheVersion;
        }
      }
      else if (currentEntityCacheVersion == 1 && c_requiredEntityCacheVersion == 2)
      {
        try
        {
          s_logger.InfoFormat("Converting caches from 1 to 2");
          EntityCacheVersionConversion.Version1To2.Convert(
            _session,
            options,
            o => EntityRelationDataAccess.GetRelationStoragePath (GetProfileDataDirectory (o.Id)),
            o => DeleteCachesForProfiles(new[] {Tuple.Create(o.Id, o.Name)}));
          _generalOptionsDataAccess.EntityCacheVersion = c_requiredEntityCacheVersion;
        }
        catch (Exception x)
        {
          s_logger.Error("Error during conversion. Deleting caches", x);
          if (DeleteCachesForProfiles(options.Select(p => Tuple.Create(p.Id, p.Name))))
            _generalOptionsDataAccess.EntityCacheVersion = c_requiredEntityCacheVersion;
        }
      }
      else if (currentEntityCacheVersion == 2 && c_requiredEntityCacheVersion == 3)
      {
        try
        {
          s_logger.InfoFormat("Converting caches from 2 to 3");
          EntityCacheVersionConversion.Version2To3.Convert(
            _session,
            options,
            o => EntityRelationDataAccess.GetRelationStoragePath(GetProfileDataDirectory(o.Id)),
            o => DeleteCachesForProfiles(new[] { Tuple.Create(o.Id, o.Name) }));
          _generalOptionsDataAccess.EntityCacheVersion = c_requiredEntityCacheVersion;
        }
        catch (Exception x)
        {
          s_logger.Error("Error during conversion. Deleting caches", x);
          if (DeleteCachesForProfiles(options.Select(p => Tuple.Create(p.Id, p.Name))))
            _generalOptionsDataAccess.EntityCacheVersion = c_requiredEntityCacheVersion;
        }
      }
      else if (currentEntityCacheVersion == 1 && c_requiredEntityCacheVersion == 3)
      {
        try
        {
          s_logger.InfoFormat("Converting caches from 1 to 2");
          EntityCacheVersionConversion.Version1To2.Convert(
            _session,
            options,
            o => EntityRelationDataAccess.GetRelationStoragePath(GetProfileDataDirectory(o.Id)),
            o => DeleteCachesForProfiles(new[] { Tuple.Create(o.Id, o.Name) }));

          s_logger.InfoFormat("Converting caches from 2 to 3");
          EntityCacheVersionConversion.Version2To3.Convert(
            _session,
            options,
            o => EntityRelationDataAccess.GetRelationStoragePath(GetProfileDataDirectory(o.Id)),
            o => DeleteCachesForProfiles(new[] { Tuple.Create(o.Id, o.Name) }));
          _generalOptionsDataAccess.EntityCacheVersion = c_requiredEntityCacheVersion;
        }
        catch (Exception x)
        {
          s_logger.Error("Error during conversion. Deleting caches", x);
          if (DeleteCachesForProfiles(options.Select(p => Tuple.Create(p.Id, p.Name))))
            _generalOptionsDataAccess.EntityCacheVersion = c_requiredEntityCacheVersion;
        }
      }
      else if (currentEntityCacheVersion != c_requiredEntityCacheVersion)
      {
        s_logger.InfoFormat ("Image requires cache version '{0}',but caches have version '{1}'. Deleting caches.", c_requiredEntityCacheVersion, currentEntityCacheVersion);
        if (DeleteCachesForProfiles (options.Select (p => Tuple.Create (p.Id, p.Name))))
          _generalOptionsDataAccess.EntityCacheVersion = c_requiredEntityCacheVersion;
      }
    }

    private void UpdateGeneralOptionDependencies (GeneralOptions generalOptions)
    {
      SystemNetSettings.UseUnsafeHeaderParsing = generalOptions.UseUnsafeHeaderParsing;

      _logReportsWithJustWarnings = generalOptions.LogReportsWithWarnings;
      _logReportsWithoutWarningsOrErrors = generalOptions.LogReportsWithoutWarningsOrErrors;

      _showReportsWithErrorsImmediately = generalOptions.ShowReportsWithErrorsImmediately;
      _showReportsWithWarningsImmediately = generalOptions.ShowReportsWithWarningsImmediately;

      _daslFilterProvider.SetDoIncludeCustomMessageClasses (generalOptions.IncludeCustomMessageClasses);
      _totalProgressFactory.ShowProgress = generalOptions.ShowProgressBar;
      _totalProgressFactory.LoadOperationThresholdForProgressDisplay = generalOptions.ThresholdForProgressDisplay;
      _queryFolderStrategyWrapper.SetStrategy (generalOptions.QueryFoldersJustByGetTable ? QueryOutlookFolderByGetTableStrategy.Instance : QueryOutlookFolderByRequestingItemStrategy.Instance);
    }

    public void PostReport (SynchronizationReport report)
    {
      SaveAndShowReport(report);
      _permanentStatusesViewModel.Update (report.ProfileId, new SynchronizationRunSummary(report));
      _trayNotifier.NotifyUser (report, _showReportsWithWarningsImmediately, _showReportsWithErrorsImmediately);
    }

    private void SaveAndShowReport (SynchronizationReport report)
    {
      if (report.HasErrors
          || _logReportsWithJustWarnings && report.HasWarnings
          || _logReportsWithoutWarningsOrErrors)
      {
        var reportName = _synchronizationReportRepository.AddReport (report);

        if (IsReportsViewVisible)
        {
          ShowReportsImplementation(); // show to bring it into foreground
          return;
        }

        var hasErrors = report.HasErrors;
        var hasWarnings = report.HasWarnings;

        if (hasErrors || hasWarnings)
        {
          if (hasWarnings && _showReportsWithWarningsImmediately
              || hasErrors && _showReportsWithErrorsImmediately)
          {
            ShowReportsImplementation();
            var reportNameAsString = reportName.ToString();
            _currentReportsViewModel.Reports.Single (r => r.ReportName.ToString() == reportNameAsString).IsSelected = true;
            return;
          }

          var handler = SynchronizationFailedWhileReportsFormWasNotVisible;
          if (handler != null)
            handler (this, EventArgs.Empty);
        }
      }
    }

    private ISynchronizationReportRepository CreateSynchronizationReportRepository ()
    {
      var reportDirectory = Path.Combine (_applicationDataDirectory, "reports");

      if (!Directory.Exists (reportDirectory))
        Directory.CreateDirectory (reportDirectory);

      return new SynchronizationReportRepository (reportDirectory);
    }

    private static void ConfigureServicePointManager (GeneralOptions options)
    {
      ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11;

      if (options.EnableTls12)
        ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

      if (options.EnableSsl3)
        ServicePointManager.SecurityProtocol |= SecurityProtocolType.Ssl3;

      if (options.DisableCertificateValidation)
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
      else
        ServicePointManager.ServerCertificateValidationCallback = null;
    }

    private void ConfigureLogLevel (bool debugLogLevel)
    {
      if (debugLogLevel)
      {
        ((Hierarchy) LogManager.GetRepository()).Root.Level = Level.Debug;
      }
      else
      {
        ((Hierarchy) LogManager.GetRepository()).Root.Level = Level.Info;
      }
      ((Hierarchy) LogManager.GetRepository()).RaiseConfigurationChanged (EventArgs.Empty);
    }

    public async void SynchronizeNowAsync ()
    {
      try
      {
        s_logger.Info ("Synchronization manually triggered");
        await _scheduler.RunNow();
      }
      catch (Exception x)
      {
        ExceptionHandler.Instance.DisplayException (x, s_logger);
      }
    }

    public async Task ShowOptionsAsync (Guid? initialVisibleProfile = null)
    {
      if (_currentVisibleOptionsFormOrNull == null)
      {
        var options = _optionsDataAccess.Load();
        GeneralOptions generalOptions = _generalOptionsDataAccess.LoadOptions();
        try
        {
          var newOptions = ShowWpfOptions (initialVisibleProfile, generalOptions, options, out var oneTimeTasks);

          if (newOptions != null)
          {
            s_logger.Info("Applying new options");
            await ApplyNewOptions(options, newOptions, generalOptions, oneTimeTasks);
            s_logger.Info("Applied new options");
          }
        }
        finally
        {
          _currentVisibleOptionsFormOrNull = null;
        }
      }
      else
      {
        _currentVisibleOptionsFormOrNull.BringToFront();
        if (initialVisibleProfile.HasValue)
          _currentVisibleOptionsFormOrNull.ShowProfile (initialVisibleProfile.Value);
      }
    }
    
    public Options[] ShowWpfOptions (Guid? initialSelectedProfileId, GeneralOptions generalOptions, Options[] options, out OneTimeChangeCategoryTask[] oneTimeTasks)
    {
      string[] categories;
      using (var categoriesWrapper = GenericComObjectWrapper.Create (_session.Categories))
      {
        categories = categoriesWrapper.Inner.ToSafeEnumerable<Category>().Select (c => c.Name).ToArray();
      }

      var faultFinder = generalOptions.FixInvalidSettings ? new SettingsFaultFinder(EnumDisplayNameProvider.Instance) : NullSettingsFaultFinder.Instance;

      var optionTasks = new OptionTasks(_session, EnumDisplayNameProvider.Instance, faultFinder );

      var viewOptions = new ViewOptions (generalOptions.EnableAdvancedView);
      OptionModelSessionData sessionData = new OptionModelSessionData(_outlookSession.GetCategories().ToDictionary(c => c.Name , _outlookSession.CategoryNameComparer));
      var viewModel = new OptionsCollectionViewModel(
        generalOptions.ExpandAllSyncProfiles,
        GetProfileDataDirectory,
        _uiService,
        optionTasks,
        _profileTypeRegistry,
        (parent, type) => type.CreateModelFactory(parent, _outlookAccountPasswordProvider, categories, optionTasks, faultFinder, generalOptions, viewOptions, sessionData),
        viewOptions);

      _currentVisibleOptionsFormOrNull = viewModel;

      viewModel.SetOptionsCollection (options, initialSelectedProfileId);

      if (_uiService.ShowOptions (viewModel))
      {
        oneTimeTasks = viewModel.GetOneTimeTasks();
        return viewModel.GetOptionsCollection();
      }
      else
      {
        oneTimeTasks = null;
        return null;
      }
    }

    private async Task ApplyNewOptions (Options[] oldOptions, Options[] newOptions, GeneralOptions generalOptions, IEnumerable<OneTimeChangeCategoryTask> oneTimeTasks)
    {
      _optionsDataAccess.Save (newOptions);
      await _scheduler.SetOptions (newOptions, generalOptions);
      _permanentStatusesViewModel.NotifyProfilesChanged (newOptions);
      DeleteEntityChachesForChangedProfiles (oldOptions, newOptions);
      _oneTimeTaskRunner.RunOneTimeTasks (oneTimeTasks);
    }

    public void ShowLatestSynchronizationReport (Guid profileId)
    {
      ShowReportsImplementation();
      _currentReportsViewModel.ShowLatestSynchronizationReportCommand (profileId);
    }
    
    public async Task EditGeneralOptionsAsync(Func<GeneralOptions, Tuple<bool, GeneralOptions>> editOptions)
    {
      var generalOptions = _generalOptionsDataAccess.LoadOptions();
      var editResult = editOptions(generalOptions);

      if (editResult.Item1)
      {
        var newOptions = editResult.Item2;

        ConfigureServicePointManager(newOptions);
        ConfigureLogLevel(newOptions.EnableDebugLog);

        _updateChecker.IsEnabled = newOptions.ShouldCheckForNewerVersions;
        _reportGarbageCollection.MaxAge = TimeSpan.FromDays(newOptions.MaxReportAgeInDays);

        _generalOptionsDataAccess.SaveOptions(newOptions);
        UpdateGeneralOptionDependencies(newOptions);
        await _scheduler.SetOptions(_optionsDataAccess.Load(), newOptions);

        if (newOptions.EnableTrayIcon != generalOptions.EnableTrayIcon)
        {
          _trayNotifier.Dispose();
          _trayNotifier = newOptions.EnableTrayIcon ? new TrayNotifier(this) : NullTrayNotifer.Instance;
        }

        if (_syncObject != null && newOptions.TriggerSyncAfterSendReceive != generalOptions.TriggerSyncAfterSendReceive)
        {
          if (newOptions.TriggerSyncAfterSendReceive)
            _syncObject.SyncEnd += SyncObject_SyncEnd;
          else
            _syncObject.SyncEnd -= SyncObject_SyncEnd;
        }
      }
    }

    public async Task ShowGeneralOptionsAsync()
    {
      await EditGeneralOptionsAsync (
        o =>
        {
          using (var optionsForm = new GeneralOptionsForm())
          {
            optionsForm.Options = o;
            if (optionsForm.Display())
            {
              return Tuple.Create(true, optionsForm.Options);
            }
            else
            {
              return Tuple.Create(false, (GeneralOptions) null);
            }
          }
        });
    }
    
    private void DeleteEntityChachesForChangedProfiles (Options[] oldOptions, Options[] newOptions)
    {
      var profilesForCacheDeletion =
          oldOptions
              .Concat (newOptions)
              .GroupBy (o => o.Id)
              .Where (g => g.GroupBy (o => new
                                           {
                                               o.OutlookFolderStoreId,
                                               o.OutlookFolderEntryId,
                                               o.CalenderUrl,
                                               o.UserName,
                                               o.DaysToSynchronizeInTheFuture,
                                               o.DaysToSynchronizeInThePast,
                                               o.IgnoreSynchronizationTimeRange,
                                               o.ServerAdapterType
                                           }).Count() > 1)
              .Select (g => Tuple.Create (g.Key, g.First().Name))
              .ToArray();

      DeleteCachesForProfiles (profilesForCacheDeletion);
    }

    private bool DeleteCachesForProfiles (IEnumerable<Tuple<Guid,string>> profileIdWithNames)
    {
      bool allCachesDeleted = true;

      foreach (var profileIdWithName in profileIdWithNames)
      {
        try
        {
          s_logger.InfoFormat ("Deleting cache for profile '{0}' ('{1}')", profileIdWithName.Item1, profileIdWithName.Item2);

          var profileDataDirectory = GetProfileDataDirectory (profileIdWithName.Item1);
          if (Directory.Exists (profileDataDirectory))
            Directory.Delete (profileDataDirectory, true);
        }
        catch (Exception x)
        {
          s_logger.Error (null, x);
          allCachesDeleted = false;
        }
      }

      return allCachesDeleted;
    }

    private string GetProfileDataDirectory (Guid profileId)
    {
      return Path.Combine (
          _applicationDataDirectory,
          profileId.ToString());
    }

    private void UpdateChecker_NewerVersionFound (object sender, NewerVersionFoundEventArgs e)
    {
      EnsureSynchronizationContext();
      SynchronizationContext.Current.Send (_ => ShowGetNewVersionForm (e), null);
    }

    private async void CheckForUpdatesNowAsync ()
    {
      try
      {
        s_logger.Info ("CheckForUpdates manually triggered");

        var availableVersion = await Task.Run ((Func<Version>) _availableVersionService.GetVersionOfDefaultDownload);
        if (availableVersion == null)
        {
          MessageBox.Show ("Did not find any default Version!", MessageBoxTitle);
          return;
        }

        var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
        if (availableVersion > currentVersion)
        {
          ShowGetNewVersionForm (
              new NewerVersionFoundEventArgs (
                  availableVersion,
                  _availableVersionService.GetWhatsNewNoThrow (currentVersion, availableVersion),
                  _availableVersionService.DownloadLink));
        }
        else
        {
          MessageBox.Show ("No newer Version available.", MessageBoxTitle);
        }
      }
      catch (Exception x)
      {
        ExceptionHandler.Instance.DisplayException (x, s_logger);
      }
    }

    private void ShowGetNewVersionForm (NewerVersionFoundEventArgs e)
    {
      try
      {
        var form = new GetNewVersionForm (
          e.WhatsNewInformation, 
          e.NewVersion, 
          e.DownloadLink,
          new GlobalOptionsDataAccess().LoadGlobalOptionsNoThrow().IsInstallNewVersionEnabled);

        form.TurnOffCheckForNewerVersions += delegate
        {
          var options = _generalOptionsDataAccess.LoadOptions();
          options.ShouldCheckForNewerVersions = false;
          _generalOptionsDataAccess.SaveOptions (options);

          MessageBox.Show ("Automatic check for newer version turned off.", MessageBoxTitle);
        };

        form.IgnoreThisVersion += delegate
        {
          _generalOptionsDataAccess.IgnoreUpdatesTilVersion = e.NewVersion;
          MessageBox.Show (string.Format ("Waiting for newer version than '{0}'.", e.NewVersion),  MessageBoxTitle);
        };

        form.ShowDialog();
      }
      catch (Exception x)
      {
        ExceptionHandler.Instance.LogException (x, s_logger);
      }
    }

    public static (string DataDirectoryPath, string ConfigFilePath) GetOrCreateDataDirectory(string applicationDataDirectoryBase, string profileName)
    {
      var profileDataAccess = new ProfileListDataAccess (Path.Combine (applicationDataDirectoryBase, "profiles.xml"));
      var profiles = profileDataAccess.Load();
      var profile = profiles.FirstOrDefault (p => String.Compare (p.ProfileName, profileName, StringComparison.OrdinalIgnoreCase) == 0);
      if (profile == null)
      {
        var profileGuid = Guid.NewGuid();
        profile = new ProfileEntry()
                  {
                      ProfileName = profileName,
                      ConfigFileName = "options.xml",
                      DataDirectoryName = profileGuid.ToString()

                  };
        profiles = profiles.Union (new[] { profile }).ToArray();
        profileDataAccess.Save (profiles);
      }

      var dataDirectory = string.IsNullOrEmpty(profile.DataDirectoryName) ? applicationDataDirectoryBase : Path.Combine(applicationDataDirectoryBase, profile.DataDirectoryName);
      if (!Directory.Exists(dataDirectory))
        Directory.CreateDirectory(dataDirectory);

      return (
        dataDirectory,
        Path.Combine(dataDirectory, profile.ConfigFileName));
    }

    /// <summary>
    /// Ensures that the syncronizationcontext is not null ( it seems to be a bug that the synchronizationcontext is null in Office Addins)
    /// </summary>
    public static void EnsureSynchronizationContext ()
    {
      lock (_synchronizationContextLock)
      {
        if (SynchronizationContext.Current == null)
        {
          SynchronizationContext.SetSynchronizationContext (new WindowsFormsSynchronizationContext());
        }
      }
    }

    public void ShowReports ()
    {
      EnsureSynchronizationContext();
      ShowReportsImplementation();
    }

    private bool IsReportsViewVisible
    {
      get { return _currentReportsViewModel != null; }
    }

    private void ShowReportsImplementation ()
    {
      if (_currentReportsViewModel == null)
      {
        try
        {
          _currentReportsViewModel = new ReportsViewModel (
              _synchronizationReportRepository,
              _optionsDataAccess.Load().ToDictionary (o => o.Id, o => o.Name),
              this);

          _currentReportsViewModel.ReportsClosed += delegate { _currentReportsViewModel = null; };

          _uiService.Show (_currentReportsViewModel);
        }
        catch
        {
          _currentReportsViewModel = null;
          throw;
        }
      }
      else
      {
        _currentReportsViewModel.RequireBringToFront();
      }
    }

    public void ShowProfileStatuses ()
    {
      EnsureSynchronizationContext();
      _permanentStatusesViewModel.SetVisible();
    }

    public void ShowAbout ()
    {
      using (var aboutForm = new AboutForm (CheckForUpdatesNowAsync))
      {
        aboutForm.ShowDialog();
      }
    }

    public void DiplayAEntity (Guid synchronizationProfileId, string entityId)
    {
      var options = GetOptionsOrNull(synchronizationProfileId);
      if (options == null)
        return;

      try
      {

        var item = _session.GetItemFromID (entityId, options.OutlookFolderStoreId);

        var appointment = item as AppointmentItem;
        if (appointment != null)
        {
          appointment.GetInspector.Activate();
          return;
        }

        var task = item as TaskItem;
        if (task != null)
        {
          task.GetInspector.Activate();
          return;
        }

        var contact = item as ContactItem;
        if (contact != null)
        {
          contact.GetInspector.Activate();
          return;
        }

        var distList = item as DistListItem;
        if (distList != null)
        {
          distList.GetInspector.Activate();
          return;
        }
      }
      catch (COMException ex)
      {
        MessageBox.Show ("Can't access Outlook item, maybe it was moved or deleted!", MessageBoxTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
        s_logger.Warn ("Can't access Outlook item, maybe it was moved or deleted!", ex);
      }
    }

    public async void DiplayBEntityAsync (Guid synchronizationProfileId, string entityId)
    {
      try
      {
        var options = GetOptionsOrNull (synchronizationProfileId);
        if (options == null)
          return;

        var availableComponents =
          (await _synchronizerFactory.CreateSynchronizerWithComponents (options, _generalOptionsDataAccess.LoadOptions ())).Item2.GetDataAccessComponents();

        if (availableComponents.CalDavDataAccess != null)
        {
          var entityName = new WebResourceName { Id = entityId, OriginalAbsolutePath = entityId };
          var entities = await availableComponents.CalDavDataAccess.GetEntities (new[] { entityName });
          DisplayFirstEntityIfAvailable (entities.FirstOrDefault());
        }
        else if (availableComponents.CardDavDataAccess != null || availableComponents.DistListDataAccess != null)
        {
          var entityName = new WebResourceName { Id = entityId, OriginalAbsolutePath = entityId };

          EntityWithId<WebResourceName, string> entity = null;

          if (availableComponents.CardDavDataAccess != null)
            entity = (await availableComponents.CardDavDataAccess.GetEntities(new[] { entityName })).FirstOrDefault();

          if (entity == null && availableComponents.DistListDataAccess != null)
            entity = (await availableComponents.DistListDataAccess.GetEntities(new[] { entityName })).FirstOrDefault();

          DisplayFirstEntityIfAvailable(entity);
        }
        else
        {
          MessageBox.Show ($"The type of profile '{options.Name}' doesn't provide a way to display server entities.");
        }
      }
      catch (Exception x)
      {
        ExceptionHandler.Instance.DisplayException (x, s_logger);
      }
    }


    private static void DisplayFirstEntityIfAvailable (EntityWithId<WebResourceName, string> entityOrNull)
    {
      if (entityOrNull == null)
      {
        MessageBox.Show ("The selected entity does not exist anymore.");
        return;
      }

      var tempFileName = Path.GetTempFileName();
      var tempTextFileName = tempFileName + ".txt";
      File.Move (tempFileName, tempTextFileName);

      File.WriteAllText (tempTextFileName, entityOrNull.Entity);
      System.Diagnostics.Process.Start (tempTextFileName);
    }

    private Options GetOptionsOrNull (Guid synchronizationProfileId)
    {
      var allOptions = _optionsDataAccess.Load ();
      var options = allOptions.FirstOrDefault (o => o.Id == synchronizationProfileId);
      if (options == null)
        MessageBox.Show ("The profile for the selected report doesn't exist anymore!");
      return options;
    }

    public void Dispose ()
    {
      _trayNotifier.Dispose();
    }
  }
}