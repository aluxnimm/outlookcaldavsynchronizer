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
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Globalization;
using CalDavSynchronizer.Implementation;
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.OAuth.Google;
using CalDavSynchronizer.Ui.ConnectionTests;
using CalDavSynchronizer.Ui.Options.Models;
using CalDavSynchronizer.Ui.Options.ResourceSelection.ViewModels;
using CalDavSynchronizer.Utilities;
using Google.Apis.Tasks.v1.Data;
using log4net;
using Microsoft.Office.Interop.Outlook;
using Exception = System.Exception;
using Task = System.Threading.Tasks.Task;

namespace CalDavSynchronizer.Ui.Options
{
  internal class OptionTasks : IOptionTasks
  {
    private static readonly ILog s_logger = LogManager.GetLogger(MethodInfo.GetCurrentMethod().DeclaringType);

    public static readonly string ConnectionTestCaption = Strings.Get($"Test settings");
    public static readonly string CreateDavResourceCaption = Strings.Get($"Create DAV server resource");
    public const string GoogleDavBaseUrl = "https://apidata.googleusercontent.com/caldav/v2";

    private readonly IEnumDisplayNameProvider _enumDisplayNameProvider;
    private readonly NameSpace _session;
    private readonly IOutlookSession _outlookSession;
    private readonly ISettingsFaultFinder _settingsFaultFinder;

    public OptionTasks(NameSpace session, IEnumDisplayNameProvider enumDisplayNameProvider, ISettingsFaultFinder settingsFaultFinder, IOutlookSession outlookSession)
    {
      if (session == null) throw new ArgumentNullException(nameof(session));
      if (enumDisplayNameProvider == null) throw new ArgumentNullException(nameof(enumDisplayNameProvider));
      if (settingsFaultFinder == null) throw new ArgumentNullException(nameof(settingsFaultFinder));
      if (outlookSession == null) throw new ArgumentNullException(nameof(outlookSession));

      _session = session;
      _enumDisplayNameProvider = enumDisplayNameProvider;
      _settingsFaultFinder = settingsFaultFinder;
      _outlookSession = outlookSession;
    }

    public static bool ValidateCategoryName(string category, StringBuilder errorMessageBuilder)
    {
      bool result = true;

      if (category.Contains(","))
      {
        errorMessageBuilder.AppendLine(Strings.Get($"- The category name must not contain commas."));
        result = false;
      }
      if (category.Contains(";"))
      {
        errorMessageBuilder.AppendLine(Strings.Get($"- The category name must not contain semicolons."));
        result = false;
      }
      return result;
    }

    public static bool ValidateWebDavUrl(string webDavUrl, StringBuilder errorMessageBuilder, bool requiresTrailingSlash)
    {
      bool result = true;

      if (string.IsNullOrWhiteSpace(webDavUrl))
      {
        errorMessageBuilder.AppendLine(Strings.Get($"- The CalDav/CardDav URL is empty."));
        errorMessageBuilder.AppendLine(Strings.Get($"- If you don't know the URL you can also enter the Email address and try to discover it."));
        return false;
      }

      if (webDavUrl.Trim() != webDavUrl)
      {
        errorMessageBuilder.AppendLine(Strings.Get($"- The CalDav/CardDav URL cannot end/start with whitespaces."));
        result = false;
      }

      if (requiresTrailingSlash && !webDavUrl.EndsWith("/"))
      {
        errorMessageBuilder.AppendLine(Strings.Get($"- The CalDav/CardDav URL has to end with a slash ('/')."));
        result = false;
      }

      try
      {
        var uri = new Uri(webDavUrl).ToString();
      }
      catch (Exception x)
      {
        errorMessageBuilder.AppendFormat(Strings.Get($"- The CalDav/CardDav URL is not a well formed URL. ({x.Message})"));
        errorMessageBuilder.AppendLine();
        result = false;
      }

      return result;
    }

