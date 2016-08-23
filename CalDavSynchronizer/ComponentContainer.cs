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
using CalDavSynchronizer.Implementation;
using CalDavSynchronizer.Implementation.Tasks;
using CalDavSynchronizer.Ui.Options;
using CalDavSynchronizer.Ui.Options.ViewModels;
using CalDavSynchronizer.Ui.SystrayNotification;
using CalDavSynchronizer.Ui.SystrayNotification.ViewModels;
using GenSync.EntityRelationManagement;
using GenSync.Logging;
using MessageBox = System.Windows.Forms.MessageBox;

namespace CalDavSynchronizer
{
  public class ComponentContainer : IReportsViewModelParent, ISynchronizationReportSink, ICalDavSynchronizerCommands, IDisposable
  {
    public const string MessageBoxTitle = "CalDav Synchronizer";
    private static readonly ILog s_logger = LogManager.GetLogger (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    // ReSharper disable once ConvertToConstant.Local
    private readonly int c_requiredEntityCacheVersion = 1;

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
    private readonly IAvailableVersionService _availableVersionService;
    private readonly ProfileStatusesViewModel _profileStatusesViewModel;
    private ITrayNotifier _trayNotifier;
    private ISynchronizationProfilesViewModel _currentVisibleOptionsFormOrNull;
    private readonly IOutlookAccountPasswordProvider _outlookAccountPasswordProvider;
    private readonly SynchronizationStatus _synchronizationStatus;

    public event EventHandler SynchronizationFailedWhileReportsFormWasNotVisible;
    public event EventHandler<SchedulerStatusEventArgs> StatusChanged
    {
      add { _synchronizationStatus.StatusChanged += value; }
      remove { _synchronizationStatus.StatusChanged -= value; }
    }

    public ComponentContainer (Application application)
    {
      s_logger.Info ("Startup...");

      _generalOptionsDataAccess = new GeneralOptionsDataAccess();

      _synchronizationStatus = new SynchronizationStatus();

      var generalOptions = _generalOptionsDataAccess.LoadOptions();

      _daslFilterProvider = new DaslFilterProvider (generalOptions.IncludeCustomMessageClasses);

      FrameworkElement.LanguageProperty.OverrideMetadata (
          typeof (FrameworkElement),
          new FrameworkPropertyMetadata (XmlLanguage.GetLanguage (CultureInfo.CurrentCulture.IetfLanguageTag)));

      ConfigureServicePointManager (generalOptions);
      ConfigureLogLevel (generalOptions.EnableDebugLog);

      _session = application.Session;

      _outlookAccountPasswordProvider =
          string.IsNullOrEmpty (_session.CurrentProfileName)
              ? NullOutlookAccountPasswordProvider.Instance
              : new OutlookAccountPasswordProvider (_session.CurrentProfileName, application.Version);

      EnsureSynchronizationContext();

      _applicationDataDirectory = Path.Combine (
          Environment.GetFolderPath (
              generalOptions.StoreAppDataInRoamingFolder ? Environment.SpecialFolder.ApplicationData : Environment.SpecialFolder.LocalApplicationData),
          "CalDavSynchronizer");

      _optionsDataAccess = new OptionsDataAccess (
          Path.Combine (
              _applicationDataDirectory,
              GetOrCreateConfigFileName (_applicationDataDirectory, _session.CurrentProfileName)
              ));

      _synchronizerFactory = new SynchronizerFactory (
          GetProfileDataDirectory,
          new TotalProgressFactory (
              new ProgressFormFactory(),
              int.Parse (ConfigurationManager.AppSettings["loadOperationThresholdForProgressDisplay"]),
              ExceptionHandler.Instance),
          _session,
          _daslFilterProvider,
          _outlookAccountPasswordProvider);

      _synchronizationReportRepository = CreateSynchronizationReportRepository();

      UpdateGeneralOptionDependencies (generalOptions);

      _scheduler = new Scheduler (
          _synchronizerFactory,
          this,
          EnsureSynchronizationContext,
          new FolderChangeWatcherFactory (
              _session),
          _synchronizationStatus);

      var options = _optionsDataAccess.LoadOptions();

      EnsureCacheCompatibility (options);


      _profileStatusesViewModel = new ProfileStatusesViewModel (this);
      _profileStatusesViewModel.EnsureProfilesDisplayed (options);


      _availableVersionService = new AvailableVersionService();
      _updateChecker = new UpdateChecker (_availableVersionService, () => _generalOptionsDataAccess.IgnoreUpdatesTilVersion);
      _updateChecker.NewerVersionFound += UpdateChecker_NewerVersionFound;
      _updateChecker.IsEnabled = generalOptions.ShouldCheckForNewerVersions;

      _reportGarbageCollection = new ReportGarbageCollection (_synchronizationReportRepository, TimeSpan.FromDays (generalOptions.MaxReportAgeInDays));

      _trayNotifier = generalOptions.EnableTrayIcon ? new TrayNotifier (this) : NullTrayNotifer.Instance;
      _uiService = new UiService (_profileStatusesViewModel);

      using (var syncObjects = GenericComObjectWrapper.Create (_session.SyncObjects))
      {
        if (syncObjects.Inner != null && syncObjects.Inner.Count > 0)
        {
          _syncObject = syncObjects.Inner[1];
          if (generalOptions.TriggerSyncAfterSendReceive)
            _syncObject.SyncEnd += _sync_SyncEnd;
        }
      }
    }

    public async Task Initialize()
    {
      var options = _optionsDataAccess.LoadOptions ();
      var generalOptions = _generalOptionsDataAccess.LoadOptions ();

      await _scheduler.SetOptions (options, generalOptions);
    }

    private void _sync_SyncEnd()
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
      else if (currentEntityCacheVersion != c_requiredEntityCacheVersion)
      {
        s_logger.InfoFormat ("Image requires cache version '{0}',but caches have version '{1}'. Deleting caches.", c_requiredEntityCacheVersion, currentEntityCacheVersion);
        if (DeleteCachesForProfiles (options.Select (p => Tuple.Create (p.Id, p.Name))))
          _generalOptionsDataAccess.EntityCacheVersion = c_requiredEntityCacheVersion;
      }
    }

