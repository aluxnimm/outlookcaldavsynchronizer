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
  class ChunkProgressLogger : IChunkProgressLogger, IDisposable
  {
    private static readonly ILog s_logger = LogManager.GetLogger(MethodInfo.GetCurrentMethod().DeclaringType);
    private readonly IExceptionLogger _exceptionLogger;
    private readonly IProgressUi _progressUi;

    public ChunkProgressLogger(IProgressUi progressUi, IExceptionLogger exceptionLogger)
    {
      if (progressUi == null) throw new ArgumentNullException(nameof(progressUi));
      if (exceptionLogger == null) throw new ArgumentNullException(nameof(exceptionLogger));

      _progressUi = progressUi;
      _exceptionLogger = exceptionLogger;
    }

    public IDisposable StartARepositoryLoad(int entityCount)
    {
      _progressUi.SetSubMessage($"Loading {entityCount} entities from Outlook...");
      return this;
    }

    public IDisposable StartBRepositoryLoad(int entityCount)
    {
      _progressUi.SetSubMessage($"Loading {entityCount} entities from Server...");
      return this;
    }

    public IProgressLogger StartProcessing(int jobCount)
    {
      _progressUi.SetSubMessage($"Executing {jobCount} operations...");
      return new ProgressLogger(_progressUi, _exceptionLogger);
    }

    public void Dispose()
    {
      try
      {
        _progressUi.IncrementValue();
      }
      catch (Exception x)
      {
        _exceptionLogger.LogException(x, s_logger);
      }
    }
  }
}