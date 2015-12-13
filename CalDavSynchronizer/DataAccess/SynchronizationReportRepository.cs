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
using CalDavSynchronizer.Utilities;
using GenSync.Logging;

namespace CalDavSynchronizer.DataAccess
{
  public class SynchronizationReportRepository : ISynchronizationReportRepository
  {
    private readonly string _reportDirectory;

    public SynchronizationReportRepository (string reportDirectory)
    {
      _reportDirectory = reportDirectory;
    }

    public void AddReport (SynchronizationReport report)
    {
      if (!Directory.Exists (_reportDirectory))
        Directory.CreateDirectory (_reportDirectory);

      using (var fileStream = File.Create (Path.Combine (_reportDirectory, Guid.NewGuid().ToString() + ".log")))
      {
        Serializer<SynchronizationReport>.SerializeTo (report, fileStream);
      }
    }
  }
}