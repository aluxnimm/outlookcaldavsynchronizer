using System;
using CalDavSynchronizer.Generic.ProgressReport;

namespace CalDavSynchronizer.Ui
{
  internal class ProgressFormFactory : IProgressUiFactory
  {
    public IProgressUi Create (int maxValue)
    {
      var form = new ProgressForm();
      form.SetMaximun (maxValue);
      form.Show();
      return form;
    }
  }
}