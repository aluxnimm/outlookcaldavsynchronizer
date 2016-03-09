using System;

namespace CalDavSynchronizer.Ui.Options.ViewModels
{
  public class CloseEventArgs : EventArgs
  {
    public CloseEventArgs (bool shouldSaveNewOptions)
    {
      ShouldSaveNewOptions = shouldSaveNewOptions;
    }

    public bool ShouldSaveNewOptions { get; }
  }
}