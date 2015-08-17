using System;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using CalDavSynchronizerTestAutomation.Infrastructure;
using Microsoft.Office.Tools.Ribbon;

namespace CalDavSynchronizerTestAutomation
{
  public partial class CalDavSynchronizerTestRibbon
  {
    private void TestAutomationRibbon_Load (object sender, RibbonUIEventArgs e)
    {
    }

    private void StartTestsButton_Click (object sender, RibbonControlEventArgs e)
    {
      EnsureSynchronizationContext();
      var display = new TestResultDisplay();
      display.Show();
      var runner = new TestRunner (display);
      ManualAssert.Initialize (display);
      OutlookTestContext.Initialize (Globals.ThisAddIn.Application.Session);
      runner.Run (Assembly.GetExecutingAssembly());
    }

    /// <summary>
    /// Ensures that the syncronizationcontext is not null ( it seems to be a bug that the synchronizationcontext is null in Office Addins)
    /// </summary>
    public static void EnsureSynchronizationContext ()
    {
      if (System.Threading.SynchronizationContext.Current == null)
      {
        System.Threading.SynchronizationContext.SetSynchronizationContext (new WindowsFormsSynchronizationContext ());
      }
    }
  }
}