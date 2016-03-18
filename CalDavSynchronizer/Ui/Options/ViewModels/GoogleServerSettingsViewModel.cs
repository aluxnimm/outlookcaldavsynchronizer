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
using System.Reflection;
using System.Security;
using System.Text;
using System.Windows.Forms;
using System.Windows.Input;
using CalDavSynchronizer.Contracts;
using log4net;
using Microsoft.Office.Interop.Outlook;
using Exception = System.Exception;

namespace CalDavSynchronizer.Ui.Options.ViewModels
{
  internal class GoogleServerSettingsViewModel : ViewModelBase, IServerSettingsViewModel
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodBase.GetCurrentMethod().DeclaringType);
    private readonly ICurrentOptions _currentOptions;
    private readonly DelegateCommandWithoutCanExecuteDelegation _doAutoDiscoveryCommand;
    private readonly ISettingsFaultFinder _settingsFaultFinder;
    private readonly DelegateCommandWithoutCanExecuteDelegation _testConnectionCommand;

    private string _calenderUrl;

    public GoogleServerSettingsViewModel (ISettingsFaultFinder settingsFaultFinder, ICurrentOptions currentOptions)
    {
      if (settingsFaultFinder == null)
        throw new ArgumentNullException (nameof (settingsFaultFinder));
      if (currentOptions == null)
        throw new ArgumentNullException (nameof (currentOptions));

      _settingsFaultFinder = settingsFaultFinder;
      _currentOptions = currentOptions;
      _currentOptions.OutlookFolderTypeChanged += CurrentOptions_OutlookFolderTypeChanged;
      _doAutoDiscoveryCommand = new DelegateCommandWithoutCanExecuteDelegation (_ => DoAutoDiscovery());
      _testConnectionCommand = new DelegateCommandWithoutCanExecuteDelegation (_ =>
      {
        ComponentContainer.EnsureSynchronizationContext();
        TestConnectionAsync();
      });
    }

    public ICommand DoAutoDiscoveryCommand => _doAutoDiscoveryCommand;
    public ICommand TestConnectionCommand => _testConnectionCommand;

    public string UserName { get; private set; }

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

    public string EmailAddress
    {
      get { return UserName; }
      set
      {
        UserName = value;
        OnPropertyChanged();
        // ReSharper disable once ExplicitCallerInfoArgument
        OnPropertyChanged (nameof (UserName));
      }
    }

    public ServerAdapterType ServerAdapterType { get; private set; }

    public bool IsGoogle { get; } = true;

    public void SetOptions (Contracts.Options options)
    {
      EmailAddress = options.EmailAddress;
      if (!string.IsNullOrEmpty (options.CalenderUrl))
        CalenderUrl = options.CalenderUrl;
      else
        CalenderUrl = OptionTasks.GoogleDavBaseUrl;
      ServerAdapterType = options.ServerAdapterType;
    }

    public void FillOptions (Contracts.Options options)
    {
      options.CalenderUrl = _calenderUrl;
      options.UserName = UserName;
      options.Password = new SecureString();
      options.EmailAddress = UserName;
      options.UseAccountPassword = false;
      options.ServerAdapterType = ServerAdapterType;
    }

    public bool Validate (StringBuilder errorMessageBuilder)
    {
      var result = true;

      if (ServerAdapterType != ServerAdapterType.GoogleTaskApi)
        result &= OptionTasks.ValidateWebDavUrl (CalenderUrl, errorMessageBuilder, true);

      result &= OptionTasks.ValidateGoogleEmailAddress (errorMessageBuilder, EmailAddress);

      return result;
    }

    private void CurrentOptions_OutlookFolderTypeChanged (object sender, EventArgs e)
    {
      UpdateServeradapterType();
    }

    private void UpdateServeradapterType ()
    {
      if (_currentOptions.OutlookFolderType == OlItemType.olTaskItem)
        ServerAdapterType = ServerAdapterType.GoogleTaskApi;
      else
        ServerAdapterType = ServerAdapterType.WebDavHttpClientBasedWithGoogleOAuth;
    }

    private async void TestConnectionAsync ()
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
        for (var ex = x; ex != null; ex = ex.InnerException)
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
      if (ServerAdapterType == ServerAdapterType.GoogleTaskApi)
        CalenderUrl = string.Empty;
      else
        CalenderUrl = OptionTasks.GoogleDavBaseUrl;

      ComponentContainer.EnsureSynchronizationContext();
      TestConnectionAsync();
    }

    public static GoogleServerSettingsViewModel DesignInstance => new GoogleServerSettingsViewModel (NullSettingsFaultFinder.Instance, new DesignCurrentOptions ())
    {
      CalenderUrl = "http://calendar.url",
      EmailAddress = "bla@dot.com"
    };
  }
}