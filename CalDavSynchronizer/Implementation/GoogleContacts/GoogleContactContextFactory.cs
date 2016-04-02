using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenSync.Synchronization;
using Google.Contacts;

namespace CalDavSynchronizer.Implementation.GoogleContacts
{
  class GoogleContactContextFactory : ISynchronizationContextFactory<GoogleContactContext>
  {
    private readonly ContactsRequest _contactFacade;
    private readonly IEqualityComparer<string> _contactIdComparer;

    public GoogleContactContextFactory (ContactsRequest contactFacade, IEqualityComparer<string> contactIdComparer)
    {
      if (contactFacade == null)
        throw new ArgumentNullException (nameof (contactFacade));
      if (contactIdComparer == null)
        throw new ArgumentNullException (nameof (contactIdComparer));

      _contactFacade = contactFacade;
      _contactIdComparer = contactIdComparer;
    }

    public async Task<GoogleContactContext> Create ()
    {
      var context = await Task.FromResult(new GoogleContactContext(new GoogleGroupCache(_contactFacade), _contactFacade, _contactIdComparer));
      await context.FillCaches();
      return context;
    }
  }
}
