using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.Implementation.Contacts;
using CalDavSynchronizer.Implementation.Events;
using CalDavSynchronizer.Implementation.GoogleContacts;
using CalDavSynchronizer.IntegrationTests.Infrastructure;
using CalDavSynchronizer.Scheduling;
using CalDavSynchronizer.Scheduling.ComponentCollectors;
using DDay.iCal;
using GenSync;
using GenSync.EntityRepositories;
using GenSync.Logging;
using GenSync.ProgressReport;
using GenSync.Synchronization;
using NUnit.Framework;
using Thought.vCards;

namespace CalDavSynchronizer.IntegrationTests.TestBase
{
  [TestFixture]
  public class SynchronizerFixtureBase : IntegrationFixtureBase
  {
    protected ComponentContainer ComponentContainer;
    protected SynchronizerFactory SynchronizerFactory;
    protected GeneralOptions GeneralOptions => new GeneralOptionsDataAccess ().LoadOptions ();
    
    [SetUp]
    public void SetUp()
    {
      ComponentContainer = new ComponentContainer(Application, new NullUiServiceFactory(), new InMemoryGeneralOptionsDataAccess());
      SynchronizerFactory = ComponentContainer.GetSynchronizerFactory();
    }

    protected static string[] GetOrCreateGoogleGroups(IGoogleApiOperationExecutor googleApiExecutor, int amount)
    {
      var groupCache = new GoogleGroupCache(googleApiExecutor);
      groupCache.Fill();

      var existingGroupNames = new HashSet<string>(groupCache.Groups.Select(g => g.Title), StringComparer.InvariantCultureIgnoreCase);
      while (existingGroupNames.Count < amount)
      {
        string groupName = "Group";
        for (var groupSuffix = 1; existingGroupNames.Contains(groupName); groupName = $"Group {groupSuffix++}")
          ;

        existingGroupNames.Add(groupName);
        groupCache.GetOrCreateGroup(groupName);
      }

      return existingGroupNames.Take(amount).ToArray();
    }

    protected async Task<Dictionary<string, ContactData>> CreateContactsInOutlook(OutlookContactRepository<IGoogleContactContext> repository, IEnumerable<ContactData> contactDatas)
    {
      var result = new Dictionary<string, ContactData>();

      foreach (var contactData in contactDatas)
      {
        var version = await repository.Create(
          contact =>
          {
            contact.Inner.FirstName = contactData.FirstName;
            contact.Inner.LastName = contactData.LastName;
            contact.Inner.Email1Address = contactData.EmailAddress;
            contact.Inner.Categories = string.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator, contactData.Groups);
            return Task.FromResult(contact);
          },
          NullGoogleContactContext.Instance);

        result.Add(version.Id, contactData);
      }

      return result;
    }

    protected async Task<AppointmentId> CreateEventInOutlook(
      IEntityRepository<AppointmentId, DateTime, AppointmentItemWrapper, IEventSynchronizationContext> repository,
      string subject,
      DateTime start,
      DateTime end)
    {
      return (await repository.Create(
        e =>
        {
          e.Inner.Start = start;
          e.Inner.End = end;
          e.Inner.Subject = subject;
          e.Inner.ReminderSet = false;
          return Task.FromResult(e);
        },
        NullEventSynchronizationContext.Instance)).Id;
    }

    protected async Task<WebResourceName> CreateEventOnServer (
     IEntityRepository<WebResourceName, string, IICalendar, IEventSynchronizationContext> repository,
     string subject,
     DateTime start,
     DateTime end,
     Action<Event> initializer = null)
    {
      return (await repository.Create (
        c =>
        {
          var evt = new Event();
          evt.Start = new iCalDateTime(start);
          evt.End = new iCalDateTime(end);
          evt.Summary = subject;

          initializer?.Invoke(evt);

          c.Events.Add(evt);
          return Task.FromResult (c);
        },
        NullEventSynchronizationContext.Instance)).Id;
    }


