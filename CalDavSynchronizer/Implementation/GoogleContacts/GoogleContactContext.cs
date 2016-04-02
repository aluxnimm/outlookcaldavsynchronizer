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
    private readonly IGoogleApiOperationExecutor _apiOperationExecutor;

    public GoogleContactContext (GoogleGroupCache groupCache, IGoogleApiOperationExecutor apiOperationExecutor, IEqualityComparer<string> contactIdComparer)
    {
      if (groupCache == null)
        throw new ArgumentNullException (nameof (groupCache));
      if (apiOperationExecutor == null)
        throw new ArgumentNullException (nameof (apiOperationExecutor));
      if (contactIdComparer == null)
        throw new ArgumentNullException (nameof (contactIdComparer));

      GroupCache = groupCache;
      _apiOperationExecutor = apiOperationExecutor;
      _contactsById = new Dictionary<string, Contact> (contactIdComparer);
    }

    public async Task FillCaches ()
    {
      await GroupCache.Fill();
      foreach (var contact in await Task.Run (() => _apiOperationExecutor.Execute(f => f.GetContacts().Entries.ToArray())))
      {
        _contactsById.Add (contact.Id, contact);
      }
    }
  }
}