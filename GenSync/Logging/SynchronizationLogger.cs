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
using System.Linq;
using GenSync.Synchronization;

namespace GenSync.Logging
{
  public class SynchronizationLogger : ISynchronizationLogger, ISynchronizationReportSink
  {
    private readonly DateTime _startTime;
    private readonly Guid _profileId;
    private readonly String _profileName;
    private readonly List<LoadError> _loadErrors = new List<LoadError>();
    private readonly object _loadErrorsLock = new Object();
    private string _exceptionThatLeadToAbortion;
    private bool _considerExceptionThatLeadToAbortionAsWarning;
    private string _aDelta;
    private string _bDelta;
    private string _aJobsInfo;
    private string _bJobsInfo;
    private ISynchronizationReportSink _reportSink;
    private List<SynchronizationReport> _subReports;
    private readonly bool _includeEntityReportsWithoutErrorsOrWarnings;

    private readonly List<IEntitySynchronizationLog> _entitySynchronizationLogs = new List<IEntitySynchronizationLog> ();
    private readonly LoadEntityLogger _aLoadEntityLogger;
    private readonly LoadEntityLogger _bLoadEntityLogger;

    public SynchronizationLogger (Guid profileId, string profileName, ISynchronizationReportSink reportSink, bool includeEntityReportsWithoutErrorsOrWarnings)
    {
      _startTime = DateTime.UtcNow;
      _profileName = profileName;
      _reportSink = reportSink;
      _includeEntityReportsWithoutErrorsOrWarnings = includeEntityReportsWithoutErrorsOrWarnings;
      _profileId = profileId;
      _aLoadEntityLogger = new LoadEntityLogger (_loadErrors, _loadErrorsLock, true);
      _bLoadEntityLogger = new LoadEntityLogger (_loadErrors, _loadErrorsLock, false);
    }
    
    public ILoadEntityLogger ALoadEntityLogger => _aLoadEntityLogger;
    public ILoadEntityLogger BLoadEntityLogger => _bLoadEntityLogger;

    public IGetVersionsLogger AGetVersionsEntityLogger => _aLoadEntityLogger;
    public IGetVersionsLogger BGetVersionsEntityLogger => _bLoadEntityLogger;

    public void LogAbortedDueToError (Exception exception)
    {
      _exceptionThatLeadToAbortion = exception.ToString();
      _considerExceptionThatLeadToAbortionAsWarning = false;
    }

    public void LogAbortedDueToWarning (Exception exception)
    {
      _exceptionThatLeadToAbortion = exception.ToString ();
      _considerExceptionThatLeadToAbortionAsWarning = true;
    }

    public void LogDeltas (VersionDeltaLoginInformation aDeltaLogInfo, VersionDeltaLoginInformation bDeltaLogInfo)
    {
      _aDelta = aDeltaLogInfo.ToString();
      _bDelta = bDeltaLogInfo.ToString();
    }

    public void LogJobs (string aJobsInfo, string bJobsInfo)
    {
      _aJobsInfo = aJobsInfo;
      _bJobsInfo = bJobsInfo;
    }

    public void AddEntitySynchronizationLog (IEntitySynchronizationLog log)
    {
      _entitySynchronizationLogs.Add (log);
    }

    private SynchronizationReport GetReport ()
    {
      var report = new SynchronizationReport
      {
        ADelta = _aDelta,
        BDelta = _bDelta,
        AJobsInfo = _aJobsInfo,
        BJobsInfo = _bJobsInfo,
        ExceptionThatLeadToAbortion = _exceptionThatLeadToAbortion,
        ConsiderExceptionThatLeadToAbortionAsWarning = _considerExceptionThatLeadToAbortionAsWarning,
        LoadErrors = _loadErrors.ToArray(),
        ProfileId = _profileId,
        ProfileName = _profileName,
        StartTime = _startTime,
        EntitySynchronizationReports = _entitySynchronizationLogs
          .Where(l => _includeEntityReportsWithoutErrorsOrWarnings || l.HasErrorsOrWarnings)
          .Select(l => l.GetReport())
          .ToArray(),
        Duration = DateTime.UtcNow - _startTime
      };

      if(_subReports != null)
        report.MergeSubReport(_subReports);

      return report;
    }

    public void Dispose()
    {
      _reportSink?.PostReport(GetReport());
      _reportSink = null;
    }

    public void PostReport(SynchronizationReport report)
    {
      if (_subReports == null)
        _subReports = new List<SynchronizationReport>();
      _subReports.Add(report);
    }

    public ISynchronizationLogger CreateSubLogger(string subProfileName)
    {
      return new SynchronizationLogger(_profileId, subProfileName, this, _includeEntityReportsWithoutErrorsOrWarnings);
    }
  }
}