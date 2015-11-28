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
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation;
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.Scheduling;
using CalDavSynchronizer.Ui.ConnectionTests;
using CalDavSynchronizer.Utilities;
using log4net;
using Microsoft.Office.Interop.Outlook;
using Exception = System.Exception;

namespace CalDavSynchronizer.Ui
{
  public partial class ServerSettingsControl : UserControl
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private ISettingsFaultFinder _settingsFaultFinder;
    private IServerSettingsControlDependencies _dependencies;

    private ServerAdapterType SelectedServerAdapterType
    {
      get { return _useGoogleOAuthCheckBox.Checked ? ServerAdapterType.GoogleOAuth : ServerAdapterType.Default; }
      set
      {
        switch (value)
        {
          case ServerAdapterType.Default:
            _useGoogleOAuthCheckBox.Checked = false;
            break;
          case ServerAdapterType.GoogleOAuth:
            _useGoogleOAuthCheckBox.Checked = true;
            break;
          default:
            throw new ArgumentOutOfRangeException ("value");
        }
      }
    }

    private void UpdatePasswordEnabled ()
    {
      _passwordTextBox.Enabled = SelectedServerAdapterType != ServerAdapterType.GoogleOAuth;
    }

    private void _useGoogleOAuthCheckBox_CheckedChanged (object sender, EventArgs e)
    {
      UpdatePasswordEnabled();
    }

    public void Initialize (ISettingsFaultFinder settingsFaultFinder, IServerSettingsControlDependencies dependencies)
    {
      InitializeComponent();

      _settingsFaultFinder = settingsFaultFinder;
      _dependencies = dependencies;

      _testConnectionButton.Click += _testConnectionButton_Click;
    }

    private async void _testConnectionButton_Click (object sender, EventArgs e)
    {
      await TestServerConnection();
    }

    private async Task TestServerConnection ()
    {
      _testConnectionButton.Enabled = false;
      try
      {
        StringBuilder errorMessageBuilder = new StringBuilder();
        if (!ValidateCalendarUrl (errorMessageBuilder, false))
        {
          MessageBox.Show (errorMessageBuilder.ToString(), "The CalDav/CardDav Url is invalid", MessageBoxButtons.OK, MessageBoxIcon.Error);
          return;
        }

        var enteredUri = new Uri (_calenderUrlTextBox.Text);
        var webDavClient = CreateWebDavClient();

        Uri autoDiscoveredUrl;
        ResourceType autoDiscoveredResourceType;

        if (ConnectionTester.RequiresAutoDiscovery (enteredUri))
        {
          var autodiscoveryResult = await DoAutoDiscovery (enteredUri, webDavClient, true);
          if (autodiscoveryResult.WasCancelled)
            return;
          if (autodiscoveryResult.RessourceUrl != null)
          {
            autoDiscoveredUrl = autodiscoveryResult.RessourceUrl;
            autoDiscoveredResourceType = autodiscoveryResult.ResourceType;
          }
          else if (!enteredUri.AbsolutePath.EndsWith ("/"))
          {
            var autodiscoveryResult2 = await DoAutoDiscovery (new Uri (enteredUri.ToString() + "/"), webDavClient, false);
            if (autodiscoveryResult2.WasCancelled)
              return;
            if (autodiscoveryResult2.RessourceUrl != null)
            {
              autoDiscoveredUrl = autodiscoveryResult2.RessourceUrl;
              autoDiscoveredResourceType = autodiscoveryResult2.ResourceType;
            }
            else
              return;
          }
          else
            return;
        }
        else
        {
          var result = await ConnectionTester.TestConnection (enteredUri, webDavClient, ResourceType.None);
          if (result.ResourceType != ResourceType.None)
          {
            _settingsFaultFinder.FixSynchronizationMode (result);

            DisplayTestReport (result);
            return;
          }
          else
          {
            var autodiscoveryResult = await DoAutoDiscovery (enteredUri, webDavClient, false);
            if (autodiscoveryResult.WasCancelled)
              return;
            if (autodiscoveryResult.RessourceUrl != null)
            {
              autoDiscoveredUrl = autodiscoveryResult.RessourceUrl;
              autoDiscoveredResourceType = autodiscoveryResult.ResourceType;
            }
            else
            {
              var autodiscoveryResult2 = await DoAutoDiscovery (enteredUri, webDavClient, true);
              if (autodiscoveryResult2.RessourceUrl != null)
              {
                autoDiscoveredUrl = autodiscoveryResult2.RessourceUrl;
                autoDiscoveredResourceType = autodiscoveryResult2.ResourceType;
              }
              else
                return;
            }
          }
        }

        _calenderUrlTextBox.Text = autoDiscoveredUrl.ToString();

        var finalResult = await ConnectionTester.TestConnection (autoDiscoveredUrl, webDavClient, autoDiscoveredResourceType);

        _settingsFaultFinder.FixSynchronizationMode (finalResult);

        DisplayTestReport (finalResult);
      }
      catch (Exception x)
      {
        s_logger.Error ("Exception while testing the connection.", x);
        string message = null;
        for (Exception ex = x; ex != null; ex = ex.InnerException)
          message += ex.Message + Environment.NewLine;
        MessageBox.Show (message, OptionsDisplayControl.ConnectionTestCaption);
      }
      finally
      {
        _testConnectionButton.Enabled = true;
      }
    }

