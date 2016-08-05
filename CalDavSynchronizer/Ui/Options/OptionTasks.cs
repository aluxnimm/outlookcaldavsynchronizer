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
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation;
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.OAuth.Google;
using CalDavSynchronizer.Ui.ConnectionTests;
using CalDavSynchronizer.Ui.Options.ResourceSelection.ViewModels;
using CalDavSynchronizer.Ui.Options.ViewModels;
using CalDavSynchronizer.Ui.Options.ViewModels.Mapping;
using CalDavSynchronizer.Utilities;
using Google.Apis.Tasks.v1.Data;
using log4net;
using Microsoft.Office.Interop.Outlook;
using Exception = System.Exception;
using Task = System.Threading.Tasks.Task;

namespace CalDavSynchronizer.Ui.Options
{
  internal static class OptionTasks
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    public const string ConnectionTestCaption = "Test settings";
    public const string GoogleDavBaseUrl = "https://apidata.googleusercontent.com/caldav/v2";

    public static ISubOptionsViewModel CoerceMappingConfiguration (
        ISubOptionsViewModel currentMappingConfiguration,
        OlItemType? outlookFolderType,
        bool isGoogleProfile,
        IMappingConfigurationViewModelFactory factory)
    {
      switch (outlookFolderType)
      {
        case OlItemType.olAppointmentItem:
          return currentMappingConfiguration as EventMappingConfigurationViewModel ?? factory.Create (new EventMappingConfiguration());
        case OlItemType.olContactItem:
          return currentMappingConfiguration as ContactMappingConfigurationViewModel ?? factory.Create (new ContactMappingConfiguration());
        case OlItemType.olTaskItem:
          return isGoogleProfile
              ? null
              : currentMappingConfiguration as TaskMappingConfigurationViewModel ?? factory.Create (new TaskMappingConfiguration());
        default:
          return currentMappingConfiguration;
      }
    }

    public static bool ValidateCategoryName (string category, StringBuilder errorMessageBuilder)
    {
      bool result = true;

      if (category.Contains (","))
      {
        errorMessageBuilder.AppendLine ("- The category name must not contain commas.");
        result = false;
      }
      if (category.Contains (";"))
      {
        errorMessageBuilder.AppendLine ("- The category name must not contain semicolons.");
        result = false;
      }
      return result;
    }

    public static bool ValidateWebDavUrl (string webDavUrl, StringBuilder errorMessageBuilder, bool requiresTrailingSlash)
    {
      bool result = true;

      if (string.IsNullOrWhiteSpace (webDavUrl))
      {
        errorMessageBuilder.AppendLine ("- The CalDav/CardDav Url is empty.");
        errorMessageBuilder.AppendLine ("- If you don't know the Url you can also enter the Email address and try to discover it.");
        return false;
      }

      if (webDavUrl.Trim() != webDavUrl)
      {
        errorMessageBuilder.AppendLine ("- The CalDav/CardDav Url cannot end/start with whitespaces.");
        result = false;
      }

      if (requiresTrailingSlash && !webDavUrl.EndsWith ("/"))
      {
        errorMessageBuilder.AppendLine ("- The CalDav/CardDav Url has to end with a slash ('/').");
        result = false;
      }

      try
      {
        var uri = new Uri (webDavUrl).ToString();
      }
      catch (Exception x)
      {
        errorMessageBuilder.AppendFormat ("- The CalDav/CardDav Url is not a well formed Url. ({0})", x.Message);
        errorMessageBuilder.AppendLine();
        result = false;
      }

      return result;
    }

    public static bool ValidateGoogleEmailAddress (StringBuilder errorMessageBuilder, string emailAddress)
    {
      if (string.IsNullOrWhiteSpace (emailAddress))
      {
        errorMessageBuilder.Append ("- The Email Address is empty.");
        return false;
      }
      return ValidateEmailAddress (errorMessageBuilder, emailAddress);
    }

    public static bool ValidateEmailAddress (StringBuilder errorMessageBuilder, string emailAddress)
    {
      try
      {
        var uri = new Uri ("mailto:" + emailAddress).ToString();
        return true;
      }
      catch (Exception x)
      {
        errorMessageBuilder.AppendFormat ("- The Email Address is invalid. ({0})", x.Message);
        errorMessageBuilder.AppendLine();
        return false;
      }
    }


