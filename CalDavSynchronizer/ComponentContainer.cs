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
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CalDavSynchronizer.AutomaticUpdates;
using CalDavSynchronizer.ChangeWatching;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Scheduling;
using CalDavSynchronizer.Ui;
using CalDavSynchronizer.Utilities;
using GenSync.ProgressReport;
using log4net;
using log4net.Config;
using Microsoft.Office.Interop.Outlook;
using Application = Microsoft.Office.Interop.Outlook.Application;
using Exception = System.Exception;

namespace CalDavSynchronizer
{
  public class ComponentContainer
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private readonly object _synchronizationContextLock = new object();
    private readonly Scheduler _scheduler;
    private readonly IOptionsDataAccess _optionsDataAccess;
    private readonly IGeneralOptionsDataAccess _generalOptionsDataAccess;
    private readonly UpdateChecker _updateChecker;
    private readonly NameSpace _session;
    private readonly OutlookItemChangeWatcher _itemChangeWatcher;
    private readonly string _applicationDataDirectory;

    public ComponentContainer (Application application)
    {
      try
      {
        XmlConfigurator.Configure();

        _generalOptionsDataAccess = new GeneralOptionsDataAccess();

        var generalOptions = _generalOptionsDataAccess.LoadOptions();

        ConfigureServicePointManager (generalOptions);

        _itemChangeWatcher = new OutlookItemChangeWatcher (application.Inspectors);
        _itemChangeWatcher.ItemSavedOrDeleted += ItemChangeWatcherItemSavedOrDeleted;
        _session = application.Session;
        s_logger.Info ("Startup...");

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

        var synchronizerFactory = new SynchronizerFactory (
            GetProfileDataDirectory,
            new TotalProgressFactory (
                new ProgressFormFactory(),
                int.Parse (ConfigurationManager.AppSettings["loadOperationThresholdForProgressDisplay"]),
                ExceptionHandler.Instance),
            _session,
            TimeSpan.Parse (ConfigurationManager.AppSettings["calDavConnectTimeout"]),
            TimeSpan.Parse (ConfigurationManager.AppSettings["calDavReadWriteTimeout"]));

        _scheduler = new Scheduler (synchronizerFactory, EnsureSynchronizationContext);
        _scheduler.SetOptions (_optionsDataAccess.LoadOptions());

        _updateChecker = new UpdateChecker (new AvailableVersionService(), () => _generalOptionsDataAccess.IgnoreUpdatesTilVersion);
        _updateChecker.NewerVersionFound += UpdateChecker_NewerVersionFound;
        _updateChecker.IsEnabled = generalOptions.ShouldCheckForNewerVersions;
      }
      catch (Exception x)
      {
        ExceptionHandler.Instance.LogException (x, s_logger);
        throw;
      }

      s_logger.Info ("Startup finnished");
    }

    private async void ItemChangeWatcherItemSavedOrDeleted (object sender, ItemSavedEventArgs e)
    {
      try
      {
        await _scheduler.RunResponsibleSynchronizationProfiles (e.EntryId, e.FolderEntryId, e.FolderStoreId);
      }
      catch (Exception x)
      {
        ExceptionHandler.Instance.LogException (x, s_logger);
      }
    }

    public static void ConfigureServicePointManager (GeneralOptions options)
    {
      ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11;

      if (options.EnableTls12)
      {
        ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
      }

      if (options.EnableSsl3)
      {
        ServicePointManager.SecurityProtocol |= SecurityProtocolType.Ssl3;
      }

      if (options.DisableCertificateValidation)
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
      else
        ServicePointManager.ServerCertificateValidationCallback = null; 
    }

    public async Task SynchronizeNowNoThrow ()
    {
      try
      {
        s_logger.Info ("Synchronization manually triggered");
        EnsureSynchronizationContext();
        await _scheduler.RunNow();
      }
      catch (Exception x)
      {
        ExceptionHandler.Instance.HandleException (x, s_logger);
      }
    }

    public void ShowOptionsNoThrow ()
    {
      try
      {
        var options = _optionsDataAccess.LoadOptions();
        Options[] newOptions;
        if (OptionsForm.EditOptions (_session, options, out newOptions, GetProfileDataDirectory))
        {
          _optionsDataAccess.SaveOptions (newOptions);
          
          _scheduler.SetOptions (newOptions);
          DeleteEntityChachesForChangedProfiles (options, newOptions);
        }
      }
      catch (Exception x)
      {
        ExceptionHandler.Instance.HandleException (x, s_logger);
      }
    }

    public void ShowGeneralOptionsNoThrow ()
    {
      try
      {
        var generalOptions = _generalOptionsDataAccess.LoadOptions ();
        using (var optionsForm = new GeneralOptionsForm())
        {
          optionsForm.Options = generalOptions;
          if(optionsForm.Display())
          {
            var newOptions = optionsForm.Options;
            _generalOptionsDataAccess.SaveOptions (newOptions);

            _updateChecker.IsEnabled = newOptions.ShouldCheckForNewerVersions;
          }
        }
      }
      catch (Exception x)
      {
        ExceptionHandler.Instance.HandleException (x, s_logger);
      }
    }

    private void DeleteEntityChachesForChangedProfiles (Options[] oldOptions, Options[] newOptions)
    {
      var profilesForCacheDeletion =
          oldOptions
              .Concat (newOptions)
              .GroupBy (o => o.Id)
              .Where (g => g.GroupBy (o => new { o.OutlookFolderStoreId, o.OutlookFolderEntryId, o.CalenderUrl }).Count() > 1)
              .Select (g => new { Id = g.Key, Name = g.First().Name })
              .ToArray();

      foreach (var profile in profilesForCacheDeletion)
      {
        try
        {
          s_logger.InfoFormat ("Deleting cache for profile '{0}' ('{1}')", profile.Id, profile.Name);

          var profileDataDirectory = GetProfileDataDirectory (profile.Id);
          if (Directory.Exists (profileDataDirectory))
            Directory.Delete (profileDataDirectory, true);
        }
        catch (Exception x)
        {
          s_logger.Error (null, x);
        }
      }
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

    private void ShowGetNewVersionForm (NewerVersionFoundEventArgs e)
    {
      try
      {
        var form = new GetNewVersionForm (e.WhatsNewInformation, e.NewVersion, e.DownloadLink);
        form.TurnOffCheckForNewerVersions += delegate
        {
          var options = _generalOptionsDataAccess.LoadOptions();
          options.ShouldCheckForNewerVersions = false;
          _generalOptionsDataAccess.SaveOptions (options);

          MessageBox.Show ("Automatic check for newer version turned off.", "CalDav Synchronizer");
        };

        form.IgnoreThisVersion += delegate
        {
          _generalOptionsDataAccess.IgnoreUpdatesTilVersion = e.NewVersion;
          MessageBox.Show (string.Format ("Waiting for newer version than '{0}'.", e.NewVersion), "CalDav Synchronizer");
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
    private void EnsureSynchronizationContext ()
    {
      lock (_synchronizationContextLock)
      {
        if (SynchronizationContext.Current == null)
        {
          SynchronizationContext.SetSynchronizationContext (new WindowsFormsSynchronizationContext());
        }
      }
    }
  }
}