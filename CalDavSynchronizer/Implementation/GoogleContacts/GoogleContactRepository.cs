using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenSync;
using GenSync.EntityRepositories;
using GenSync.Logging;
using Google.Contacts;
using Google.GData.Client;
using Google.GData.Contacts;

namespace CalDavSynchronizer.Implementation.GoogleContacts
{
  class GoogleContactRepository : IEntityRepository<Contact, string, string>
  {
    private readonly ContactsRequest _contactFacade;
    private readonly string _userName;

    public GoogleContactRepository (ContactsRequest contactFacade, string userName)
    {
      if (contactFacade == null)
        throw new ArgumentNullException (nameof (contactFacade));
      if (String.IsNullOrEmpty (userName))
        throw new ArgumentException ("Argument is null or empty", nameof (userName));

      _userName = userName;
      _contactFacade = contactFacade;
    }

    public Task Delete (string entityId, string version)
    {
      var httpsUrl = GetContactUrl (entityId, ContactsQuery.fullProjection);

      return Task.Run (() => _contactFacade.Delete (httpsUrl, version));
    }

    public Task<EntityVersion<string, string>> Update (string entityId, string version, Contact entityToUpdate, Func<Contact, Contact> entityModifier)
    {
      var contact = entityModifier (entityToUpdate);
      return Task.Run (() =>
      {
        var updatedContact = _contactFacade.Update (contact);
        return EntityVersion.Create (updatedContact.Id, updatedContact.ETag);
      });
    }

    public Task<EntityVersion<string, string>> Create (Func<Contact, Contact> entityInitializer)
    {
      var contact = entityInitializer (new Contact());
      Uri feedUri = new Uri (ContactsQuery.CreateContactsUri ("default"));
      return Task.Run (() =>
      {
        var newContact = _contactFacade.Insert (feedUri, contact);
        return EntityVersion.Create (newContact.Id, newContact.ETag);
      });
    }

    public async Task<IReadOnlyList<EntityVersion<string, string>>> GetVersions (IEnumerable<IdWithAwarenessLevel<string>> idsOfEntitiesToQuery)
    {
      var idsOfEntitiesToQueryDictionary = idsOfEntitiesToQuery.ToDictionary (i => i.Id);

      return (await GetAllVersions (new string[] { })).Where (v => idsOfEntitiesToQueryDictionary.ContainsKey (v.Id)).ToArray();
    }

    public Task<IReadOnlyList<EntityVersion<string, string>>> GetAllVersions (IEnumerable<string> idsOfknownEntities)
    {
      var query = new ContactsQuery (ContactsQuery.CreateContactsUri (_userName, ContactsQuery.baseProjection));
      query.NumberToRetrieve = int.MaxValue;
      return Task.Run (() =>
      {
        var contactsFeed = _contactFacade.Service.Query (query);
        var contacts = contactsFeed.Entries
            .Cast<ContactEntry>()
            .Select (c => EntityVersion.Create (c.Id.AbsoluteUri.ToString(), c.Etag))
            .ToArray();
        return (IReadOnlyList<EntityVersion<string, string>>) contacts;
      });
    }

    public Task<IReadOnlyList<EntityWithId<string, Contact>>> Get (ICollection<string> ids, ILoadEntityLogger logger)
    {
      return Task.Run (() =>
      {
        var result = ExecuteChunked (
            new List<EntityWithId<string, Contact>>(),
            ids.ToList(),
            () => new List<Contact>(),
            (id, contactList) =>
            {
              var contact = new Contact();
              contact.Id = GetContactUrl (id, ContactsQuery.fullProjection).ToString();
              contact.BatchData = new GDataBatchEntryData (contact.Id, GDataBatchOperationType.query);
              contactList.Add (contact);
            },
            (contactList, r) =>
            {
              var contactsFeed = _contactFacade.Batch (
                  contactList,
                  _contactFacade.GetContacts(),
                  GDataBatchOperationType.query);

              if (contactsFeed != null)
                r.AddRange (contactsFeed.Entries.Select (c => EntityWithId.Create (c.Id, c)));
            });

        return (IReadOnlyList<EntityWithId<string, Contact>>) result;

      });
    }

    private TResult ExecuteChunked<TItem, TChunkLocal, TResult> (
        TResult resultToProcess,
        IReadOnlyList<TItem> items,
        Func<TChunkLocal> chunkLocalFactory,
        Action<TItem, TChunkLocal> processItem,
        Action<TChunkLocal, TResult> processChunk)
    {
      const int maxBatchSize = 100;

      for (int i = 0; i < items.Count; i += maxBatchSize)
      {
        var chunkLocal = chunkLocalFactory();
        var currentBatchEnd = Math.Min (items.Count, i + maxBatchSize);

        for (int k = i; k < currentBatchEnd; k++)
        {
          processItem (items[k], chunkLocal);
        }

        processChunk (chunkLocal, resultToProcess);
      }

      return resultToProcess;
    }


    public void Cleanup (IReadOnlyDictionary<string, Contact> entities)
    {
    }

    private Uri GetContactUrl (string entityId, string projection)
    {
      return new Uri (ContactsQuery.CreateContactsUri (_userName, projection) + "/" + new Uri (entityId).Segments.Last());
    }
  }
}