using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using GenSync;
using GenSync.EntityRepositories;
using GenSync.Logging;
using GenSync.ProgressReport;
using Google.Contacts;
using Google.GData.Client;
using Google.GData.Contacts;
using log4net;

namespace CalDavSynchronizer.Implementation.GoogleContacts
{
  class GoogleContactRepository : IReadOnlyEntityRepository<Contact, string, string, int>, IBatchWriteOnlyEntityRepository<Contact, string, string, int>
  {
    private static readonly ILog s_logger = LogManager.GetLogger (System.Reflection.MethodBase.GetCurrentMethod ().DeclaringType);

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

    public async Task PerformOperations (
      IEnumerable<ICreateJob<Contact, string, string>> createJobs,
      IEnumerable<IUpdateJob<Contact, string, string>>
      updateJobs, IEnumerable<IDeleteJob<string, string>> deleteJobs, 
      IProgressLogger progressLogger,
      int context)
    {
      try
      {
        await RunCreateBatch (createJobs);
      }
      catch (Exception x)
      {
        s_logger.Error (null, x);
      }
      try
      {
        await RunUpdateBatch (updateJobs);
      }
      catch (Exception x)
      {
        s_logger.Error (null, x);
      }
      try
      {
        await RunDeleteBatch (deleteJobs);
      }
      catch (Exception x)
      {
        s_logger.Error (null, x);
      }
    }

    private async Task RunCreateBatch (IEnumerable<ICreateJob<Contact, string, string>> jobs)
    {
      var requests = new List<Contact>();

      var jobList = jobs.ToList();
      foreach (var job in jobList)
      {
        try
        {
          var contact = job.InitializeEntity (new Contact());
          contact.BatchData = new GDataBatchEntryData (GDataBatchOperationType.insert);
          requests.Add (contact);
        }
        catch (Exception x)
        {
          job.NotifyOperationFailed (x);
        }
      }

      var responses = await Task.Run (() =>
      {
        var result = ExecuteChunked (
            new List<Contact>(),
            requests.ToList(),
            () => new List<Contact>(),
            (contact, contactList) => { contactList.Add (contact); },
            (contactList, r) =>
            {
              var contactsFeed = _contactFacade.Batch (
                  contactList,
                  new Uri ("https://www.google.com/m8/feeds/contacts/default/full/batch"),
                  GDataBatchOperationType.insert);

              r.AddRange (contactsFeed.Entries);
            });
        return result;
      });

      for (var i = 0; i < responses.Count; i++)
      {
        var contact = responses[i];

        if (contact.BatchData.Status.Reason == "Created")
        {
          jobList[i].NotifyOperationSuceeded (EntityVersion.Create (contact.Id, contact.ETag));
        }
        else
        {
          var sw = new StringWriter ();
          using (var writer = new XmlTextWriter (sw))
            contact.BatchData.Save (writer);
          jobList[i].NotifyOperationFailed (sw.GetStringBuilder ().ToString ());
        }
      }
    }

    private async Task RunUpdateBatch (IEnumerable<IUpdateJob<Contact, string, string>> jobs)
    {
      var jobsById = new Dictionary<string, IUpdateJob<Contact, string, string>>();
      var requests = new List<Contact>();

      foreach (var job in jobs)
      {
        try
        {
          var updatedContact = job.UpdateEntity (job.EntityToUpdate);
          updatedContact.BatchData = new GDataBatchEntryData (GDataBatchOperationType.update);
          requests.Add (updatedContact);
          jobsById.Add (job.EntityId, job);

        }
        catch (Exception x)
        {
          job.NotifyOperationFailed (x);
        }
      }

      var responses = await Task.Run (() =>
      {
        var result = ExecuteChunked (
            new List<Contact>(),
            requests.ToList(),
            () => new List<Contact>(),
            (contact, contactList) => { contactList.Add (contact); },
            (contactList, r) =>
            {
              var contactsFeed = _contactFacade.Batch (
                  contactList,
                  new Uri ("https://www.google.com/m8/feeds/contacts/default/full/batch"),
                  GDataBatchOperationType.update);

              r.AddRange (contactsFeed.Entries);
            });
        return result;
      });

      foreach (var contact in responses)
      {
        IUpdateJob<Contact, string, string> job;
        if (jobsById.TryGetValue (contact.Id, out job))
        {
          if (contact.BatchData.Status.Reason == "Success")
          {
            job.NotifyOperationSuceeded (EntityVersion.Create (contact.Id, contact.ETag));
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
    }

    private async Task RunDeleteBatch (IEnumerable<IDeleteJob<string, string>> jobs)
    {
      var jobsById = new Dictionary<string, IDeleteJob<string, string>>();
      var requests = new List<Contact>();

      foreach (var job in jobs)
      {
        var contact = new Contact();
        contact.Id = GetContactUrl (job.EntityId, ContactsQuery.fullProjection).ToString();
        contact.BatchData = new GDataBatchEntryData (GDataBatchOperationType.delete);
        contact.BatchData.Id = "delete";
        contact.ETag = job.Version;

        requests.Add (contact);
        jobsById.Add (contact.Id, job);
      }

      var responses = await Task.Run (() =>
      {
        var result = ExecuteChunked (
            new List<Contact>(),
            requests.ToList(),
            () => new List<Contact>(),
            (contact, contactList) => { contactList.Add (contact); },
            (contactList, r) =>
            {
              var contactsFeed = _contactFacade.Batch (
                  contactList,
                  new Uri ("https://www.google.com/m8/feeds/contacts/default/full/batch"),
                  GDataBatchOperationType.delete);

              r.AddRange (contactsFeed.Entries);
            });
        return result;
      });

      foreach (var contact in responses)
      {
        IDeleteJob<string, string> job;
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

    public Task<IReadOnlyList<EntityWithId<string, Contact>>> Get (ICollection<string> ids, ILoadEntityLogger logger, int context)
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