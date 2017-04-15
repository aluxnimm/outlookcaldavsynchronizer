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
using System.Linq;
using System.Threading.Tasks;
using CalDavSynchronizer.Implementation.Contacts;
using Thought.vCards;

namespace CalDavSynchronizer.IntegrationTests.TestBase
{
  public class ContactsSynchronizerFixtureBase
  {
   private static async Task CreateContacts(CardDavEntityRepository<vCard, vCardStandardReader, int> repository)
    {
      var numberOfDays = Enum.GetValues(typeof(DayOfWeek)).Cast<int>().Max() + 1;

      for (var i = 1; i <= 500; i++)
      {
        await repository.Create(
          vcard =>
          {
            vcard.GivenName = "Homer" + i;
            vcard.FamilyName = ((DayOfWeek) (i % numberOfDays)).ToString();
            vcard.EmailAddresses.Add(new vCardEmailAddress($"homer{i}@blubb.com"));
            return Task.FromResult(vcard);
          },
          0);

        if (i % 100 == 0)
          Console.WriteLine(i);
      }
    }
  }
}