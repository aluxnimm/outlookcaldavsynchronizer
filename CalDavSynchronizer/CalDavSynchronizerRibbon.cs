using System;
using System.Reflection;
using CalDavSynchronizer.Ui;
using CalDavSynchronizer.Utilities;
using log4net;
using Microsoft.Office.Tools.Ribbon;

namespace CalDavSynchronizer
{
  public partial class CalDavSynchronizerRibbon
  {
    private void CalDavSynchronizerRibbon_Load (object sender, RibbonUIEventArgs e)
    {
    }

    private async void SynchronizeNowButton_Click (object sender, RibbonControlEventArgs e)
    {
      await ThisAddIn.ComponentContainer.SynchronizeNowNoThrow();
    }

    private void OptionsButton_Click (object sender, RibbonControlEventArgs e)
    {
      ThisAddIn.ComponentContainer.ShowOptionsNoThrow ();
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