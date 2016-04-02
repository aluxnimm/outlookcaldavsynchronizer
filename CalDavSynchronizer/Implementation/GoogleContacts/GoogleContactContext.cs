using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Contacts;

namespace CalDavSynchronizer.Implementation.GoogleContacts
{
  public class GoogleContactContext
  {
    public GoogleGroupCache GroupCache { get; }

    public Dictionary<string, Contact> ContactsById => _contactsById;

    private readonly Dictionary<string, Contact> _contactsById;
    private readonly ContactsRequest _contactFacade;

    public GoogleContactContext (GoogleGroupCache groupCache, ContactsRequest contactFacade, IEqualityComparer<string> contactIdComparer)
    {
      if (groupCache == null)
        throw new ArgumentNullException (nameof (groupCache));
      if (contactFacade == null)
        throw new ArgumentNullException (nameof (contactFacade));
      if (contactIdComparer == null)
        throw new ArgumentNullException (nameof (contactIdComparer));

      GroupCache = groupCache;
      _contactFacade = contactFacade;
      _contactsById = new Dictionary<string, Contact> (contactIdComparer);
    }

    public async Task FillCaches ()
    {
      await GroupCache.Fill();
      foreach (var contact in await Task.Run (() => _contactFacade.GetContacts().Entries.ToArray()))
      {
        _contactsById.Add (contact.Id, contact);
      }
    }
  }
}