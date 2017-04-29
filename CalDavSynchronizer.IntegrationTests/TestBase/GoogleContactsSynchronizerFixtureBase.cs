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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.Implementation.GoogleContacts;
using CalDavSynchronizer.IntegrationTests.Infrastructure;
using CalDavSynchronizer.Scheduling.ComponentCollectors;
using CalDavSynchronizer.Synchronization;
using GenSync.Synchronization;
using Google.GData.Extensions;
using NUnit.Framework;

namespace CalDavSynchronizer.IntegrationTests.TestBase
{
 
  public class GoogleContactsSynchronizerFixtureBase : SynchronizerFixtureBase
  {
    private Guid _profileId;

    protected IOutlookSynchronizer Synchronizer { get; private set; }
    protected AvailableGoogleContactSynchronizerSynchronizerComponents Components { get; private set; }
    protected EasyAccessRepositoryAdapter<string, DateTime, ContactItemWrapper, IGoogleContactContext> Outlook { get; private set; }
    protected EasyAccessRepositoryAdapter<string, GoogleContactVersion, GoogleContactWrapper, IGoogleContactContext> Server { get; private set; }

    public async Task DeleteAllEntites ()
    {
      await Outlook.DeleteAllEntities ();
      await Server.DeleteAllEntities ();
    }

    protected void ClearCache ()
    {
      var profileDataDirectory = ComponentContainer.GetProfileDataDirectory (_profileId);
      if (Directory.Exists (profileDataDirectory))
        Directory.Delete (profileDataDirectory, true);
    }

    protected async Task ClearEventRepositoriesAndCache ()
    {
      await DeleteAllEntites();
      ClearCache ();
    }


    protected async Task InitializeFor(Options options)
    {
      _profileId = options.Id;
      var synchronizerWithComponents = await SynchronizerFactory.CreateSynchronizerWithComponents(options, GeneralOptions);

      var components = (AvailableGoogleContactSynchronizerSynchronizerComponents) synchronizerWithComponents.Item2;

      Synchronizer = synchronizerWithComponents.Item1;
      Components = components;
      Outlook = EasyAccessRepositoryAdapter.Create(components.OutlookContactRepository, new SynchronizationContextFactory<IGoogleContactContext>(() => NullGoogleContactContext.Instance));
      Server = EasyAccessRepositoryAdapter.Create(components.GoogleContactRepository, components.GoogleContactRepository, components.GoogleContactContextFactory);
    }

    protected string[] GetOrCreateGoogleGroups (int amount)
    {
      var groupCache = new GoogleGroupCache (Components.GoogleApiOperationExecutor);
      groupCache.Fill ();

      var existingGroupNames = new HashSet<string> (groupCache.Groups.Select (g => g.Title), StringComparer.InvariantCultureIgnoreCase);
      while (existingGroupNames.Count < amount)
      {
        string groupName = "Group";
        for (var groupSuffix = 1; existingGroupNames.Contains (groupName); groupName = $"Group {groupSuffix++}")
          ;

        existingGroupNames.Add (groupName);
        groupCache.GetOrCreateGroup (groupName);
      }

      return existingGroupNames.Take (amount).ToArray ();
    }

    protected async Task<IReadOnlyList<string>> CreateContactsInGoogle(IEnumerable<ContactData> contactDatas)
    {
      return await Server.CreateEntities(
        contactDatas.Select(
          contactData =>
            new Action<GoogleContactWrapper>(
              contact =>
              {
                contact.Contact.Name.GivenName = contactData.FirstName;
                contact.Contact.Name.FamilyName = contactData.LastName;
                contact.Contact.Emails.Add(new EMail
                {
                  Primary = true,
                  Address = contactData.EmailAddress,
                  Rel = ContactsRelationships.IsWork,
                });
                contact.Groups.AddRange(contactData.Groups);
              })));
    }

    protected async Task<Dictionary<string, ContactData>> CreateContactsInOutlook (IEnumerable<ContactData> contactDatas)
    {
      var result = new Dictionary<string, ContactData> ();

      foreach (var contactData in contactDatas)
      {
        var id = await Outlook.CreateEntity (
          contact =>
          {
            contact.Inner.FirstName = contactData.FirstName;
            contact.Inner.LastName = contactData.LastName;
            contact.Inner.Email1Address = contactData.EmailAddress;
            contact.Inner.Categories = string.Join (CultureInfo.CurrentCulture.TextInfo.ListSeparator, contactData.Groups);
          });

        result.Add (id, contactData);
      }

      return result;
    }

    protected IEnumerable<ContactData> CreateTestContactData (IReadOnlyList<string> groupNames, int amount)
    {
      var numberOfDays = Enum.GetValues (typeof (DayOfWeek)).Cast<int> ().Max () + 1;

      for (var i = 1; i <= amount; i++)
      {
        yield return new ContactData (
          "Homer" + i,
          ((DayOfWeek) (i % numberOfDays)).ToString (),
          $"homer{i}@blubb.com",
          new[]
          {
            groupNames[(i + 7) % groupNames.Count],
            groupNames[(i + 13) % groupNames.Count]
          });
      }
    }
  }
}