﻿// This file is Part of CalDavSynchronizer (http://outlookcaldavsynchronizer.sourceforge.net/)
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
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.Implementation;
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.Implementation.Contacts;
using CalDavSynchronizer.Implementation.DistributionLists;
using CalDavSynchronizer.Implementation.DistributionLists.Sogo;
using CalDavSynchronizer.IntegrationTests.Infrastructure;
using CalDavSynchronizer.IntegrationTests.TestBase;
using DDay.iCal;
using GenSync.Logging;
using NUnit.Framework;
using Thought.vCards;

namespace CalDavSynchronizer.IntegrationTests
{

  public class ContactSynchronizerFixture
  {
    private TestComponentContainer _testComponentContainer;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
      _testComponentContainer = new TestComponentContainer();
    }


    [TestCase(false)]
    [TestCase(true)]
    [Apartment(System.Threading.ApartmentState.STA)]
    public async Task Synchronize_AnyCase_SyncsSogoDistLists(bool useWebDavCollectionSync)
    {
      var options = TestOptionsFactory.CreateSogoContacts();
      ((ContactMappingConfiguration)options.MappingConfiguration).MapDistributionLists = true;
      options.UseWebDavCollectionSync = useWebDavCollectionSync;

      var synchronizer = await CreateSynchronizer(options); 
      await synchronizer.ClearEventRepositoriesAndCache();

      await synchronizer.Server.CreateEntity(
        c =>
        {
          c.EmailAddresses.Add(new vCardEmailAddress("nihil@bla.com"));
          c.GivenName = "Nihil";
          c.FamilyName = "Baxter";
        });

      var masonId = await synchronizer.Server.CreateEntity (
       c =>
       {
         c.EmailAddresses.Add (new vCardEmailAddress ("mason@bla.com"));
         c.GivenName = "Agent";
         c.FamilyName = "Mason";
       });

      var steinbergId = await synchronizer.Server.CreateEntity (
       c =>
       {
         c.EmailAddresses.Add (new vCardEmailAddress ("steinberg@bla.com"));
         c.GivenName = "Agent";
         c.FamilyName = "Steinberg";
       });


      await synchronizer.ServerSogoDistListsOrNull.CreateEntity(
        l =>
        {
          l.Name = "Agents";
          l.Members.Add(new KnownDistributionListMember("mason@bla.com", "Mason", masonId.GetServerFileName()));
          l.Members.Add(new KnownDistributionListMember ("steinberg@bla.com", "Steinberg", steinbergId.GetServerFileName()));
        });
      
      await synchronizer.SynchronizeAndAssertNoErrors();

      var outlookNames = (await synchronizer.Outlook.GetAllEntities()).Select(c => c.Entity.Inner.LastName).ToArray();

      CollectionAssert.AreEquivalent(
        new[] {"Baxter", "Mason", "Steinberg"},
        outlookNames);

      using (var outlookDistList = (await synchronizer.OutlookDistListsOrNull.GetAllEntities()).SingleOrDefault()?.Entity)
      {
        Assert.That(outlookDistList, Is.Not.Null);

        Assert.That(outlookDistList.Inner.DLName, Is.EqualTo("Agents"));

        var outlookMembers = Enumerable
          .Range(1, outlookDistList.Inner.MemberCount)
          .Select(i => outlookDistList.Inner.GetMember(i))
          .ToSafeEnumerable()
          .Select(d => d.Address)
          .ToArray();

        CollectionAssert.AreEquivalent(
          new[] {"mason@bla.com", "steinberg@bla.com"},
          outlookMembers);

        outlookDistList.Inner.DLName = "All";
        var recipient = _testComponentContainer.Application.Session.CreateRecipient("Baxter");
        Assert.That(recipient.Resolve(), Is.True);
        outlookDistList.Inner.AddMember(recipient);
        outlookDistList.Inner.Save();
      }

      await synchronizer.SynchronizeAndAssertNoErrors();

      var serverDistList = (await synchronizer.ServerSogoDistListsOrNull.GetAllEntities ()).SingleOrDefault ()?.Entity;
      Assert.That (serverDistList, Is.Not.Null);

      Assert.That (serverDistList.Name, Is.EqualTo ("All"));

      CollectionAssert.AreEquivalent (
       new[] { "mason@bla.com", "steinberg@bla.com", "nihil@bla.com" },
       serverDistList.Members.Select(m => m.EmailAddress));

    }

    [TestCase(true)]
    [TestCase(false)]
    [Apartment(System.Threading.ApartmentState.STA)]
    public async Task CreateOutlookEntity_ExceptionOccurs_DoesNotLeaveEmptyEntityInRepository(bool saveAndReload)
    {
      var options = TestOptionsFactory.CreateSogoContacts();

      var synchronizer = await CreateSynchronizer(options);
      await synchronizer.ClearEventRepositoriesAndCache();

      bool exceptionCatched = false;
      var exception = new Exception("bla");
      try
      {
        await synchronizer.Components.OutlookContactRepository.Create(
          w =>
          {
            if (saveAndReload)
              w.SaveAndReload();
            throw exception;
          },
          NullCardDavRepositoryLogger.Instance);
      }
      catch (Exception x)
      {
        if (ReferenceEquals(x, exception))
          exceptionCatched = true;
      }

      Assert.That(exceptionCatched, Is.EqualTo(true));

      Assert.That(
        (await synchronizer.Outlook.GetAllEntities()).Count(),
        Is.EqualTo(0));
    }

