using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenSync;
using Google.Contacts;
using Google.GData.Client;
using Google.GData.Contacts;

namespace CalDavSynchronizer.Implementation.GoogleContacts
{
  public class GoogleContactContext
  {
    public GoogleGroupCache GroupCache { get; }

    public Dictionary<string, Contact> ContactsById => _contactsById;

    private readonly Dictionary<string, Contact> _contactsById;
    private readonly IGoogleApiOperationExecutor _apiOperationExecutor;
    private readonly string _userName;
    private readonly int _chunkSize;

    public GoogleContactContext (GoogleGroupCache groupCache, IGoogleApiOperationExecutor apiOperationExecutor, IEqualityComparer<string> contactIdComparer, string userName, int chunkSize)
    {
      if (groupCache == null)
        throw new ArgumentNullException (nameof (groupCache));
      if (apiOperationExecutor == null)
        throw new ArgumentNullException (nameof (apiOperationExecutor));
      if (contactIdComparer == null)
        throw new ArgumentNullException (nameof (contactIdComparer));
      if (String.IsNullOrEmpty (userName))
        throw new ArgumentException ("Argument is null or empty", nameof (userName));

      GroupCache = groupCache;
      _apiOperationExecutor = apiOperationExecutor;
      _userName = userName;
      _chunkSize = chunkSize;
      _contactsById = new Dictionary<string, Contact> (contactIdComparer);
    }

    public void FillCaches()
    {
      GroupCache.Fill();

      var query = new ContactsQuery(ContactsQuery.CreateContactsUri(_userName, ContactsQuery.fullProjection));
      query.StartIndex = 0;
      query.NumberToRetrieve = _chunkSize;

      if (GroupCache.DefaultGroupIdOrNull != null)
        query.Group = GroupCache.DefaultGroupIdOrNull;

      for (
        var contactsFeed = _apiOperationExecutor.Execute(f => f.Get<Contact>(query));
        contactsFeed != null;
        contactsFeed = _apiOperationExecutor.Execute(f => f.Get(contactsFeed, FeedRequestType.Next)))
      {
        foreach (Contact contact in contactsFeed.Entries)
        {
          _contactsById[contact.Id] = contact;
        }
      }

    }
  }
}