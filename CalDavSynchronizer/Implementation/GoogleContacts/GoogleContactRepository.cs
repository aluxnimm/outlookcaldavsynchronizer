using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.Utilities;
using GenSync;
using GenSync.EntityRepositories;
using GenSync.Logging;
using GenSync.ProgressReport;
using GenSync.Utilities;
using Google.Contacts;
using Google.GData.Client;
using Google.GData.Contacts;
using log4net;

namespace CalDavSynchronizer.Implementation.GoogleContacts
{
  public class GoogleContactRepository 
    : IVersionAwareReadOnlyEntityRepository<string, GoogleContactVersion, GoogleContactWrapper, IGoogleContactContext>,
    IBatchWriteOnlyEntityRepository<string, GoogleContactVersion, GoogleContactWrapper, IGoogleContactContext>
  {
    private static readonly ILog s_logger = LogManager.GetLogger (System.Reflection.MethodBase.GetCurrentMethod ().DeclaringType);

    private readonly IGoogleApiOperationExecutor _apiOperationExecutor;
    private readonly string _userName;
    private readonly ContactMappingConfiguration _contactMappingConfiguration;
    private readonly IEqualityComparer<string> _contactIdComparer;
    private readonly IChunkedExecutor _writeOperationExecutor;
    private readonly IChunkedExecutor _readOperationExecutor;

    public GoogleContactRepository (IGoogleApiOperationExecutor apiOperationExecutor, string userName, ContactMappingConfiguration contactMappingConfiguration, IEqualityComparer<string> contactIdComparer, IChunkedExecutor writeOperationExecutor, IChunkedExecutor readOperationExecutor)
    {
      if (apiOperationExecutor == null)
        throw new ArgumentNullException (nameof (apiOperationExecutor));
      if (contactMappingConfiguration == null)
        throw new ArgumentNullException (nameof (contactMappingConfiguration));
      if (contactIdComparer == null)
        throw new ArgumentNullException (nameof (contactIdComparer));
      if (writeOperationExecutor == null) throw new ArgumentNullException(nameof(writeOperationExecutor));
      if (readOperationExecutor == null) throw new ArgumentNullException(nameof(readOperationExecutor));
      if (String.IsNullOrEmpty (userName))
        throw new ArgumentException ("Argument is null or empty", nameof (userName));

      _userName = userName;
      _contactMappingConfiguration = contactMappingConfiguration;
      _contactIdComparer = contactIdComparer;
      _writeOperationExecutor = writeOperationExecutor;
      _readOperationExecutor = readOperationExecutor;
      _apiOperationExecutor = apiOperationExecutor;
    }

    public async Task PerformOperations (
      IReadOnlyList<ICreateJob<string, GoogleContactVersion, GoogleContactWrapper>> createJobs,
      IReadOnlyList<IUpdateJob<string, GoogleContactVersion, GoogleContactWrapper>> updateJobs,
      IReadOnlyList<IDeleteJob<string, GoogleContactVersion>> deleteJobs, 
      IProgressLogger progressLogger,
      IGoogleContactContext context)
    {
      var createRequestsAndJobs = await CreateCreateRequests (createJobs);
      var updateRequestsAndJobs = await CreateUpdateRequests (updateJobs);

      await Task.Run (() =>
      {
        AssignGroupsToContacts (createRequestsAndJobs.Item1, context.GroupCache);
        AssignGroupsToContacts (updateRequestsAndJobs.Item1.Values, context.GroupCache);

        try
        {
          RunCreateBatch (createRequestsAndJobs);
        }
        catch (Exception x)
        {
          s_logger.Error (null, x);
        }
        try
        {
          RunUpdateBatch (updateRequestsAndJobs);
        }
        catch (Exception x)
        {
          s_logger.Error (null, x);
        }
        try
        {
          RunDeleteBatch (deleteJobs);
        }
        catch (Exception x)
        {
          s_logger.Error (null, x);
        }
      });
    }

    private static void AssignGroupsToContacts (IEnumerable<GoogleContactWrapper> contacts, IGoogleGroupCache groupCache)
    {
      foreach (var contactWrapper in contacts)
      {
        contactWrapper.Contact.GroupMembership.Clear();
        groupCache.AddDefaultGroupToContact (contactWrapper.Contact);

        foreach (var groupName in contactWrapper.Groups)
        {
          var group = groupCache.GetOrCreateGroup (groupName);
          if (!groupCache.IsDefaultGroup (group))
            contactWrapper.Contact.GroupMembership.Add (new GroupMembership() { HRef = group.Id });
        }
      }
    }

    private void RunCreateBatch (Tuple<List<GoogleContactWrapper>, IReadOnlyList<ICreateJob<string, GoogleContactVersion, GoogleContactWrapper>>> requestsAndJobs)
    {
      if (requestsAndJobs.Item1.Count == 0)
        return;

     var responses = _writeOperationExecutor.Execute (
            new List<Contact>(),
            requestsAndJobs.Item1.Select (i => i.Contact),
            (contactList, r) =>
            {
              var contactsFeed = _apiOperationExecutor.Execute (f => f.Batch (
                  contactList,
                  new Uri ("https://www.google.com/m8/feeds/contacts/default/full/batch"),
                  GDataBatchOperationType.insert));

              r.AddRange (contactsFeed.Entries);
            });

        var requeryEtagJobs = new Dictionary<string, ICreateJob<string, GoogleContactVersion, GoogleContactWrapper>> (_contactIdComparer);

        for (var i = 0; i < responses.Count; i++)
        {
          var contact = responses[i];

          var createJob = requestsAndJobs.Item2[i];
          var request = requestsAndJobs.Item1[i];
          if (contact.BatchData.Status.Code == (int)HttpStatusCode.Created)
          {
            if (UpdatePhoto (contact, request.PhotoOrNull))
              requeryEtagJobs.Add (contact.Id, createJob);
            else
              createJob.NotifyOperationSuceeded (EntityVersion.Create (contact.Id, new GoogleContactVersion { ContactEtag = contact.ETag }));
          }
          else
          {
            var sw = new StringWriter();
            using (var writer = new XmlTextWriter (sw))
              contact.BatchData.Save (writer);
            createJob.NotifyOperationFailed (sw.GetStringBuilder().ToString());
          }
        }

        var contacts = GetContactsFromGoogle (requeryEtagJobs.Keys);
        var contactsById = contacts.ToDictionary (c => c.Id, _contactIdComparer);

        foreach (var requeryEtagJob in requeryEtagJobs)
        {
          Contact contact;
          if (contactsById.TryGetValue (requeryEtagJob.Key, out contact))
            requeryEtagJob.Value.NotifyOperationSuceeded (EntityVersion.Create (contact.Id, new GoogleContactVersion { ContactEtag = contact.ETag }));
          else
            requeryEtagJob.Value.NotifyOperationFailed ("Could not requery etag");
        }
    }

    public IReadOnlyList<Contact> GetContactsFromGoogle (ICollection<string> ids)
    {
      return _readOperationExecutor.Execute (
          new List<Contact>(),
          ids.Select (id =>
          {
            var contact = new Contact();
            contact.Id = GetContactUrl (id, ContactsQuery.fullProjection).ToString();
            contact.BatchData = new GDataBatchEntryData (contact.Id, GDataBatchOperationType.query);
            return contact;
          }),
          (contactList, r) =>
          {
            var contactsFeed = _apiOperationExecutor.Execute (f => f.Batch (
                contactList,
                _apiOperationExecutor.Execute (g => g.GetContacts()),
                GDataBatchOperationType.query));

            if (contactsFeed != null)
              r.AddRange (contactsFeed.Entries);
          });
    }

    private static async Task<Tuple<List<GoogleContactWrapper>, IReadOnlyList<ICreateJob<string, GoogleContactVersion, GoogleContactWrapper>>>> CreateCreateRequests (IReadOnlyList<ICreateJob<string, GoogleContactVersion, GoogleContactWrapper>> jobs)
    {
      var requests = new List<GoogleContactWrapper> ();
      foreach (var job in jobs)
      {
        try
        {
          var contact = await job.InitializeEntity (new GoogleContactWrapper (new Contact ()));
          contact.Contact.BatchData = new GDataBatchEntryData (GDataBatchOperationType.insert);
          requests.Add (contact);
        }
        catch (Exception x)
        {
          job.NotifyOperationFailed (x);
        }
      }

      return Tuple.Create (requests, jobs);
    }

    private void RunUpdateBatch (Tuple<Dictionary<string,GoogleContactWrapper>, Dictionary<string, IUpdateJob<string, GoogleContactVersion, GoogleContactWrapper>>> requestsAndJobs)
    {
      if (requestsAndJobs.Item1.Count == 0)
        return;

     var responses = _writeOperationExecutor.Execute (
            new List<Contact>(),
            requestsAndJobs.Item1.Values.Select(i => i.Contact),
            (contactList, r) =>
            {
              var contactsFeed = _apiOperationExecutor.Execute (f => f.Batch (
                  contactList,
                  new Uri ("https://www.google.com/m8/feeds/contacts/default/full/batch"),
                  GDataBatchOperationType.update));

              r.AddRange (contactsFeed.Entries);
            });
       
      var requeryEtagJobs = new Dictionary<string, IUpdateJob<string, GoogleContactVersion, GoogleContactWrapper>> (_contactIdComparer);

      foreach (var contact in responses)
      {
        IUpdateJob<string, GoogleContactVersion, GoogleContactWrapper> job;
        if (requestsAndJobs.Item2.TryGetValue (contact.Id, out job))
        {
          if (contact.BatchData.Status.Reason == "Success")
          {
            if (UpdatePhoto (contact, requestsAndJobs.Item1[contact.Id].PhotoOrNull))
              requeryEtagJobs.Add (contact.Id, job);
            else
              job.NotifyOperationSuceeded (EntityVersion.Create (contact.Id, new GoogleContactVersion { ContactEtag = contact.ETag }));
          }
          else
          {
            var sw = new StringWriter();
            using (var writer = new XmlTextWriter (sw))
              contact.BatchData.Save (writer);
            job.NotifyOperationFailed (sw.GetStringBuilder().ToString());
          }
        }
      }

      var contacts = GetContactsFromGoogle (requeryEtagJobs.Keys);
      var contactsById = contacts.ToDictionary (c => c.Id, _contactIdComparer);

      foreach (var requeryEtagJob in requeryEtagJobs)
      {
        Contact contact;
        if (contactsById.TryGetValue (requeryEtagJob.Key, out contact))
          requeryEtagJob.Value.NotifyOperationSuceeded (EntityVersion.Create (contact.Id, new GoogleContactVersion { ContactEtag = contact.ETag }));
        else
          requeryEtagJob.Value.NotifyOperationFailed ("Could not requery etag");
      }
    }

    async Task<Tuple<Dictionary<string, GoogleContactWrapper>, Dictionary<string, IUpdateJob<string, GoogleContactVersion, GoogleContactWrapper>>>> CreateUpdateRequests (IEnumerable<IUpdateJob<string, GoogleContactVersion, GoogleContactWrapper>> jobs)
    {
      var jobsById = new Dictionary<string, IUpdateJob<string, GoogleContactVersion, GoogleContactWrapper>>(_contactIdComparer);
      var requestsById = new Dictionary<string, GoogleContactWrapper> (_contactIdComparer);

      foreach (var job in jobs)
      {
        try
        {
          var updatedContact = await job.UpdateEntity (job.EntityToUpdate);
          updatedContact.Contact.BatchData = new GDataBatchEntryData (GDataBatchOperationType.update);
          requestsById.Add (job.EntityId, updatedContact);
          jobsById.Add (job.EntityId, job);

        }
        catch (Exception x)
        {
          job.NotifyOperationFailed (x);
        }
      }

      return Tuple.Create (requestsById, jobsById);
    }

    private void RunDeleteBatch (IReadOnlyList<IDeleteJob<string, GoogleContactVersion>> jobs)
    {
      if (jobs.Count == 0)
        return;

      var jobsById = new Dictionary<string, IDeleteJob<string, GoogleContactVersion>>(_contactIdComparer);
      var requests = new List<Contact>();

      foreach (var job in jobs)
      {
        var contact = new Contact();
        contact.Id = GetContactUrl (job.EntityId, ContactsQuery.fullProjection).ToString();
        contact.BatchData = new GDataBatchEntryData (GDataBatchOperationType.delete);
        contact.BatchData.Id = "delete";
        contact.ETag = job.Version.ContactEtag;

        requests.Add (contact);
        jobsById.Add (job.EntityId, job);
      }

      var responses = _writeOperationExecutor.Execute (
            new List<Contact>(),
            requests,
            (contactList, r) =>
            {
              var contactsFeed = _apiOperationExecutor.Execute (f => f.Batch (
                  contactList,
                  new Uri ("https://www.google.com/m8/feeds/contacts/default/full/batch"),
                  GDataBatchOperationType.delete));

              r.AddRange (contactsFeed.Entries);
            });
     
      foreach (var contact in responses)
      {
        IDeleteJob<string, GoogleContactVersion> job;
        if (jobsById.TryGetValue (contact.Id, out job))
        {
          if (contact.BatchData.Status.Reason == "Success")
          {
            job.NotifyOperationSuceeded ();
          }
          else
          {
            var sw = new StringWriter ();
            using (var writer = new XmlTextWriter (sw))
              contact.BatchData.Save (writer);
            job.NotifyOperationFailed (sw.GetStringBuilder ().ToString ());
          }
        }
      }
    }

    public Task<IEnumerable<EntityVersion<string, GoogleContactVersion>>> GetVersions (IEnumerable<IdWithAwarenessLevel<string>> idsOfEntitiesToQuery, IGoogleContactContext context, IGetVersionsLogger logger)
    {
      var contacts = new List<EntityVersion<string, GoogleContactVersion>>();

      foreach (var id in idsOfEntitiesToQuery)
      {
        Contact contact;
        if (context.ContactCache.TryGetValue (id.Id, out contact))
          contacts.Add (EntityVersion.Create (contact.Id, new GoogleContactVersion { ContactEtag = contact.ETag }));
      }

      return Task.FromResult<IEnumerable<EntityVersion<string, GoogleContactVersion>>> (contacts);
    }

    public Task<IEnumerable<EntityVersion<string, GoogleContactVersion>>> GetAllVersions (IEnumerable<string> idsOfknownEntities, IGoogleContactContext context, IGetVersionsLogger logger)
    {
      return context.ContactCache.GetAllVersions();
    }

    public async Task<IEnumerable<EntityWithId<string, GoogleContactWrapper>>> Get(ICollection<string> ids, ILoadEntityLogger logger, IGoogleContactContext context)
    {
      var result = new List<EntityWithId<string, GoogleContactWrapper>>();
      foreach (var id in ids)
      {
        Contact contact;
        if (context.ContactCache.TryGetValue(id, out contact))
          result.Add(EntityWithId.Create(contact.Id, new GoogleContactWrapper(contact)));
      }

      var groups = context.GroupCache.Groups.ToDictionary(g => g.Id);

      foreach (var contactWrapper in result)
      {
        foreach (var groupMembership in contactWrapper.Entity.Contact.GroupMembership)
        {
          if (!context.GroupCache.IsDefaultGroupId(groupMembership.HRef))
          {
            Group group;
            if (groups.TryGetValue(groupMembership.HRef, out group))
              contactWrapper.Entity.Groups.Add(group.Title);
            else
              s_logger.Warn($"Could not find Group '{groupMembership.HRef}' in the cached groups");
          }
        }
      }

      if (_contactMappingConfiguration.MapContactPhoto)
      {
        await Task.Run(() =>
        {
          foreach (var contactWrapper in result)
          {
            if (contactWrapper.Entity.Contact.PhotoEtag != null)
            {
              using (var photoStream = _apiOperationExecutor.Execute(f => f.Service.Query(contactWrapper.Entity.Contact.PhotoUri)))
              {
                var memoryStream = new MemoryStream();
                photoStream.CopyTo(memoryStream);
                // ReSharper disable once PossibleAssignmentToReadonlyField
                contactWrapper.Entity.PhotoOrNull = memoryStream.ToArray();
              }
            }
          }
        });
      }

      return result;
    }

    public Task VerifyUnknownEntities (Dictionary<string, GoogleContactVersion> unknownEntites, IGoogleContactContext context)
    {
      return Task.FromResult (0);
    }

    bool UpdatePhoto (Contact contact, byte[] photoOrNull)
    {
      if (!_contactMappingConfiguration.MapContactPhoto)
        return false;

      if (photoOrNull != null)
      {
        _apiOperationExecutor.Execute (f => f.SetPhoto (contact, new MemoryStream (photoOrNull)));
        return true;
      }
      else
      {
        if (contact.PhotoEtag != null)
        {
          _apiOperationExecutor.Execute (f => f.Service.Delete (contact.PhotoUri));
          return true;
        }
      }

      return false;
    }

    public void Cleanup(GoogleContactWrapper entity)
    {
      // nothing to do
    }

    public void Cleanup(IEnumerable<GoogleContactWrapper> entities)
    {
      // nothing to do
    }
    
    private Uri GetContactUrl (string entityId, string projection)
    {
      return new Uri (ContactsQuery.CreateContactsUri (_userName, projection) + "/" + new Uri (entityId).Segments.Last());
    }
  }
}