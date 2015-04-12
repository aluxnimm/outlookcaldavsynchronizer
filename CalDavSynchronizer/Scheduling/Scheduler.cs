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
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using CalDavSynchronizer.Contracts;
using log4net;
using Microsoft.Office.Interop.Outlook;
using Exception = System.Exception;

namespace CalDavSynchronizer.Scheduling
{
  internal class Scheduler
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private readonly Timer _synchronizationTimer = new Timer();

    private readonly NameSpace _outlookSession;
    private Dictionary<Guid, SynchronizationWorker> _workersById = new Dictionary<Guid, SynchronizationWorker>();
    private readonly string _outlookEmailAddress;
    private readonly string _applicationDataDirectory;
    private readonly TimeSpan _timerInterval = TimeSpan.FromSeconds (30);

    public Scheduler (NameSpace outlookSession, string applicationDataDirectory)
    {
      _outlookEmailAddress = outlookSession.CurrentUser.Address;
      _outlookSession = outlookSession;
      _applicationDataDirectory = applicationDataDirectory;
      _synchronizationTimer.Tick += _synchronizationTimer_Tick;
      _synchronizationTimer.Interval = (int) _timerInterval.TotalMilliseconds;
      _synchronizationTimer.Start();
    }


    private void _synchronizationTimer_Tick (object sender, EventArgs e)
    {
      _synchronizationTimer.Stop();
      foreach (var worker in _workersById.Values)
        worker.RunIfRequiredAndReschedule();
      _synchronizationTimer.Start();
    }

    public void SetOptions (Options[] options)
    {
      Dictionary<Guid, SynchronizationWorker> workersById = new Dictionary<Guid, SynchronizationWorker>();
      foreach (var option in options)
      {
        try
        {
          SynchronizationWorker worker;
          if (!_workersById.TryGetValue (option.Id, out worker))
            worker = new SynchronizationWorker (_outlookEmailAddress, _applicationDataDirectory);
          worker.UpdateOptions (_outlookSession, option);
          workersById.Add (option.Id, worker);
        }
        catch (Exception x)
        {
          s_logger.Error (null, x);
        }
      }
      _workersById = workersById;
    }

    public void RunNow ()
    {
      foreach (var worker in _workersById.Values)
        worker.RunNoThrowAndReschedule();
    }
  }
}