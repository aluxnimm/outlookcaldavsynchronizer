using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace CalDavSynchronizerTestAutomation.Infrastructure
{
  internal class TestRunner
  {
    private ITestDisplay _testDisplay;

    public TestRunner (ITestDisplay testDisplay)
    {
      if (testDisplay == null)
        throw new ArgumentNullException ("testDisplay");

      _testDisplay = testDisplay;
    }

    public void Run (Assembly testAssembly)
    {
      var testFixtures = testAssembly.GetTypes().Where (t => t.GetCustomAttributes (typeof (TestFixtureAttribute), true).Any());
      foreach (var testFixture in testFixtures)
        Run (testFixture);
    }

    private void Run (Type testFixture)
    {
      var tests = testFixture.GetMethods().Where (m => m.GetCustomAttributes (typeof (TestAttribute), true).Any());
      var testFixtureInstance = testFixture.GetConstructor (Type.EmptyTypes).Invoke (new object[] { });
      foreach (var test in tests)
        Run (testFixtureInstance, test);
    }

    private void Run (object testFixture, MethodInfo test)
    {
      try
      {
        test.Invoke (testFixture, new object[] { });
        _testDisplay.AddPassed (test);
      }
      catch (TargetInvocationException x)
      {
        _testDisplay.AddFailed (test, x.InnerException);
      }
    }
  }
}