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

    public GoogleContactContextFactory (ContactsRequest contactFacade)
    {
      _contactFacade = contactFacade;
    }

    public Task<GoogleContactContext> Create ()
    {
      return Task.FromResult(new GoogleContactContext(new GoogleGroupCache(_contactFacade)));
    }
  }
}