    private void DisplayTestReport (TestResult result)
    {
      bool hasError = false;
      var errorMessageBuilder = new StringBuilder();

      var isCalendar = result.ResourceType.HasFlag (ResourceType.Calendar);
      var isAddressBook = result.ResourceType.HasFlag (ResourceType.AddressBook);

      if (!isCalendar && !isAddressBook)
      {
        errorMessageBuilder.AppendLine ("- The specified Url is neither a calendar nor an addressbook!");
        hasError = true;
      }

      if (isCalendar && isAddressBook)
      {
        errorMessageBuilder.AppendLine ("- Ressources which are a calendar and an addressbook are not valid!");
        hasError = true;
      }

      if (isCalendar)
      {
        if (!result.CalendarProperties.HasFlag (CalendarProperties.CalendarAccessSupported))
        {
          errorMessageBuilder.AppendLine ("- The specified Url does not support calendar access.");
          hasError = true;
        }

        if (!result.CalendarProperties.HasFlag (CalendarProperties.SupportsCalendarQuery))
        {
          errorMessageBuilder.AppendLine ("- The specified Url does not support calendar queries. Some features like time range filter may not work!");
          hasError = true;
        }

        if (!result.CalendarProperties.HasFlag (CalendarProperties.IsWriteable))
        {
          if (_dependencies.SelectedSynchronizationModeRequiresWriteableServerResource)
          {
            errorMessageBuilder.AppendFormat (
                "- The specified calendar is not writeable. Therefore it is not possible to use the synchronization mode '{0}'.",
                _dependencies.SelectedSynchronizationModeDisplayName);
            errorMessageBuilder.AppendLine();
            hasError = true;
          }
        }

        if (_dependencies.OutlookFolderType != OlItemType.olAppointmentItem && _dependencies.OutlookFolderType != OlItemType.olTaskItem)
        {
          errorMessageBuilder.AppendLine ("- The outlook folder is not a calendar or task folder, or there is no folder selected.");
          hasError = true;
        }
      }

      if (isAddressBook)
      {
        if (!result.AddressBookProperties.HasFlag (AddressBookProperties.AddressBookAccessSupported))
        {
          errorMessageBuilder.AppendLine ("- The specified Url does not support address books.");
          hasError = true;
        }

        if (!result.AddressBookProperties.HasFlag (AddressBookProperties.IsWriteable))
        {
          if (_dependencies.SelectedSynchronizationModeRequiresWriteableServerResource)
          {
            errorMessageBuilder.AppendFormat (
                "- The specified address book is not writeable. Therefore it is not possible to use the synchronization mode '{0}'.",
                _dependencies.SelectedSynchronizationModeDisplayName);
            errorMessageBuilder.AppendLine();
            hasError = true;
          }
        }

        if (_dependencies.OutlookFolderType != OlItemType.olContactItem)
        {
          errorMessageBuilder.AppendLine ("- The outlook folder is not an address book, or there is no folder selected.");
          hasError = true;
        }
      }

      if (hasError)
        MessageBox.Show ("Connection test NOT successful:" + Environment.NewLine + errorMessageBuilder, OptionsDisplayControl.ConnectionTestCaption);
      else
        MessageBox.Show ("Connection test successful.", OptionsDisplayControl.ConnectionTestCaption);
    }

    private struct AutoDiscoveryResult
    {
      private readonly bool _wasCancelled;
      private readonly Uri _ressourceUrl;
      private readonly ResourceType _resourceType;

      public AutoDiscoveryResult (Uri ressourceUrl, bool wasCancelled, ResourceType resourceType)
          : this()
      {
        _wasCancelled = wasCancelled;
        _ressourceUrl = ressourceUrl;
        _resourceType = resourceType;
      }

