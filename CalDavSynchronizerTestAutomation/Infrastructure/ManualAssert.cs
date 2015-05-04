using System;
using NUnit.Framework;

namespace CalDavSynchronizerTestAutomation.Infrastructure
{
  internal static class ManualAssert
  {
    private static IManualAssertUi _assertUi;

    public static void Initialize (IManualAssertUi value)
    {
      _assertUi = value;
    }

    public static void Assert (string instruction)
    {
      if (!_assertUi.Assert (instruction))
        throw new AssertionException (string.Format ("FAILED: {0}", instruction));
    }
  }
}