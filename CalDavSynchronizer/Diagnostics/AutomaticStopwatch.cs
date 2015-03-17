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
using System.Diagnostics;
using log4net;
using log4net.Core;

namespace CalDavSynchronizer.Diagnostics
{
  internal class AutomaticStopwatch : IDisposable
  {
    private readonly string _name;
    private readonly Stopwatch _stopwatch;
    private readonly ILog _logger;
    private readonly Level _logLevel;


    public static AutomaticStopwatch StartInfo (ILog logger, string measuredProcessDescription)
    {
      return new AutomaticStopwatch (logger, measuredProcessDescription, Level.Info);
    }


    public static AutomaticStopwatch StartDebug (ILog logger)
    {
      return new AutomaticStopwatch (logger, string.Empty, Level.Debug);
    }

    private AutomaticStopwatch (ILog logger, string name, Level logLevel)
    {
      _name = name;
      _logLevel = logLevel;
      _logger = logger;

      if (_name != string.Empty)
        _logger.Logger.Log (typeof (AutomaticStopwatch), _logLevel, string.Format ("Starting '{0}'", _name), null);

      _stopwatch = Stopwatch.StartNew();
    }


    public void Dispose ()
    {
      _stopwatch.Stop();
      if (_name != string.Empty)
        _logger.Logger.Log (typeof (AutomaticStopwatch), _logLevel, string.Format ("Duration of '{0}': {1}", _name, _stopwatch.Elapsed), null);
      else
        _logger.Logger.Log (typeof (AutomaticStopwatch), _logLevel, string.Format ("Duration: {0}", _stopwatch.Elapsed), null);
    }
  }
}