      public bool WasCancelled
      {
        get { return _wasCancelled; }
      }

      public Uri RessourceUrl
      {
        get { return _ressourceUrl; }
      }

      public ResourceType ResourceType
      {
        get { return _resourceType; }
      }
    }

    private async Task<AutoDiscoveryResult> DoAutoDiscovery (Uri autoDiscoveryUri, IWebDavClient webDavClient, bool useWellKnownUrl)
    {
      var calDavDataAccess = new CalDavDataAccess (autoDiscoveryUri, webDavClient);
      IReadOnlyList<Tuple<Uri, string, string>> foundCaldendars = await calDavDataAccess.GetUserCalendarsNoThrow (useWellKnownUrl);

      var cardDavDataAccess = new CardDavDataAccess (autoDiscoveryUri, webDavClient);
      IReadOnlyList<Tuple<Uri, string>> foundAddressBooks = await cardDavDataAccess.GetUserAddressBooksNoThrow (useWellKnownUrl);

      if (foundCaldendars.Count > 0 || foundAddressBooks.Count > 0)
      {
        using (SelectResourceForm listCalendarsForm = new SelectResourceForm (foundCaldendars, foundAddressBooks, _dependencies.OutlookFolderType == OlItemType.olContactItem))
        {
          if (listCalendarsForm.ShowDialog() == DialogResult.OK)
            return new AutoDiscoveryResult (new Uri (autoDiscoveryUri.GetLeftPart (UriPartial.Authority) + listCalendarsForm.SelectedUrl), false, listCalendarsForm.ResourceType);
          else
            return new AutoDiscoveryResult (null, true, ResourceType.None);
        }
      }
      else
      {
        MessageBox.Show ("No resources were found via autodiscovery!", OptionsDisplayControl.ConnectionTestCaption);
        return new AutoDiscoveryResult (null, false, ResourceType.None);
      }
    }

    private IWebDavClient CreateWebDavClient ()
    {
      return SynchronizerFactory.CreateWebDavClient (
          _userNameTextBox.Text,
          _passwordTextBox.Text,
          TimeSpan.Parse (ConfigurationManager.AppSettings["calDavConnectTimeout"]),
          SelectedServerAdapterType,
          _dependencies.CloseConnectionAfterEachRequest,
          _dependencies.ProxyOptions);
    }

    public bool ValidateCalendarUrl (StringBuilder errorMessageBuilder, bool requiresTrailingSlash)
    {
      bool result = true;

      if (string.IsNullOrWhiteSpace (_calenderUrlTextBox.Text))
      {
        errorMessageBuilder.AppendLine ("- The CalDav/CardDav Url is empty.");
        return false;
      }

      if (_calenderUrlTextBox.Text.Trim() != _calenderUrlTextBox.Text)
      {
        errorMessageBuilder.AppendLine ("- The CalDav/CardDav Url cannot end/start with whitespaces.");
        result = false;
      }

      if (requiresTrailingSlash && !_calenderUrlTextBox.Text.EndsWith ("/"))
      {
        errorMessageBuilder.AppendLine ("- The CalDav/CardDav Url has to end with a slash ('/').");
        result = false;
      }

      try
      {
        var uri = new Uri (_calenderUrlTextBox.Text).ToString();
      }
      catch (Exception x)
      {
        errorMessageBuilder.AppendFormat ("- The CalDav/CardDav Url is not a well formed Url. ({0})", x.Message);
        errorMessageBuilder.AppendLine();
        result = false;
      }

      return result;
    }

    public void SetOptions (Options value)
    {
      _emailAddressTextBox.Text = value.EmailAddress;
      _calenderUrlTextBox.Text = value.CalenderUrl;
      _userNameTextBox.Text = value.UserName;
      _passwordTextBox.Text = value.Password;

      SelectedServerAdapterType = value.ServerAdapterType;
    }

    public void FillOptions (Options optionsToFill)
    {
      optionsToFill.EmailAddress = _emailAddressTextBox.Text;
      optionsToFill.CalenderUrl = _calenderUrlTextBox.Text;
      optionsToFill.UserName = _userNameTextBox.Text;
      optionsToFill.Password = _passwordTextBox.Text;

      optionsToFill.ServerAdapterType = SelectedServerAdapterType;
    }

    public Control UiControl
    {
      get { return this; }
    }

    public string CalendarUrl
    {
      get { return _calenderUrlTextBox.Text; }
    }

    public string EmailAddress
    {
      get { return _emailAddressTextBox.Text; }
    }
  }
}