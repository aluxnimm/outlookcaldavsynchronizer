using System;
using System.Reflection;
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

    public ITotalProgress Create (int aLoadCount, int bLoadCount)
    {
      try
      {
        if (aLoadCount + bLoadCount > _loadOperationThresholdForProgressDisplay)
          return new TotalProgress (_progressUiFactory, aLoadCount, bLoadCount);
        else
          return NullTotalProgress.Instance;
      }
      catch (Exception x)
      {
        s_logger.Error (null, x);
        return NullTotalProgress.Instance;
      }
    }
  }
}