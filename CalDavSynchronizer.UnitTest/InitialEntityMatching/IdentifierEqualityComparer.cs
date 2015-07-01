using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalDavSynchronizer.UnitTest.InitialEntityMatching
{
  internal class IdentifierEqualityComparer<T> : IEqualityComparer<Identifier<T>>
  {
    public bool Equals (Identifier<T> x, Identifier<T> y)
    {
      return x.Value.Equals (y.Value);
    }

    public int GetHashCode (Identifier<T> obj)
    {
      return obj.GetHashCode();
    }
  }
}