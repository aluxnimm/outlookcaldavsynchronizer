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
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Scheduling;
using CalDavSynchronizer.Ui.ConnectionTests;
using Google.Apis.Tasks.v1.Data;
using log4net;
using Microsoft.Office.Interop.Outlook;
using Exception = System.Exception;
using Task = System.Threading.Tasks.Task;

namespace CalDavSynchronizer.Ui.Options
{
  public partial class GoogleServerSettingsControl : UserControl, IServerSettingsControl
  {
    private const string c_googleDavBaseUrl = "https://apidata.googleusercontent.com/caldav/v2";
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private ISettingsFaultFinder _settingsFaultFinder;
    private IServerSettingsControlDependencies _dependencies;
    public ServerAdapterType UsedServerAdapterType { get; private set; }

    public void Initialize (ISettingsFaultFinder settingsFaultFinder, IServerSettingsControlDependencies dependencies)
    {
      InitializeComponent();

      _settingsFaultFinder = settingsFaultFinder;
      _dependencies = dependencies;

      _testConnectionButton.Click += _testConnectionButton_Click;
    }

    private async void _doAutodiscoveryButton_Click (object sender, EventArgs e)
    {
      _calenderUrlTextBox.Text = c_googleDavBaseUrl;

      await TestServerConnection ();
    }

    private async void _testConnectionButton_Click (object sender, EventArgs e)
    {
      if (UsedServerAdapterType == ServerAdapterType.GoogleTaskApi)
        _calenderUrlTextBox.Text = c_googleDavBaseUrl;
      await TestServerConnection();
    }