    public static bool ValidateGoogleEmailAddress(StringBuilder errorMessageBuilder, string emailAddress)
    {
      if (string.IsNullOrWhiteSpace(emailAddress))
      {
        errorMessageBuilder.AppendLine(Strings.Get($"- The Email address is empty."));
        return false;
      }
      return ValidateEmailAddress(errorMessageBuilder, emailAddress);
    }

    public static bool ValidateEmailAddress(StringBuilder errorMessageBuilder, string emailAddress)
    {
      try
      {
        var uri = new Uri("mailto:" + emailAddress).ToString();
        return true;
      }
      catch (Exception x)
      {
        errorMessageBuilder.AppendFormat(Strings.Get($"- The Email address is invalid. ({x.Message})"));
        errorMessageBuilder.AppendLine();
        return false;
      }
    }


    public static void DisplayTestReport(
        TestResult result,
        SynchronizationMode synchronizationMode,
        string selectedSynchronizationModeDisplayName,
        OlItemType outlookFolderType)
    {
      bool hasError = false;
      bool hasWarning = false;
      var errorMessageBuilder = new StringBuilder();

      var isCalendar = result.ResourceType.HasFlag(ResourceType.Calendar);
      var isAddressBook = result.ResourceType.HasFlag(ResourceType.AddressBook);
      var isTaskList = result.ResourceType.HasFlag(ResourceType.TaskList);

      if (isCalendar && isAddressBook)
      {
        errorMessageBuilder.AppendLine(Strings.Get($"- Resources which are a calendar and an addressbook are not valid!"));
        hasError = true;
      }

      switch (outlookFolderType)
      {
        case OlItemType.olAppointmentItem:
          if (isCalendar)
          {
            if (!result.CalendarProperties.HasFlag(CalendarProperties.CalendarAccessSupported))
            {
              errorMessageBuilder.AppendLine(Strings.Get($"- The specified URL does not support calendar access."));
              hasError = true;
            }

            if (!result.CalendarProperties.HasFlag(CalendarProperties.SupportsCalendarQuery))
            {
              errorMessageBuilder.AppendLine(Strings.Get($"- The specified URL does not support calendar queries. Some features like time range filter may not work!"));
              hasWarning = true;
            }

            if (DoesModeRequireWriteableServerResource(synchronizationMode))
            {
              if (!result.AccessPrivileges.HasFlag(AccessPrivileges.Modify))
              {
                errorMessageBuilder.AppendFormat(
                  Strings.Get($"- The specified calendar is not writeable. Therefore it is not possible to use the synchronization mode '{selectedSynchronizationModeDisplayName}'."));
                errorMessageBuilder.AppendLine();
                hasError = true;
              }
              if (!result.AccessPrivileges.HasFlag(AccessPrivileges.Create))
              {
                errorMessageBuilder.AppendLine(Strings.Get($"- The specified calendar doesn't allow creation of appointments!"));
                hasWarning = true;
              }
              if (!result.AccessPrivileges.HasFlag(AccessPrivileges.Delete))
              {
                errorMessageBuilder.AppendLine(Strings.Get($"- The specified calendar doesn't allow deletion of appointments!"));
                hasWarning = true;
              }
            }
          }
          else
          {
            errorMessageBuilder.AppendLine(Strings.Get($"- The specified URL is not a calendar!"));
            hasError = true;
          }
          break;

        case OlItemType.olContactItem:
          if (isAddressBook)
          {
            if (!result.AddressBookProperties.HasFlag(AddressBookProperties.AddressBookAccessSupported))
            {
              errorMessageBuilder.AppendLine(Strings.Get($"- The specified URL does not support address books."));
              hasError = true;
            }

            if (DoesModeRequireWriteableServerResource(synchronizationMode))
            {
              if (!result.AccessPrivileges.HasFlag(AccessPrivileges.Modify))
              {
                errorMessageBuilder.AppendFormat(
                  Strings.Get(
                    $"- The specified address book is not writeable. Therefore it is not possible to use the synchronization mode '{selectedSynchronizationModeDisplayName}'."));
                errorMessageBuilder.AppendLine();
                hasError = true;
              }
              if (!result.AccessPrivileges.HasFlag(AccessPrivileges.Create))
              {
                errorMessageBuilder.AppendLine(Strings.Get($"- The specified address book doesn't allow creation of contacts!"));
                hasWarning = true;
              }
              if (!result.AccessPrivileges.HasFlag(AccessPrivileges.Delete))
              {
                errorMessageBuilder.AppendLine(Strings.Get($"- The specified address book doesn't allow deletion of contacts!"));
                hasWarning = true;
              }
            }
          }
          else
          {
            errorMessageBuilder.AppendLine(Strings.Get($"- The specified URL is not an addressbook!"));
            hasError = true;
          }
          break;
        case OlItemType.olTaskItem:
          if (!isTaskList)
          {
            errorMessageBuilder.AppendLine(Strings.Get($"- The specified URL is not an task list!"));
            hasError = true;
          }
          break;
      }

      if (hasError)
        MessageBox.Show(Strings.Get($"Connection test NOT successful:") + Environment.NewLine + errorMessageBuilder, ConnectionTestCaption);
      else if (hasWarning)
        MessageBox.Show(Strings.Get($"Connection test successful BUT:") + Environment.NewLine + errorMessageBuilder, ConnectionTestCaption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
      else
        MessageBox.Show(Strings.Get($"Connection test successful."), ConnectionTestCaption);
    }

    public static string DoSrvLookup(string serverEmail, OlItemType selectedOutlookFolderType, out bool success)
    {
      string emailDomain = serverEmail.Substring(serverEmail.IndexOf('@') + 1);
      string lookupUrl = "https://" + emailDomain;
      success = false;
      string srvBase = selectedOutlookFolderType == OlItemType.olContactItem ? "_carddav" : "_caldav";
      string lookupString = srvBase + "s._tcp." + emailDomain;

      var srvRecordsCaldavs = DnsQueryHelper.GetSRVRecordList(lookupString);
      if (srvRecordsCaldavs.Count > 0)
      {
        lookupUrl = "https://" + srvRecordsCaldavs[0];
        success = true;
        var txtRecords = DnsQueryHelper.GetTxtRecord(lookupString);
        if (txtRecords != null && txtRecords.StartsWith("path="))
          lookupUrl += txtRecords.Substring(txtRecords.IndexOf('=') + 1);
      }
      else
      {
        lookupString = srvBase + "._tcp." + emailDomain;
        var srvRecordsCaldav = DnsQueryHelper.GetSRVRecordList(lookupString);
        if (srvRecordsCaldav.Count > 0)
        {
          lookupUrl = "http://" + srvRecordsCaldav[0];
          success = true;
          var txtRecords = DnsQueryHelper.GetTxtRecord(lookupString);
          if (txtRecords != null && txtRecords.StartsWith("path="))
            lookupUrl += txtRecords.Substring(txtRecords.IndexOf('=') + 1);
        }
      }
      return lookupUrl;
    }

    public static async Task<Uri> AddResource(Uri davUri, IWebDavClient webDavClient, OlItemType selectedOutlookFolderType)
    {
      switch (selectedOutlookFolderType)
      {
        case OlItemType.olAppointmentItem:
        case OlItemType.olTaskItem:
          var calDavDataAccess = new CalDavDataAccess(davUri, webDavClient);
          using (var addResourceForm = new AddResourceForm(true))
          {
            addResourceForm.Text = Strings.Get($"Add calendar resource on server");
            if (addResourceForm.ShowDialog() == DialogResult.OK)
            {
              try
              {
                var newUri = await calDavDataAccess.AddResource(addResourceForm.ResourceName, addResourceForm.UseRandomUri);
                MessageBox.Show(Strings.Get($"Added calendar resource '{addResourceForm.ResourceName}' successfully!"), CreateDavResourceCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                if (!await new CalDavDataAccess(newUri, webDavClient).SetCalendarColorNoThrow(new ArgbColor(addResourceForm.CalendarColor.ToArgb())))
                  MessageBox.Show(Strings.Get($"Can't set the calendar color!'"), CreateDavResourceCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return newUri;
              }
              catch (Exception ex)
              {
                s_logger.Error($"Can't add calendar resource '{addResourceForm.ResourceName}'", ex);
                MessageBox.Show(Strings.Get($"Can't add calendar resource '{addResourceForm.ResourceName}'"), CreateDavResourceCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
              }

            }
          }
          break;
        case OlItemType.olContactItem:
          var cardDavDataAccess = new CardDavDataAccess(davUri, webDavClient, string.Empty, contentType => true);
          using (var addResourceForm = new AddResourceForm(false))
          {
            addResourceForm.Text = Strings.Get($"Add addressbook resource on server");
            if (addResourceForm.ShowDialog() == DialogResult.OK)
            {
              try
              {
                var newUri = await cardDavDataAccess.AddResource(addResourceForm.ResourceName, addResourceForm.UseRandomUri);
                MessageBox.Show(Strings.Get($"Added addressbook resource '{addResourceForm.ResourceName}' successfully!"), CreateDavResourceCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return newUri;
              }
              catch (Exception ex)
              {
                s_logger.Error($"Can't add addressbook resource '{addResourceForm.ResourceName}'", ex);
                MessageBox.Show(Strings.Get($"Can't add addressbook resource '{addResourceForm.ResourceName}'"), CreateDavResourceCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
              }
            }
          }
          break;
      }
      return davUri;
    }

    public static async Task<AutoDiscoveryResult> DoAutoDiscovery(Uri autoDiscoveryUri, IWebDavClient webDavClient, bool useWellKnownCalDav, bool useWellKnownCardDav, OlItemType selectedOutlookFolderType)
    {

      switch (selectedOutlookFolderType)
      {
        case OlItemType.olAppointmentItem:
          var calDavDataAccess = new CalDavDataAccess(autoDiscoveryUri, webDavClient);
          var foundCalendars = (await calDavDataAccess.GetUserResourcesIncludingCalendarProxies(useWellKnownCalDav)).CalendarResources;
          if (foundCalendars.Count == 0)
            return new AutoDiscoveryResult(null, AutoDiscoverResultStatus.NoResourcesFound);
          var selectedCalendar = SelectCalendar(foundCalendars);
          if (selectedCalendar != null)
            return new AutoDiscoveryResult(selectedCalendar.Uri, AutoDiscoverResultStatus.ResourceSelected);
          else
            return new AutoDiscoveryResult(null, AutoDiscoverResultStatus.UserCancelled);
        case OlItemType.olTaskItem:
          var calDavDataAccessTasks = new CalDavDataAccess(autoDiscoveryUri, webDavClient);
          var foundTasks = (await calDavDataAccessTasks.GetUserResourcesIncludingCalendarProxies(useWellKnownCalDav)).TaskListResources;
          if (foundTasks.Count == 0)
            return new AutoDiscoveryResult(null, AutoDiscoverResultStatus.NoResourcesFound);
          var selectedTask = SelectTaskList(foundTasks);
          if (selectedTask != null)
            return new AutoDiscoveryResult(new Uri(selectedTask.Id), AutoDiscoverResultStatus.ResourceSelected);
          else
            return new AutoDiscoveryResult(null, AutoDiscoverResultStatus.UserCancelled);
        case OlItemType.olContactItem:
          var cardDavDataAccess = new CardDavDataAccess(autoDiscoveryUri, webDavClient, string.Empty, contentType => true);
          var foundAddressBooks = await cardDavDataAccess.GetUserAddressBooksNoThrow(useWellKnownCardDav);
          if (foundAddressBooks.Count == 0)
            return new AutoDiscoveryResult(null, AutoDiscoverResultStatus.NoResourcesFound);
          var selectedAddressBook = SelectAddressBook(foundAddressBooks);
          if (selectedAddressBook != null)
            return new AutoDiscoveryResult(selectedAddressBook.Uri, AutoDiscoverResultStatus.ResourceSelected);
          else
            return new AutoDiscoveryResult(null, AutoDiscoverResultStatus.UserCancelled);
        default:
          throw new NotImplementedException($"'{selectedOutlookFolderType}' not implemented.");
      }
    }


    static CalendarData SelectCalendar(IReadOnlyList<CalendarData> items)
    {
      using (SelectResourceForm selectResourceForm = SelectResourceForm.CreateForResourceSelection(ResourceType.Calendar, items.Select(d => new CalendarDataViewModel(d)).ToArray()))
      {
        if (selectResourceForm.ShowDialog() == DialogResult.OK)
          return ((CalendarDataViewModel)selectResourceForm.SelectedObject).Model;
        else
          return null;
      }
    }

    static AddressBookData SelectAddressBook(IReadOnlyList<AddressBookData> items)
    {
      using (SelectResourceForm selectResourceForm = SelectResourceForm.CreateForResourceSelection(ResourceType.AddressBook, null, items.Select(d => new AddressBookDataViewModel(d)).ToArray()))
      {
        if (selectResourceForm.ShowDialog() == DialogResult.OK)
          return ((AddressBookDataViewModel)selectResourceForm.SelectedObject).Model;
        else
          return null;
      }
    }

    static TaskListData SelectTaskList(IReadOnlyList<TaskListData> items)
    {
      using (SelectResourceForm selectResourceForm = SelectResourceForm.CreateForResourceSelection(ResourceType.TaskList, null, null, items.Select(d => new TaskListDataViewModel(d)).ToArray()))
      {
        if (selectResourceForm.ShowDialog() == DialogResult.OK)
          return ((TaskListDataViewModel)selectResourceForm.SelectedObject).Model;
        else
          return null;
      }
    }


    public string GetFolderAccountNameOrNull(string folderStoreId)
    {
      if (ThisAddIn.IsOutlookVersionSmallerThan2010)
        return null;

      try
      {
        foreach (Account account in _session.Accounts.ToSafeEnumerable<Account>())
        {
          using (var deliveryStore = GenericComObjectWrapper.Create(account.DeliveryStore))
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
        s_logger.Error("Can't access Account Name of folder.", ex);
      }
      return null;
    }

    public OutlookFolderDescriptor GetFolderFromId(string entryId, object storeId)
    {
      return new OutlookFolderDescriptor(_session.GetFolderFromID(entryId, storeId));
    }

    public OutlookFolderDescriptor PickFolderOrNull()
    {
      var folder = _session.PickFolder();
      if (folder != null)
      {
        using (var wrapper = GenericComObjectWrapper.Create(folder))
          return new OutlookFolderDescriptor(wrapper.Inner);
      }
      else
      {
        return null;
      }
    }

    public OutlookFolderDescriptor GetDefaultCalendarFolderOrNull()
    {
      var folder = _session.GetDefaultFolder (OlDefaultFolders.olFolderCalendar);
      if (folder != null)
      {
        using (var wrapper = GenericComObjectWrapper.Create (folder))
          return new OutlookFolderDescriptor(wrapper.Inner);
      }
      else
      {
        return null;
      }
    }

    private IProfileExportProcessor _profileExportProcessor;
    public IProfileExportProcessor ProfileExportProcessor
    {
      get
      {
        if (_profileExportProcessor == null)
          _profileExportProcessor = new ProfileExportProcessor(_outlookSession, this);

        return _profileExportProcessor;
      }
    }

    public void SaveOptions(Contracts.Options[] options, string fileName)
    {
      var dataAccess = new OptionsDataAccess(fileName);
      dataAccess.Save(options);
    }

    public Contracts.Options[] LoadOptions(string fileName)
    {
      var dataAccess = new OptionsDataAccess(fileName);
      return dataAccess.Load();
    }

    public static bool DoesModeRequireWriteableServerResource(SynchronizationMode synchronizationMode)
    {
      return synchronizationMode == SynchronizationMode.MergeInBothDirections
             || synchronizationMode == SynchronizationMode.MergeOutlookIntoServer
             || synchronizationMode == SynchronizationMode.ReplicateOutlookIntoServer;
    }


    public static async Task<string> CreateDavResource(OptionsModel options, string url)
    {
      if (options.SelectedFolderOrNull == null)
      {
        MessageBox.Show(Strings.Get($"Please select an Outlook folder to specify the item type for this profile"), ConnectionTestCaption);
        return url;
      }

      var outlookFolderType = options.SelectedFolderOrNull.DefaultItemType;

      string serverUrl = url;
      StringBuilder errorMessageBuilder = new StringBuilder();

      if (!ValidateWebDavUrl(serverUrl, errorMessageBuilder, false))
      {
        MessageBox.Show(errorMessageBuilder.ToString(), Strings.Get($"The CalDav/CardDav URL is invalid"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        return url;
      }

      var enteredUri = new Uri(serverUrl);
      var webDavClient = options.CreateWebDavClient(enteredUri);
      var newResourceUri = await AddResource(enteredUri, webDavClient, outlookFolderType);

      return newResourceUri.ToString();
    }

    public async Task<string> TestWebDavConnection(OptionsModel options)
    {
      string url = options.CalenderUrl;
      string serverEmail = options.EmailAddress;

      if (options.SelectedFolderOrNull == null)
      {
        MessageBox.Show(Strings.Get($"Please select an Outlook folder to specify the item type for this profile"), ConnectionTestCaption);
        return url;
      }

      var outlookFolderType = options.SelectedFolderOrNull.DefaultItemType;

      string serverUrl = url;
      StringBuilder errorMessageBuilder = new StringBuilder();

      if (string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(serverEmail))
      {
        if (!ValidateEmailAddress(errorMessageBuilder, serverEmail))
        {
          MessageBox.Show(errorMessageBuilder.ToString(), Strings.Get($"The Email address is invalid"), MessageBoxButtons.OK, MessageBoxIcon.Error);
          return url;
        }
        bool success;
        serverUrl = DoSrvLookup(serverEmail, outlookFolderType, out success);
      }


      if (!ValidateWebDavUrl(serverUrl, errorMessageBuilder, false))
      {
        MessageBox.Show(errorMessageBuilder.ToString(), Strings.Get($"The CalDav/CardDav URL is invalid"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        return url;
      }

      var enteredUri = new Uri(serverUrl);
      var webDavClient = options.CreateWebDavClient(enteredUri);

      Uri autoDiscoveredUrl;

      if (ConnectionTester.RequiresAutoDiscovery(enteredUri))
      {
        var autodiscoveryResult = await DoAutoDiscovery(enteredUri, webDavClient, true, true, outlookFolderType);
        switch (autodiscoveryResult.Status)
        {
          case AutoDiscoverResultStatus.UserCancelled:
            return url;
          case AutoDiscoverResultStatus.ResourceSelected:
            autoDiscoveredUrl = autodiscoveryResult.RessourceUrl;
            break;
          case AutoDiscoverResultStatus.NoResourcesFound:
            var autodiscoveryResult2 = await DoAutoDiscovery(enteredUri.AbsolutePath.EndsWith("/") ? enteredUri : new Uri(enteredUri.ToString() + "/"), webDavClient, false, false, outlookFolderType);
            switch (autodiscoveryResult2.Status)
            {
              case AutoDiscoverResultStatus.UserCancelled:
                return url;
              case AutoDiscoverResultStatus.ResourceSelected:
                autoDiscoveredUrl = autodiscoveryResult2.RessourceUrl;
                break;
              case AutoDiscoverResultStatus.NoResourcesFound:
                MessageBox.Show(Strings.Get($"No resources were found via autodiscovery!"), ConnectionTestCaption);
                return url;
              default:
                throw new NotImplementedException(autodiscoveryResult2.Status.ToString());
            }
            break;
          default:
            throw new NotImplementedException(autodiscoveryResult.Status.ToString());
        }
      }
      else
      {
        var result = await ConnectionTester.TestConnection(enteredUri, webDavClient);
        if (result.ResourceType != ResourceType.None)
        {
          _settingsFaultFinder.FixSynchronizationMode(options, result);
          _settingsFaultFinder.FixWebDavCollectionSync(options, result);
          _settingsFaultFinder.UpdateServerEmailAndSchedulingSettings(options, result);

          DisplayTestReport(
              result,
              options.SynchronizationMode,
              _enumDisplayNameProvider.Get(options.SynchronizationMode),
              outlookFolderType);
          return url;
        }
        else
        {
          var autodiscoveryResult = await DoAutoDiscovery(enteredUri, webDavClient, false, false, outlookFolderType);
          switch (autodiscoveryResult.Status)
          {
            case AutoDiscoverResultStatus.UserCancelled:
              return url;
            case AutoDiscoverResultStatus.ResourceSelected:
              autoDiscoveredUrl = autodiscoveryResult.RessourceUrl;
              break;
            case AutoDiscoverResultStatus.NoResourcesFound:
              var autodiscoveryResult2 = await DoAutoDiscovery(enteredUri, webDavClient, true, true, outlookFolderType);
              switch (autodiscoveryResult2.Status)
              {
                case AutoDiscoverResultStatus.UserCancelled:
                  return url;
                case AutoDiscoverResultStatus.ResourceSelected:
                  autoDiscoveredUrl = autodiscoveryResult2.RessourceUrl;
                  break;
                case AutoDiscoverResultStatus.NoResourcesFound:
                  MessageBox.Show(Strings.Get($"No resources were found via autodiscovery!"), ConnectionTestCaption);
                  return url;
                default:
                  throw new NotImplementedException(autodiscoveryResult2.Status.ToString());
              }
              break;
            default:
              throw new NotImplementedException(autodiscoveryResult.Status.ToString());
          }

        }
      }


      var finalResult = await ConnectionTester.TestConnection(autoDiscoveredUrl, webDavClient);

      _settingsFaultFinder.FixSynchronizationMode(options, finalResult);
      _settingsFaultFinder.FixWebDavCollectionSync(options, finalResult);
      _settingsFaultFinder.UpdateServerEmailAndSchedulingSettings(options, finalResult);

      DisplayTestReport(
          finalResult,
          options.SynchronizationMode,
           _enumDisplayNameProvider.Get(options.SynchronizationMode),
          outlookFolderType);

      return autoDiscoveredUrl.ToString();
    }

    public async Task<string> TestGoogleConnection(OptionsModel options, string url)
    {
      if (options.SelectedFolderOrNull == null)
      {
        MessageBox.Show(Strings.Get($"Please select an Outlook folder to specify the item type for this profile"), ConnectionTestCaption);
        return url;
      }

      var outlookFolderType = options.SelectedFolderOrNull.DefaultItemType;

      StringBuilder errorMessageBuilder = new StringBuilder();

      if (!ValidateGoogleEmailAddress(errorMessageBuilder, options.EmailAddress))
      {
        MessageBox.Show(errorMessageBuilder.ToString(), Strings.Get($"The Email address is invalid"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        return url;
      }

      if (outlookFolderType == OlItemType.olTaskItem)
      {
        return await TestGoogleTaskConnection(options, errorMessageBuilder, outlookFolderType, url);
      }

      if (outlookFolderType == OlItemType.olContactItem && options.UseGoogleNativeApi)
      {
        return await TestGoogleContactsConnection(options, outlookFolderType, url);
      }

      if (!ValidateWebDavUrl(url, errorMessageBuilder, false))
      {
        MessageBox.Show(errorMessageBuilder.ToString(), Strings.Get($"The CalDav/CardDav URL is invalid"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        return url;
      }

      var enteredUri = new Uri(url);
      var webDavClient = options.CreateWebDavClient(enteredUri);

      Uri autoDiscoveredUrl;

      if (ConnectionTester.RequiresAutoDiscovery(enteredUri))
      {
        var autoDiscoveryResult = await DoAutoDiscovery(enteredUri, webDavClient, false, true, outlookFolderType);
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

      var result = await ConnectionTester.TestConnection(new Uri(finalUrl), webDavClient);

      if (result.ResourceType != ResourceType.None)
      {
        _settingsFaultFinder.FixSynchronizationMode(options, result);
        _settingsFaultFinder.FixWebDavCollectionSync(options, result);
      }

      if (outlookFolderType == OlItemType.olContactItem)
      {
        // Google Addressbook doesn't have any properties. As long as there doesn't occur an exception, the test is successful.
        MessageBox.Show(Strings.Get($"Connection test successful."), ConnectionTestCaption);
      }
      else
      {
        DisplayTestReport(
            result,
            options.SynchronizationMode,
            _enumDisplayNameProvider.Get(options.SynchronizationMode),
            outlookFolderType);
      }

      return finalUrl;
    }

    private async Task<string> TestGoogleTaskConnection(OptionsModel options, StringBuilder errorMessageBuilder, OlItemType outlookFolderType, string url)
    {
      var service = await GoogleHttpClientFactory.LoginToGoogleTasksService(options.EmailAddress, options.GetProxyIfConfigured());

      string connectionTestUrl;
      if (string.IsNullOrEmpty(url))
      {
        TaskLists taskLists = await service.Tasklists.List().ExecuteAsync();

        if (taskLists.Items.Any())
        {
          var selectedTaskList = SelectTaskList(taskLists.Items.Select(i => new TaskListData(i.Id, i.Title, AccessPrivileges.All)).ToArray());
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
        await service.Tasklists.Get(connectionTestUrl).ExecuteAsync();
      }
      catch (Exception x)
      {
        s_logger.Error(null, x);
        errorMessageBuilder.AppendFormat(Strings.Get($"The tasklist with id '{connectionTestUrl}' is invalid."));
        MessageBox.Show(errorMessageBuilder.ToString(), Strings.Get($"The tasklist is invalid"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        return url;
      }
      TestResult result = new TestResult(ResourceType.TaskList, CalendarProperties.None, AddressBookProperties.None, AccessPrivileges.None, false, null);

      DisplayTestReport(
          result,
          options.SynchronizationMode,
          _enumDisplayNameProvider.Get(options.SynchronizationMode),
          outlookFolderType);
      return connectionTestUrl;
    }

    private async Task<string> TestGoogleContactsConnection(OptionsModel options, OlItemType outlookFolderType, string url)
    {
      var service = await GoogleHttpClientFactory.LoginToContactsService(options.EmailAddress, options.GetProxyIfConfigured());

      try
      {
        await Task.Run(() => service.GetGroups());
      }
      catch (Exception x)
      {
        s_logger.Error(null, x);
        MessageBox.Show(x.Message, ConnectionTestCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        return url;
      }
      TestResult result = new TestResult(
          ResourceType.AddressBook,
          CalendarProperties.None,
          AddressBookProperties.AddressBookAccessSupported,
          AccessPrivileges.All,
          false,
          null);

      DisplayTestReport(
          result,
          options.SynchronizationMode,
          _enumDisplayNameProvider.Get(options.SynchronizationMode),
          outlookFolderType);
      return string.Empty;
    }


  }
}