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
using System.Threading.Tasks;
using CalDavSynchronizer.DataAccess;

namespace CalDavSynchronizer.Ui.ConnectionTests
{
  public static class ConnectionTester
  {
    public static async Task<TestResult> TestConnection (Uri url, IWebDavClient webDavClient)
    {
      var calDavDataAccess = new CalDavDataAccess (url, webDavClient);
      var cardDavDataAccess = new CardDavDataAccess (url, webDavClient);

      var ressourceType =
          (await calDavDataAccess.IsResourceCalender() ? ResourceType.Calendar : ResourceType.None) |
          (await cardDavDataAccess.IsResourceAddressBook() ? ResourceType.AddressBook : ResourceType.None);


      return new TestResult (
          ressourceType,
          ressourceType.HasFlag (ResourceType.Calendar) ? await GetCalendarProperties (calDavDataAccess) : CalendarProperties.None,
          ressourceType.HasFlag (ResourceType.AddressBook) ? await GetAddressBookProperties (cardDavDataAccess) : AddressBookProperties.None);
    }

    private static async Task<CalendarProperties> GetCalendarProperties (CalDavDataAccess calDavDataAccess)
    {
      return
          (await calDavDataAccess.IsCalendarAccessSupported() ? CalendarProperties.CalendarAccessSupported : CalendarProperties.None) |
          (await calDavDataAccess.IsWriteable() ? CalendarProperties.IsWriteable : CalendarProperties.None) |
          (await calDavDataAccess.DoesSupportCalendarQuery() ? CalendarProperties.SupportsCalendarQuery : CalendarProperties.None);
    }

    private static async Task<AddressBookProperties> GetAddressBookProperties (CardDavDataAccess cardDavDataAccess)
    {
      return
          (await cardDavDataAccess.IsAddressBookAccessSupported() ? AddressBookProperties.AddressBookAccessSupported : AddressBookProperties.None) |
          (await cardDavDataAccess.IsWriteable() ? AddressBookProperties.IsWriteable : AddressBookProperties.None);
    }


    public static bool RequiresAutoDiscovery (Uri uri)
    {
      return uri.AbsolutePath == "/" || !uri.AbsolutePath.EndsWith ("/");
    }
  }
}