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
// along with this program.  If not, see <http://www.gnu.org/licenses/>.using System;

using System;
using System.IO;
using System.Reflection;
using System.Threading;
using CalDavSynchronizer.DataAccess;
using log4net;

namespace CalDavSynchronizer.Reports
{
  internal class ReportGarbageCollection
  {
    private readonly ISynchronizationReportRepository _reportRepository;
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);
    private readonly object _maxAgeLock = new object();
    private TimeSpan _maxAge;

    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly Timer _timer;

    public ReportGarbageCollection (ISynchronizationReportRepository reportRepository, TimeSpan maxAge)
    {
      _reportRepository = reportRepository;

      MaxAge = maxAge;
      _timer = new Timer (Timer_Elapsed);
      Thread.MemoryBarrier();
      _timer.Change (TimeSpan.FromMinutes (1), TimeSpan.FromDays (1));
    }

    public TimeSpan MaxAge
    {
      get
      {
        lock (_maxAgeLock)
          return _maxAge;
      }
      set
      {
        lock (_maxAgeLock)
          _maxAge = value;
      }
    }

    private void Timer_Elapsed (object _)
    {
      try
      {
        var maxAge = MaxAge;

        foreach (var reportName in _reportRepository.GetAvailableReports())
        {
          if (DateTime.UtcNow - reportName.StartTime > maxAge)
          {
            try
            {
              _reportRepository.DeleteReport (reportName);
            }
            catch (Exception x)
            {
              s_logger.Error (null, x);
            }
          }
        }
      }
      catch (Exception x)
      {
        s_logger.Error (null, x);
      }
    }
  }
}