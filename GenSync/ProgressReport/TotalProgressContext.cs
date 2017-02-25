// This file is Part of CalDavSynchronizer (http://outlookcaldavsynchronizer.sourceforge.net/)
// Copyright (c) 2015 Gerhard Zehetbauer
// Copyright (c) 2015 Alexander Nimmervoll
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Reflection;
using log4net;

namespace GenSync.ProgressReport
{
  internal class TotalProgressContext : ITotalProgressLogger
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);
    private readonly IProgressUiFactory _progressUiFactory;
    private readonly int _loadOperationThresholdForProgressDisplay;
    private ITotalProgressLogger _logger;
    private readonly IExceptionLogger _exceptionLogger;

    public TotalProgressContext (IProgressUiFactory progressUiFactory, int loadOperationThresholdForProgressDisplay, IExceptionLogger exceptionLogger)
    {
      _progressUiFactory = progressUiFactory;
      _loadOperationThresholdForProgressDisplay = loadOperationThresholdForProgressDisplay;
      _exceptionLogger = exceptionLogger;
    }

    public void Dispose ()
    {
      (_logger ?? NullTotalProgressLogger.Instance).Dispose();
    }

    public IDisposable StartARepositoryLoad ()
    {
      return (_logger ?? NullTotalProgressLogger.Instance).StartARepositoryLoad();
    }

    public IDisposable StartBRepositoryLoad ()
    {
      return (_logger ?? NullTotalProgressLogger.Instance).StartBRepositoryLoad();
    }

    public IProgressLogger StartProcessing (int entityCount)
    {
      return (_logger ?? NullTotalProgressLogger.Instance).StartProcessing (entityCount);
    }


    public void NotifyLoadCount (int aLoadCount, int bLoadCount)
    {
      if (_logger != null)
        return;

      try
      {
        if (aLoadCount + bLoadCount >= _loadOperationThresholdForProgressDisplay)
          _logger = new TotalProgressLogger (_progressUiFactory, _exceptionLogger);
        else
          _logger = NullTotalProgressLogger.Instance;
      }
      catch (Exception x)
      {
        _exceptionLogger.LogException (x, s_logger);
        _logger = NullTotalProgressLogger.Instance;
      }
    }
  }
}