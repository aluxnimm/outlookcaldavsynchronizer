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
using System.Configuration;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation;
using CalDavSynchronizer.Scheduling;
using CalDavSynchronizer.Ui.ConnectionTests;
using Google.Apis.Tasks.v1.Data;
using log4net;
using Microsoft.Office.Interop.Outlook;
using Exception = System.Exception;
using Task = System.Threading.Tasks.Task;

namespace CalDavSynchronizer.Ui.Options
{
  public partial class GoogleServerSettingsControl : UserControl, IServerSettingsControl, ICurrentOptions
  {
   
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private ISettingsFaultFinder _settingsFaultFinder;
    private IServerSettingsControlDependencies _dependencies;
    private NetworkAndProxyOptions _networkAndProxyOptions;

    public void Initialize (ISettingsFaultFinder settingsFaultFinder, IServerSettingsControlDependencies dependencies)
    {
      InitializeComponent();

      _settingsFaultFinder = settingsFaultFinder;
      _dependencies = dependencies;

      _testConnectionButton.Click += _testConnectionButton_Click;
    }

    private async void _doAutodiscoveryButton_Click (object sender, EventArgs e)
    {
      _calenderUrlTextBox.Text = OptionTasks.GoogleDavBaseUrl;

      await TestServerConnection ();
    }

    private async void _testConnectionButton_Click (object sender, EventArgs e)
    {
      await TestServerConnection();
    }

    private async Task TestServerConnection ()
    {
      _testConnectionButton.Enabled = false;
      _doAutodiscoveryButton.Enabled = false;
      try
      {
        await OptionTasks.TestGoogleConnection (this,_settingsFaultFinder);
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
        _testConnectionButton.Enabled = true;
        _doAutodiscoveryButton.Enabled = true;
      }
    }

    public IWebProxy GetProxyIfConfigured ()
    {
      return _networkAndProxyOptions.ProxyOptions != null ? SynchronizerFactory.CreateProxy (_networkAndProxyOptions.ProxyOptions) : null;
    }

    public IWebDavClient CreateWebDavClient ()
    {
      return SynchronizerFactory.CreateWebDavClient (
          _emailAddressTextBox.Text,
          null,
          _calenderUrlTextBox.Text,
          TimeSpan.Parse (ConfigurationManager.AppSettings["calDavConnectTimeout"]),
          ServerAdapterType.WebDavHttpClientBasedWithGoogleOAuth,
          _networkAndProxyOptions.CloseConnectionAfterEachRequest,
          _networkAndProxyOptions.PreemptiveAuthentication,
          _networkAndProxyOptions.ForceBasicAuthentication,
          _networkAndProxyOptions.ProxyOptions);
    }

    public ICalDavDataAccess CreateCalDavDataAccess ()
    {
      return new CalDavDataAccess (new Uri (_calenderUrlTextBox.Text), CreateWebDavClient ());
    }

    public void SetOptions (Contracts.Options value)
    {
      _emailAddressTextBox.Text = value.EmailAddress;
      if (!string.IsNullOrEmpty (value.CalenderUrl))
        _calenderUrlTextBox.Text = value.CalenderUrl;
      else
        _calenderUrlTextBox.Text = OptionTasks.GoogleDavBaseUrl;
      ServerAdapterType = value.ServerAdapterType;
      _networkAndProxyOptions = new NetworkAndProxyOptions (value.CloseAfterEachRequest, value.PreemptiveAuthentication, value.ForceBasicAuthentication, value.ProxyOptions ?? new ProxyOptions());

    }

    public void FillOptions (Contracts.Options optionsToFill)
    {
      optionsToFill.EmailAddress = _emailAddressTextBox.Text;
      optionsToFill.CalenderUrl = _calenderUrlTextBox.Text;
      optionsToFill.UserName = _emailAddressTextBox.Text;
      optionsToFill.ServerAdapterType = ServerAdapterType;
      optionsToFill.CloseAfterEachRequest = _networkAndProxyOptions.CloseConnectionAfterEachRequest;
      optionsToFill.PreemptiveAuthentication = _networkAndProxyOptions.PreemptiveAuthentication;
      optionsToFill.ForceBasicAuthentication = _networkAndProxyOptions.ForceBasicAuthentication;
      optionsToFill.ProxyOptions = _networkAndProxyOptions.ProxyOptions;
    }

    public string CalendarUrl
    {
      get { return _calenderUrlTextBox.Text; }
    }

    public SynchronizationMode SynchronizationMode => _dependencies.SelectedSynchronizationMode;

    public string SynchronizationModeDisplayName => _dependencies.SelectedSynchronizationModeDisplayName;

    public string ServerUrl
    {
      get { return _calenderUrlTextBox.Text; }
      set {  _calenderUrlTextBox.Text = value; }
    }

    public OlItemType? OutlookFolderType => _dependencies.OutlookFolderType;

    public string EmailAddress => _emailAddressTextBox.Text;

    public ServerAdapterType ServerAdapterType { get; set; }


    private void _editUrlManuallyButton_Click (object sender, EventArgs e)
    {
      _calenderUrlTextBox.ReadOnly = false;
    }

    private void _networkAndProxyOptionsButton_Click (object sender, EventArgs e)
    {
      using (NetworkAndProxyOptionsForm networkAndProxyOptionsForm = new NetworkAndProxyOptionsForm())
      {
        networkAndProxyOptionsForm.Options= _networkAndProxyOptions;

        if (networkAndProxyOptionsForm.ShowDialog() == DialogResult.OK)
        {
          _networkAndProxyOptions = networkAndProxyOptionsForm.Options;
        }
      }
    }
  }
}