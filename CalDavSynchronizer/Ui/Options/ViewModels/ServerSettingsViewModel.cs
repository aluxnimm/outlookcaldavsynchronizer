using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using CalDavSynchronizer.Contracts;
using log4net;

namespace CalDavSynchronizer.Ui.Options.ViewModels
{
  internal class ServerSettingsViewModel : ViewModelBase, IServerSettingsViewModel
  {
    private static readonly ILog s_logger = LogManager.GetLogger (System.Reflection.MethodBase.GetCurrentMethod ().DeclaringType);

    private string _calenderUrl;
    private string _emailAddress;
    private string _password;
    private bool _useAccountPassword;
    private string _userName;
    private readonly ISettingsFaultFinder _settingsFaultFinder;
    private readonly ICurrentOptions _currentOptions;
    private readonly DelegateCommandWithoutCanExecuteDelegation _testConnectionCommand;

    public ServerSettingsViewModel (ISettingsFaultFinder settingsFaultFinder, ICurrentOptions currentOptions)
    {
      if (settingsFaultFinder == null)
        throw new ArgumentNullException (nameof (settingsFaultFinder));
      if (currentOptions == null)
        throw new ArgumentNullException (nameof (currentOptions));

      _settingsFaultFinder = settingsFaultFinder;
      _currentOptions = currentOptions;
      _testConnectionCommand = new DelegateCommandWithoutCanExecuteDelegation (_ => TestConnection());
    }

    public ICommand TestConnectionCommand => _testConnectionCommand;

    public string CalenderUrl
    {
      get { return _calenderUrl; }
      set
      {
        _calenderUrl = value;
        OnPropertyChanged();
      }
    }

    public string UserName
    {
      get { return _userName; }
      set
      {
        _userName = value;
        OnPropertyChanged();
      }
    }

    public string Password
    {
      get { return _password; }
      set
      {
        _password = value;
        OnPropertyChanged();
      }
    }

    public string EmailAddress
    {
      get { return _emailAddress; }
      set
      {
        _emailAddress = value;
        OnPropertyChanged();
      }
    }

    public bool UseAccountPassword
    {
      get { return _useAccountPassword; }
      set
      {
        _useAccountPassword = value;
        OnPropertyChanged();
      }
    }

    public static ServerSettingsViewModel DesignInstance => new ServerSettingsViewModel(NullSettingsFaultFinder.Instance,new DesignCurrentOptions())
                                                            {
                                                                CalenderUrl = "http://calendar.url",
                                                                EmailAddress = "bla@dot.com",
                                                                Password = "password",
                                                                UseAccountPassword = true,
                                                                UserName = "username"
                                                            };
    
    public void SetOptions (Contracts.Options options)
    {
      CalenderUrl = options.CalenderUrl;
      UserName = options.UserName;
      Password = options.Password;
      EmailAddress = options.EmailAddress;
      UseAccountPassword = options.UseAccountPassword;
    }

    public void FillOptions (Contracts.Options options)
    {
      options.CalenderUrl = _calenderUrl;
      options.UserName = _userName;
      options.Password = _password;
      options.EmailAddress = _emailAddress;
      options.UseAccountPassword = _useAccountPassword;
      options.ServerAdapterType = ServerAdapterType.WebDavHttpClientBased;
    }

    public ServerAdapterType ServerAdapterType
    {
      get { return ServerAdapterType.WebDavHttpClientBased; }
      set { throw new NotSupportedException ("Cannot change ServerAdapterType of general profile."); }
    }


    public bool Validate (StringBuilder errorBuilder)
    {
      return true;
    }

    private async void TestConnection ()
    {
      _testConnectionCommand.SetCanExecute(false);
      try
      {
        await OptionTasks.TestWebDavConnection (_currentOptions, _settingsFaultFinder);
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
      }
   
    }
  }
}