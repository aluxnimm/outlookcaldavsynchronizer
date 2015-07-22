using System;

namespace GenSync.UnitTests.InitialEntityMatching
{
  public struct Identifier<T>
  {
    public readonly T Value;

    public Identifier (T value)
    {
      Value = value;
    }

    // Override equals and return false, to verify if EqualityComparer is used
    public override bool Equals (object obj)
    {
      return false;
    }

    public override int GetHashCode ()
    {
      return 0;
    }
  }
}