    public static void DisplayTestReport (
        TestResult result,
        SynchronizationMode synchronizationMode,
        string selectedSynchronizationModeDisplayName,
        OlItemType outlookFolderType)
    {
      bool hasError = false;
      bool hasWarning = false;
      var errorMessageBuilder = new StringBuilder();

      var isCalendar = result.ResourceType.HasFlag (ResourceType.Calendar);
      var isAddressBook = result.ResourceType.HasFlag (ResourceType.AddressBook);
      var isTaskList = result.ResourceType.HasFlag (ResourceType.TaskList);

      if (isCalendar && isAddressBook)
      {
        errorMessageBuilder.AppendLine ("- Ressources which are a calendar and an addressbook are not valid!");
        hasError = true;
      }

      switch (outlookFolderType)
      {
        case OlItemType.olAppointmentItem:
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
              hasWarning = true;
            }

            if (!result.CalendarProperties.HasFlag (CalendarProperties.IsWriteable))
            {
              if (DoesModeRequireWriteableServerResource (synchronizationMode))
              {
                errorMessageBuilder.AppendFormat (
                    "- The specified calendar is not writeable. Therefore it is not possible to use the synchronization mode '{0}'.",
                    selectedSynchronizationModeDisplayName);
                errorMessageBuilder.AppendLine();
                hasError = true;
              }
            }
          }
          else
          {
            errorMessageBuilder.AppendLine ("- The specified Url is not a calendar!");
            hasError = true;
          }
          break;

