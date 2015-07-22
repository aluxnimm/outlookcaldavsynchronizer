using System;
using System.Collections.Generic;

namespace GenSync.UnitTests.InitialEntityMatching
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