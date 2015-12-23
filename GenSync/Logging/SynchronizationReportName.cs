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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace GenSync.Logging
{
  public class SynchronizationReportName
  {
    private readonly Guid _syncronizationProfileId;
    private readonly DateTime _startTime;
    private readonly bool _hasErrors;
    private readonly bool _hasWarnings;
    private readonly int _sequenceNumber;

  
    public static SynchronizationReportName Create (Guid syncronizationProfileId, DateTime startTime, bool hasWarnings, bool hasErrors)
    {
      return new SynchronizationReportName (syncronizationProfileId, startTime, hasWarnings, hasErrors, 0);
    }

    private SynchronizationReportName (Guid syncronizationProfileId, DateTime startTime, bool hasWarnings, bool hasErrors, int sequenceNumber)
    {
      _syncronizationProfileId = syncronizationProfileId;
      _startTime = startTime;
      _hasWarnings = hasWarnings;
      _hasErrors = hasErrors;
      _sequenceNumber = sequenceNumber;
    }

    public Guid SyncronizationProfileId
    {
      get { return _syncronizationProfileId; }
    }

    public DateTime StartTime
    {
      get { return _startTime; }
    }

    public bool HasErrors
    {
      get { return _hasErrors; }
    }

    public bool HasWarnings
    {
      get { return _hasWarnings; }
    }

    public override string ToString ()
    {
      return string.Format ("{0}{1:yyyyMMddHHmmss}{2}{3}{4}.log", _syncronizationProfileId, _startTime, _hasWarnings ? 1 : 0, _hasErrors ? 1 : 0, _sequenceNumber);
    }

    public static bool TryParse (string value, out SynchronizationReportName name)
    {
      var match = Regex.Match (value, @"^(?<id>.*)(?<start>\d{14})(?<warnings>[01])(?<errors>[01])(?<sequence>\d+).log$");
      if (match.Success)
      {
        name = new SynchronizationReportName (
            new Guid (match.Groups["id"].Value),
            DateTime.SpecifyKind (DateTime.ParseExact (match.Groups["start"].Value, "yyyyMMddHHmmss", CultureInfo.InvariantCulture), DateTimeKind.Utc),
            match.Groups["warnings"].Value == "1",
            match.Groups["errors"].Value == "1",
            int.Parse (match.Groups["sequence"].Value));
        return true;
      }
      else
      {
        name = null;
        return false;
      }
    }

    public SynchronizationReportName IncreaseSequence ()
    {
      return new SynchronizationReportName (_syncronizationProfileId, _startTime, _hasWarnings, _hasErrors, _sequenceNumber + 1);
    }
    
  }
}