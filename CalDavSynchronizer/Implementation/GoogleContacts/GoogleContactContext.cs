using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalDavSynchronizer.Implementation.GoogleContacts
{
  public class GoogleContactContext
  {
    public GoogleGroupCache GroupCache { get; }

    public GoogleContactContext (GoogleGroupCache groupCache)
    {
      if (groupCache == null)
        throw new ArgumentNullException (nameof (groupCache));

      GroupCache = groupCache;
    }
  }
}
