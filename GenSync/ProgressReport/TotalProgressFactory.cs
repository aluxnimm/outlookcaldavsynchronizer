using System;
using System.Reflection;
using log4net;

namespace GenSync.ProgressReport
{
  public class TotalProgressFactory : ITotalProgressFactory
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);
    private readonly IProgressUiFactory _progressUiFactory;
    private readonly int _loadOperationThresholdForProgressDisplay;
    private readonly IExceptionLogger _exceptionLogger;


    public TotalProgressFactory (IProgressUiFactory progressUiFactory, int loadOperationThresholdForProgressDisplay, IExceptionLogger exceptionLogger)
    {
      _progressUiFactory = progressUiFactory;
      _loadOperationThresholdForProgressDisplay = loadOperationThresholdForProgressDisplay;
      _exceptionLogger = exceptionLogger;
    }

    public ITotalProgressLogger Create ()
    {
      return new TotalProgressContext (_progressUiFactory, _loadOperationThresholdForProgressDisplay, _exceptionLogger);
    }
  }
}