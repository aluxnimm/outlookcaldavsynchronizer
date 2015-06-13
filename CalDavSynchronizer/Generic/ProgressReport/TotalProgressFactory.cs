using System;
using System.Reflection;
using CalDavSynchronizer.Utilities;
using log4net;

namespace CalDavSynchronizer.Generic.ProgressReport
{
  internal class TotalProgressFactory : ITotalProgressFactory
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);
    private readonly IProgressUiFactory _progressUiFactory;
    private readonly int _loadOperationThresholdForProgressDisplay;

    public TotalProgressFactory (IProgressUiFactory progressUiFactory, int loadOperationThresholdForProgressDisplay)
    {
      _progressUiFactory = progressUiFactory;
      _loadOperationThresholdForProgressDisplay = loadOperationThresholdForProgressDisplay;
    }

    public ITotalProgressLogger Create ()
    {
      return new TotalProgressContext (_progressUiFactory, _loadOperationThresholdForProgressDisplay);
    }
  }
}