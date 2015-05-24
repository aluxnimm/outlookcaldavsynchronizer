using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using CalDavSynchronizer.Ui;
using CalDavSynchronizer.Utilities;
using log4net;
using Microsoft.Office.Tools.Ribbon;

namespace CalDavSynchronizer
{
  public partial class CalDavSynchronizerRibbon
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private void CalDavSynchronizerRibbon_Load (object sender, RibbonUIEventArgs e)
    {
    }

    private void SynchronizeNowButton_Click (object sender, RibbonControlEventArgs e)
    {
      try
      {
        s_logger.Info ("Synchronization manually triggered");
        ThisAddIn.Scheduler.RunNow();
      }
      catch (Exception x)
      {
        ExceptionHandler.Instance.HandleException (x, s_logger);
      }
    }

    private void OptionsButton_Click (object sender, RibbonControlEventArgs e)
    {
      try
      {
        var options = ThisAddIn.OptionsDataAccess.LoadOptions();
        if (OptionsForm.EditOptions (ThisAddIn.Session, options, out options))
        {
          ThisAddIn.OptionsDataAccess.SaveOptions (options);
          ThisAddIn.Scheduler.SetOptions (options);
        }
      }
      catch (Exception x)
      {
        ExceptionHandler.Instance.HandleException (x, s_logger);
      }
    }

    private void AboutButton_Click (object sender, RibbonControlEventArgs e)
    {
      using (var aboutForm = new AboutForm())
      {
        aboutForm.ShowDialog();
      }
    }
  }
}