    private void UpdateGeneralOptionDependencies (GeneralOptions generalOptions)
    {
      _logReportsWithJustWarnings = generalOptions.LogReportsWithWarnings;
      _logReportsWithoutWarningsOrErrors = generalOptions.LogReportsWithoutWarningsOrErrors;

      _showReportsWithErrorsImmediately = generalOptions.ShowReportsWithErrorsImmediately;
      _showReportsWithWarningsImmediately = generalOptions.ShowReportsWithWarningsImmediately;

      _daslFilterProvider.SetDoIncludeCustomMessageClasses (generalOptions.IncludeCustomMessageClasses);
    }

    public void PostReport (SynchronizationReport report)
    {
      SaveAndShowReport (report);
      _profileStatusesViewModel.Update (report);
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

    public async Task ShowOptions (Guid? initialVisibleProfile = null)
    {
      if (_currentVisibleOptionsFormOrNull == null)
      {
        var options = _optionsDataAccess.LoadOptions();
        GeneralOptions generalOptions = _generalOptionsDataAccess.LoadOptions();
        try
        {
          var newOptions = ShowWpfOptions (initialVisibleProfile, generalOptions, options);

          if (newOptions != null)
            await ApplyNewOptions (options, newOptions, generalOptions);
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
    
    public Options[] ShowWpfOptions (Guid? initialSelectedProfileId, GeneralOptions generalOptions, Options[] options)
    {
      string[] categories;
      using (var categoriesWrapper = GenericComObjectWrapper.Create (_session.Categories))
      {
        categories = categoriesWrapper.Inner.ToSafeEnumerable<Category>().Select (c => c.Name).ToArray();
      }

      var viewModel = new OptionsCollectionViewModel (
          _session,
          generalOptions,
          _outlookAccountPasswordProvider,
          categories,
          GetProfileDataDirectory);

      _currentVisibleOptionsFormOrNull = viewModel;

      viewModel.SetOptionsCollection (options, initialSelectedProfileId);

      if (_uiService.ShowOptions (viewModel))
      {
        _optionsDataAccess.EnsureBackupExists ("Wpf");
        return viewModel.GetOptionsCollection();
      }
      else
      {
        return null;
      }
    }

    private async Task ApplyNewOptions (Options[] oldOptions, Options[] newOptions, GeneralOptions generalOptions)
    {
      _optionsDataAccess.SaveOptions (newOptions);
      await _scheduler.SetOptions (newOptions, generalOptions);
      _profileStatusesViewModel.EnsureProfilesDisplayed (newOptions);
      DeleteEntityChachesForChangedProfiles (oldOptions, newOptions);
      var changedOptions = CreateChangePairs (oldOptions, newOptions);
      SwitchCategories (changedOptions);
    }

    public void ShowLatestSynchronizationReport (Guid profileId)
    {
      ShowReportsImplementation();
      _currentReportsViewModel.ShowLatestSynchronizationReportCommand (profileId);
    }

    private ChangedOptions[] CreateChangePairs (Options[] oldOptions, Options[] newOptions)
    {
      var newOptionsById = newOptions.ToDictionary (o => o.Id);

      return (
          from o in oldOptions
          let no = GetNewOptionsOrNull (o, newOptionsById)
          where no != null
          select new ChangedOptions (o, no)).ToArray ();
    }

    private void SwitchCategories (ChangedOptions[] changedOptions)
    {
      foreach (var changedOption in changedOptions)
      {
        var oldEventCategory = GetMappingRefPropertyOrNull<EventMappingConfiguration, string> (changedOption.Old.MappingConfiguration, o => o.EventCategory);
        var newEventCategory = GetMappingRefPropertyOrNull<EventMappingConfiguration, string> (changedOption.New.MappingConfiguration, o => o.EventCategory);
        var negateEventCategoryFilter = GetMappingPropertyOrNull<EventMappingConfiguration, bool> (changedOption.New.MappingConfiguration, o => o.InvertEventCategoryFilter);

        if (oldEventCategory != newEventCategory && !String.IsNullOrEmpty (oldEventCategory) && !negateEventCategoryFilter.Value)
        {
          try
          {
            SwitchEventCategories (changedOption, oldEventCategory, newEventCategory);
          }
          catch (Exception x)
          {
            s_logger.Error (null, x);
          }
        }

        if (!String.IsNullOrEmpty (newEventCategory))
        {
          var mappingConfiguration = (EventMappingConfiguration) changedOption.New.MappingConfiguration;

          if (mappingConfiguration.UseEventCategoryColorAndMapFromCalendarColor || mappingConfiguration.CategoryShortcutKey != OlCategoryShortcutKey.olCategoryShortcutKeyNone)
          {
            try
            {
              using (var categoriesWrapper = GenericComObjectWrapper.Create (_session.Categories))
              {
                foreach (var existingCategory in categoriesWrapper.Inner.ToSafeEnumerable<Category>())
                {
                  if (existingCategory.ShortcutKey == mappingConfiguration.CategoryShortcutKey)
                  {
                    existingCategory.ShortcutKey = OlCategoryShortcutKey.olCategoryShortcutKeyNone;
                  }
                }
                
                using (var categoryWrapper = GenericComObjectWrapper.Create (categoriesWrapper.Inner[newEventCategory]))
                {
                  if (categoryWrapper.Inner == null)
                  {
                    categoriesWrapper.Inner.Add (newEventCategory, mappingConfiguration.EventCategoryColor, mappingConfiguration.CategoryShortcutKey);
                  }
                  else
                  {
                    categoryWrapper.Inner.Color = mappingConfiguration.EventCategoryColor;
                    categoryWrapper.Inner.ShortcutKey = mappingConfiguration.CategoryShortcutKey;
                  }
                }
              }
            }
            catch (Exception x)
            {
              s_logger.Error (null, x);
            }
          }
        }

        var oldTaskCategory = GetMappingRefPropertyOrNull<TaskMappingConfiguration, string> (changedOption.Old.MappingConfiguration, o => o.TaskCategory);
        var newTaskCategory = GetMappingRefPropertyOrNull<TaskMappingConfiguration, string> (changedOption.New.MappingConfiguration, o => o.TaskCategory);
        var negateTaskCategoryFilter = GetMappingPropertyOrNull<TaskMappingConfiguration, bool> (changedOption.New.MappingConfiguration, o => o.InvertTaskCategoryFilter);

        if (oldTaskCategory != newTaskCategory && !String.IsNullOrEmpty (oldTaskCategory) && !negateTaskCategoryFilter.Value)
        {
          try
          {
            SwitchTaskCategories (changedOption, oldTaskCategory, newTaskCategory);
          }
          catch (Exception x)
          {
            s_logger.Error (null, x);
          }
        }
      }
    }

    private void SwitchEventCategories (ChangedOptions changedOption, string oldCategory, string newCategory)
    {
      using (var calendarFolderWrapper = GenericComObjectWrapper.Create (
          (Folder) _session.GetFolderFromID (changedOption.New.OutlookFolderEntryId, changedOption.New.OutlookFolderStoreId)))
      {
        bool isInstantSearchEnabled = false;

        try
        {
          using (var store = GenericComObjectWrapper.Create (calendarFolderWrapper.Inner.Store))
          {
            if (store.Inner != null) isInstantSearchEnabled = store.Inner.IsInstantSearchEnabled;
          }
        }
        catch (COMException)
        {
          s_logger.Info ("Can't access IsInstantSearchEnabled property of store, defaulting to false.");
        }
        var filterBuilder = new StringBuilder (_daslFilterProvider.GetAppointmentFilter (isInstantSearchEnabled));
        OutlookEventRepository.AddCategoryFilter (filterBuilder, oldCategory, false);
        var eventIds = OutlookEventRepository.QueryFolder (_session, calendarFolderWrapper, filterBuilder).Select(e => e.Id);
        // todo concat Ids from cache

        foreach (var eventId in eventIds)
        {
          try
          {
            SwitchEventCategories (changedOption, oldCategory, newCategory, eventId);
          }
          catch (Exception x)
          {
            s_logger.Error (null, x);
          }
        }
      }
    }

    private void SwitchTaskCategories (ChangedOptions changedOption, string oldCategory, string newCategory)
    {
      using (var taskFolderWrapper = GenericComObjectWrapper.Create (
          (Folder)_session.GetFolderFromID (changedOption.New.OutlookFolderEntryId, changedOption.New.OutlookFolderStoreId)))
      {
        bool isInstantSearchEnabled = false;

        try
        {
          using (var store = GenericComObjectWrapper.Create (taskFolderWrapper.Inner.Store))
          {
            if (store.Inner != null) isInstantSearchEnabled = store.Inner.IsInstantSearchEnabled;
          }
        }
        catch (COMException)
        {
          s_logger.Info ("Can't access IsInstantSearchEnabled property of store, defaulting to false.");
        }
        var filterBuilder = new StringBuilder (_daslFilterProvider.GetTaskFilter (isInstantSearchEnabled));
        OutlookEventRepository.AddCategoryFilter (filterBuilder, oldCategory, false);
        var taskIds = OutlookTaskRepository.QueryFolder (_session, taskFolderWrapper, filterBuilder).Select(e => e.Id);
        // todo concat Ids from cache

        foreach (var taskId in taskIds)
        {
          try
          {
            SwitchTaskCategories (changedOption, oldCategory, newCategory, taskId);
          }
          catch (Exception x)
          {
            s_logger.Error (null, x);
          }
        }
      }
    }

    private void SwitchEventCategories (ChangedOptions changedOption, string oldCategory, string newCategory, string eventId)
    {
      using (var eventWrapper = new AppointmentItemWrapper (
          (AppointmentItem) _session.GetItemFromID (eventId, changedOption.New.OutlookFolderStoreId),
          entryId => (AppointmentItem) _session.GetItemFromID (entryId, changedOption.New.OutlookFolderStoreId)))
      {
        var categories = eventWrapper.Inner.Categories
            .Split (new[] { CultureInfo.CurrentCulture.TextInfo.ListSeparator }, StringSplitOptions.RemoveEmptyEntries)
            .Select (c => c.Trim());

        eventWrapper.Inner.Categories = string.Join (
            CultureInfo.CurrentCulture.TextInfo.ListSeparator,
            categories
                .Except (new[] { oldCategory })
                .Concat (new[] { newCategory })
                .Distinct());

        eventWrapper.Inner.Save();
      }
    }

    private void SwitchTaskCategories (ChangedOptions changedOption, string oldCategory, string newCategory, string eventId)
    {
      using (var taskWrapper = new TaskItemWrapper (
          (TaskItem)_session.GetItemFromID (eventId, changedOption.New.OutlookFolderStoreId),
          entryId => (TaskItem)_session.GetItemFromID (entryId, changedOption.New.OutlookFolderStoreId)))
      {
        var categories = taskWrapper.Inner.Categories
            .Split(new[] { CultureInfo.CurrentCulture.TextInfo.ListSeparator }, StringSplitOptions.RemoveEmptyEntries)
            .Select(c => c.Trim());

        taskWrapper.Inner.Categories = string.Join (
            CultureInfo.CurrentCulture.TextInfo.ListSeparator,
            categories
                .Except (new[] { oldCategory })
                .Concat (new[] { newCategory })
                .Distinct());

        taskWrapper.Inner.Save();
      }
    }

    public async Task ShowGeneralOptions ()
    {
      var generalOptions = _generalOptionsDataAccess.LoadOptions();
      using (var optionsForm = new GeneralOptionsForm())
      {
        optionsForm.Options = generalOptions;
        if (optionsForm.Display())
        {
          var newOptions = optionsForm.Options;

          ConfigureServicePointManager (newOptions);
          ConfigureLogLevel (newOptions.EnableDebugLog);

          _updateChecker.IsEnabled = newOptions.ShouldCheckForNewerVersions;
          _reportGarbageCollection.MaxAge = TimeSpan.FromDays (newOptions.MaxReportAgeInDays);

          _generalOptionsDataAccess.SaveOptions (newOptions);
          UpdateGeneralOptionDependencies (newOptions);
          await _scheduler.SetOptions (_optionsDataAccess.LoadOptions(), newOptions);

          if (newOptions.EnableTrayIcon != generalOptions.EnableTrayIcon)
          {
            _trayNotifier.Dispose();
            _trayNotifier = newOptions.EnableTrayIcon ? new TrayNotifier (this) : NullTrayNotifer.Instance;
          }

          if (_syncObject != null && newOptions.TriggerSyncAfterSendReceive != generalOptions.TriggerSyncAfterSendReceive)
          {
            if (newOptions.TriggerSyncAfterSendReceive)
              _syncObject.SyncEnd += _sync_SyncEnd;
            else
              _syncObject.SyncEnd -= _sync_SyncEnd;
          }
        }
      }
    }

    private struct ChangedOptions
    {
      private readonly Options _old;
      private readonly Options _new;

      public ChangedOptions (Options old, Options @new)
          : this()
      {
        _old = old;
        _new = @new;
      }

      public Options Old
      {
        get { return _old; }
      }

      public Options New
      {
        get { return _new; }
      }
    }

    private Options GetNewOptionsOrNull (Options oldOptions, Dictionary<Guid, Options> newOptionsById)
    {
      Options newOptions;
      newOptionsById.TryGetValue (oldOptions.Id, out newOptions);
      return newOptions;
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

    private TProperty? GetMappingPropertyOrNull<TMappingConfiguration, TProperty> (MappingConfigurationBase mappingConfiguration, Func<TMappingConfiguration, TProperty> selector)
      where TMappingConfiguration : MappingConfigurationBase
      where TProperty : struct
    {
      var typedMappingConfiguration = mappingConfiguration as TMappingConfiguration;
      
      if (typedMappingConfiguration != null)
        return selector (typedMappingConfiguration);
      else
        return null;
    }

    private TProperty GetMappingRefPropertyOrNull<TMappingConfiguration, TProperty> (MappingConfigurationBase mappingConfiguration, Func<TMappingConfiguration, TProperty> selector)
      where TMappingConfiguration : MappingConfigurationBase
      where TProperty : class
    {
      var typedMappingConfiguration = mappingConfiguration as TMappingConfiguration;

      if (typedMappingConfiguration != null)
        return selector (typedMappingConfiguration);
      else
        return null;
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

    public static string GetOrCreateConfigFileName (string applicationDataDirectory, string profileName)
    {
      var profileDataAccess = new ProfileListDataAccess (Path.Combine (applicationDataDirectory, "profiles.xml"));
      var profiles = profileDataAccess.Load();
      var profile = profiles.FirstOrDefault (p => String.Compare (p.ProfileName, profileName, StringComparison.OrdinalIgnoreCase) == 0);
      if (profile == null)
      {
        profile = new ProfileEntry()
                  {
                      ProfileName = profileName,
                      ConfigFileName = string.Format ("options_{0}.xml", Guid.NewGuid())
                  };
        profiles = profiles.Union (new[] { profile }).ToArray();
        profileDataAccess.Save (profiles);
      }
      return profile.ConfigFileName;
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
              _optionsDataAccess.LoadOptions().ToDictionary (o => o.Id, o => o.Name),
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
      _uiService.ShowProfileStatusesWindow();
    }

    public void ShowAbout ()
    {
      using (var aboutForm = new AboutForm (ThisAddIn.ComponentContainer.CheckForUpdatesNowAsync))
      {
        aboutForm.ShowDialog();
      }
    }

    public void DiplayAEntity (Guid synchronizationProfileId, string entityId)
    {
      var options = GetOptionsOrNull(synchronizationProfileId);
      if (options == null)
        return;

      var item = _session.GetItemFromID (entityId, options.OutlookFolderStoreId);

      var appointment = item as AppointmentItem;
      if (appointment != null)
      {
        appointment.GetInspector.Activate ();
        return;
      }

      var task = item as TaskItem;
      if (task != null)
      {
        task.GetInspector.Activate ();
        return;
      }
      
      var contact = item as ContactItem;
      if (contact != null)
      {
        contact.GetInspector.Activate ();
        return;
      }
    }

    public async void DiplayBEntityAsync (Guid synchronizationProfileId, string entityId)
    {
      try
      {
        var options = GetOptionsOrNull (synchronizationProfileId);
        if (options == null)
          return;

        var availableSynchronizerComponents =
          (await _synchronizerFactory.CreateSynchronizerWithComponents (options, _generalOptionsDataAccess.LoadOptions ())).Item2;

        if (availableSynchronizerComponents.CalDavDataAccess != null)
        {
          var entityName = new WebResourceName { Id = entityId, OriginalAbsolutePath = entityId };
          var entities = await availableSynchronizerComponents.CalDavDataAccess.GetEntities (new[] { entityName });
          DisplayFirstEntityIfAvailable (entities);
        }
        else if (availableSynchronizerComponents.CardDavDataAccess != null)
        {
          var entityName = new WebResourceName { Id = entityId, OriginalAbsolutePath = entityId };
          var entities = await availableSynchronizerComponents.CardDavDataAccess.GetEntities (new[] { entityName });
          DisplayFirstEntityIfAvailable (entities);
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

    private static void DisplayFirstEntityIfAvailable (IReadOnlyList<EntityWithId<WebResourceName, string>> entities)
    {
      if (entities.Count == 0)
      {
        MessageBox.Show ("The selected entity does not exist anymore.");
        return;
      }

      var tempFileName = Path.GetTempFileName();
      var tempTextFileName = tempFileName + ".txt";
      File.Move (tempFileName, tempTextFileName);

      File.WriteAllText (tempTextFileName, entities[0].Entity);
      System.Diagnostics.Process.Start (tempTextFileName);
    }

    private Options GetOptionsOrNull (Guid synchronizationProfileId)
    {
      var allOptions = _optionsDataAccess.LoadOptions ();
      var options = allOptions.FirstOrDefault (o => o.Id == synchronizationProfileId);
      if (options == null)
        MessageBox.Show ("The profile for the selected report doesn't exist anymore!");
      return options;
    }

    public void Dispose ()
    {
      _trayNotifier.Dispose();
    }

    public void SaveToolBarSettings(ToolbarSettings settings)
    {
      _generalOptionsDataAccess.SaveToolBarSettings(settings);
    }

    public ToolbarSettings LoadToolBarSettings()
    {
      return _generalOptionsDataAccess.LoadToolBarSettings();
    }
  }
}