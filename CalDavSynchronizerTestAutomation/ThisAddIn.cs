using System;
using Microsoft.Office.Tools.Ribbon;
using Office = Microsoft.Office.Core;

namespace CalDavSynchronizerTestAutomation
{
  public partial class ThisAddIn
  {
    private void ThisAddIn_Startup (object sender, System.EventArgs e)
    {
    }

    private void ThisAddIn_Shutdown (object sender, System.EventArgs e)
    {
    }


    protected override IRibbonExtension[] CreateRibbonObjects ()
    {
      return new IRibbonExtension[] { new CalDavSynchronizerTestRibbon() };
    }

    #region VSTO generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InternalStartup ()
    {
      this.Startup += new System.EventHandler (ThisAddIn_Startup);
      this.Shutdown += new System.EventHandler (ThisAddIn_Shutdown);
    }

    #endregion
  }
}