        case OlItemType.olContactItem:
          if (isAddressBook)
          {
            if (!result.AddressBookProperties.HasFlag (AddressBookProperties.AddressBookAccessSupported))
            {
              errorMessageBuilder.AppendLine ("- The specified Url does not support address books.");
              hasError = true;
            }

            if (!result.AddressBookProperties.HasFlag (AddressBookProperties.IsWriteable))
            {
              if (DoesModeRequireWriteableServerResource (synchronizationMode))
              {
                errorMessageBuilder.AppendFormat (
                    "- The specified address book is not writeable. Therefore it is not possible to use the synchronization mode '{0}'.",
                    selectedSynchronizationModeDisplayName);
                errorMessageBuilder.AppendLine();
                hasError = true;
              }
            }
          }
          else
          {
            errorMessageBuilder.AppendLine ("- The specified Url is not an addressbook!");
            hasError = true;
          }
          break;
        case OlItemType.olTaskItem:
          if (!isTaskList)
          {
            errorMessageBuilder.AppendLine ("- The specified Url is not an task list!");
            hasError = true;
          }
          break;
      }

      if (hasError)
        MessageBox.Show ("Connection test NOT successful:" + Environment.NewLine + errorMessageBuilder, ConnectionTestCaption);
      else if (hasWarning)
        MessageBox.Show ("Connection test successful BUT:" + Environment.NewLine + errorMessageBuilder, ConnectionTestCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
      else
        MessageBox.Show ("Connection test successful.", ConnectionTestCaption);
    }

    public static string DoSrvLookup (string serverEmail, OlItemType selectedOutlookFolderType, out bool success)
    {
      string emailDomain = serverEmail.Substring(serverEmail.IndexOf('@') + 1);
      string lookupUrl = "https://" + emailDomain;
      success = false;
      string srvBase = selectedOutlookFolderType == OlItemType.olContactItem ? "_carddav" : "_caldav";
      string lookupString = srvBase + "s._tcp." + emailDomain;

      var srvRecordsCaldavs = DnsQueryHelper.GetSRVRecordList (lookupString);
      if (srvRecordsCaldavs.Count > 0)
      {
        lookupUrl = "https://" + srvRecordsCaldavs[0];
        success = true;
        var txtRecords = DnsQueryHelper.GetTxtRecord (lookupString);
        if (txtRecords != null && txtRecords.StartsWith ("path="))
          lookupUrl += txtRecords.Substring (txtRecords.IndexOf ('=')+1);
      }
      else
      {
        lookupString = srvBase + "._tcp." + emailDomain;
        var srvRecordsCaldav = DnsQueryHelper.GetSRVRecordList (lookupString);
        if (srvRecordsCaldav.Count > 0)
        {
          lookupUrl = "http://" + srvRecordsCaldav[0];
          success = true;
          var txtRecords = DnsQueryHelper.GetTxtRecord (lookupString);
          if (txtRecords != null && txtRecords.StartsWith ("path="))
            lookupUrl += txtRecords.Substring (txtRecords.IndexOf ('=')+1);
        }
      }
      return lookupUrl;
    }

    public static async Task<AutoDiscoveryResult> DoAutoDiscovery (Uri autoDiscoveryUri, IWebDavClient webDavClient, bool useWellKnownCalDav, bool useWellKnownCardDav, OlItemType selectedOutlookFolderType)
    {

      switch (selectedOutlookFolderType)
      {
        case OlItemType.olAppointmentItem:
          var calDavDataAccess = new CalDavDataAccess (autoDiscoveryUri, webDavClient);
          var foundCalendars = (await calDavDataAccess.GetUserResourcesNoThrow (useWellKnownCalDav)).CalendarResources;
          if (foundCalendars.Count == 0)
            return new AutoDiscoveryResult (null, AutoDiscoverResultStatus.NoResourcesFound);
          var selectedCalendar = SelectCalendar (foundCalendars);
          if (selectedCalendar != null)
            return new AutoDiscoveryResult (selectedCalendar.Uri, AutoDiscoverResultStatus.ResourceSelected);
          else
            return new AutoDiscoveryResult (null, AutoDiscoverResultStatus.UserCancelled);
        case OlItemType.olTaskItem:
          var calDavDataAccessTasks = new CalDavDataAccess (autoDiscoveryUri, webDavClient);
          var foundTasks = (await calDavDataAccessTasks.GetUserResourcesNoThrow (useWellKnownCalDav)).TaskListResources;
          if (foundTasks.Count == 0)
            return new AutoDiscoveryResult (null, AutoDiscoverResultStatus.NoResourcesFound);
          var selectedTask = SelectTaskList (foundTasks);
          if (selectedTask != null)
            return new AutoDiscoveryResult (new Uri (selectedTask.Id), AutoDiscoverResultStatus.ResourceSelected);
          else
            return new AutoDiscoveryResult (null, AutoDiscoverResultStatus.UserCancelled);
        case OlItemType.olContactItem:
          var cardDavDataAccess = new CardDavDataAccess (autoDiscoveryUri, webDavClient);
          var foundAddressBooks = await cardDavDataAccess.GetUserAddressBooksNoThrow (useWellKnownCardDav);
          if (foundAddressBooks.Count == 0)
            return new AutoDiscoveryResult (null, AutoDiscoverResultStatus.NoResourcesFound);
          var selectedAddressBook = SelectAddressBook (foundAddressBooks);
          if (selectedAddressBook != null)
            return new AutoDiscoveryResult (selectedAddressBook.Uri, AutoDiscoverResultStatus.ResourceSelected);
          else
            return new AutoDiscoveryResult (null, AutoDiscoverResultStatus.UserCancelled);
        default:
          throw new NotImplementedException ($"'{selectedOutlookFolderType}' not implemented.");
      }
    }


    static CalendarData SelectCalendar (IReadOnlyList<CalendarData> items)
    {
      using (SelectResourceForm selectResourceForm = new SelectResourceForm (ResourceType.Calendar, items.Select (d => new CalendarDataViewModel (d)).ToArray ()))
      {
        if (selectResourceForm.ShowDialog () == DialogResult.OK)
          return ((CalendarDataViewModel) selectResourceForm.SelectedObject).Model;
        else
          return null;
      }
    }

    static AddressBookData SelectAddressBook (IReadOnlyList<AddressBookData> items)
    {
      using (SelectResourceForm selectResourceForm = new SelectResourceForm (ResourceType.AddressBook,null, items.Select (d => new AddressBookDataViewModel (d)).ToArray ()))
      {
        if (selectResourceForm.ShowDialog () == DialogResult.OK)
          return ((AddressBookDataViewModel) selectResourceForm.SelectedObject).Model;
        else
          return null;
      }
    }

    static TaskListData SelectTaskList (IReadOnlyList<TaskListData> items)
    {
      using (SelectResourceForm selectResourceForm = new SelectResourceForm (ResourceType.TaskList, null, null, items.Select (d => new TaskListDataViewModel (d)).ToArray ()))
      {
        if (selectResourceForm.ShowDialog () == DialogResult.OK)
          return ((TaskListDataViewModel) selectResourceForm.SelectedObject).Model;
        else
          return null;
      }
    }


    public static string GetFolderAccountNameOrNull (NameSpace session, string folderStoreId)
    {
      if (ThisAddIn.IsOutlookVersionSmallerThan2010)
        return null;

      try
      {
        foreach (Account account in session.Accounts.ToSafeEnumerable<Account>())
        {
          using (var deliveryStore = GenericComObjectWrapper.Create (account.DeliveryStore))
          {
            if (deliveryStore.Inner != null && deliveryStore.Inner.StoreID == folderStoreId)
            {
              return account.DisplayName;
            }
          }
        }
      }
      catch (Exception ex)
      {
        s_logger.Error ("Can't access Account Name of folder.", ex);
      }
      return null;
    }

    public static bool DoesModeRequireWriteableServerResource (SynchronizationMode synchronizationMode)
    {
      return synchronizationMode == SynchronizationMode.MergeInBothDirections
             || synchronizationMode == SynchronizationMode.MergeOutlookIntoServer
             || synchronizationMode == SynchronizationMode.ReplicateOutlookIntoServer;
    }


    public static async Task<string> TestWebDavConnection (ICurrentOptions environment, ISettingsFaultFinder settingsFaultFinder, string url, string serverEmail)
    {
      if (environment.OutlookFolderType == null)
      {
        MessageBox.Show ("Please select an Outlook folder to specify the item type for this profile", ConnectionTestCaption);
        return url;
      }

      var outlookFolderType = environment.OutlookFolderType.Value;

      string serverUrl=url;
      StringBuilder errorMessageBuilder = new StringBuilder();

      if (string.IsNullOrEmpty (url) && !string.IsNullOrEmpty (serverEmail))
      {
        if (!ValidateEmailAddress (errorMessageBuilder, serverEmail))
        {
          MessageBox.Show (errorMessageBuilder.ToString(), "The Email Address is invalid", MessageBoxButtons.OK, MessageBoxIcon.Error);
          return url;
        }
        bool success;
        serverUrl = DoSrvLookup (serverEmail, outlookFolderType, out success);
      }

 
      if (!ValidateWebDavUrl (serverUrl, errorMessageBuilder, false))
      {
        MessageBox.Show (errorMessageBuilder.ToString(), "The CalDav/CardDav Url is invalid", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return url;
      }

      var enteredUri = new Uri (serverUrl);
      var webDavClient = environment.CreateWebDavClient (enteredUri);

      Uri autoDiscoveredUrl;

      if (ConnectionTester.RequiresAutoDiscovery (enteredUri))
      {
        var autodiscoveryResult = await DoAutoDiscovery (enteredUri, webDavClient, true, true, outlookFolderType);
        switch (autodiscoveryResult.Status)
        {
          case AutoDiscoverResultStatus.UserCancelled:
            return url;
          case AutoDiscoverResultStatus.ResourceSelected:
            autoDiscoveredUrl = autodiscoveryResult.RessourceUrl;
            break;
          case AutoDiscoverResultStatus.NoResourcesFound:
            var autodiscoveryResult2 = await DoAutoDiscovery (enteredUri.AbsolutePath.EndsWith ("/") ? enteredUri : new Uri (enteredUri.ToString() + "/"), webDavClient, false, false, outlookFolderType);
            switch (autodiscoveryResult2.Status)
            {
              case AutoDiscoverResultStatus.UserCancelled:
                return url;
              case AutoDiscoverResultStatus.ResourceSelected:
                autoDiscoveredUrl = autodiscoveryResult2.RessourceUrl;
                break;
              case AutoDiscoverResultStatus.NoResourcesFound:
                MessageBox.Show ("No resources were found via autodiscovery!", ConnectionTestCaption);
                return url;
              default:
                throw new NotImplementedException (autodiscoveryResult2.Status.ToString ());
            }
            break;
          default:
            throw new NotImplementedException (autodiscoveryResult.Status.ToString());
        }
      }
      else
      {
        var result = await ConnectionTester.TestConnection (enteredUri, webDavClient);
        if (result.ResourceType != ResourceType.None)
        {
          settingsFaultFinder.FixSynchronizationMode (result);

          DisplayTestReport (
              result,
              environment.SynchronizationMode,
              environment.SynchronizationModeDisplayName,
              outlookFolderType);
          return url;
        }
        else
        {
          var autodiscoveryResult = await DoAutoDiscovery (enteredUri, webDavClient, false, false, outlookFolderType);
          switch (autodiscoveryResult.Status)
          {
            case AutoDiscoverResultStatus.UserCancelled:
              return url;
            case AutoDiscoverResultStatus.ResourceSelected:
              autoDiscoveredUrl = autodiscoveryResult.RessourceUrl;
              break;
            case AutoDiscoverResultStatus.NoResourcesFound:
              var autodiscoveryResult2 = await DoAutoDiscovery (enteredUri, webDavClient, true, true, outlookFolderType);
              switch (autodiscoveryResult2.Status)
              {
                case AutoDiscoverResultStatus.UserCancelled:
                  return url;
                case AutoDiscoverResultStatus.ResourceSelected:
                  autoDiscoveredUrl = autodiscoveryResult2.RessourceUrl;
                  break;
                case AutoDiscoverResultStatus.NoResourcesFound:
                  MessageBox.Show ("No resources were found via autodiscovery!", ConnectionTestCaption);
                  return url;
                default:
                  throw new NotImplementedException (autodiscoveryResult2.Status.ToString ());
              }
              break;
            default:
              throw new NotImplementedException (autodiscoveryResult.Status.ToString ());
          }
         
        }
      }


      var finalResult = await ConnectionTester.TestConnection (autoDiscoveredUrl, webDavClient);

      settingsFaultFinder.FixSynchronizationMode (finalResult);

      DisplayTestReport (
          finalResult,
          environment.SynchronizationMode,
          environment.SynchronizationModeDisplayName,
          outlookFolderType);

      return autoDiscoveredUrl.ToString ();
    }

    public static async Task<string> TestGoogleConnection (ICurrentOptions currentOptions, ISettingsFaultFinder settingsFaultFinder, string url)
    {
      if (currentOptions.OutlookFolderType == null)
      {
        MessageBox.Show ("Please select an Outlook folder to specify the item type for this profile", ConnectionTestCaption);
        return url;
      }

      var outlookFolderType = currentOptions.OutlookFolderType.Value;

      StringBuilder errorMessageBuilder = new StringBuilder();

      if (!ValidateGoogleEmailAddress (errorMessageBuilder, currentOptions.EmailAddress))
      {
        MessageBox.Show (errorMessageBuilder.ToString(), "The Email Address is invalid", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return url;
      }

      if (outlookFolderType == OlItemType.olTaskItem)
      {
        return await TestGoogleTaskConnection(currentOptions, errorMessageBuilder, outlookFolderType, url);
      }

      if (outlookFolderType == OlItemType.olContactItem && currentOptions.ServerAdapterType == ServerAdapterType.GoogleContactApi)
      {
        return await TestGoogleContactsConnection (currentOptions, outlookFolderType, url);
      }

      if (!ValidateWebDavUrl (url, errorMessageBuilder, false))
      {
        MessageBox.Show (errorMessageBuilder.ToString(), "The CalDav/CardDav Url is invalid", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return url;
      }

      var enteredUri = new Uri (url);
      var webDavClient = currentOptions.CreateWebDavClient(enteredUri);

      Uri autoDiscoveredUrl;

      if (ConnectionTester.RequiresAutoDiscovery (enteredUri))
      {
        var autoDiscoveryResult = await DoAutoDiscovery (enteredUri, webDavClient, false, true, outlookFolderType);
        switch (autoDiscoveryResult.Status)
        {
          case AutoDiscoverResultStatus.UserCancelled:
            return url;
          case AutoDiscoverResultStatus.ResourceSelected:
            autoDiscoveredUrl = autoDiscoveryResult.RessourceUrl;
            break;
          default:
            autoDiscoveredUrl = null;
            break;
        }
      }
      else
      {
        autoDiscoveredUrl = null;
      }


      var finalUrl = autoDiscoveredUrl?.ToString() ?? url;

      var result = await ConnectionTester.TestConnection (new Uri(finalUrl), webDavClient);

      if (result.ResourceType != ResourceType.None)
      {
        settingsFaultFinder.FixSynchronizationMode (result);
      }

      if (outlookFolderType == OlItemType.olContactItem)
      {
        // Google Addressbook doesn't have any properties. As long as there doesn't occur an exception, the test is successful.
        MessageBox.Show ("Connection test successful.", ConnectionTestCaption);
      }
      else
      {
        DisplayTestReport (
            result,
            currentOptions.SynchronizationMode,
            currentOptions.SynchronizationModeDisplayName,
            outlookFolderType);
      }

      return finalUrl;
    }

    private static async Task<string> TestGoogleTaskConnection (ICurrentOptions currentOptions, StringBuilder errorMessageBuilder, OlItemType outlookFolderType, string url)
    {
      var service = await GoogleHttpClientFactory.LoginToGoogleTasksService (currentOptions.EmailAddress, currentOptions.GetProxyIfConfigured());

      string connectionTestUrl;
      if (string.IsNullOrEmpty (url))
      {
        TaskLists taskLists = await service.Tasklists.List().ExecuteAsync();

        if (taskLists.Items.Any())
        {
          var selectedTaskList = SelectTaskList (taskLists.Items.Select (i => new TaskListData (i.Id, i.Title)).ToArray());
          if (selectedTaskList != null)
            connectionTestUrl = selectedTaskList.Id;
          else
            return url;
        }
        else
        {
          connectionTestUrl = url;
        }
      }
      else
      {
        connectionTestUrl = url;
      }

      try
      {
        await service.Tasklists.Get (connectionTestUrl).ExecuteAsync();
      }
      catch (Exception x)
      {
        s_logger.Error (null, x);
        errorMessageBuilder.AppendFormat ("The tasklist with id '{0}' is invalid.", connectionTestUrl);
        MessageBox.Show (errorMessageBuilder.ToString(), "The tasklist is invalid", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return url;
      }
      TestResult result = new TestResult (ResourceType.TaskList, CalendarProperties.None, AddressBookProperties.None);

      DisplayTestReport (
          result,
          currentOptions.SynchronizationMode,
          currentOptions.SynchronizationModeDisplayName,
          outlookFolderType);
      return connectionTestUrl;
    }

    private static async Task<string> TestGoogleContactsConnection (ICurrentOptions currentOptions, OlItemType outlookFolderType, string url)
    {
      var service = await GoogleHttpClientFactory.LoginToContactsService (currentOptions.EmailAddress, currentOptions.GetProxyIfConfigured());

      try
      {
        await Task.Run (() => service.GetGroups());
      }
      catch (Exception x)
      {
        s_logger.Error (null, x);
        MessageBox.Show (x.Message, ConnectionTestCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        return url;
      }
      TestResult result = new TestResult (
          ResourceType.AddressBook,
          CalendarProperties.None,
          AddressBookProperties.AddressBookAccessSupported | AddressBookProperties.IsWriteable);

      DisplayTestReport (
          result,
          currentOptions.SynchronizationMode,
          currentOptions.SynchronizationModeDisplayName,
          outlookFolderType);
      return string.Empty;
    }

    public static Contracts.Options CreateNewSynchronizationProfileOrNull ()
    {
      ProfileType? type;
      return CreateNewSynchronizationProfileOrNull (out type);
    }

    public static Contracts.Options CreateNewSynchronizationProfileOrNull (out ProfileType? type)
    {
      type = SelectOptionsDisplayTypeForm.QueryProfileType ();
      if (!type.HasValue)
        return null;

      var options = Contracts.Options.CreateDefault (type.Value);
      options.ServerAdapterType = (type == ProfileType.Google)
          ? ServerAdapterType.WebDavHttpClientBasedWithGoogleOAuth
          : ServerAdapterType.WebDavHttpClientBased;
      return options;
    }
  }
}