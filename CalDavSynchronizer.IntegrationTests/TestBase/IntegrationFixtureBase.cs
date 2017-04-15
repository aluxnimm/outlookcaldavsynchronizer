using System;
using System.Runtime.InteropServices;
using Microsoft.Office.Interop.Outlook;
using NUnit.Framework;

namespace CalDavSynchronizer.IntegrationTests.TestBase
{
  [TestFixture]
  public class IntegrationFixtureBase
  {
    protected Application Application { get; private set; }

    [OneTimeSetUp]
    public void Init()
    {
      Application = new Application();
      Application.Session.Logon();
    }

    [OneTimeTearDown]
    public void Dispose()
    {
      try
      {
        Application.Session.Logoff();
      }
      finally
      {
        Marshal.FinalReleaseComObject(Application);
        Application = null;

        GC.Collect();
        GC.WaitForPendingFinalizers();
      }
    }
  }
}