    [TestCase(false)]
    [TestCase(true)]
    [Apartment (System.Threading.ApartmentState.STA)]
    public async Task SynchronizeTwoWay_LocalContactChanges_IsSyncedToServerAndPreservesExtendedPropertiesAndUid (bool useWebDavCollectionSync)
    {
      var options = TestOptionsFactory.CreateSogoContacts();
      options.UseWebDavCollectionSync = useWebDavCollectionSync;

      var synchronizer = await CreateSynchronizer(options);
      await synchronizer.ClearEventRepositoriesAndCache();

      string initialUid = null;

      await synchronizer.Server.CreateEntity (
      c =>
      {
        c.EmailAddresses.Add (new vCardEmailAddress ("nihil@bla.com"));
        c.GivenName = "Nihil";
        c.FamilyName = "Baxter";
        c.OtherProperties.Add (new vCardProperty ("X-CALDAVSYNCHRONIZER-INTEGRATIONTEST", "TheValueBlaBLubb"));
        initialUid = c.UniqueId;
      });

      await synchronizer.SynchronizeAndAssertNoErrors();

      using (var outlookEvent = (await synchronizer.Outlook.GetAllEntities ()).Single ().Entity)
      {
        outlookEvent.Inner.FirstName = "TheNewNihil";
        outlookEvent.Inner.Save ();
      }

      await synchronizer.SynchronizeAndAssertNoErrors();

      var serverContact = (await synchronizer.Server.GetAllEntities ()).Single ().Entity;

      Assert.That (serverContact.GivenName, Is.EqualTo ("TheNewNihil"));
      Assert.That (serverContact.UniqueId, Is.EqualTo (initialUid));

      Assert.That (
        serverContact.OtherProperties.SingleOrDefault (p => p.Name == "X-CALDAVSYNCHRONIZER-INTEGRATIONTEST")?.Value,
        Is.EqualTo ("TheValueBlaBLubb"));
    }

    [TestCase(false)]
    [TestCase(true)]
    [Apartment(System.Threading.ApartmentState.STA)]
    public async Task Synchronize_AnyCase_SyncsVCardGroupDistLists(bool useWebDavCollectionSync)
    {
      var options = TestOptionsFactory.CreateSogoContacts();
      options.UseWebDavCollectionSync = useWebDavCollectionSync;
      ((ContactMappingConfiguration) options.MappingConfiguration).MapDistributionLists = true;
      ((ContactMappingConfiguration)options.MappingConfiguration).DistributionListType = DistributionListType.VCardGroup;

      var synchronizer = await CreateSynchronizer(options);
      await synchronizer.ClearEventRepositoriesAndCache();

      await synchronizer.Server.CreateEntity(
        c =>
        {
          c.EmailAddresses.Add(new vCardEmailAddress("nihil@bla.com"));
          c.GivenName = "Nihil";
          c.FamilyName = "Baxter";
        });

      var masonId = await synchronizer.Server.CreateEntity(
        c =>
        {
          c.EmailAddresses.Add(new vCardEmailAddress("mason@bla.com"));
          c.GivenName = "Agent";
          c.FamilyName = "Mason";
        });

      var steinbergId = await synchronizer.Server.CreateEntity(
        c =>
        {
          c.EmailAddresses.Add(new vCardEmailAddress("steinberg@bla.com"));
          c.GivenName = "Agent";
          c.FamilyName = "Steinberg";
        });


      await synchronizer.ServerVCardGroupsOrNull.CreateEntity(
        l =>
        {
          l.FormattedName = "Agents";
          l.FamilyName = "Agents";

          var member = new vCardMember
          {
            EmailAddress = "mason@bla.com",
            DisplayName = "Mason"
          };
          l.Members.Add(member);

          member = new vCardMember
          {
            EmailAddress = "steinberg@bla.com",
            DisplayName = "Steinberg"
          };
          l.Members.Add(member);
        });

      await synchronizer.SynchronizeAndAssertNoErrors();

      var outlookNames = (await synchronizer.Outlook.GetAllEntities()).Select(c => c.Entity.Inner.LastName).ToArray();

      CollectionAssert.AreEquivalent(
        new[] { "Baxter", "Mason", "Steinberg" },
        outlookNames);

      using (var outlookDistList = (await synchronizer.OutlookDistListsOrNull.GetAllEntities()).SingleOrDefault()?.Entity)
      {
        Assert.That(outlookDistList, Is.Not.Null);

        Assert.That(outlookDistList.Inner.DLName, Is.EqualTo("Agents"));

        var outlookMembers = Enumerable
          .Range(1, outlookDistList.Inner.MemberCount)
          .Select(i => outlookDistList.Inner.GetMember(i))
          .ToSafeEnumerable()
          .Select(d => d.Address)
          .ToArray();

        CollectionAssert.AreEquivalent(
          new[] { "mason@bla.com", "steinberg@bla.com" },
          outlookMembers);

        outlookDistList.Inner.DLName = "All";
        var recipient = _testComponentContainer.Application.Session.CreateRecipient("Baxter");
        Assert.That(recipient.Resolve(), Is.True);
        outlookDistList.Inner.AddMember(recipient);
        outlookDistList.Inner.Save();
      }

      await synchronizer.SynchronizeAndAssertNoErrors();

      var serverDistList = (await synchronizer.ServerVCardGroupsOrNull.GetAllEntities()).SingleOrDefault()?.Entity;
      Assert.That(serverDistList, Is.Not.Null);

      Assert.That(serverDistList.FormattedName, Is.EqualTo("All"));

      CollectionAssert.AreEquivalent(
        new[] { "mason@bla.com", "steinberg@bla.com", "nihil@bla.com" },
        serverDistList.Members.Select(m => m.EmailAddress));

    }

