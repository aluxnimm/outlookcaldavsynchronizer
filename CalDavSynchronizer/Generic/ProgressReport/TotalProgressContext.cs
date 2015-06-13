// This file is Part of CalDavSynchronizer (http://outlookcaldavsynchronizer.sourceforge.net/)
// Copyright (c) 2015 Gerhard Zehetbauer 
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
using CalDavSynchronizer.Utilities;
using log4net;

namespace CalDavSynchronizer.Generic.ProgressReport
{
  internal class TotalProgressContext : ITotalProgressLogger
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);
    private readonly IProgressUiFactory _progressUiFactory;
    private readonly int _loadOperationThresholdForProgressDisplay;
    private ITotalProgressLogger _logger;

    public TotalProgressContext (IProgressUiFactory progressUiFactory, int loadOperationThresholdForProgressDisplay)
    {
      _progressUiFactory = progressUiFactory;
      _loadOperationThresholdForProgressDisplay = loadOperationThresholdForProgressDisplay;
    }

    public void Dispose ()
    {
      (_logger ?? NullTotalProgressLogger.Instance).Dispose();
    }

    public IProgressLogger StartStep (int stepCompletedCount, string stepDescription)
    {
      return (_logger ?? NullTotalProgressLogger.Instance).StartStep (stepCompletedCount, stepDescription);
    }

    public void NotifyLoadCount (int aLoadCount, int bLoadCount)
    {
      if (_logger != null)
        return;

      try
      {
        if (aLoadCount + bLoadCount > _loadOperationThresholdForProgressDisplay)
          _logger = new TotalProgressLogger (_progressUiFactory, aLoadCount, bLoadCount);
        else
          _logger = NullTotalProgressLogger.Instance;
      }
      catch (Exception x)
      {
        ExceptionHandler.Instance.LogException (x, s_logger);
        _logger = NullTotalProgressLogger.Instance;
      }
    }
  }
}