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
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace CalDavSynchronizer.AutomaticUpdates
{
  public class UpdateChecker
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private readonly IAvailableVersionService _availableVersionService;
    private readonly Object _timerLock = new object();
    private readonly Timer _timer;
    private bool _isEnabled;

    public event EventHandler<NewerVersionFoundEventArgs> NewerVersionFound;

    protected virtual void OnNewerVersionFound (NewerVersionFoundEventArgs e)
    {
      var handler = NewerVersionFound;
      if (handler != null)
        handler (this, e);
    }

    public UpdateChecker (IAvailableVersionService availableVersionService)
    {
      if (availableVersionService == null)
        throw new ArgumentNullException ("availableVersionService");

      _availableVersionService = availableVersionService;

      _timer = new Timer (Timer_Elapsed);
    }

    public bool IsEnabled
    {
      get { return _isEnabled; }
      set
      {
        if (value != _isEnabled)
        {
          if (value)
          {
            _timer.Change (TimeSpan.FromSeconds (10), TimeSpan.FromDays (1));
          }
          else
          {
            lock (_timerLock)
            {
              _timer.Change (Timeout.Infinite, Timeout.Infinite);
            }
          }

          _isEnabled = value;
        }
      }
    }

    private void Timer_Elapsed (object _)
    {
      try
      {
        var availableVersion = _availableVersionService.GetVersionOfDefaultDownload();
        if (availableVersion == null)
        {
          s_logger.Error ("Did not find any default Version!");
          return;
        }

        var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
        if (availableVersion > currentVersion)
        {
          OnNewerVersionFound (
              new NewerVersionFoundEventArgs (
                  availableVersion,
                  _availableVersionService.GetWhatsNewNoThrow (currentVersion, availableVersion),
                  _availableVersionService.DownloadLink));
        }
      }
      catch (Exception x)
      {
        s_logger.Error (null, x);
      }
    }
  }
}