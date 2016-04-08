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
    private readonly string _userName;

    public GoogleContactContextFactory (IGoogleApiOperationExecutor apiOperationExecutor, IEqualityComparer<string> contactIdComparer, string userName)
    {
      if (apiOperationExecutor == null)
        throw new ArgumentNullException (nameof (apiOperationExecutor));
      if (contactIdComparer == null)
        throw new ArgumentNullException (nameof (contactIdComparer));
      if (String.IsNullOrEmpty (userName))
        throw new ArgumentException ("Argument is null or empty", nameof (userName));

      _apiOperationExecutor = apiOperationExecutor;
      _contactIdComparer = contactIdComparer;
      _userName = userName;
    }

    public async Task<GoogleContactContext> Create ()
    {
      return await Task.Run (() =>
      {
        var context = new GoogleContactContext (new GoogleGroupCache (_apiOperationExecutor), _apiOperationExecutor, _contactIdComparer, _userName);
        context.FillCaches();
        return context;
      });
    }
  }
}