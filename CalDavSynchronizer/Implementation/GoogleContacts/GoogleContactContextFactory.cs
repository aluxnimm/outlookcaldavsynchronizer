using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenSync.Synchronization;
using Google.Contacts;

namespace CalDavSynchronizer.Implementation.GoogleContacts
{
  class GoogleContactContextFactory : ISynchronizationContextFactory<GoogleGroupCache>
  {
    private readonly ContactsRequest _contactFacade;

    public GoogleContactContextFactory (ContactsRequest contactFacade)
    {
      _contactFacade = contactFacade;
    }

    public GoogleGroupCache Create ()
    {
      return new GoogleGroupCache(_contactFacade);
    }
  }
}