    private async Task TestServerConnection ()
    {
      _testConnectionButton.Enabled = false;
      _doAutodiscoveryButton.Enabled = false;
      try
      {
        await TestGoogleConnection();
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

    private async Task TestGoogleConnection ()
    {
      StringBuilder errorMessageBuilder = new StringBuilder ();

      if (!OptionTasks.ValidateGoogleEmailAddress (errorMessageBuilder, _emailAddressTextBox.Text)) 
      {
        MessageBox.Show (errorMessageBuilder.ToString(), "The Email Address is invalid", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }

      if (!OptionTasks.ValidateWebDavUrl (_calenderUrlTextBox.Text, errorMessageBuilder, false))
      {
        MessageBox.Show (errorMessageBuilder.ToString(), "The CalDav/CardDav Url is invalid", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }

      var enteredUri = new Uri (_calenderUrlTextBox.Text);
      var webDavClient = CreateWebDavClient ();

      Uri autoDiscoveredUrl;
      ResourceType autoDiscoveredResourceType;

      if (ConnectionTester.RequiresAutoDiscovery (enteredUri))
      {
        var calDavDataAccess = new CalDavDataAccess(enteredUri, webDavClient);
        IReadOnlyList<Tuple<Uri, string, string>> foundCaldendars = await calDavDataAccess.GetUserCalendarsNoThrow(false);

        var cardDavDataAccess = new CardDavDataAccess(enteredUri, webDavClient);
        IReadOnlyList<Tuple<Uri, string>> foundAddressBooks = await cardDavDataAccess.GetUserAddressBooksNoThrow(true);

        var service = await OAuth.Google.GoogleHttpClientFactory.LoginToGoogleTasksService(_emailAddressTextBox.Text);

        TaskLists taskLists = await service.Tasklists.List().ExecuteAsync();

        if (foundCaldendars.Count > 0 || foundAddressBooks.Count > 0 || taskLists.Items.Any())
        {
          ResourceType initalResourceType;
          if (_dependencies.OutlookFolderType == OlItemType.olContactItem)
          {
            initalResourceType = ResourceType.AddressBook;
          }
          else if (_dependencies.OutlookFolderType == OlItemType.olTaskItem)
          {
            initalResourceType = ResourceType.TaskList;
          }
          else
          {
            initalResourceType = ResourceType.Calendar;
          }

          using (SelectResourceForm listCalendarsForm =
              new SelectResourceForm(
                  foundCaldendars,
                  foundAddressBooks,
                  taskLists.Items.Select(i => Tuple.Create(i.Id, i.Title)).ToArray(),
                  initalResourceType))
          {
            if (listCalendarsForm.ShowDialog() == DialogResult.OK)
            {
              if (listCalendarsForm.ResourceType == ResourceType.TaskList)
              {
                autoDiscoveredUrl = null;
                _calenderUrlTextBox.Text = listCalendarsForm.SelectedUrl;
                UsedServerAdapterType = ServerAdapterType.GoogleTaskApi;
              }
              else
              {
                autoDiscoveredUrl = new Uri(enteredUri.GetLeftPart(UriPartial.Authority) + listCalendarsForm.SelectedUrl);
                UsedServerAdapterType = ServerAdapterType.WebDavHttpClientBasedWithGoogleOAuth;
              }
              autoDiscoveredResourceType = listCalendarsForm.ResourceType;
            }
            else
            {
              autoDiscoveredUrl = null;
              autoDiscoveredResourceType = ResourceType.None;
            }
          }
        }
        else
        {
          MessageBox.Show("No resources were found via autodiscovery!", OptionTasks.ConnectionTestCaption);
          autoDiscoveredUrl = null;
          autoDiscoveredResourceType = ResourceType.None;
        }
      }
      else
      {
        var result = await ConnectionTester.TestConnection (enteredUri, webDavClient, ResourceType.None);
        if (result.ResourceType != ResourceType.None)
        {
          _settingsFaultFinder.FixSynchronizationMode (result);
        }
        OptionTasks.DisplayTestReport (
            result,
            _dependencies.SelectedSynchronizationModeRequiresWriteableServerResource,
            _dependencies.SelectedSynchronizationModeDisplayName,
            _dependencies.OutlookFolderType);
        return;
      }

      if (autoDiscoveredUrl != null)
      {
        _calenderUrlTextBox.Text = autoDiscoveredUrl.ToString();
        var finalResult =
          await ConnectionTester.TestConnection(autoDiscoveredUrl, webDavClient, autoDiscoveredResourceType);
        _settingsFaultFinder.FixSynchronizationMode(finalResult);

        OptionTasks.DisplayTestReport(
          finalResult,
          _dependencies.SelectedSynchronizationModeRequiresWriteableServerResource,
          _dependencies.SelectedSynchronizationModeDisplayName,
          _dependencies.OutlookFolderType);
      }
      else if (UsedServerAdapterType == ServerAdapterType.GoogleTaskApi)
      {
        TestResult result = new TestResult(ResourceType.TaskList, CalendarProperties.None, AddressBookProperties.None);

        OptionTasks.DisplayTestReport (result,false,_dependencies.SelectedSynchronizationModeDisplayName, _dependencies.OutlookFolderType);
      }
    }

    private IWebDavClient CreateWebDavClient ()
    {
      return SynchronizerFactory.CreateWebDavClient (
          _emailAddressTextBox.Text,
          null,
          TimeSpan.Parse (ConfigurationManager.AppSettings["calDavConnectTimeout"]),
          ServerAdapterType.WebDavHttpClientBasedWithGoogleOAuth,
          _dependencies.CloseConnectionAfterEachRequest,
          _dependencies.PreemptiveAuthentication,
          _dependencies.ProxyOptions);
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
        _calenderUrlTextBox.Text = c_googleDavBaseUrl;
      UsedServerAdapterType = value.ServerAdapterType;
    }

    public void FillOptions (Contracts.Options optionsToFill)
    {
      optionsToFill.EmailAddress = _emailAddressTextBox.Text;
      optionsToFill.CalenderUrl = _calenderUrlTextBox.Text;
      optionsToFill.UserName = _emailAddressTextBox.Text;
      optionsToFill.ServerAdapterType = UsedServerAdapterType;
    }

    public string CalendarUrl
    {
      get { return _calenderUrlTextBox.Text; }
    }

    public string EmailAddress
    {
      get { return _emailAddressTextBox.Text; }
    }

    private void _editUrlManuallyButton_Click (object sender, EventArgs e)
    {
      _calenderUrlTextBox.ReadOnly = false;
    }

   
  }
}