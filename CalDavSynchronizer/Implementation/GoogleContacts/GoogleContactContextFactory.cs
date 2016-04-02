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
    private readonly IGoogleApiOperationExecutor _apiOperationExecutor;
    private readonly IEqualityComparer<string> _contactIdComparer;

    public GoogleContactContextFactory (IGoogleApiOperationExecutor apiOperationExecutor, IEqualityComparer<string> contactIdComparer)
    {
      if (apiOperationExecutor == null)
        throw new ArgumentNullException (nameof (apiOperationExecutor));
      if (contactIdComparer == null)
        throw new ArgumentNullException (nameof (contactIdComparer));

      _apiOperationExecutor = apiOperationExecutor;
      _contactIdComparer = contactIdComparer;
    }

    public async Task<GoogleContactContext> Create ()
    {
      var context = await Task.FromResult(new GoogleContactContext(new GoogleGroupCache(_apiOperationExecutor), _apiOperationExecutor, _contactIdComparer));
      await context.FillCaches();
      return context;
    }
  }
}
