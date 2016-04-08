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

    public GoogleContactContext (GoogleGroupCache groupCache, IGoogleApiOperationExecutor apiOperationExecutor, IEqualityComparer<string> contactIdComparer, string userName)
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
      _contactsById = new Dictionary<string, Contact> (contactIdComparer);
    }

    public void FillCaches ()
    {
      GroupCache.Fill();

      var query = new ContactsQuery (ContactsQuery.CreateContactsUri (_userName, ContactsQuery.fullProjection));
      query.NumberToRetrieve = int.MaxValue;

      var contacts = _apiOperationExecutor.Execute (f =>
      {
        var contactsFeed = f.Get<Contact> (query);
        return contactsFeed?.Entries.ToArray() ?? new Contact[] { };
      });

      foreach (var contact in contacts)
      {
        _contactsById[contact.Id] = contact;
      }
    }
  }
}