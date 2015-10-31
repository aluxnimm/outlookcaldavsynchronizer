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
    private readonly UpdateChecker _updateChecker;
    private readonly NameSpace _session;
    private readonly OutlookItemChangeWatcher _itemChangeWatcher;

    public ComponentContainer (Application application)
    {
      try
      {
        XmlConfigurator.Configure();

        _itemChangeWatcher = new OutlookItemChangeWatcher (application.Inspectors);
        _itemChangeWatcher.ItemSavedOrDeleted += ItemChangeWatcherItemSavedOrDeleted;
        _session = application.Session;
        s_logger.Info ("Startup...");

        EnsureSynchronizationContext();

        var applicationDataDirectory = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.LocalApplicationData), "CalDavSynchronizer");

        _optionsDataAccess = new OptionsDataAccess (
            Path.Combine (
                applicationDataDirectory,
                GetOrCreateConfigFileName (applicationDataDirectory, _session.CurrentProfileName)
                ));

        var synchronizerFactory = new SynchronizerFactory (
            applicationDataDirectory,
            new TotalProgressFactory (
                new ProgressFormFactory(),
                int.Parse (ConfigurationManager.AppSettings["loadOperationThresholdForProgressDisplay"]),
                ExceptionHandler.Instance),
            _session,
            TimeSpan.Parse (ConfigurationManager.AppSettings["calDavConnectTimeout"]),
            TimeSpan.Parse (ConfigurationManager.AppSettings["calDavReadWriteTimeout"]));

        _scheduler = new Scheduler (synchronizerFactory, EnsureSynchronizationContext);
        _scheduler.SetOptions (_optionsDataAccess.LoadOptions());

        _updateChecker = new UpdateChecker (new AvailableVersionService(), () => _optionsDataAccess.IgnoreUpdatesTilVersion);
        _updateChecker.NewerVersionFound += UpdateChecker_NewerVersionFound;
        _updateChecker.IsEnabled = _optionsDataAccess.ShouldCheckForNewerVersions;
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

    public static void ConfigureServicePointManager ()
    {
      ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11;

      if (Boolean.Parse (ConfigurationManager.AppSettings["enableTls12"]))
      {
        ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
      }

      if (Boolean.Parse (ConfigurationManager.AppSettings["enableSsl3"]))
      {
        ServicePointManager.SecurityProtocol |= SecurityProtocolType.Ssl3;
      }

      if (Boolean.Parse (ConfigurationManager.AppSettings["disableCertificateValidation"]))
      {
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
      }
    }

    private bool ShouldCheckForNewerVersions
    {
      get { return _optionsDataAccess.ShouldCheckForNewerVersions; }
      set
      {
        _updateChecker.IsEnabled = value;
        _optionsDataAccess.ShouldCheckForNewerVersions = value;
      }
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
        var shouldCheckForNewerVersions = ShouldCheckForNewerVersions;
        if (OptionsForm.EditOptions (_session, options, out options, shouldCheckForNewerVersions, out shouldCheckForNewerVersions))
        {
          _optionsDataAccess.SaveOptions (options);
          ShouldCheckForNewerVersions = shouldCheckForNewerVersions;
          _scheduler.SetOptions (options);
        }
      }
      catch (Exception x)
      {
        ExceptionHandler.Instance.HandleException (x, s_logger);
      }
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
          ShouldCheckForNewerVersions = false;
          MessageBox.Show ("Automatic check for newer version turned off.", "CalDav Synchronizer");
        };

        form.IgnoreThisVersion += delegate
        {
          _optionsDataAccess.IgnoreUpdatesTilVersion = e.NewVersion;
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