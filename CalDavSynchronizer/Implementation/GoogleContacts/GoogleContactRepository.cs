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

    public GoogleContactRepository (ContactsRequest contactFacade)
    {
      if (contactFacade == null)
        throw new ArgumentNullException (nameof (contactFacade));

      _contactFacade = contactFacade;
    }

    public Task Delete (string entityId, string version)
    {
      // calculate url
      // https://developers.google.com/google-apps/contacts/v3/#retrieving_contacts_using_query_parameters
      // Contact contact = cr.Retrieve<Contact>("https://www.google.com/m8/feeds/contacts/default/full/contactId");

      Uri feedUri = new Uri (ContactsQuery.CreateContactsUri ("default"));
      var contactUrl = new Uri (feedUri, entityId);
      return Task.Run (() => _contactFacade.Delete (contactUrl, version));
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
      return Task.Run(() =>
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
      return Task.Run (() => (IReadOnlyList<EntityVersion<string, string>>) _contactFacade.GetContacts().Entries.Select (c => EntityVersion.Create (c.Id, c.ETag)).ToArray());
    }

    public Task<IReadOnlyList<EntityWithId<string, Contact>>> Get (ICollection<string> ids, ILoadEntityLogger logger)
    {
      return Task.Run (() => (IReadOnlyList<EntityWithId<string, Contact>>) _contactFacade.GetContacts().Entries.Select (c => EntityWithId.Create (c.Id, c)).ToArray());
    }

    public void Cleanup (IReadOnlyDictionary<string, Contact> entities)
    {

    }
  }
}