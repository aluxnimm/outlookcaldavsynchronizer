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
using System.Collections.Generic;
using GenSync.Synchronization;

namespace GenSync.Logging
{
  public class SynchronizationLogger : ISynchronizationLogger
  {
    private readonly DateTime _startTime;
    private readonly Guid _profileId;
    private readonly String _profileName;
    private readonly List<LoadError> _loadErrors = new List<LoadError>();
    private readonly object _loadErrorsLock = new Object();
    private string _exceptionThatLeadToAbortion;
    private string _aDelta;
    private string _bDelta;

    private readonly EntitySynchronizationLogger _currentSynchronitzationLogger;
    private readonly List<EntitySynchronizationReport> _entitySynchronizationReports = new List<EntitySynchronizationReport>();
    private readonly ILoadEntityLogger _aLoadEntityLogger;
    private readonly ILoadEntityLogger _bLoadEntityLogger;

    public SynchronizationLogger (Guid profileId, string profileName)
    {
      _startTime = DateTime.UtcNow;
      _profileName = profileName;
      _profileId = profileId;
      _currentSynchronitzationLogger = new EntitySynchronizationLogger();
      _currentSynchronitzationLogger.Disposed += CurrentSynchronitzationLogger_Disposed;
      _aLoadEntityLogger = new LoadEntityLogger (_loadErrors, _loadErrorsLock, true);
      _bLoadEntityLogger = new LoadEntityLogger (_loadErrors, _loadErrorsLock, false);
    }

    private void CurrentSynchronitzationLogger_Disposed (object sender, EventArgs e)
    {
      if (_currentSynchronitzationLogger.HasErrorsOrWarnings)
      {
        _entitySynchronizationReports.Add (_currentSynchronitzationLogger.GetReport());
      }
      _currentSynchronitzationLogger.Clear();
    }

    public ILoadEntityLogger ALoadEntityLogger => _aLoadEntityLogger;
    public ILoadEntityLogger BLoadEntityLogger => _bLoadEntityLogger;

    public void LogAbortedDueToError (Exception exception)
    {
      _exceptionThatLeadToAbortion = exception.ToString();
    }

    public void LogDeltas (VersionDeltaLoginInformation aDeltaLogInfo, VersionDeltaLoginInformation bDeltaLogInfo)
    {
      _aDelta = aDeltaLogInfo.ToString();
      _bDelta = bDeltaLogInfo.ToString();
    }

    public IEntitySynchronizationLogger CreateEntitySynchronizationLogger ()
    {
      // Due to performance optimizations, this implementation reuses the instance
      // of EntitySynchronizationLogger. This means that this Implementation can only be used
      // for Synchronizers, which synchronize the entities subsequently
      return _currentSynchronitzationLogger;
    }


    public SynchronizationReport GetReport ()
    {
      return new SynchronizationReport()
             {
                 ADelta = _aDelta,
                 BDelta = _bDelta,
                 ExceptionThatLeadToAbortion = _exceptionThatLeadToAbortion,
                 LoadErrors = _loadErrors.ToArray(),
                 ProfileId = _profileId,
                 ProfileName = _profileName,
                 StartTime = _startTime,
                 EntitySynchronizationReports = _entitySynchronizationReports.ToArray(),
                 Duration = DateTime.UtcNow - _startTime
             };
    }
  }
}