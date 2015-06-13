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

    public ITotalProgressLogger Create (int aLoadCount, int bLoadCount)
    {
      try
      {
        if (aLoadCount + bLoadCount > _loadOperationThresholdForProgressDisplay)
          return new TotalProgressLogger (_progressUiFactory, aLoadCount, bLoadCount);
        else
          return NullTotalProgressLogger.Instance;
      }
      catch (Exception x)
      {
        ExceptionHandler.Instance.LogException (x, s_logger);
        return NullTotalProgressLogger.Instance;
      }
    }
  }
}