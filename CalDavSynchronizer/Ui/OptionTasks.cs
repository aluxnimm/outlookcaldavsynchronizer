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
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Ui.ConnectionTests;
using Microsoft.Office.Interop.Outlook;
using Exception = System.Exception;

namespace CalDavSynchronizer.Ui
{
  internal static class OptionTasks
  {
    public const string ConnectionTestCaption = "Test settings";

    public static MappingConfigurationBase CoreceMappingConfiguration (OlItemType? outlookFolderType, MappingConfigurationBase mappingConfiguration)
    {
      switch (outlookFolderType)
      {
        case OlItemType.olAppointmentItem:
          if (mappingConfiguration == null || mappingConfiguration.GetType() != typeof (EventMappingConfiguration))
            return new EventMappingConfiguration();
          break;
        case OlItemType.olContactItem:
          if (mappingConfiguration == null || mappingConfiguration.GetType() != typeof (ContactMappingConfiguration))
            return new ContactMappingConfiguration();
          break;
      }

      return mappingConfiguration;
    }

    public static bool ValidateCalendarUrl (string calendarUrl, StringBuilder errorMessageBuilder, bool requiresTrailingSlash)
    {
      bool result = true;

      if (string.IsNullOrWhiteSpace (calendarUrl))
      {
        errorMessageBuilder.AppendLine ("- The CalDav/CardDav Url is empty.");
        return false;
      }

      if (calendarUrl.Trim() != calendarUrl)
      {
        errorMessageBuilder.AppendLine ("- The CalDav/CardDav Url cannot end/start with whitespaces.");
        result = false;
      }

      if (requiresTrailingSlash && !calendarUrl.EndsWith ("/"))
      {
        errorMessageBuilder.AppendLine ("- The CalDav/CardDav Url has to end with a slash ('/').");
        result = false;
      }

      try
      {
        var uri = new Uri (calendarUrl).ToString();
      }
      catch (Exception x)
      {
        errorMessageBuilder.AppendFormat ("- The CalDav/CardDav Url is not a well formed Url. ({0})", x.Message);
        errorMessageBuilder.AppendLine();
        result = false;
      }

      return result;
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
        bool selectedSynchronizationModeRequiresWriteableServerResource,
        string selectedSynchronizationModeDisplayName,
        OlItemType? outlookFolderType)
    {
      bool hasError = false;
      var errorMessageBuilder = new StringBuilder ();

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
          if (selectedSynchronizationModeRequiresWriteableServerResource)
          {
            errorMessageBuilder.AppendFormat (
                "- The specified calendar is not writeable. Therefore it is not possible to use the synchronization mode '{0}'.",
                selectedSynchronizationModeDisplayName);
            errorMessageBuilder.AppendLine ();
            hasError = true;
          }
        }

        if (outlookFolderType != OlItemType.olAppointmentItem && outlookFolderType != OlItemType.olTaskItem)
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
          if (selectedSynchronizationModeRequiresWriteableServerResource)
          {
            errorMessageBuilder.AppendFormat (
                "- The specified address book is not writeable. Therefore it is not possible to use the synchronization mode '{0}'.",
                selectedSynchronizationModeDisplayName);
            errorMessageBuilder.AppendLine ();
            hasError = true;
          }
        }

        if (outlookFolderType != OlItemType.olContactItem)
        {
          errorMessageBuilder.AppendLine ("- The outlook folder is not an address book, or there is no folder selected.");
          hasError = true;
        }
      }

      if (hasError)
        MessageBox.Show ("Connection test NOT successful:" + Environment.NewLine + errorMessageBuilder, OptionTasks.ConnectionTestCaption);
      else
        MessageBox.Show ("Connection test successful.", OptionTasks.ConnectionTestCaption);
    }

    public static async Task<AutoDiscoveryResult> DoAutoDiscovery (Uri autoDiscoveryUri, IWebDavClient webDavClient, bool useWellKnownCalDav, bool useWellKnownCardDav, OlItemType? selectedOutlookFolderType)
    {
      var calDavDataAccess = new CalDavDataAccess (autoDiscoveryUri, webDavClient);
      IReadOnlyList<Tuple<Uri, string, string>> foundCaldendars = await calDavDataAccess.GetUserCalendarsNoThrow (useWellKnownCalDav);

      var cardDavDataAccess = new CardDavDataAccess (autoDiscoveryUri, webDavClient);
      IReadOnlyList<Tuple<Uri, string>> foundAddressBooks = await cardDavDataAccess.GetUserAddressBooksNoThrow (useWellKnownCardDav);

      if (foundCaldendars.Count > 0 || foundAddressBooks.Count > 0)
      {
        using (SelectResourceForm listCalendarsForm = new SelectResourceForm (foundCaldendars, foundAddressBooks, selectedOutlookFolderType == OlItemType.olContactItem))
        {
          if (listCalendarsForm.ShowDialog () == DialogResult.OK)
            return new AutoDiscoveryResult (new Uri (autoDiscoveryUri.GetLeftPart (UriPartial.Authority) + listCalendarsForm.SelectedUrl), false, listCalendarsForm.ResourceType);
          else
            return new AutoDiscoveryResult (null, true, ResourceType.None);
        }
      }
      else
      {
        MessageBox.Show ("No resources were found via autodiscovery!", OptionTasks.ConnectionTestCaption);
        return new AutoDiscoveryResult (null, false, ResourceType.None);
      }
    }

  }
}