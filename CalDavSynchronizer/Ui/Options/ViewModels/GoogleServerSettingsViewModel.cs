using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Windows.Forms;
using System.Windows.Input;
using CalDavSynchronizer.Contracts;
using log4net;

namespace CalDavSynchronizer.Ui.Options.ViewModels
{
  internal class GoogleServerSettingsViewModel : ViewModelBase, IServerSettingsViewModel
  {
    private static readonly ILog s_logger = LogManager.GetLogger (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    private string _calenderUrl;
    private string _emailAddress;
    private readonly DelegateCommandWithoutCanExecuteDelegation _testConnectionCommand;
    private readonly ISettingsFaultFinder _settingsFaultFinder;
    private readonly ICurrentOptions _currentOptions;
    private readonly DelegateCommandWithoutCanExecuteDelegation _doAutoDiscoveryCommand;


    public GoogleServerSettingsViewModel (ISettingsFaultFinder settingsFaultFinder, ICurrentOptions currentOptions)
    {
      if (settingsFaultFinder == null)
        throw new ArgumentNullException (nameof (settingsFaultFinder));
      if (currentOptions == null)
        throw new ArgumentNullException (nameof (currentOptions));

      _settingsFaultFinder = settingsFaultFinder;
      _currentOptions = currentOptions;
      _doAutoDiscoveryCommand = new DelegateCommandWithoutCanExecuteDelegation (_ => DoAutoDiscovery());
      _testConnectionCommand = new DelegateCommandWithoutCanExecuteDelegation (_ => TestConnection());
    }

    private async void TestConnection ()
    {
      _testConnectionCommand.SetCanExecute (false);
      _doAutoDiscoveryCommand.SetCanExecute (false);
      try
      {
        await OptionTasks.TestGoogleConnection (_currentOptions, _settingsFaultFinder);
      }
      catch (Exception x)
      {
        s_logger.Error ("Exception while testing the connection.", x);
        string message = null;
        for (Exception ex = x; ex != null; ex = ex.InnerException)
          message += ex.Message + Environment.NewLine;
        MessageBox.Show (message, OptionTasks.ConnectionTestCaption);
      }
      finally
      {
        _testConnectionCommand.SetCanExecute (true);
        _doAutoDiscoveryCommand.SetCanExecute (true);
      }
    }

    private void DoAutoDiscovery ()
    {
      CalenderUrl = OptionTasks.GoogleDavBaseUrl;
      TestConnection();
    }

    public ICommand DoAutoDiscoveryCommand => _doAutoDiscoveryCommand;
    public ICommand TestConnectionCommand => _testConnectionCommand;

    public ServerAdapterType ServerAdapterType { get; private set; }


    public string UserName
    {
      get { return _emailAddress; }
    }

    public string CalenderUrl
    {
      get { return _calenderUrl; }
      set
      {
        _calenderUrl = value;
        OnPropertyChanged();
      }
    }

    public bool UseAccountPassword
    {
      get { return false; }
    }

    public SecureString Password
    {
      get { return new SecureString(); }
    }

    ServerAdapterType IServerSettingsViewModel.ServerAdapterType
    {
      get { return ServerAdapterType; }
      set { ServerAdapterType = value; OnPropertyChanged (); }
    }

    public string EmailAddress
    {
      get { return _emailAddress; }
      set
      {
        _emailAddress = value;
        OnPropertyChanged();
        // ReSharper disable once ExplicitCallerInfoArgument
        OnPropertyChanged (nameof (UserName));
      }
    }



    public static GoogleServerSettingsViewModel DesignInstance => new GoogleServerSettingsViewModel (NullSettingsFaultFinder.Instance,new DesignCurrentOptions())
                                                                  {
                                                                      CalenderUrl = "http://calendar.url",
                                                                      EmailAddress = "bla@dot.com"
                                                                  };


    public void SetOptions (CalDavSynchronizer.Contracts.Options options)
    {
      EmailAddress = options.EmailAddress;
      if (!string.IsNullOrEmpty (options.CalenderUrl))
        CalenderUrl = options.CalenderUrl;
      else
        CalenderUrl = OptionTasks.GoogleDavBaseUrl;
      ServerAdapterType = options.ServerAdapterType;
    }

    public void FillOptions (CalDavSynchronizer.Contracts.Options options)
    {
      options.CalenderUrl = _calenderUrl;
      options.UserName = _emailAddress;
      options.Password = new SecureString();
      options.EmailAddress = _emailAddress;
      options.UseAccountPassword = false;
      options.DisplayType = CalDavSynchronizer.Contracts.OptionsDisplayType.Google;
      options.ServerAdapterType = ServerAdapterType;
    }

    public bool Validate (StringBuilder errorMessageBuilder)
    {
      bool result = true;

      if (ServerAdapterType != ServerAdapterType.GoogleTaskApi)
        result &= OptionTasks.ValidateWebDavUrl (CalenderUrl, errorMessageBuilder, true);

      result &= OptionTasks.ValidateGoogleEmailAddress (errorMessageBuilder, EmailAddress);

      return result;
    }
  }
}