    protected IEnumerable<ContactData> CreateTestContactData(IReadOnlyList<string> groupNames, int amount)
    {
      var numberOfDays = Enum.GetValues(typeof(DayOfWeek)).Cast<int>().Max() + 1;

      for (var i = 1; i <= amount; i++)
      {
        yield return new ContactData(
          "Homer" + i,
          ((DayOfWeek) (i % numberOfDays)).ToString(),
          $"homer{i}@blubb.com",
          new[]
          {
            groupNames[(i + 7) % groupNames.Count],
            groupNames[(i + 13) % groupNames.Count]
          });
      }
    }

    public async Task DeleteGoogleContacts(GoogleContactRepository repository, ISynchronizationContextFactory<IGoogleContactContext> googleContactContextFactory)
    {
      System.Diagnostics.Debug.WriteLine(System.Threading.Thread.CurrentThread.ManagedThreadId);

      var context = await googleContactContextFactory.Create();

      var versions = await repository.GetAllVersions(new string[0], context);

      await repository.PerformOperations(
        new ICreateJob<string, GoogleContactVersion, GoogleContactWrapper>[0],
        new IUpdateJob<string, GoogleContactVersion, GoogleContactWrapper>[0],
        versions.Select(v => new TestDeleteJob<string, GoogleContactVersion>(v)).ToArray(),
        NullProgressLogger.Instance,
        context);
    }

 
    public async Task DeleteCalDavEvents (IEntityRepository<WebResourceName, string, IICalendar, IEventSynchronizationContext> repository)
    {
      foreach (var version in await repository.GetAllVersions (new WebResourceName[0], NullEventSynchronizationContext.Instance))
        await repository.TryDelete (version.Id, version.Version, NullEventSynchronizationContext.Instance);
    }

    public async Task DeleteOutlookEvents (IEntityRepository<AppointmentId, DateTime, AppointmentItemWrapper, IEventSynchronizationContext> repository)
    {
      foreach (var version in await repository.GetAllVersions (new AppointmentId[0], NullEventSynchronizationContext.Instance))
        await repository.TryDelete (version.Id, version.Version, NullEventSynchronizationContext.Instance);
    }

    public async Task DeleteOutlookContacts(OutlookContactRepository<IGoogleContactContext> repository)
    {
      foreach (var version in await repository.GetAllVersions(new string[0], NullGoogleContactContext.Instance))
        await repository.TryDelete(version.Id, version.Version, NullGoogleContactContext.Instance);
    }

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


    protected static Options GetOptions(string profileName)
    {
      var applicationDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CalDavSynchronizer");

      var optionsDataAccess = new OptionsDataAccess(
        Path.Combine(
          applicationDataDirectory,
          ComponentContainer.GetOrCreateConfigFileName(applicationDataDirectory, "Outlook")
        ));

      var options = optionsDataAccess.Load().Single(o => o.Name == profileName);
      return options;
    }

    protected void ClearCache (Options options)
    {
      var profileDataDirectory = ComponentContainer.GetProfileDataDirectory (options.Id);
      if (Directory.Exists (profileDataDirectory))
        Directory.Delete (profileDataDirectory, true);
    }

    protected async Task ClearEventRepositoriesAndCache (Options options)
    {
      var tempOptions = options.Clone ();

      tempOptions.IgnoreSynchronizationTimeRange = true;
      var components = (AvailableEventSynchronizerComponents) (await SynchronizerFactory.CreateSynchronizerWithComponents (tempOptions, GeneralOptions)).Item2;

      await DeleteOutlookEvents (components.OutlookEventRepository);
      await DeleteCalDavEvents (components.CalDavRepository);
      ClearCache (tempOptions);
    }

    protected async Task<IReadOnlyList<EntityWithId<TEntityId, TEntity>>> GetAllEntities<TEntityId, TEntityVersion, TEntity, TContext>(
      IEntityRepository<TEntityId, TEntityVersion, TEntity, TContext> repository,
      TContext context)
    {
      return await repository.Get(
        (await repository.GetAllVersions(new TEntityId[0], context)).Select(v => v.Id).ToArray(),
        NullLoadEntityLogger.Instance,
        context);
    }
  }
}