    [TestCase(false)]
    [TestCase(true)]
    [Apartment(System.Threading.ApartmentState.STA)]
    public async Task Synchronize_AnyCase_SyncsVCardGroupWithUidDistLists(bool useWebDavCollectionSync)
    {
      var options = TestOptionsFactory.CreateSogoContacts();
      options.UseWebDavCollectionSync = useWebDavCollectionSync;

      ((ContactMappingConfiguration)options.MappingConfiguration).MapDistributionLists = true;
      ((ContactMappingConfiguration)options.MappingConfiguration).DistributionListType = DistributionListType.VCardGroupWithUid;

      var synchronizer = await CreateSynchronizer(options);
      await synchronizer.ClearEventRepositoriesAndCache();

      string nihilUid = null;
      await synchronizer.Server.CreateEntity(
        c =>
        {
          c.EmailAddresses.Add(new vCardEmailAddress("nihil@bla.com"));
          c.GivenName = "Nihil";
          c.FamilyName = "Baxter";
          nihilUid = c.UniqueId;
        });

      string masonUid = null;
      var masonId = await synchronizer.Server.CreateEntity(
        c =>
        {
          c.EmailAddresses.Add(new vCardEmailAddress("mason@bla.com"));
          c.GivenName = "Agent";
          c.FamilyName = "Mason";
          masonUid = c.UniqueId;
        });

      string steinbergUid = null;
      var steinbergId = await synchronizer.Server.CreateEntity(
        c =>
        {
          c.EmailAddresses.Add(new vCardEmailAddress("steinberg@bla.com"));
          c.GivenName = "Agent";
          c.FamilyName = "Steinberg";
          steinbergUid = c.UniqueId;
        });


      await synchronizer.ServerVCardGroupsOrNull.CreateEntity(
        l =>
        {
          l.FormattedName = "Agents";
          l.FamilyName = "Agents";

          var member = new vCardMember();
          member.Uid = masonUid;
          l.Members.Add(member);

          member = new vCardMember();
          member.Uid = steinbergUid;
          l.Members.Add(member);
        });

      await synchronizer.SynchronizeAndAssertNoErrors();

      var outlookNames = (await synchronizer.Outlook.GetAllEntities()).Select(c => c.Entity.Inner.LastName).ToArray();

      CollectionAssert.AreEquivalent(
        new[] { "Baxter", "Mason", "Steinberg" },
        outlookNames);

      using (var outlookDistList = (await synchronizer.OutlookDistListsOrNull.GetAllEntities()).SingleOrDefault()?.Entity)
      {
        Assert.That(outlookDistList, Is.Not.Null);

        Assert.That(outlookDistList.Inner.DLName, Is.EqualTo("Agents"));

        var outlookMembers = Enumerable
          .Range(1, outlookDistList.Inner.MemberCount)
          .Select(i => outlookDistList.Inner.GetMember(i))
          .ToSafeEnumerable()
          .Select(d => d.Address)
          .ToArray();

        CollectionAssert.AreEquivalent(
          new[] { "mason@bla.com", "steinberg@bla.com" },
          outlookMembers);

        outlookDistList.Inner.DLName = "All";
        var recipient = _testComponentContainer.Application.Session.CreateRecipient("Baxter");
        Assert.That(recipient.Resolve(), Is.True);
        outlookDistList.Inner.AddMember(recipient);
        outlookDistList.Inner.Save();
      }

      await synchronizer.SynchronizeAndAssertNoErrors();

      var serverDistList = (await synchronizer.ServerVCardGroupsOrNull.GetAllEntities()).SingleOrDefault()?.Entity;
      Assert.That(serverDistList, Is.Not.Null);

      Assert.That(serverDistList.FormattedName, Is.EqualTo("All"));

      CollectionAssert.AreEquivalent(
        new[] { masonUid, nihilUid, steinbergUid },
        serverDistList.Members.Select(m => m.Uid));

    }

    private async Task<ContactTestSynchronizer> CreateSynchronizer(Options options)
    {
      var synchronizer = new ContactTestSynchronizer(options, _testComponentContainer);
      await synchronizer.Initialize();
      return synchronizer;
    }

  }


}
