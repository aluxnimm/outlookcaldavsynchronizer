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
using System.Threading.Tasks;
using System.Windows.Forms;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.Utilities;
using log4net;

namespace CalDavSynchronizer.Scheduling
{
  public class Scheduler
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private readonly Timer _synchronizationTimer = new Timer();

    private Dictionary<Guid, SynchronizationWorker> _workersById = new Dictionary<Guid, SynchronizationWorker>();
    private readonly TimeSpan _timerInterval = TimeSpan.FromSeconds (30);
    private readonly ISynchronizerFactory _synchronizerFactory;
    private readonly Action _ensureSynchronizationContext;

    public Scheduler (ISynchronizerFactory synchronizerFactory, Action ensureSynchronizationContext)
    {
      if (synchronizerFactory == null)
        throw new ArgumentNullException ("synchronizerFactory");
      if (ensureSynchronizationContext == null)
        throw new ArgumentNullException ("ensureSynchronizationContext");

      _synchronizerFactory = synchronizerFactory;
      _ensureSynchronizationContext = ensureSynchronizationContext;
      _synchronizationTimer.Tick += _synchronizationTimer_Tick;
      _synchronizationTimer.Interval = (int) _timerInterval.TotalMilliseconds;
      _synchronizationTimer.Start();
    }


    private async void _synchronizationTimer_Tick (object sender, EventArgs e)
    {
      try
      {
        _ensureSynchronizationContext();
        _synchronizationTimer.Stop();
        foreach (var worker in _workersById.Values)
          await worker.RunIfRequiredAndReschedule();
        _synchronizationTimer.Start();
      }
      catch (Exception x)
      {
        ExceptionHandler.Instance.LogException (x, s_logger);
      }
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
            worker = new SynchronizationWorker (_synchronizerFactory);
          worker.UpdateOptions (option);
          workersById.Add (option.Id, worker);
        }
        catch (Exception x)
        {
          ExceptionHandler.Instance.LogException (x, s_logger);
        }
      }
      _workersById = workersById;
    }

    public async Task RunNow ()
    {
      foreach (var worker in _workersById.Values)
        await worker.RunNoThrowAndRescheduleIfNotRunning ();
    }
  }
}