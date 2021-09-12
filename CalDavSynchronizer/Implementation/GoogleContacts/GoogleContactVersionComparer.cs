using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CalDavSynchronizer.Implementation.GoogleContacts
{
    public class GoogleContactVersionComparer : IEqualityComparer<GoogleContactVersion>
    {
        public bool Equals(GoogleContactVersion x, GoogleContactVersion y)
        {
            return object.Equals(x.ContactEtag, y.ContactEtag);
        }

        public int GetHashCode(GoogleContactVersion obj)
        {
            return obj?.ContactEtag?.GetHashCode() ?? 0;
        }
    }
}