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

    public static IOptionsViewModel CoerceMappingConfiguration (
        IOptionsViewModel currentMappingConfiguration,
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

    public static async Task<AutoDiscoveryResult> DoAutoDiscovery (Uri autoDiscoveryUri, IWebDavClient webDavClient, bool useWellKnownCalDav, bool useWellKnownCardDav, OlItemType? selectedOutlookFolderType)
    {
      var calDavDataAccess = new CalDavDataAccess (autoDiscoveryUri, webDavClient);
      var foundCaldendars = await calDavDataAccess.GetUserCalendarsNoThrow (useWellKnownCalDav);

      var cardDavDataAccess = new CardDavDataAccess (autoDiscoveryUri, webDavClient);
      IReadOnlyList<Tuple<Uri, string>> foundAddressBooks = await cardDavDataAccess.GetUserAddressBooksNoThrow (useWellKnownCardDav);

      if (foundCaldendars.Count > 0 || foundAddressBooks.Count > 0)
      {
        using (SelectResourceForm listCalendarsForm =
            new SelectResourceForm (
                foundCaldendars,
                foundAddressBooks,
                new Tuple<string, string>[0],
                selectedOutlookFolderType == OlItemType.olContactItem ? ResourceType.AddressBook : ResourceType.Calendar))
        {
          if (listCalendarsForm.ShowDialog() == DialogResult.OK)
            return new AutoDiscoveryResult (new Uri (autoDiscoveryUri.GetLeftPart (UriPartial.Authority) + listCalendarsForm.SelectedUrl), false, listCalendarsForm.ResourceType);
          else
            return new AutoDiscoveryResult (null, true, ResourceType.None);
        }
      }
      else
      {
        return new AutoDiscoveryResult (null, false, ResourceType.None);
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


    public static async Task TestWebDavConnection (ICurrentOptions environment, ISettingsFaultFinder settingsFaultFinder)
    {
      if (environment.OutlookFolderType == null)
      {
        MessageBox.Show ("Please select an Outlook folder to specify the item type for this profile", ConnectionTestCaption);
        return;
      }

      var outlookFolderType = environment.OutlookFolderType.Value;
      
      StringBuilder errorMessageBuilder = new StringBuilder();
      if (!ValidateWebDavUrl (environment.ServerUrl, errorMessageBuilder, false))
      {
        MessageBox.Show (errorMessageBuilder.ToString(), "The CalDav/CardDav Url is invalid", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }

      var enteredUri = new Uri (environment.ServerUrl);
      var webDavClient = environment.CreateWebDavClient();

      Uri autoDiscoveredUrl;

      if (ConnectionTester.RequiresAutoDiscovery (enteredUri))
      {
        var autodiscoveryResult = await DoAutoDiscovery (enteredUri, webDavClient, true, true, environment.OutlookFolderType);
        if (autodiscoveryResult.WasCancelled)
          return;
        if (autodiscoveryResult.RessourceUrl != null)
        {
          autoDiscoveredUrl = autodiscoveryResult.RessourceUrl;
        }
        else
        {
          var autodiscoveryResult2 = await DoAutoDiscovery (enteredUri.AbsolutePath.EndsWith ("/") ? enteredUri : new Uri (enteredUri.ToString() + "/"), webDavClient, false, false, environment.OutlookFolderType);
          if (autodiscoveryResult2.WasCancelled)
            return;
          if (autodiscoveryResult2.RessourceUrl != null)
          {
            autoDiscoveredUrl = autodiscoveryResult2.RessourceUrl;
          }
          else
          {
            MessageBox.Show ("No resources were found via autodiscovery!", ConnectionTestCaption);
            return;
          }
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
          return;
        }
        else
        {
          var autodiscoveryResult = await DoAutoDiscovery (enteredUri, webDavClient, false, false, environment.OutlookFolderType);
          if (autodiscoveryResult.WasCancelled)
            return;
          if (autodiscoveryResult.RessourceUrl != null)
          {
            autoDiscoveredUrl = autodiscoveryResult.RessourceUrl;
          }
          else
          {
            var autodiscoveryResult2 = await DoAutoDiscovery (enteredUri, webDavClient, true, true, environment.OutlookFolderType);
            if (autodiscoveryResult2.RessourceUrl != null)
            {
              autoDiscoveredUrl = autodiscoveryResult2.RessourceUrl;
            }
            else
            {
              MessageBox.Show ("No resources were found via autodiscovery!", ConnectionTestCaption);
              return;
            }
          }
        }
      }

      environment.ServerUrl = autoDiscoveredUrl.ToString();

      var finalResult = await ConnectionTester.TestConnection (autoDiscoveredUrl, webDavClient);

      settingsFaultFinder.FixSynchronizationMode (finalResult);

      DisplayTestReport (
          finalResult,
          environment.SynchronizationMode,
          environment.SynchronizationModeDisplayName,
          outlookFolderType);
    }

    public static async Task TestGoogleConnection (ICurrentOptions currentOptions, ISettingsFaultFinder settingsFaultFinder)
    {
      if (currentOptions.OutlookFolderType == null)
      {
        MessageBox.Show ("Please select an Outlook folder to specify the item type for this profile", ConnectionTestCaption);
        return;
      }

      var outlookFolderType = currentOptions.OutlookFolderType.Value;

      StringBuilder errorMessageBuilder = new StringBuilder();

      if (!ValidateGoogleEmailAddress (errorMessageBuilder, currentOptions.EmailAddress))
      {
        MessageBox.Show (errorMessageBuilder.ToString(), "The Email Address is invalid", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }

      if (outlookFolderType == OlItemType.olTaskItem)
      {
        await TestGoogleTaskConnection(currentOptions, errorMessageBuilder, outlookFolderType);
        return;
      }

      if (outlookFolderType == OlItemType.olContactItem && currentOptions.ServerAdapterType == ServerAdapterType.GoogleContactApi)
      {
        await TestGoogleContactsConnection (currentOptions, errorMessageBuilder, outlookFolderType);
        return;
      }

      if (!ValidateWebDavUrl (currentOptions.ServerUrl, errorMessageBuilder, false))
      {
        MessageBox.Show (errorMessageBuilder.ToString(), "The CalDav/CardDav Url is invalid", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }

      var enteredUri = new Uri (currentOptions.ServerUrl);
      var webDavClient = currentOptions.CreateWebDavClient();

      Uri autoDiscoveredUrl;

      if (ConnectionTester.RequiresAutoDiscovery (enteredUri))
      {
        var calDavDataAccess = new CalDavDataAccess (enteredUri, webDavClient);
        var foundCaldendars = await calDavDataAccess.GetUserCalendarsNoThrow (false);

        var cardDavDataAccess = new CardDavDataAccess (enteredUri, webDavClient);
        IReadOnlyList<Tuple<Uri, string>> foundAddressBooks = await cardDavDataAccess.GetUserAddressBooksNoThrow (true);

        if (foundCaldendars.Count > 0 || foundAddressBooks.Count > 0)
        {
          ResourceType initalResourceType;
          if (currentOptions.OutlookFolderType == OlItemType.olContactItem)
          {
            initalResourceType = ResourceType.AddressBook;
          }
          else if (currentOptions.OutlookFolderType == OlItemType.olTaskItem)
          {
            initalResourceType = ResourceType.TaskList;
          }
          else
          {
            initalResourceType = ResourceType.Calendar;
          }

          using (SelectResourceForm listCalendarsForm =
              new SelectResourceForm (
                  foundCaldendars,
                  foundAddressBooks,
                  new Tuple<string, string>[] { },
                  initalResourceType))
          {
            if (listCalendarsForm.ShowDialog() == DialogResult.OK)
            {
              if (listCalendarsForm.ResourceType == ResourceType.TaskList)
              {
                autoDiscoveredUrl = null;
                currentOptions.ServerUrl = listCalendarsForm.SelectedUrl;
              }
              else
              {
                autoDiscoveredUrl = new Uri (enteredUri.GetLeftPart (UriPartial.Authority) + listCalendarsForm.SelectedUrl);
              }
            }
            else
            {
              autoDiscoveredUrl = null;
            }
          }
        }
        else
        {
          MessageBox.Show ("No resources were found via autodiscovery!", ConnectionTestCaption);
          autoDiscoveredUrl = null;
        }
      }
      else
      {
        autoDiscoveredUrl = null;
      }

      if (autoDiscoveredUrl != null)
      {
        currentOptions.ServerUrl = autoDiscoveredUrl.ToString ();
      }

      
      var result = await ConnectionTester.TestConnection (new Uri(currentOptions.ServerUrl), webDavClient);

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
    }

    private static async Task TestGoogleTaskConnection (ICurrentOptions currentOptions, StringBuilder errorMessageBuilder, OlItemType outlookFolderType)
    {
      var service = await GoogleHttpClientFactory.LoginToGoogleTasksService (currentOptions.EmailAddress, currentOptions.GetProxyIfConfigured());

      if (string.IsNullOrEmpty (currentOptions.ServerUrl))
      {
        TaskLists taskLists = await service.Tasklists.List().ExecuteAsync();

        if (taskLists.Items.Any())
        {

          using (SelectResourceForm listCalendarsForm =
              new SelectResourceForm (
                  new Tuple<Uri, string, ArgbColor?>[] { },
                  new Tuple<Uri, string>[] { },
                  taskLists.Items.Select (i => Tuple.Create (i.Id, i.Title)).ToArray(),
                  ResourceType.TaskList))
          {
            if (listCalendarsForm.ShowDialog() == DialogResult.OK)
              currentOptions.ServerUrl = listCalendarsForm.SelectedUrl;
            else
              return;
          }
        }

      }

      try
      {
        await service.Tasklists.Get (currentOptions.ServerUrl).ExecuteAsync();
      }
      catch (Exception x)
      {
        s_logger.Error (null, x);
        errorMessageBuilder.AppendFormat ("The tasklist with id '{0}' is invalid.", currentOptions.ServerUrl);
        MessageBox.Show (errorMessageBuilder.ToString(), "The tasklist is invalid", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }
      TestResult result = new TestResult (ResourceType.TaskList, CalendarProperties.None, AddressBookProperties.None);

      DisplayTestReport (
          result,
          currentOptions.SynchronizationMode,
          currentOptions.SynchronizationModeDisplayName,
          outlookFolderType);
    }

    private static async Task TestGoogleContactsConnection (ICurrentOptions currentOptions, StringBuilder errorMessageBuilder, OlItemType outlookFolderType)
    {
      var service = await GoogleHttpClientFactory.LoginToContactsService (currentOptions.EmailAddress, currentOptions.GetProxyIfConfigured());

      try
      {
        await Task.Run (() => service.GetGroups());
        currentOptions.ServerUrl = string.Empty;
      }
      catch (Exception x)
      {
        s_logger.Error (null, x);
        MessageBox.Show (x.Message, ConnectionTestCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
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
    }

    public static Contracts.Options CreateNewSynchronizationProfileOrNull ()
    {
      var type = SelectOptionsDisplayTypeForm.QueryProfileType ();
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