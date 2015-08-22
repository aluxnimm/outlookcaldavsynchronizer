using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CalDavSynchronizer.AutomaticUpdates;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Scheduling;
using CalDavSynchronizer.Ui;
using CalDavSynchronizer.Utilities;
using GenSync.ProgressReport;
using log4net;
using log4net.Config;
using Microsoft.Office.Interop.Outlook;
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

    public ComponentContainer (NameSpace session)
    {
      try
      {
        XmlConfigurator.Configure();

        _session = session;
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
            TimeSpan.Parse (ConfigurationManager.AppSettings["calDavReadWriteTimeout"]),
            Boolean.Parse (ConfigurationManager.AppSettings["disableCertificateValidation"]),
            Boolean.Parse (ConfigurationManager.AppSettings["enableSsl3"]),
            Boolean.Parse (ConfigurationManager.AppSettings["enableTls12"]));

        _scheduler = new Scheduler (synchronizerFactory, EnsureSynchronizationContext);
        _scheduler.SetOptions (_optionsDataAccess.LoadOptions());

        _updateChecker = new UpdateChecker (new AvailableVersionService());
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
        form.ShowDialog();
      }
      catch (Exception x)
      {
        ExceptionHandler.Instance.LogException (x, s_logger);
      }
    }

    private static string GetOrCreateConfigFileName (string applicationDataDirectory, string profileName)
    {
      var profileDataAccess = new ProfileListDataAccess (Path.Combine (applicationDataDirectory, "profiles.xml"));
      var profiles = profileDataAccess.Load();
      var profile = profiles.FirstOrDefault (p => p.ProfileName.Equals (profileName, StringComparison.OrdinalIgnoreCase));
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